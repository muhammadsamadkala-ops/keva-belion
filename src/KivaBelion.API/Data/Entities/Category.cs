using System.ComponentModel.DataAnnotations;

namespace KivaBelion.API.Data.Entities;

public class Category : ITimestamped
{
    public int Id { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-friendly name, lower-case letters, digits and hyphens, e.g. "eau-de-parfum".</summary>
    [Required, StringLength(120, MinimumLength = 2)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug may contain only lower-case letters, digits and single hyphens.")]
    public string Slug { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public List<Product> Products { get; set; } = new();
}
