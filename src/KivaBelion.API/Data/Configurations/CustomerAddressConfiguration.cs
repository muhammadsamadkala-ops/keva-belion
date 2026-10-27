using KivaBelion.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KivaBelion.API.Data.Configurations;

public sealed class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("CustomerAddresses");
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => a.CustomerId);

        // At most one default address per customer.
        builder.HasIndex(a => a.CustomerId)
            .IsUnique()
            .HasDatabaseName("UX_CustomerAddresses_OneDefaultPerCustomer")
            .HasFilter("\"IsDefault\" = TRUE");

        // Deleting a customer removes their saved addresses (past orders keep their own copy).
        builder.HasOne(a => a.Customer)
            .WithMany(c => c.Addresses)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
