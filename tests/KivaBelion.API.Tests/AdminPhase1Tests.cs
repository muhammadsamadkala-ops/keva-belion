using System.Net;
using System.Net.Http.Json;
using KivaBelion.API.Catalog;
using KivaBelion.API.Data.Entities;
using KivaBelion.API.Orders;

namespace KivaBelion.API.Tests;

public class OrderStatusRulesTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Processing)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Processing, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Processing, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered)]
    public void Allowed_moves(OrderStatus from, OrderStatus to) =>
        Assert.True(OrderStatusRules.CanMove(from, to));

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Pending, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Pending)]
    [InlineData(OrderStatus.Pending, OrderStatus.Pending)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Pending)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Confirmed)]
    public void Refused_moves(OrderStatus from, OrderStatus to) =>
        Assert.False(OrderStatusRules.CanMove(from, to));
}

public class OrderStockTests
{
    private static ProductInventory Stock(int onHand, int reserved) =>
        new() { QuantityOnHand = onHand, ReservedQuantity = reserved };

    [Fact]
    public void Reserve_holds_bottles_but_keeps_them_on_the_shelf_count()
    {
        var i = Stock(10, 2);
        OrderStock.Reserve(i, 3);
        Assert.Equal(10, i.QuantityOnHand);
        Assert.Equal(5, i.ReservedQuantity);
        Assert.Equal(5, i.AvailableQuantity);
    }

    [Fact]
    public void Release_puts_bottles_back_and_never_goes_below_zero()
    {
        var i = Stock(10, 2);
        OrderStock.Release(i, 3);
        Assert.Equal(0, i.ReservedQuantity);
        Assert.Equal(10, i.QuantityOnHand);
    }

    [Fact]
    public void Deliver_takes_bottles_out_for_good()
    {
        var i = Stock(10, 4);
        OrderStock.Deliver(i, 3);
        Assert.Equal(7, i.QuantityOnHand);
        Assert.Equal(1, i.ReservedQuantity);
    }

    [Fact]
    public void Deliver_for_an_order_placed_before_reservations_existed_stays_valid()
    {
        var i = Stock(2, 0);
        OrderStock.Deliver(i, 3);
        Assert.Equal(0, i.QuantityOnHand);
        Assert.Equal(0, i.ReservedQuantity);
    }
}

public class ProductWriteValidatorTests
{
    private static ProductWriteRequest Valid() => new()
    {
        CategoryId = 1, Sku = "KB-TEST-50", Name = "Test Scent", Slug = "test-scent",
        SizeMl = 50, Price = 7500m, Gender = "unisex", Family = "woody", Intensity = "bold",
        ImageUrl = "assets/products/test.jpg", SortOrder = 3,
    };

    [Fact]
    public void A_complete_product_is_valid() =>
        Assert.Empty(ProductWriteValidator.Validate(Valid()));

    [Fact]
    public void Null_request_is_refused() =>
        Assert.Contains("request", ProductWriteValidator.Validate(null).Keys);

    [Fact]
    public void Missing_basics_are_reported()
    {
        var errors = ProductWriteValidator.Validate(new ProductWriteRequest());
        Assert.Contains("categoryId", errors.Keys);
        Assert.Contains("sku", errors.Keys);
        Assert.Contains("name", errors.Keys);
        Assert.Contains("slug", errors.Keys);
        Assert.Contains("sizeMl", errors.Keys);
    }

    [Theory]
    [InlineData("Not A Slug")]
    [InlineData("UPPER")]
    [InlineData("double--hyphen")]
    [InlineData("-leading")]
    public void Bad_slugs_are_refused(string slug)
    {
        var r = Valid(); r.Slug = slug;
        Assert.Contains("slug", ProductWriteValidator.Validate(r).Keys);
    }

    [Fact]
    public void Negative_or_huge_price_is_refused()
    {
        var r = Valid(); r.Price = -1m;
        Assert.Contains("price", ProductWriteValidator.Validate(r).Keys);
        r.Price = 10_000_000m;
        Assert.Contains("price", ProductWriteValidator.Validate(r).Keys);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:image/png;base64,AAAA")]
    [InlineData("http://insecure.example/x.jpg")]
    [InlineData("../secret.jpg")]
    [InlineData("assets/a b.jpg")]
    public void Unsafe_image_addresses_are_refused(string image)
    {
        var r = Valid(); r.ImageUrl = image;
        Assert.Contains("imageUrl", ProductWriteValidator.Validate(r).Keys);
    }

    [Theory]
    [InlineData("assets/products/noir.jpg")]
    [InlineData("https://cdn.example.com/noir.jpg")]
    [InlineData("")]
    [InlineData(null)]
    public void Safe_or_empty_image_addresses_are_accepted(string? image)
    {
        var r = Valid(); r.ImageUrl = image;
        Assert.DoesNotContain("imageUrl", ProductWriteValidator.Validate(r).Keys);
    }

    [Fact]
    public void Gender_must_be_one_of_the_three_values()
    {
        var r = Valid(); r.Gender = "kids";
        Assert.Contains("gender", ProductWriteValidator.Validate(r).Keys);
    }

    [Fact]
    public void Clean_trims_and_turns_blank_into_null()
    {
        Assert.Null(ProductWriteValidator.Clean("   "));
        Assert.Null(ProductWriteValidator.Clean(null));
        Assert.Equal("woody", ProductWriteValidator.Clean("  woody "));
    }
}

public class AdminPhase1AccessTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;
    public AdminPhase1AccessTests(ApiFactory factory) => _client = factory.CreateClient();

    [Theory]
    [InlineData("/api/v1/admin/products")]
    [InlineData("/api/v1/admin/products/1")]
    [InlineData("/api/v1/admin/customers")]
    [InlineData("/api/v1/admin/customers/1")]
    [InlineData("/api/v1/admin/dashboard")]
    public async Task Admin_reads_need_a_token(string url) =>
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync(url)).StatusCode);

    [Fact]
    public async Task Admin_writes_need_a_token()
    {
        var body = new { };
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.PostAsJsonAsync("/api/v1/admin/products", body)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.PutAsJsonAsync("/api/v1/admin/products/1", body)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.PutAsJsonAsync("/api/v1/admin/products/1/stock", body)).StatusCode);
        var patch = new HttpRequestMessage(HttpMethod.Patch, "/api/v1/admin/products/1/visibility")
        {
            Content = JsonContent.Create(body),
        };
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.SendAsync(patch)).StatusCode);
    }
}
