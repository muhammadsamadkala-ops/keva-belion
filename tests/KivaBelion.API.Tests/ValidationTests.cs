using System.ComponentModel.DataAnnotations;
using KivaBelion.API.Data.Entities;

namespace KivaBelion.API.Tests;

public class ValidationTests
{
    private static List<ValidationResult> Validate(object instance)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);
        return results;
    }

    private static bool Fails(object instance, string property) =>
        Validate(instance).Any(r => r.MemberNames.Contains(property));

    private static Product ValidProduct() => new() { Sku = "KB-001-50", Name = "Test Parfum", Slug = "test-parfum", SizeMl = 50, Price = 4500m };

    [Fact]
    public void Valid_product_passes() => Assert.Empty(Validate(ValidProduct()));

    [Fact]
    public void Product_rejects_negative_price()
    {
        var p = ValidProduct(); p.Price = -1m;
        Assert.True(Fails(p, nameof(Product.Price)));
    }

    [Fact]
    public void Product_rejects_zero_size_and_bad_sku_and_bad_slug()
    {
        var p = ValidProduct(); p.SizeMl = 0; p.Sku = "bad sku!"; p.Slug = "Bad Slug";
        Assert.True(Fails(p, nameof(Product.SizeMl)));
        Assert.True(Fails(p, nameof(Product.Sku)));
        Assert.True(Fails(p, nameof(Product.Slug)));
    }

    [Fact]
    public void Product_rejects_empty_name()
    {
        var p = ValidProduct(); p.Name = "";
        Assert.True(Fails(p, nameof(Product.Name)));
    }

    [Fact]
    public void Inventory_rejects_negative_quantities()
    {
        var i = new ProductInventory { QuantityOnHand = -1, ReservedQuantity = -2 };
        Assert.True(Fails(i, nameof(ProductInventory.QuantityOnHand)));
        Assert.True(Fails(i, nameof(ProductInventory.ReservedQuantity)));
    }

    [Fact]
    public void Customer_validates_email_and_phone()
    {
        var good = new Customer { Email = "a@example.com", FullName = "Ali Khan", Phone = "+92 300 1234567" };
        Assert.Empty(Validate(good));

        var bad = new Customer { Email = "not-an-email", FullName = "A", Phone = "abc" };
        Assert.True(Fails(bad, nameof(Customer.Email)));
        Assert.True(Fails(bad, nameof(Customer.FullName)));
        Assert.True(Fails(bad, nameof(Customer.Phone)));
    }

    [Fact]
    public void Customer_email_is_normalized_to_lower_case()
        => Assert.Equal("ali@example.com", Customer.NormalizeEmail("  Ali@Example.COM "));

    [Fact]
    public void Address_requires_core_fields_and_two_letter_country()
    {
        var a = new CustomerAddress { CountryCode = "PAK" };
        Assert.True(Fails(a, nameof(CustomerAddress.RecipientName)));
        Assert.True(Fails(a, nameof(CustomerAddress.Line1)));
        Assert.True(Fails(a, nameof(CustomerAddress.City)));
        Assert.True(Fails(a, nameof(CustomerAddress.CountryCode)));
    }

    [Fact]
    public void Order_item_rejects_zero_quantity()
    {
        var item = new OrderItem { ProductSku = "KB-1", ProductName = "X", UnitPrice = 10m, Quantity = 0, LineTotal = 0m };
        Assert.True(Fails(item, nameof(OrderItem.Quantity)));
    }

    [Fact]
    public void Order_requires_number_and_shipping_address()
    {
        var o = new Order();
        Assert.True(Fails(o, nameof(Order.OrderNumber)));
        Assert.True(Fails(o, nameof(Order.ShipRecipientName)));
        Assert.True(Fails(o, nameof(Order.ShipLine1)));
        Assert.True(Fails(o, nameof(Order.ShipCity)));
    }
}
