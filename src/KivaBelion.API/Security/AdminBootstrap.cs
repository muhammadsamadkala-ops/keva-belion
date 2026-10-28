using System.ComponentModel.DataAnnotations;
using KivaBelion.API.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace KivaBelion.API.Security;

/// <summary>
/// One-time command to create the first admin account:
///   dotnet run --project src\KivaBelion.API -- create-admin --email you@example.com --name "Your Name"
/// The password is read from the environment variable KIVA_ADMIN_PASSWORD (set by scripts\create-admin.ps1),
/// so it never appears in a file, in the command line, or in the logs.
/// </summary>
public static class AdminBootstrap
{
    public const string Command = "create-admin";
    public const string PasswordVariable = "KIVA_ADMIN_PASSWORD";

    public static async Task<int> CreateAdminAsync(IServiceProvider services, string[] args)
    {
        using var scope = services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IAdminUserStore>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AdminUser>>();

        var email = GetOption(args, "--email")?.Trim().ToLowerInvariant();
        var name = GetOption(args, "--name")?.Trim();
        var password = Environment.GetEnvironmentVariable(PasswordVariable);

        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
        {
            Console.Error.WriteLine("A valid --email is required.");
            return 2;
        }
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2 || name.Length > 100)
        {
            Console.Error.WriteLine("--name is required (2 to 100 characters).");
            return 2;
        }

        var problems = PasswordPolicy.Validate(password, email);
        if (problems.Count > 0)
        {
            foreach (var problem in problems) Console.Error.WriteLine(problem);
            return 2;
        }

        if (await store.FindByEmailAsync(email, CancellationToken.None) is not null)
        {
            Console.Error.WriteLine("An admin with that email already exists. Nothing was changed.");
            return 3;
        }

        var user = new AdminUser { Email = email, DisplayName = name, Role = AdminRole.Admin, IsActive = true };
        user.PasswordHash = hasher.HashPassword(user, password!);

        await store.AddAsync(user, CancellationToken.None);
        await store.SaveChangesAsync(CancellationToken.None);

        Console.WriteLine($"Created admin account for {email}.");
        return 0;
    }

    private static string? GetOption(string[] args, string name)
    {
        var index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
