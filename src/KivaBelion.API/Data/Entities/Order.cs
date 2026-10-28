using System.ComponentModel.DataAnnotations;

namespace KivaBelion.API.Data.Entities;

/// <summary>
/// An order record. Phase B only creates the table: no code places real orders yet.
/// The shipping address is COPIED into the order, so later address edits never change past orders.
/// No card data or payment credentials are stored here.
/// </summary>
public class Order : ITimestamped
{
    public long Id { get; set; }

    /// <summary>Safe identifier to show in URLs and APIs instead of the internal number.</summary>
    public Guid PublicId { get; set; } = Guid.NewGuid();

    /// <summary>Human-friendly, hard-to-guess number such as "KB-7F3K9Q2M" (generated in Phase C).</summary>
    [Required, StringLength(30, MinimumLength = 6)]
    [RegularExpression("^[A-Z0-9-]+$")]
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>Optional key sent by the browser so a double-click cannot create two orders (Phase C).</summary>
    [StringLength(64)]
    public string? IdempotencyKey { get; set; }

    public long CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    /// <summary>True while the project is in test mode. Real orders do not exist yet.</summary>
    public bool IsTestOrder { get; set; } = true;

    [Required, StringLength(3, MinimumLength = 3)]
    public string CurrencyCode { get; set; } = "PKR";

    [Range(typeof(decimal), "0", "9999999999")] public decimal Subtotal { get; set; }
    [Range(typeof(decimal), "0", "9999999999")] public decimal ShippingFee { get; set; }
    [Range(typeof(decimal), "0", "9999999999")] public decimal DiscountTotal { get; set; }
    /// <summary>Must equal Subtotal + ShippingFee - DiscountTotal (the database checks this).</summary>
    [Range(typeof(decimal), "0", "9999999999")] public decimal Total { get; set; }

    // ----- Shipping address snapshot -----
    [Required, StringLength(150, MinimumLength = 2)] public string ShipRecipientName { get; set; } = string.Empty;
    [Required, StringLength(30)] public string ShipPhone { get; set; } = string.Empty;
    [Required, StringLength(200, MinimumLength = 3)] public string ShipLine1 { get; set; } = string.Empty;
    [StringLength(200)] public string? ShipLine2 { get; set; }
    [Required, StringLength(100, MinimumLength = 2)] public string ShipCity { get; set; } = string.Empty;
    [StringLength(100)] public string? ShipProvince { get; set; }
    [StringLength(20)] public string? ShipPostalCode { get; set; }
    [Required, StringLength(2, MinimumLength = 2)] public string ShipCountryCode { get; set; } = "PK";

    [StringLength(500)]
    public string? CustomerNote { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}
