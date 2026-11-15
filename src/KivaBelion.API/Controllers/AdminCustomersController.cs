using KivaBelion.API.Data;
using KivaBelion.API.Data.Entities;
using KivaBelion.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KivaBelion.API.Controllers;

/// <summary>Customers are created automatically when they place an order. Read-only here.</summary>
[ApiController]
[Route("api/v1/admin/customers")]
[Authorize(Policy = AuthorizationPolicies.StaffOrAdmin)]
public sealed class AdminCustomersController(KivaBelionDbContext db) : ControllerBase
{
    /// <summary>Newest first. Optional search: ?q=ali (matches name or email).</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        var query = db.Customers.AsNoTracking().AsQueryable();
        var term = q?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(term))
            query = query.Where(c => c.Email.Contains(term) || c.FullName.ToLower().Contains(term));

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(c => c.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new AdminCustomerDto(
                c.Id, c.FullName, c.Email, c.Phone, c.CreatedAtUtc,
                c.Orders.Count,
                c.Orders.Where(o => o.Status != OrderStatus.Cancelled).Sum(o => o.Total)))
            .ToListAsync(cancellationToken);

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken cancellationToken)
    {
        var customer = await db.Customers.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id, c.FullName, c.Email, c.Phone, c.CreatedAtUtc,
                Orders = c.Orders.OrderByDescending(o => o.CreatedAtUtc)
                    .Select(o => new AdminCustomerOrderDto(o.PublicId, o.OrderNumber, o.Status.ToString(), o.Total, o.ShipCity, o.CreatedAtUtc))
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }
}

public sealed record AdminCustomerDto(
    long Id, string FullName, string Email, string Phone, DateTimeOffset CreatedAtUtc,
    int OrderCount, decimal TotalSpent);

public sealed record AdminCustomerOrderDto(
    Guid PublicId, string OrderNumber, string Status, decimal Total, string City, DateTimeOffset CreatedAtUtc);
