using KivaBelion.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KivaBelion.API.Data.Configurations;

public sealed class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("AdminUsers", t =>
        {
            t.HasCheckConstraint("CK_AdminUsers_EmailLowercase", "\"Email\" = lower(\"Email\")");
            t.HasCheckConstraint("CK_AdminUsers_FailedLoginCount", "\"FailedLoginCount\" >= 0");
        });

        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
    }
}
