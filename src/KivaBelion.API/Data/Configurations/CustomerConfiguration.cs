using KivaBelion.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KivaBelion.API.Data.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", t =>
            // Emails are stored in lower case, so the unique index below cannot be bypassed by capital letters.
            t.HasCheckConstraint("CK_Customers_EmailLowercase", "\"Email\" = lower(\"Email\")"));

        builder.HasKey(c => c.Id);
        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => c.Phone);
    }
}
