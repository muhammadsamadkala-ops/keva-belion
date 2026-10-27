using KivaBelion.API.Data.Entities;
using KivaBelion.API.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;

namespace KivaBelion.API.Tests;

public class AdminAuthServiceTests
{
    private const string Email = "owner@example.com";
    private const string GoodPassword = "Correct-Horse-4-Battery";

    private static (AdminAuthService Service, InMemoryAdminUserStore Store, ManualTimeProvider Time, AdminUser User) Create(
        AdminRole role = AdminRole.Admin, bool active = true)
    {
        var store = new InMemoryAdminUserStore();
        var user = store.Seed(Email, GoodPassword, role, active);
        var time = new ManualTimeProvider(new DateTimeOffset(2026, 10, 27, 9, 0, 0, TimeSpan.Zero));
        var service = new AdminAuthService(store, new PasswordHasher<AdminUser>(), time, NullLogger<AdminAuthService>.Instance);
        return (service, store, time, user);
    }

    [Fact]
    public async Task Correct_password_signs_in_and_records_the_time()
    {
        var (service, _, time, user) = Create();
        var result = await service.LoginAsync("  OWNER@Example.com ", GoodPassword, default);   // email is case-insensitive
        Assert.True(result.Succeeded);
        Assert.Equal(time.GetUtcNow(), user.LastLoginAtUtc);
    }

    [Fact]
    public async Task Wrong_password_fails_and_counts()
    {
        var (service, _, _, user) = Create();
        var result = await service.LoginAsync(Email, "wrong-password-1", default);
        Assert.False(result.Succeeded);
        Assert.Equal(1, user.FailedLoginCount);
    }

    [Fact]
    public async Task Unknown_email_fails()
    {
        var (service, _, _, _) = Create();
        Assert.False((await service.LoginAsync("nobody@example.com", GoodPassword, default)).Succeeded);
    }

    [Fact]
    public async Task Disabled_account_cannot_sign_in()
    {
        var (service, _, _, _) = Create(active: false);
        Assert.False((await service.LoginAsync(Email, GoodPassword, default)).Succeeded);
    }

    [Fact]
    public async Task Account_locks_after_five_failures_even_for_the_correct_password()
    {
        var (service, _, _, user) = Create();
        for (var i = 0; i < AdminAuthService.MaxFailedAttempts; i++)
            await service.LoginAsync(Email, "wrong-password-1", default);

        Assert.NotNull(user.LockoutEndUtc);
        Assert.False((await service.LoginAsync(Email, GoodPassword, default)).Succeeded);
    }

    [Fact]
    public async Task Lock_ends_after_the_lockout_time()
    {
        var (service, _, time, _) = Create();
        for (var i = 0; i < AdminAuthService.MaxFailedAttempts; i++)
            await service.LoginAsync(Email, "wrong-password-1", default);

        time.Advance(AdminAuthService.LockoutDuration + TimeSpan.FromSeconds(1));
        Assert.True((await service.LoginAsync(Email, GoodPassword, default)).Succeeded);
    }

    [Fact]
    public async Task Successful_login_resets_the_failure_counter()
    {
        var (service, _, _, user) = Create();
        await service.LoginAsync(Email, "wrong-password-1", default);
        await service.LoginAsync(Email, GoodPassword, default);
        Assert.Equal(0, user.FailedLoginCount);
    }

    [Fact]
    public void Stored_hash_is_not_the_password()
    {
        var (_, _, _, user) = Create();
        Assert.DoesNotContain(GoodPassword, user.PasswordHash);
        Assert.True(user.PasswordHash.Length > 40);
    }

    [Fact]
    public void Token_contains_role_and_expires_after_the_configured_time()
    {
        var (_, _, time, user) = Create(AdminRole.Staff);
        var settings = Microsoft.Extensions.Options.Options.Create(new JwtSettings
        {
            Issuer = "KivaBelion.API", Audience = "KivaBelion.Admin", AccessTokenMinutes = 15, SigningKey = TestSettings.JwtKey
        });
        var token = new TokenService(settings, time).CreateAccessToken(user);
        Assert.Equal(time.GetUtcNow().AddMinutes(15), token.ExpiresAtUtc);

        var parsed = new Microsoft.IdentityModel.JsonWebTokens.JsonWebToken(token.Token);
        Assert.Equal("Staff", parsed.GetClaim("role").Value);
        Assert.DoesNotContain("PasswordHash", token.Token);
    }
}
