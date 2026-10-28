using KivaBelion.API.Data.Entities;

namespace KivaBelion.API.Security;

/// <summary>Where admin accounts are kept. Tests use an in-memory copy; the app uses the database.</summary>
public interface IAdminUserStore
{
    Task<AdminUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task AddAsync(AdminUser user, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
