using System.ComponentModel.DataAnnotations;

namespace KivaBelion.API.Data.Entities;

/// <summary>
/// One line of an order. Name, SKU and price are COPIED from the product at order time, so later price
/// changes never alter past orders.
/// </summary>
public class OrderItem
{
    public long Id { get; set; }

    public long OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Required, StringLength(50)] public string ProductSku { get; set; } = string.Empty;
    [Required, StringLength(150)] public string ProductName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "9999999999")]
    public decimal UnitPrice { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }

    /// <summary>Must equal UnitPrice x Quantity (the database checks this).</summary>
    [Range(typeof(decimal), "0", "9999999999")]
    public decimal LineTotal { get; set; }
}
