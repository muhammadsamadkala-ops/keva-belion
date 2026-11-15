using System.ComponentModel.DataAnnotations;
using KivaBelion.API.Data;
using KivaBelion.API.Data.Entities;
using KivaBelion.API.Orders;
using KivaBelion.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KivaBelion.API.Controllers;

/// <summary>Staff view of incoming orders (sign in first). This is how the owner sees and manages orders.</summary>
[ApiController]
[Route("api/v1/admin/orders")]
public sealed class AdminOrdersController(KivaBelionDbContext db) : ControllerBase
{
    private static AdminOrderDto ToAdminDto(Order o) => new(
        o.PublicId, o.OrderNumber, o.Status.ToString(), OrderService.PaymentStatusText(o), o.IsTestOrder,
        o.Customer?.FullName ?? o.ShipRecipientName, o.Customer?.Email ?? "", o.ShipPhone,
        o.ShipLine1, o.ShipCity, o.ShipProvince, o.CustomerNote,
        o.Subtotal, o.ShippingFee, o.Total, o.CreatedAtUtc,
        o.Items.Select(i => new OrderLineDto(i.ProductName, i.Quantity, i.UnitPrice, i.LineTotal)).ToList());

    /// <summary>Newest first. Optional filter: ?status=Pending</summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.StaffOrAdmin)]
    public async Task<IActionResult> List([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        var query = db.Orders.AsNoTracking().Include(o => o.Customer).Include(o => o.Items).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsed))
                return BadRequest(new ProblemDetails { Status = 400, Title = "Unknown status." });
            query = query.Where(o => o.Status == parsed);
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(o => o.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return Ok(new { total, page, pageSize, items = rows.Select(ToAdminDto).ToList() });
    }

    [HttpGet("{publicId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.StaffOrAdmin)]
    public async Task<IActionResult> Get(Guid publicId, CancellationToken cancellationToken)
    {
        var order = await db.Orders.AsNoTracking().Include(o => o.Customer).Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.PublicId == publicId, cancellationToken);
        return order is null ? NotFound() : Ok(ToAdminDto(order));
    }

    /// <summary>Moves an order along (Confirmed, Processing, Shipped, Delivered) or cancels it. Admin only.</summary>
    [HttpPatch("{publicId:guid}/status")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> SetStatus(Guid publicId, [FromBody] SetStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var next) || !Enum.IsDefined(next))
            return BadRequest(new ProblemDetails { Status = 400, Title = "Unknown status." });

        var order = await db.Orders.Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.Inventory)
            .FirstOrDefaultAsync(o => o.PublicId == publicId, cancellationToken);
        if (order is null) return NotFound();

        if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
            return Conflict(new ProblemDetails { Status = 409, Title = $"A {order.Status} order cannot be changed." });

        if (!OrderStatusRules.CanMove(order.Status, next))
            return Conflict(new ProblemDetails { Status = 409, Title = $"An order that is {order.Status} cannot be moved to {next}." });

        foreach (var item in order.Items)
        {
            var inventory = item.Product?.Inventory;
            if (inventory is null) continue;
            if (next == OrderStatus.Cancelled) OrderStock.Release(inventory, item.Quantity);
            else if (next == OrderStatus.Delivered) OrderStock.Deliver(inventory, item.Quantity);
        }

        order.Status = next;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToAdminDto(order));
    }
}

public sealed record AdminOrderDto(
    Guid PublicId, string OrderNumber, string Status, string PaymentStatus, bool IsTestOrder,
    string CustomerName, string CustomerEmail, string Phone,
    string AddressLine, string City, string? Province, string? Note,
    decimal Subtotal, decimal ShippingFee, decimal Total, DateTimeOffset CreatedAtUtc,
    IReadOnlyList<OrderLineDto> Items);

public sealed class SetStatusRequest
{
    [Required] public string Status { get; set; } = string.Empty;
}
