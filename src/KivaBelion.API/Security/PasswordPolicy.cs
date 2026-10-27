namespace KivaBelion.API.Security;

public static class PasswordPolicy
{
    public const int MinLength = 12;
    public const int MaxLength = 128;

    private static readonly string[] Blocked =
    {
        "password1234", "123456789012", "qwertyuiop12", "admin1234567", "letmein12345", "kivabelion123"
    };

    /// <summary>Returns a list of problems. An empty list means the password is acceptable.</summary>
    public static IReadOnlyList<string> Validate(string? password, string? email = null)
    {
        var problems = new List<string>();
        if (string.IsNullOrEmpty(password))
        {
            problems.Add("Password is required.");
            return problems;
        }

        if (password.Length < MinLength) problems.Add($"Password must be at least {MinLength} characters.");
        if (password.Length > MaxLength) problems.Add($"Password must be at most {MaxLength} characters.");
        if (!password.Any(char.IsLetter)) problems.Add("Password must contain at least one letter.");
        if (!password.Any(char.IsDigit)) problems.Add("Password must contain at least one digit.");
        if (password.Distinct().Count() < 5) problems.Add("Password is too repetitive.");
        if (Blocked.Contains(password.ToLowerInvariant())) problems.Add("Password is too common.");

        var localPart = email?.Split('@')[0];
        if (!string.IsNullOrEmpty(localPart) && localPart.Length >= 4 &&
            password.Contains(localPart, StringComparison.OrdinalIgnoreCase))
        {
            problems.Add("Password must not contain your email name.");
        }

        return problems;
    }
}
