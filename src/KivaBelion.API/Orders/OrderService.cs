using KivaBelion.API.Data;
using KivaBelion.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KivaBelion.API.Orders;

public enum PlaceOrderOutcome
{
    Created,
    AlreadyExists,        // the same Idempotency-Key was used before: the original order is returned
    OrdersClosed,
    Invalid,
    ProductUnavailable,
    PriceChanged
}

public sealed class PlaceOrderResult
{
    public PlaceOrderOutcome Outcome { get; init; }
    public OrderDto? Order { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
    public string? Message { get; init; }
    public decimal? ServerTotal { get; init; }
}

/// <summary>
/// Places Cash on Delivery orders. Everything that matters (prices, totals, product existence, stock)
/// is decided here from the database, never from numbers sent by the browser.
/// </summary>
public sealed class OrderService(KivaBelionDbContext db, IConfiguration config, ILogger<OrderService> logger)
{
    public static OrderDto ToDto(Order o) => new(
        o.OrderNumber,
        o.PublicId,
        o.Status.ToString(),
        o.PaymentMethod.ToString(),
        PaymentStatusText(o),
        o.CurrencyCode,
        o.Subtotal,
        o.ShippingFee,
        o.Total,
        o.Items.Select(i => new OrderLineDto(i.ProductName, i.Quantity, i.UnitPrice, i.LineTotal)).ToList(),
        o.CreatedAtUtc,
        o.IsTestOrder);

    /// <summary>Cash on Delivery orders are unpaid until the cash is handed over.</summary>
    public static string PaymentStatusText(Order o) =>
        o.Status == OrderStatus.Delivered ? "Paid in cash on delivery"
        : o.Status == OrderStatus.Cancelled ? "Cancelled"
        : "Unpaid - pay cash on delivery";

    public async Task<PlaceOrderResult> PlaceAsync(CreateOrderRequest request, string? idempotencyKey, CancellationToken ct)
    {
        if (!config.GetValue("Features:OrdersEnabled", false))
            return new PlaceOrderResult { Outcome = PlaceOrderOutcome.OrdersClosed, Message = "Online ordering is not open yet." };

        var errors = OrderRequestValidator.Validate(request);
        if (!string.IsNullOrEmpty(idempotencyKey) && !OrderRequestValidator.IsValidIdempotencyKey(idempotencyKey))
            errors["idempotencyKey"] = new[] { "Invalid Idempotency-Key header." };
        if (errors.Count > 0)
            return new PlaceOrderResult { Outcome = PlaceOrderOutcome.Invalid, Errors = errors };

        // A repeated submission (double click, retry after a lost response) returns the order that already exists.
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existing = await FindByKeyAsync(idempotencyKey, ct);
            if (existing is not null)
                return new PlaceOrderResult { Outcome = PlaceOrderOutcome.AlreadyExists, Order = ToDto(existing) };
        }

        // ----- look every product up in the database -----
        var wanted = request.Items!.Select(i => i.ProductSlug!.Trim()).ToList();
        var products = await db.Products
            .Include(p => p.Inventory)
            .Where(p => wanted.Contains(p.Slug) && p.IsActive)
            .ToListAsync(ct);

        var unavailable = new List<string>();
        var lines = new List<(Product Product, int Quantity)>();
        foreach (var item in request.Items!)
        {
            var slug = item.ProductSlug!.Trim();
            var product = products.FirstOrDefault(p => p.Slug == slug);
            if (product is null) { unavailable.Add(slug); continue; }

            // Stock is only enforced when an inventory row exists for the product.
            if (product.Inventory is not null && product.Inventory.AvailableQuantity < item.Quantity)
            {
                unavailable.Add(product.Name);
                continue;
            }
            lines.Add((product, item.Quantity));
        }
        if (unavailable.Count > 0)
        {
            return new PlaceOrderResult
            {
                Outcome = PlaceOrderOutcome.ProductUnavailable,
                Message = "Some items are no longer available: " + string.Join(", ", unavailable) + ".",
            };
        }

        // ----- money (server side only) -----
        var shippingFee = config.GetValue("Orders:ShippingFeePkr", 0m);
        var totals = OrderPricing.Calculate(lines.Select(l => (l.Product.Price, l.Quantity)), shippingFee);
        if (request.ExpectedTotal.HasValue && decimal.Round(request.ExpectedTotal.Value, 2) != totals.Total)
        {
            return new PlaceOrderResult
            {
                Outcome = PlaceOrderOutcome.PriceChanged,
                Message = "Prices have changed since you added these items. Please review your bag.",
                ServerTotal = totals.Total,
            };
        }

        // ----- customer (one row per email; name and phone follow the latest order) -----
        var email = Customer.NormalizeEmail(request.Customer!.Email!);
        var fullName = request.Customer.FullName!.Trim();
        var phone = request.Customer.Phone!.Trim();
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Email == email, ct);
        if (customer is null)
        {
            customer = new Customer { Email = email, FullName = fullName, Phone = phone };
            db.Customers.Add(customer);
        }
        else
        {
            customer.FullName = fullName;
            customer.Phone = phone;
        }

        // ----- order -----
        var order = new Order
        {
            PublicId = Guid.NewGuid(),
            OrderNumber = await NewUniqueOrderNumberAsync(ct),
            IdempotencyKey = string.IsNullOrEmpty(idempotencyKey) ? null : idempotencyKey,
            Customer = customer,
            Status = OrderStatus.Pending,
            PaymentMethod = PaymentMethod.CashOnDelivery,
            IsTestOrder = config.GetValue("Features:TestMode", true),
            CurrencyCode = "PKR",
            Subtotal = totals.Subtotal,
            ShippingFee = totals.ShippingFee,
            DiscountTotal = 0m,
            Total = totals.Total,
            ShipRecipientName = fullName,
            ShipPhone = phone,
            ShipLine1 = request.Shipping!.Line1!.Trim(),
            ShipCity = request.Shipping.City!.Trim(),
            ShipProvince = string.IsNullOrWhiteSpace(request.Shipping.Province) ? null : request.Shipping.Province.Trim(),
            ShipPostalCode = string.IsNullOrWhiteSpace(request.Shipping.PostalCode) ? null : request.Shipping.PostalCode.Trim(),
            ShipCountryCode = "PK",
            CustomerNote = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
        };
        foreach (var (product, quantity) in lines)
        {
            order.Items.Add(new OrderItem
            {
                Product = product,
                ProductSku = product.Sku,
                ProductName = product.Name,
                UnitPrice = decimal.Round(product.Price, 2),
                Quantity = quantity,
                LineTotal = decimal.Round(product.Price, 2) * quantity,
            });
        }
        db.Orders.Add(order);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (!string.IsNullOrEmpty(idempotencyKey))
        {
            // Two identical requests raced each other: the unique Idempotency-Key let only one through.
            logger.LogInformation(ex, "Order insert conflict for an idempotency key; looking up the existing order.");
            db.ChangeTracker.Clear();
            var existing = await FindByKeyAsync(idempotencyKey, ct);
            if (existing is not null)
                return new PlaceOrderResult { Outcome = PlaceOrderOutcome.AlreadyExists, Order = ToDto(existing) };
            throw;
        }

        logger.LogInformation("Order {OrderNumber} created: {ItemCount} line(s), total {Total} PKR, test={IsTest}",
            order.OrderNumber, order.Items.Count, order.Total, order.IsTestOrder);
        return new PlaceOrderResult { Outcome = PlaceOrderOutcome.Created, Order = ToDto(order) };
    }

    private Task<Order?> FindByKeyAsync(string key, CancellationToken ct) =>
        db.Orders.AsNoTracking().Include(o => o.Items).FirstOrDefaultAsync(o => o.IdempotencyKey == key, ct);

    private async Task<string> NewUniqueOrderNumberAsync(CancellationToken ct)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = OrderNumberGenerator.Next();
            if (!await db.Orders.AnyAsync(o => o.OrderNumber == candidate, ct)) return candidate;
        }
        throw new InvalidOperationException("Could not generate a unique order number.");
    }
}
