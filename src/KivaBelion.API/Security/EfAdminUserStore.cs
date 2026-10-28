using KivaBelion.API.Data;
using KivaBelion.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KivaBelion.API.Security;

public sealed class EfAdminUserStore(KivaBelionDbContext db) : IAdminUserStore
{
    public Task<AdminUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        db.AdminUsers.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

    public async Task AddAsync(AdminUser user, CancellationToken cancellationToken) =>
        await db.AdminUsers.AddAsync(user, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
