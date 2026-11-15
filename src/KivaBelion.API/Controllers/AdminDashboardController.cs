using KivaBelion.API.Data;
using KivaBelion.API.Data.Entities;
using KivaBelion.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KivaBelion.API.Controllers;

/// <summary>One call that fills the admin home screen.</summary>
[ApiController]
[Route("api/v1/admin/dashboard")]
[Authorize(Policy = AuthorizationPolicies.StaffOrAdmin)]
public sealed class AdminDashboardController(KivaBelionDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var weekAgo = DateTimeOffset.UtcNow.AddDays(-7);

        var byStatus = await db.Orders.AsNoTracking()
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var ordersLast7Days = await db.Orders.AsNoTracking().CountAsync(o => o.CreatedAtUtc >= weekAgo, cancellationToken);
        var testOrders = await db.Orders.AsNoTracking().CountAsync(o => o.IsTestOrder, cancellationToken);
        var deliveredRevenue = await db.Orders.AsNoTracking()
            .Where(o => o.Status == OrderStatus.Delivered && !o.IsTestOrder)
            .SumAsync(o => (decimal?)o.Total, cancellationToken) ?? 0m;
        var customers = await db.Customers.AsNoTracking().CountAsync(cancellationToken);
        var activeProducts = await db.Products.AsNoTracking().CountAsync(p => p.IsActive, cancellationToken);

        var lowStock = await db.ProductInventories.AsNoTracking()
            .Where(i => i.Product!.IsActive && i.QuantityOnHand - i.ReservedQuantity <= i.LowStockThreshold)
            .OrderBy(i => i.QuantityOnHand - i.ReservedQuantity)
            .Select(i => new LowStockDto(i.ProductId, i.Product!.Name, i.QuantityOnHand - i.ReservedQuantity, i.LowStockThreshold))
            .ToListAsync(cancellationToken);

        // Every status is listed, even when its count is 0.
        var statusCounts = Enum.GetValues<OrderStatus>()
            .ToDictionary(s => s.ToString(), s => byStatus.FirstOrDefault(x => x.Status == s)?.Count ?? 0);

        return Ok(new
        {
            ordersByStatus = statusCounts,
            ordersLast7Days,
            testOrders,
            deliveredRevenuePkr = deliveredRevenue,
            customers,
            activeProducts,
            lowStock,
        });
    }
}

public sealed record LowStockDto(int ProductId, string Name, int Available, int LowStockThreshold);
