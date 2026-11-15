using KivaBelion.API.Catalog;
using KivaBelion.API.Data;
using KivaBelion.API.Data.Entities;
using KivaBelion.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KivaBelion.API.Controllers;

/// <summary>Owner tools for the catalogue. Nothing is ever hard-deleted: a product is hidden instead.</summary>
[ApiController]
[Route("api/v1/admin/products")]
public sealed class AdminProductsController(KivaBelionDbContext db) : ControllerBase
{
    private static AdminProductDto ToDto(Product p) => new(
        p.Id, p.CategoryId, p.Category?.Name, p.Sku, p.Name, p.Slug,
        p.ShortDescription, p.Description, p.SizeMl, p.Price, p.IsActive,
        p.Notes, p.ImageUrl, p.Gender, p.Family, p.Intensity, p.SortOrder,
        p.TopNotes, p.HeartNotes, p.BaseNotes,
        p.Inventory?.QuantityOnHand, p.Inventory?.ReservedQuantity, p.Inventory?.AvailableQuantity, p.Inventory?.LowStockThreshold);

    private IQueryable<Product> WithDetails() =>
        db.Products.Include(p => p.Category).Include(p => p.Inventory);

    private static ProblemDetails Err(int status, string title) => new() { Status = status, Title = title };

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.StaffOrAdmin)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var rows = await WithDetails().AsNoTracking()
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
        return Ok(rows.Select(ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.StaffOrAdmin)]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var product = await WithDetails().AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return product is null ? NotFound() : Ok(ToDto(product));
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> Create([FromBody] ProductWriteRequest request, CancellationToken cancellationToken)
    {
        var errors = ProductWriteValidator.Validate(request);
        if (errors.Count > 0) return ValidationProblem(new ValidationProblemDetails(errors));

        var conflict = await CheckUniqueAsync(request, null, cancellationToken);
        if (conflict is not null) return conflict;
        if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken))
            return BadRequest(Err(400, "That category does not exist."));

        var product = new Product();
        Apply(product, request);
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        var created = await WithDetails().AsNoTracking().FirstAsync(p => p.Id == product.Id, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = product.Id }, ToDto(created));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> Update(int id, [FromBody] ProductWriteRequest request, CancellationToken cancellationToken)
    {
        var errors = ProductWriteValidator.Validate(request);
        if (errors.Count > 0) return ValidationProblem(new ValidationProblemDetails(errors));

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null) return NotFound();

        var conflict = await CheckUniqueAsync(request, id, cancellationToken);
        if (conflict is not null) return conflict;
        if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken))
            return BadRequest(Err(400, "That category does not exist."));

        Apply(product, request);
        await db.SaveChangesAsync(cancellationToken);

        var updated = await WithDetails().AsNoTracking().FirstAsync(p => p.Id == id, cancellationToken);
        return Ok(ToDto(updated));
    }

    /// <summary>Hide a product from the shop (or show it again). Old orders keep their product.</summary>
    [HttpPatch("{id:int}/visibility")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> SetVisibility(int id, [FromBody] VisibilityRequest request, CancellationToken cancellationToken)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null) return NotFound();
        product.IsActive = request.IsActive;
        await db.SaveChangesAsync(cancellationToken);

        var updated = await WithDetails().AsNoTracking().FirstAsync(p => p.Id == id, cancellationToken);
        return Ok(ToDto(updated));
    }

    /// <summary>Sets how many bottles are on the shelf. Reserved bottles (pending orders) are kept as they are.</summary>
    [HttpPut("{id:int}/stock")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> SetStock(int id, [FromBody] StockRequest request, CancellationToken cancellationToken)
    {
        if (request.QuantityOnHand < 0 || request.QuantityOnHand > 1_000_000)
            return BadRequest(Err(400, "Quantity must be between 0 and 1,000,000."));
        if (request.LowStockThreshold is < 0 or > 100000)
            return BadRequest(Err(400, "Low-stock level must be between 0 and 100,000."));

        var product = await db.Products.Include(p => p.Inventory).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null) return NotFound();

        var inventory = product.Inventory;
        if (inventory is null)
        {
            inventory = new ProductInventory { Product = product, LowStockThreshold = request.LowStockThreshold ?? 5 };
            db.ProductInventories.Add(inventory);
        }
        else if (request.QuantityOnHand < inventory.ReservedQuantity)
        {
            return Conflict(Err(409,
                $"{inventory.ReservedQuantity} bottles are held for open orders, so the quantity cannot be lower than that."));
        }

        inventory.QuantityOnHand = request.QuantityOnHand;
        if (request.LowStockThreshold.HasValue) inventory.LowStockThreshold = request.LowStockThreshold.Value;
        await db.SaveChangesAsync(cancellationToken);

        var updated = await WithDetails().AsNoTracking().FirstAsync(p => p.Id == id, cancellationToken);
        return Ok(ToDto(updated));
    }

    private async Task<IActionResult?> CheckUniqueAsync(ProductWriteRequest r, int? ownId, CancellationToken ct)
    {
        var sku = r.Sku!.Trim();
        var slug = r.Slug!.Trim();
        if (await db.Products.AnyAsync(p => p.Sku == sku && p.Id != ownId, ct))
            return Conflict(Err(409, "Another product already uses this SKU."));
        if (await db.Products.AnyAsync(p => p.Slug == slug && p.Id != ownId, ct))
            return Conflict(Err(409, "Another product already uses this slug."));
        return null;
    }

    private static void Apply(Product p, ProductWriteRequest r)
    {
        p.CategoryId = r.CategoryId!.Value;
        p.Sku = r.Sku!.Trim();
        p.Name = r.Name!.Trim();
        p.Slug = r.Slug!.Trim();
        p.ShortDescription = ProductWriteValidator.Clean(r.ShortDescription);
        p.Description = ProductWriteValidator.Clean(r.Description);
        p.SizeMl = r.SizeMl;
        p.Price = decimal.Round(r.Price, 2);
        p.IsActive = r.IsActive;
        p.Notes = ProductWriteValidator.Clean(r.Notes);
        p.ImageUrl = ProductWriteValidator.Clean(r.ImageUrl);
        p.Gender = ProductWriteValidator.Clean(r.Gender);
        p.Family = ProductWriteValidator.Clean(r.Family);
        p.Intensity = ProductWriteValidator.Clean(r.Intensity);
        p.SortOrder = r.SortOrder;
        p.TopNotes = ProductWriteValidator.Clean(r.TopNotes);
        p.HeartNotes = ProductWriteValidator.Clean(r.HeartNotes);
        p.BaseNotes = ProductWriteValidator.Clean(r.BaseNotes);
    }
}
