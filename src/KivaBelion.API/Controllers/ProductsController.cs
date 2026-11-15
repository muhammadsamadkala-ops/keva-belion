using KivaBelion.API.Catalog;
using KivaBelion.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KivaBelion.API.Controllers;

[ApiController]
[Route("api/v1/products")]
public sealed class ProductsController(KivaBelionDbContext dbContext) : ControllerBase
{
    private IQueryable<PublicProductDto> PublicProducts() =>
        dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
            .Select(p => new PublicProductDto(
                p.Id, p.Sku, p.Name, p.Slug, p.ShortDescription, p.Description,
                p.SizeMl, p.Price, p.Category == null ? null : p.Category.Name,
                p.Notes, p.ImageUrl, p.Gender, p.Family, p.Intensity,
                p.Inventory != null && p.Inventory.QuantityOnHand - p.Inventory.ReservedQuantity <= 0));

    [HttpGet]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken) =>
        Ok(await PublicProducts().ToListAsync(cancellationToken));

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var product = await PublicProducts().FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }
}
