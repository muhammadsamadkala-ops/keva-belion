using System.Text.RegularExpressions;

namespace KivaBelion.API.Orders;

/// <summary>
/// Checks an order request without touching the database, so it is easy to test.
/// Error keys look like "customer.phone" so the website can show each message beside the right field.
/// </summary>
public static class OrderRequestValidator
{
    public const int MaxLines = 20;
    public const int MaxQuantityPerLine = 99;

    // Pakistani numbers: 03001234567, +923001234567, 923001234567, 3001234567 or a landline such as 02134567890.
    private static readonly Regex PhoneCompact = new(@"^(?:\+?92|0)?[0-9]{9,10}$", RegexOptions.Compiled);
    private static readonly Regex PhoneAllowed = new(@"^\+?[0-9 ()\-]{7,30}$", RegexOptions.Compiled);
    private static readonly Regex Email = new(@"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$", RegexOptions.Compiled);
    private static readonly Regex Slug = new(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);
    private static readonly Regex IdempotencyKey = new(@"^[A-Za-z0-9_-]{16,64}$", RegexOptions.Compiled);

    public static bool IsValidIdempotencyKey(string? key) => key is not null && IdempotencyKey.IsMatch(key);

    /// <summary>Removes spaces, hyphens and brackets, keeping a leading +.</summary>
    public static string CompactPhone(string phone) =>
        new string(phone.Trim().Where((c, i) => char.IsDigit(c) || (c == '+' && i == 0)).ToArray());

    public static Dictionary<string, string[]> Validate(CreateOrderRequest? request)
    {
        var errors = new Dictionary<string, string[]>();
        void Add(string key, string message) => errors[key] = new[] { message };

        if (request is null)
        {
            Add("request", "The order is empty.");
            return errors;
        }

        // ----- customer -----
        var name = request.Customer?.FullName?.Trim() ?? "";
        if (name.Length < 2 || name.Length > 150 || !name.Any(char.IsLetter))
            Add("customer.fullName", "Please enter your full name.");

        var phone = request.Customer?.Phone?.Trim() ?? "";
        if (!PhoneAllowed.IsMatch(phone) || !PhoneCompact.IsMatch(CompactPhone(phone)))
            Add("customer.phone", "Please enter a valid phone number, for example 0300 1234567.");

        var email = request.Customer?.Email?.Trim() ?? "";
        if (email.Length == 0 || email.Length > 254 || !Email.IsMatch(email))
            Add("customer.email", "Please enter a valid email address.");

        // ----- delivery address -----
        var line1 = request.Shipping?.Line1?.Trim() ?? "";
        if (line1.Length < 10 || line1.Length > 200)
            Add("shipping.line1", "Please enter your complete delivery address (house, street, area).");

        var city = request.Shipping?.City?.Trim() ?? "";
        if (city.Length < 2 || city.Length > 100 || !city.Any(char.IsLetter))
            Add("shipping.city", "Please enter your city.");

        if ((request.Shipping?.Province?.Trim().Length ?? 0) > 100)
            Add("shipping.province", "Province is too long.");
        if ((request.Shipping?.PostalCode?.Trim().Length ?? 0) > 20)
            Add("shipping.postalCode", "Postal code is too long.");

        if ((request.Note?.Trim().Length ?? 0) > 500)
            Add("note", "The note can be at most 500 characters.");

        // ----- payment -----
        // The browser may say CashOnDelivery or AdvancePayment, but the server decides from the city (Karachi = cash, others = advance).
        if (!string.IsNullOrWhiteSpace(request.PaymentMethod)
            && !string.Equals(request.PaymentMethod.Trim(), "CashOnDelivery", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(request.PaymentMethod.Trim(), "AdvancePayment", StringComparison.OrdinalIgnoreCase))
            Add("paymentMethod", "Only Cash on Delivery (Karachi) and Advance Payment (other cities) are available.");

        if (request.ExpectedTotal.HasValue && request.ExpectedTotal.Value < 0)
            Add("expectedTotal", "Invalid total.");

        // ----- items -----
        var items = request.Items;
        if (items is null || items.Count == 0)
        {
            Add("items", "Your bag is empty.");
        }
        else if (items.Count > MaxLines)
        {
            Add("items", $"An order can contain at most {MaxLines} different products.");
        }
        else
        {
            var seen = new HashSet<string>();
            for (var i = 0; i < items.Count; i++)
            {
                var slug = items[i]?.ProductSlug?.Trim() ?? "";
                if (slug.Length == 0 || slug.Length > 160 || !Slug.IsMatch(slug))
                    Add($"items[{i}].productSlug", "Unknown product.");
                else if (!seen.Add(slug))
                    Add($"items[{i}].productSlug", "This product appears twice. Please combine the quantities.");

                var qty = items[i]?.Quantity ?? 0;
                if (qty < 1 || qty > MaxQuantityPerLine)
                    Add($"items[{i}].quantity", $"Quantity must be between 1 and {MaxQuantityPerLine}.");
            }
        }

        return errors;
    }
}
