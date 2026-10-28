namespace KivaBelion.API.Security;

/// <summary>Settings section "Jwt". The SigningKey is a SECRET: it comes from User Secrets / environment variables only.</summary>
public sealed class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public string SigningKey { get; set; } = string.Empty;
}
