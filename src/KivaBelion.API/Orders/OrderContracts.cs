namespace KivaBelion.API.Orders;

/// <summary>
/// What the website sends to place a Cash on Delivery order.
/// Prices are deliberately NOT part of it: the server looks every price up in the database.
/// </summary>
public sealed class CreateOrderRequest
{
    public CustomerInput? Customer { get; set; }
    public ShippingInput? Shipping { get; set; }

    /// <summary>Optional delivery note or gift message (max 500 characters).</summary>
    public string? Note { get; set; }

    /// <summary>Only "CashOnDelivery" is accepted. Omitted means Cash on Delivery.</summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// The total the customer saw. If it no longer matches the server's own total (a price changed),
    /// the order is refused with 409 so the customer can review the new price. Optional.
    /// </summary>
    public decimal? ExpectedTotal { get; set; }

    public List<OrderLineInput>? Items { get; set; }
}

public sealed class CustomerInput
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public sealed class ShippingInput
{
    public string? Line1 { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
}

public sealed class OrderLineInput
{
    /// <summary>The product's slug, which equals the product id used by the website (for example "noir").</summary>
    public string? ProductSlug { get; set; }
    public int Quantity { get; set; }
}

public sealed record OrderLineDto(string Name, int Quantity, decimal UnitPrice, decimal LineTotal);

/// <summary>What the website gets back. The server's numbers are the only ones that count.</summary>
public sealed record OrderDto(
    string OrderNumber,
    Guid PublicId,
    string Status,
    string PaymentMethod,
    string PaymentStatus,
    string Currency,
    decimal Subtotal,
    decimal ShippingFee,
    decimal Total,
    IReadOnlyList<OrderLineDto> Items,
    DateTimeOffset CreatedAtUtc,
    bool IsTestOrder);
