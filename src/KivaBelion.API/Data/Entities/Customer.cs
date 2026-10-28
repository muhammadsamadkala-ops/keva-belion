using System.ComponentModel.DataAnnotations;

namespace KivaBelion.API.Data.Entities;

public class Customer : ITimestamped
{
    public long Id { get; set; }

    /// <summary>Stored in lower case (the database enforces this) so one person cannot be registered twice.</summary>
    [Required, StringLength(254), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(30)]
    [RegularExpression(@"^\+?[0-9 ()\-]{7,30}$", ErrorMessage = "Phone may contain only digits, spaces, brackets, hyphens and a leading +.")]
    public string Phone { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public List<CustomerAddress> Addresses { get; set; } = new();
    public List<Order> Orders { get; set; } = new();

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
