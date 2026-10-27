using KivaBelion.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KivaBelion.API.Data.Configurations;

public sealed class ProductInventoryConfiguration : IEntityTypeConfiguration<ProductInventory>
{
    public void Configure(EntityTypeBuilder<ProductInventory> builder)
    {
        builder.ToTable("ProductInventories", t =>
        {
            t.HasCheckConstraint("CK_ProductInventories_OnHand", "\"QuantityOnHand\" >= 0");
            t.HasCheckConstraint("CK_ProductInventories_Reserved", "\"ReservedQuantity\" >= 0 AND \"ReservedQuantity\" <= \"QuantityOnHand\"");
            t.HasCheckConstraint("CK_ProductInventories_LowStock", "\"LowStockThreshold\" >= 0");
        });

        builder.HasKey(i => i.Id);

        // Exactly one inventory row per product.
        builder.HasIndex(i => i.ProductId).IsUnique();
        builder.HasOne(i => i.Product)
            .WithOne(p => p.Inventory)
            .HasForeignKey<ProductInventory>(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
