using KivaBelion.API.Data.Entities;

namespace KivaBelion.API.Orders;

/// <summary>Which order status changes are allowed, and what each change does to stock.</summary>
public static class OrderStatusRules
{
    /// <summary>
    /// Pending to Confirmed or Cancelled; Confirmed to Processing or Cancelled; Processing to Shipped or Cancelled;
    /// Shipped to Delivered. Delivered and Cancelled are final.
    /// </summary>
    public static bool CanMove(OrderStatus from, OrderStatus to) => from switch
    {
        OrderStatus.Pending => to is OrderStatus.Confirmed or OrderStatus.Cancelled,
        OrderStatus.Confirmed => to is OrderStatus.Processing or OrderStatus.Cancelled,
        OrderStatus.Processing => to is OrderStatus.Shipped or OrderStatus.Cancelled,
        OrderStatus.Shipped => to is OrderStatus.Delivered,
        _ => false,
    };
}

/// <summary>
/// Stock rule: bottles are reserved when an order is placed, put back when it is cancelled,
/// and taken out of stock for good when it is delivered.
/// </summary>
public static class OrderStock
{
    public static void Reserve(ProductInventory inventory, int quantity) =>
        inventory.ReservedQuantity += quantity;

    public static void Release(ProductInventory inventory, int quantity) =>
        inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - quantity);

    public static void Deliver(ProductInventory inventory, int quantity)
    {
        inventory.ReservedQuantity = Math.Max(0, inventory.ReservedQuantity - quantity);
        inventory.QuantityOnHand = Math.Max(0, inventory.QuantityOnHand - quantity);
    }
}
