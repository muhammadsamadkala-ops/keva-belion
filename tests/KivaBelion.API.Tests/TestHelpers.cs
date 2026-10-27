using KivaBelion.API.Data.Entities;
using KivaBelion.API.Security;
using Microsoft.AspNetCore.Identity;

namespace KivaBelion.API.Tests;

public sealed class InMemoryAdminUserStore : IAdminUserStore
{
    private readonly Dictionary<string, AdminUser> _users = new();
    private int _nextId = 1;

    public Task<AdminUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        Task.FromResult(_users.GetValueOrDefault(normalizedEmail));

    public Task AddAsync(AdminUser user, CancellationToken cancellationToken)
    {
        if (user.Id == 0) user.Id = _nextId++;
        _users[user.Email] = user;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public AdminUser Seed(string email, string password, AdminRole role = AdminRole.Admin, bool active = true)
    {
        var user = new AdminUser { Email = email.ToLowerInvariant(), DisplayName = "Test Person", Role = role, IsActive = active };
        user.PasswordHash = new PasswordHasher<AdminUser>().HashPassword(user, password);
        AddAsync(user, CancellationToken.None).GetAwaiter().GetResult();
        return user;
    }
}

public sealed class ManualTimeProvider(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;
    public override DateTimeOffset GetUtcNow() => _now;
    public void Advance(TimeSpan by) => _now += by;
}
