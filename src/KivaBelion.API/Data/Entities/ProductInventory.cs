using System.ComponentModel.DataAnnotations;

namespace KivaBelion.API.Data.Entities;

/// <summary>
/// Stock for one product (exactly one row per product). Quantities are whole bottles, so they are integers.
/// "Reserved" is stock held for pending orders (used in a later phase).
/// </summary>
public class ProductInventory : ITimestamped
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(0, int.MaxValue)]
    public int QuantityOnHand { get; set; }

    [Range(0, int.MaxValue)]
    public int ReservedQuantity { get; set; }

    [Range(0, 100000)]
    public int LowStockThreshold { get; set; } = 5;

    /// <summary>Bottles that can still be sold. Calculated, not stored.</summary>
    public int AvailableQuantity => QuantityOnHand - ReservedQuantity;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
