using KivaBelion.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KivaBelion.API.Controllers;

[ApiController]
[Route("api/v1/products")]
public sealed class ProductsController(KivaBelionDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Id,
                p.Sku,
                p.Name,
                p.Slug,
                p.ShortDescription,
                p.Description,
                p.SizeMl,
                p.Price,
                Category = p.Category == null ? null : p.Category.Name
            })
            .ToListAsync(cancellationToken);

        return Ok(products);
    }
}
