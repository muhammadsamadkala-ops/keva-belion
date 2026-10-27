using System.ComponentModel.DataAnnotations;

namespace KivaBelion.API.Data.Entities;

/// <summary>A staff member who may sign in to the admin area. Customers are NOT admin users.</summary>
public class AdminUser : ITimestamped
{
    public int Id { get; set; }

    /// <summary>Stored in lower case (the database enforces this).</summary>
    [Required, StringLength(254), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Salted, slow hash (PBKDF2). The real password is never stored anywhere.</summary>
    [Required, StringLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>New accounts get the lowest role unless told otherwise.</summary>
    public AdminRole Role { get; set; } = AdminRole.Staff;

    public bool IsActive { get; set; } = true;

    public int FailedLoginCount { get; set; }
    public DateTimeOffset? LockoutEndUtc { get; set; }
    public DateTimeOffset? LastLoginAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
