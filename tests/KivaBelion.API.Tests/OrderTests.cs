using System.Net;
using System.Net.Http.Json;
using KivaBelion.API.Orders;

namespace KivaBelion.API.Tests;

public class OrderRequestValidatorTests
{
    private static CreateOrderRequest Valid() => new()
    {
        Customer = new CustomerInput { FullName = "Ayesha Khan", Phone = "0300 1234567", Email = "ayesha@example.com" },
        Shipping = new ShippingInput { Line1 = "House 12, Street 4, Gulshan-e-Iqbal", City = "Karachi" },
        Items = new List<OrderLineInput> { new() { ProductSlug = "noir", Quantity = 2 } },
        PaymentMethod = "CashOnDelivery",
    };

    [Fact] public void Valid_request_has_no_errors() => Assert.Empty(OrderRequestValidator.Validate(Valid()));

    [Fact] public void Null_request_is_rejected() => Assert.Contains("request", OrderRequestValidator.Validate(null).Keys);

    [Theory]
    [InlineData("03001234567")]
    [InlineData("0300 1234567")]
    [InlineData("0300-1234567")]
    [InlineData("+923001234567")]
    [InlineData("+92 300 1234567")]
    [InlineData("923001234567")]
    [InlineData("3001234567")]
    [InlineData("02134567890")]
    public void Accepts_pakistani_phone_formats(string phone)
    {
        var r = Valid(); r.Customer!.Phone = phone;
        Assert.DoesNotContain("customer.phone", OrderRequestValidator.Validate(r).Keys);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("abcdefghijk")]
    [InlineData("0300123")]
    [InlineData("+1 415 555 2671 999")]
    [InlineData("0300 12345 67; DROP TABLE")]
    public void Rejects_bad_phone(string phone)
    {
        var r = Valid(); r.Customer!.Phone = phone;
        Assert.Contains("customer.phone", OrderRequestValidator.Validate(r).Keys);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("a@b")]
    [InlineData("a b@example.com")]
    public void Rejects_bad_email(string email)
    {
        var r = Valid(); r.Customer!.Email = email;
        Assert.Contains("customer.email", OrderRequestValidator.Validate(r).Keys);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("12345")]
    public void Rejects_bad_name(string name)
    {
        var r = Valid(); r.Customer!.FullName = name;
        Assert.Contains("customer.fullName", OrderRequestValidator.Validate(r).Keys);
    }

    [Fact]
    public void Rejects_short_address_and_missing_city()
    {
        var r = Valid(); r.Shipping!.Line1 = "Home"; r.Shipping.City = "";
        var errors = OrderRequestValidator.Validate(r);
        Assert.Contains("shipping.line1", errors.Keys);
        Assert.Contains("shipping.city", errors.Keys);
    }

    [Fact]
    public void Rejects_missing_customer_and_shipping_blocks()
    {
        var r = Valid(); r.Customer = null; r.Shipping = null;
        var errors = OrderRequestValidator.Validate(r);
        Assert.Contains("customer.fullName", errors.Keys);
        Assert.Contains("customer.phone", errors.Keys);
        Assert.Contains("customer.email", errors.Keys);
        Assert.Contains("shipping.line1", errors.Keys);
        Assert.Contains("shipping.city", errors.Keys);
    }

    [Fact]
    public void Rejects_empty_and_oversized_item_lists()
    {
        var r = Valid(); r.Items = new();
        Assert.Contains("items", OrderRequestValidator.Validate(r).Keys);
        r.Items = null;
        Assert.Contains("items", OrderRequestValidator.Validate(r).Keys);
        r.Items = Enumerable.Range(0, OrderRequestValidator.MaxLines + 1)
            .Select(i => new OrderLineInput { ProductSlug = "p" + i, Quantity = 1 }).ToList();
        Assert.Contains("items", OrderRequestValidator.Validate(r).Keys);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    public void Rejects_quantity_out_of_range(int qty)
    {
        var r = Valid(); r.Items![0].Quantity = qty;
        Assert.Contains("items[0].quantity", OrderRequestValidator.Validate(r).Keys);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Noir")]
    [InlineData("noir; drop")]
    [InlineData("../etc")]
    public void Rejects_bad_product_slug(string slug)
    {
        var r = Valid(); r.Items![0].ProductSlug = slug;
        Assert.Contains("items[0].productSlug", OrderRequestValidator.Validate(r).Keys);
    }

    [Fact]
    public void Rejects_duplicate_product_lines()
    {
        var r = Valid(); r.Items!.Add(new OrderLineInput { ProductSlug = "noir", Quantity = 1 });
        Assert.Contains("items[1].productSlug", OrderRequestValidator.Validate(r).Keys);
    }

    [Theory]
    [InlineData("Card")]
    [InlineData("Safepay")]
    [InlineData("BankTransfer")]
    public void Rejects_any_payment_method_except_cash_on_delivery(string method)
    {
        var r = Valid(); r.PaymentMethod = method;
        Assert.Contains("paymentMethod", OrderRequestValidator.Validate(r).Keys);
    }

    [Fact]
    public void Missing_payment_method_means_cash_on_delivery()
    {
        var r = Valid(); r.PaymentMethod = null;
        Assert.Empty(OrderRequestValidator.Validate(r));
    }

    [Fact]
    public void Rejects_overlong_note()
    {
        var r = Valid(); r.Note = new string('x', 501);
        Assert.Contains("note", OrderRequestValidator.Validate(r).Keys);
    }

    [Theory]
    [InlineData("0123456789abcdef", true)]
    [InlineData("a1b2c3d4-e5f6-7890-abcd-ef1234567890", true)]
    [InlineData("short", false)]
    [InlineData("has spaces in it 1234", false)]
    [InlineData(null, false)]
    public void Idempotency_key_format(string? key, bool expected) =>
        Assert.Equal(expected, OrderRequestValidator.IsValidIdempotencyKey(key));
}

public class OrderPricingTests
{
    [Fact]
    public void Adds_lines_and_shipping()
    {
        var t = OrderPricing.Calculate(new[] { (8900m, 2), (7200m, 1) }, 250m);
        Assert.Equal(25000m, t.Subtotal);
        Assert.Equal(250m, t.ShippingFee);
        Assert.Equal(25250m, t.Total);
    }

    [Theory]
    [InlineData("Karachi", 0)]
    [InlineData("karachi", 0)]
    [InlineData("  KARACHI  ", 0)]
    [InlineData("Karachi City", 0)]
    [InlineData("Lahore", 500)]
    [InlineData("Islamabad", 500)]
    [InlineData("", 500)]
    [InlineData(null, 500)]
    public void Delivery_is_free_only_in_karachi(string? city, int expectedFee) =>
        Assert.Equal((decimal)expectedFee, OrderPricing.ShippingFeeFor(city, "Karachi", 500m));

    [Fact]
    public void Other_city_total_includes_fee()
    {
        var fee = OrderPricing.ShippingFeeFor("Lahore", "Karachi", 500m);
        var t = OrderPricing.Calculate(new[] { (7500m, 2) }, fee);
        Assert.Equal(15500m, t.Total);
    }

    [Fact]
    public void Zero_shipping_by_default_policy()
    {
        var t = OrderPricing.Calculate(new[] { (7500m, 3) }, 0m);
        Assert.Equal(22500m, t.Total);
    }

    [Fact]
    public void Rejects_bad_inputs()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderPricing.Calculate(new[] { (100m, 0) }, 0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderPricing.Calculate(new[] { (-1m, 1) }, 0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderPricing.Calculate(new[] { (100m, 1) }, -5m));
    }
}

public class OrderNumberTests
{
    [Fact]
    public void Has_expected_shape_and_is_not_repeated()
    {
        var numbers = Enumerable.Range(0, 500).Select(_ => OrderNumberGenerator.Next()).ToList();
        Assert.All(numbers, n => Assert.Matches("^KB-[A-HJKMNP-Z2-9]{8}$", n));
        Assert.Equal(numbers.Count, numbers.Distinct().Count());
    }
}

public class OrdersApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;
    public OrdersApiTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Orders_endpoint_answers_503_while_orders_are_switched_off()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/orders", new { items = new[] { new { productSlug = "noir", quantity = 1 } } });
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Admin_orders_need_sign_in()
    {
        var response = await _client.GetAsync("/api/v1/admin/orders");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_status_change_needs_sign_in()
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/admin/orders/{Guid.NewGuid()}/status")
        {
            Content = JsonContent.Create(new { status = "Confirmed" })
        };
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
