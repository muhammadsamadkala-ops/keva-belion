using KivaBelion.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KivaBelion.API.Data.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", t =>
        {
            t.HasCheckConstraint("CK_Orders_Money",
                "\"Subtotal\" >= 0 AND \"ShippingFee\" >= 0 AND \"DiscountTotal\" >= 0 " +
                "AND \"Total\" >= 0 AND \"Total\" = \"Subtotal\" + \"ShippingFee\" - \"DiscountTotal\"");
        });

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Subtotal).HasPrecision(12, 2);
        builder.Property(o => o.ShippingFee).HasPrecision(12, 2);
        builder.Property(o => o.DiscountTotal).HasPrecision(12, 2);
        builder.Property(o => o.Total).HasPrecision(12, 2);

        // Enums are saved as readable text.
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.PublicId).IsUnique();
        // PostgreSQL allows many NULLs in a unique index, so only real keys must be unique.
        builder.HasIndex(o => o.IdempotencyKey).IsUnique();
        builder.HasIndex(o => new { o.CustomerId, o.CreatedAtUtc });
        builder.HasIndex(o => new { o.Status, o.CreatedAtUtc });

        // A customer who has orders cannot be deleted.
        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
