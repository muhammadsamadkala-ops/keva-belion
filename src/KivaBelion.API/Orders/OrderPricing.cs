namespace KivaBelion.API.Orders;

/// <summary>The only place where order money is calculated.</summary>
public static class OrderPricing
{
    public readonly record struct Totals(decimal Subtotal, decimal ShippingFee, decimal Total);

    /// <summary>
    /// Delivery fee rule: free inside the free-delivery city (Karachi), a fixed fee everywhere else.
    /// Case, extra spaces and a trailing "city" are ignored, so "karachi", " KARACHI " and "Karachi City" all count as Karachi.
    /// </summary>
    public static decimal ShippingFeeFor(string? city, string freeCity, decimal feeElsewhere)
    {
        return Normalize(city) == Normalize(freeCity) ? 0m : feeElsewhere;
    }

    private static string Normalize(string? value)
    {
        var v = string.Join(' ', (value ?? "").Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (v.EndsWith(" city")) v = v[..^5].TrimEnd();
        return v;
    }

    public static Totals Calculate(IEnumerable<(decimal UnitPrice, int Quantity)> lines, decimal shippingFee)
    {
        if (shippingFee < 0) throw new ArgumentOutOfRangeException(nameof(shippingFee));
        decimal subtotal = 0m;
        foreach (var (unitPrice, quantity) in lines)
        {
            if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(lines), "Negative price.");
            if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(lines), "Quantity must be at least 1.");
            subtotal += decimal.Round(unitPrice, 2) * quantity;
        }
        subtotal = decimal.Round(subtotal, 2);
        var fee = decimal.Round(shippingFee, 2);
        return new Totals(subtotal, fee, subtotal + fee);
    }
}
