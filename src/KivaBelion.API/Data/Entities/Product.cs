using System.ComponentModel.DataAnnotations;

namespace KivaBelion.API.Data.Entities;

public class Product : ITimestamped
{
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    /// <summary>Stock-keeping code, unique, e.g. "KB-001-50".</summary>
    [Required, StringLength(50, MinimumLength = 2)]
    [RegularExpression("^[A-Za-z0-9._-]+$", ErrorMessage = "SKU may contain only letters, digits, dot, underscore and hyphen.")]
    public string Sku { get; set; } = string.Empty;

    [Required, StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(160, MinimumLength = 2)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug may contain only lower-case letters, digits and single hyphens.")]
    public string Slug { get; set; } = string.Empty;

    [StringLength(300)]
    public string? ShortDescription { get; set; }

    [StringLength(4000)]
    public string? Description { get; set; }

    /// <summary>Bottle size in millilitres.</summary>
    [Range(1, 5000)]
    public int SizeMl { get; set; }

    /// <summary>Selling price in PKR, two decimal places.</summary>
    [Range(typeof(decimal), "0", "9999999999")]
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ProductInventory? Inventory { get; set; }
}
