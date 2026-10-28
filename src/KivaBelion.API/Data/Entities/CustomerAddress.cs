using System.ComponentModel.DataAnnotations;

namespace KivaBelion.API.Data.Entities;

public class CustomerAddress : ITimestamped
{
    public long Id { get; set; }

    public long CustomerId { get; set; }
    public Customer? Customer { get; set; }

    [StringLength(50)]
    public string? Label { get; set; }

    [Required, StringLength(150, MinimumLength = 2)]
    public string RecipientName { get; set; } = string.Empty;

    [Required, StringLength(30)]
    [RegularExpression(@"^\+?[0-9 ()\-]{7,30}$")]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 3)]
    public string Line1 { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Line2 { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Province { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    /// <summary>Two-letter country code (ISO 3166-1 alpha-2).</summary>
    [Required, StringLength(2, MinimumLength = 2)]
    public string CountryCode { get; set; } = "PK";

    /// <summary>At most one default address per customer (enforced by the database).</summary>
    public bool IsDefault { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
