using KivaBelion.API.Data;
using KivaBelion.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KivaBelion.API.Controllers;

/// <summary>The category list for the product form in the admin panel. Read-only.</summary>
[ApiController]
[Route("api/v1/admin/categories")]
[Authorize(Policy = AuthorizationPolicies.StaffOrAdmin)]
public sealed class AdminCategoriesController(KivaBelionDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var items = await db.Categories.AsNoTracking()
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new AdminCategoryDto(c.Id, c.Name, c.IsActive))
            .ToListAsync(cancellationToken);
        return Ok(items);
    }
}

public sealed record AdminCategoryDto(int Id, string Name, bool IsActive);
