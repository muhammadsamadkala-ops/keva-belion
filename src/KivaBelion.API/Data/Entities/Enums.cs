namespace KivaBelion.API.Data.Entities;

/// <summary>Stored in the database as readable text (e.g. "Pending"), so reports are easy to read.</summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}

/// <summary>Only cash on delivery exists for now. Online payments are a later phase.</summary>
public enum PaymentMethod
{
    CashOnDelivery = 0
}

/// <summary>Roles for staff accounts. Admin can change the catalogue; Staff is read-only for now.</summary>
public enum AdminRole
{
    Admin = 0,
    Staff = 1
}
