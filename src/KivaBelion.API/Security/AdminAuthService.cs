using KivaBelion.API.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace KivaBelion.API.Security;

public sealed record LoginResult(AdminUser? User)
{
    public bool Succeeded => User is not null;
}

/// <summary>
/// Checks admin sign-ins. Every failure looks identical to the caller (no hints about which part was wrong),
/// repeated failures lock the account for a while, and the real reason is written to the log (never the password).
/// </summary>
public sealed class AdminAuthService(
    IAdminUserStore store,
    IPasswordHasher<AdminUser> hasher,
    TimeProvider time,
    ILogger<AdminAuthService> logger)
{
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public async Task<LoginResult> LoginAsync(string? email, string? password, CancellationToken cancellationToken)
    {
        var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();
        var attempt = password ?? string.Empty;
        var now = time.GetUtcNow();

        var user = await store.FindByEmailAsync(normalized, cancellationToken);

        if (user is null)
        {
            // Do the same amount of slow hashing work so response time does not reveal whether the email exists.
            hasher.HashPassword(new AdminUser(), attempt);
            logger.LogWarning("Admin login failed: unknown account.");
            return new LoginResult(null);
        }

        if (!user.IsActive)
        {
            hasher.HashPassword(user, attempt);
            logger.LogWarning("Admin login failed: account {AdminId} is disabled.", user.Id);
            return new LoginResult(null);
        }

        if (user.LockoutEndUtc is { } lockedUntil && lockedUntil > now)
        {
            hasher.HashPassword(user, attempt);
            logger.LogWarning("Admin login refused: account {AdminId} is locked until {LockedUntil}.", user.Id, lockedUntil);
            return new LoginResult(null);
        }

        var verification = hasher.VerifyHashedPassword(user, user.PasswordHash, attempt);

        if (verification == PasswordVerificationResult.Failed)
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= MaxFailedAttempts)
            {
                user.LockoutEndUtc = now + LockoutDuration;
                user.FailedLoginCount = 0;
                logger.LogWarning("Admin account {AdminId} locked after too many failed attempts.", user.Id);
            }
            else
            {
                logger.LogWarning("Admin login failed: wrong password for account {AdminId}.", user.Id);
            }

            await store.SaveChangesAsync(cancellationToken);
            return new LoginResult(null);
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, attempt);   // upgrade old hashes silently
        }

        user.FailedLoginCount = 0;
        user.LockoutEndUtc = null;
        user.LastLoginAtUtc = now;
        await store.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Admin {AdminId} ({Role}) signed in.", user.Id, user.Role);
        return new LoginResult(user);
    }
}
