using KivaBelion.API.Data;
using KivaBelion.API.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace KivaBelion.API.Tests;

/// <summary>
/// Checks the database design (tables, keys, indexes, money precision, delete rules) from the EF model.
/// No database connection is opened: the address below is fake and never contacted.
/// </summary>
public class DataModelTests
{
    private static KivaBelionDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<KivaBelionDbContext>()
            .UseNpgsql("Host=localhost;Port=1;Database=none;Username=none;Password=none;Timeout=1")
            .Options;
        return new KivaBelionDbContext(options);
    }

    private static IEntityType Entity<T>(KivaBelionDbContext ctx) =>
        ctx.Model.FindEntityType(typeof(T)) ?? throw new Xunit.Sdk.XunitException($"{typeof(T).Name} is not in the model.");

    private static bool HasUniqueIndex(IEntityType entity, params string[] properties) =>
        entity.GetIndexes().Any(i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(properties));

    private static IForeignKey ForeignKey(IEntityType entity, string property) =>
        entity.GetForeignKeys().Single(fk => fk.Properties.Single().Name == property);

    [Fact]
    public void All_seven_entities_are_in_the_model()
    {
        using var ctx = CreateContext();
        foreach (var type in new[] { typeof(Category), typeof(Product), typeof(ProductInventory), typeof(Customer),
                                     typeof(CustomerAddress), typeof(Order), typeof(OrderItem) })
        {
            var entity = ctx.Model.FindEntityType(type);
            Assert.NotNull(entity);
            Assert.NotNull(entity!.FindPrimaryKey());
        }
    }

    [Theory]
    [InlineData(typeof(Product), nameof(Product.Price))]
    [InlineData(typeof(Order), nameof(Order.Subtotal))]
    [InlineData(typeof(Order), nameof(Order.ShippingFee))]
    [InlineData(typeof(Order), nameof(Order.DiscountTotal))]
    [InlineData(typeof(Order), nameof(Order.Total))]
    [InlineData(typeof(OrderItem), nameof(OrderItem.UnitPrice))]
    [InlineData(typeof(OrderItem), nameof(OrderItem.LineTotal))]
    public void Money_columns_use_decimal_12_2(Type entityType, string property)
    {
        using var ctx = CreateContext();
        var p = ctx.Model.FindEntityType(entityType)!.FindProperty(property)!;
        Assert.Equal(12, p.GetPrecision());
        Assert.Equal(2, p.GetScale());
    }

    [Fact]
    public void Unique_indexes_exist_where_duplicates_must_be_impossible()
    {
        using var ctx = CreateContext();
        Assert.True(HasUniqueIndex(Entity<Category>(ctx), nameof(Category.Slug)));
        Assert.True(HasUniqueIndex(Entity<Product>(ctx), nameof(Product.Sku)));
        Assert.True(HasUniqueIndex(Entity<Product>(ctx), nameof(Product.Slug)));
        Assert.True(HasUniqueIndex(Entity<ProductInventory>(ctx), nameof(ProductInventory.ProductId)));
        Assert.True(HasUniqueIndex(Entity<Customer>(ctx), nameof(Customer.Email)));
        Assert.True(HasUniqueIndex(Entity<Order>(ctx), nameof(Order.OrderNumber)));
        Assert.True(HasUniqueIndex(Entity<Order>(ctx), nameof(Order.PublicId)));
        Assert.True(HasUniqueIndex(Entity<Order>(ctx), nameof(Order.IdempotencyKey)));
        Assert.True(HasUniqueIndex(Entity<OrderItem>(ctx), nameof(OrderItem.OrderId), nameof(OrderItem.ProductId)));
    }

    [Fact]
    public void Foreign_keys_have_the_intended_delete_rules()
    {
        using var ctx = CreateContext();

        // Things that have history can never be deleted by accident.
        Assert.Equal(DeleteBehavior.Restrict, ForeignKey(Entity<Product>(ctx), nameof(Product.CategoryId)).DeleteBehavior);
        Assert.Equal(DeleteBehavior.Restrict, ForeignKey(Entity<Order>(ctx), nameof(Order.CustomerId)).DeleteBehavior);
        Assert.Equal(DeleteBehavior.Restrict, ForeignKey(Entity<OrderItem>(ctx), nameof(OrderItem.ProductId)).DeleteBehavior);

        // Owned child rows follow their parent.
        Assert.Equal(DeleteBehavior.Cascade, ForeignKey(Entity<OrderItem>(ctx), nameof(OrderItem.OrderId)).DeleteBehavior);
        Assert.Equal(DeleteBehavior.Cascade, ForeignKey(Entity<CustomerAddress>(ctx), nameof(CustomerAddress.CustomerId)).DeleteBehavior);
        Assert.Equal(DeleteBehavior.Cascade, ForeignKey(Entity<ProductInventory>(ctx), nameof(ProductInventory.ProductId)).DeleteBehavior);
    }

    [Fact]
    public void Foreign_keys_point_to_the_right_parents()
    {
        using var ctx = CreateContext();
        Assert.Equal(typeof(Category), ForeignKey(Entity<Product>(ctx), nameof(Product.CategoryId)).PrincipalEntityType.ClrType);
        Assert.Equal(typeof(Customer), ForeignKey(Entity<Order>(ctx), nameof(Order.CustomerId)).PrincipalEntityType.ClrType);
        Assert.Equal(typeof(Order), ForeignKey(Entity<OrderItem>(ctx), nameof(OrderItem.OrderId)).PrincipalEntityType.ClrType);
        Assert.Equal(typeof(Product), ForeignKey(Entity<OrderItem>(ctx), nameof(OrderItem.ProductId)).PrincipalEntityType.ClrType);
        Assert.Equal(typeof(Customer), ForeignKey(Entity<CustomerAddress>(ctx), nameof(CustomerAddress.CustomerId)).PrincipalEntityType.ClrType);
        Assert.Equal(typeof(Product), ForeignKey(Entity<ProductInventory>(ctx), nameof(ProductInventory.ProductId)).PrincipalEntityType.ClrType);
    }

    [Fact]
    public void Required_fields_and_lengths_are_configured()
    {
        using var ctx = CreateContext();
        var product = Entity<Product>(ctx);
        Assert.False(product.FindProperty(nameof(Product.Sku))!.IsNullable);
        Assert.Equal(50, product.FindProperty(nameof(Product.Sku))!.GetMaxLength());
        Assert.True(product.FindProperty(nameof(Product.Description))!.IsNullable);

        var order = Entity<Order>(ctx);
        Assert.False(order.FindProperty(nameof(Order.OrderNumber))!.IsNullable);
        Assert.True(order.FindProperty(nameof(Order.IdempotencyKey))!.IsNullable);
        Assert.False(order.FindProperty(nameof(Order.ShipLine1))!.IsNullable);
    }

    [Fact]
    public void Order_enums_are_stored_as_text()
    {
        using var ctx = CreateContext();
        var order = Entity<Order>(ctx);
        // The finished (runtime) model keeps the converter on the property's type mapping, not as a metadata annotation.
        Assert.Equal(typeof(string), order.FindProperty(nameof(Order.Status))!.GetTypeMapping().Converter?.ProviderClrType);
        Assert.Equal(typeof(string), order.FindProperty(nameof(Order.PaymentMethod))!.GetTypeMapping().Converter?.ProviderClrType);
    }

    [Fact]
    public void Calculated_available_quantity_is_not_a_column()
    {
        using var ctx = CreateContext();
        Assert.Null(Entity<ProductInventory>(ctx).FindProperty(nameof(ProductInventory.AvailableQuantity)));
    }

    [Fact]
    public void No_card_or_payment_credential_columns_exist()
    {
        using var ctx = CreateContext();
        var forbidden = new[] { "card", "cvv", "cvc", "pan", "secret", "apikey", "token", "password" };
        var names = ctx.Model.GetEntityTypes().SelectMany(e => e.GetProperties()).Select(p => p.Name.ToLowerInvariant())
            .Where(n => n != "passwordhash");   // AdminUser.PasswordHash holds a one-way hash, never a password
        Assert.DoesNotContain(names, n => forbidden.Any(f => n.Contains(f)));
    }

    [Fact]
    public void New_orders_default_to_pending_cash_on_delivery_test_orders()
    {
        var order = new Order();
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(PaymentMethod.CashOnDelivery, order.PaymentMethod);
        Assert.True(order.IsTestOrder);
        Assert.Equal("PKR", order.CurrencyCode);
        Assert.NotEqual(Guid.Empty, order.PublicId);
    }

    [Fact]
    public void Relationship_navigation_works_in_memory()
    {
        var category = new Category { Name = "Eau de Parfum", Slug = "eau-de-parfum" };
        var product = new Product { Category = category, Sku = "KB-001", Name = "Test", Slug = "test", SizeMl = 50, Price = 1000m };
        category.Products.Add(product);
        product.Inventory = new ProductInventory { Product = product, QuantityOnHand = 10, ReservedQuantity = 3 };

        Assert.Same(category, product.Category);
        Assert.Equal(7, product.Inventory.AvailableQuantity);
    }

    [Fact]
    public void Timestamps_are_set_in_utc_for_new_rows_and_updated_for_changed_rows()
    {
        using var ctx = CreateContext();
        var created = new DateTimeOffset(2026, 10, 25, 10, 0, 0, TimeSpan.Zero);
        var later = created.AddHours(5);

        var category = new Category { Id = 7, Name = "Test", Slug = "test" };
        ctx.Categories.Add(category);
        ctx.StampTimestamps(created);
        Assert.Equal(created, category.CreatedAtUtc);
        Assert.Equal(created, category.UpdatedAtUtc);
        Assert.Equal(TimeSpan.Zero, category.CreatedAtUtc.Offset);

        // Pretend it was saved, then changed.
        ctx.Entry(category).State = EntityState.Unchanged;
        category.Name = "Changed";
        ctx.ChangeTracker.DetectChanges();
        ctx.StampTimestamps(later);
        Assert.Equal(created, category.CreatedAtUtc);
        Assert.Equal(later, category.UpdatedAtUtc);
    }
}
