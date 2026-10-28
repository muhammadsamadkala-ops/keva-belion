using KivaBelion.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KivaBelion.API.Data.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems", t =>
        {
            t.HasCheckConstraint("CK_OrderItems_Quantity", "\"Quantity\" > 0");
            t.HasCheckConstraint("CK_OrderItems_Money", "\"UnitPrice\" >= 0 AND \"LineTotal\" = \"UnitPrice\" * \"Quantity\"");
        });

        builder.HasKey(i => i.Id);
        builder.Property(i => i.UnitPrice).HasPrecision(12, 2);
        builder.Property(i => i.LineTotal).HasPrecision(12, 2);

        // The same product appears once per order (change the quantity instead).
        builder.HasIndex(i => new { i.OrderId, i.ProductId }).IsUnique();
        builder.HasIndex(i => i.ProductId);

        builder.HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // A product that was ever ordered cannot be deleted (deactivate it instead).
        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
