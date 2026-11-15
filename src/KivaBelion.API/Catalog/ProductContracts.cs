namespace KivaBelion.API.Catalog;

/// <summary>What the admin panel sends to create or edit a product. Stock is set separately.</summary>
public sealed class ProductWriteRequest
{
    public int? CategoryId { get; set; }
    public string? Sku { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public int SizeMl { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public string? ImageUrl { get; set; }
    public string? Gender { get; set; }
    public string? Family { get; set; }
    public string? Intensity { get; set; }
    public int SortOrder { get; set; }
    public string? TopNotes { get; set; }
    public string? HeartNotes { get; set; }
    public string? BaseNotes { get; set; }
}

public sealed class StockRequest
{
    public int QuantityOnHand { get; set; }
    public int? LowStockThreshold { get; set; }
}

public sealed class VisibilityRequest
{
    public bool IsActive { get; set; }
}

/// <summary>Product as the owner sees it, including hidden products, stock and unverified notes.</summary>
public sealed record AdminProductDto(
    int Id, int CategoryId, string? Category, string Sku, string Name, string Slug,
    string? ShortDescription, string? Description, int SizeMl, decimal Price, bool IsActive,
    string? Notes, string? ImageUrl, string? Gender, string? Family, string? Intensity, int SortOrder,
    string? TopNotes, string? HeartNotes, string? BaseNotes,
    int? QuantityOnHand, int? ReservedQuantity, int? AvailableQuantity, int? LowStockThreshold);

/// <summary>Product as shoppers see it. Verified-note fields are deliberately left out.</summary>
public sealed record PublicProductDto(
    int Id, string Sku, string Name, string Slug, string? ShortDescription, string? Description,
    int SizeMl, decimal Price, string? Category,
    string? Notes, string? ImageUrl, string? Gender, string? Family, string? Intensity, bool SoldOut);
