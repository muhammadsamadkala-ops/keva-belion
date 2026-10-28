namespace KivaBelion.API.Security;

public static class AuthorizationPolicies
{
    /// <summary>Required for anything that changes the catalogue (create, update, delete).</summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>Any signed-in staff member (Admin or Staff), for read-only admin views.</summary>
    public const string StaffOrAdmin = "StaffOrAdmin";
}
