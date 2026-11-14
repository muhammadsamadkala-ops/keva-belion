namespace KivaBelion.API.Orders;

/// <summary>The only place where order money is calculated.</summary>
public static class OrderPricing
{
    public readonly record struct Totals(decimal Subtotal, decimal ShippingFee, decimal Total);

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
