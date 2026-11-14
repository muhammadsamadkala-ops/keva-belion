using System.Security.Cryptography;

namespace KivaBelion.API.Orders;

/// <summary>Creates hard-to-guess order references such as "KB-7F3K9Q2M" (no 0/O/1/I/L to avoid mix-ups).</summary>
public static class OrderNumberGenerator
{
    private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    public const int RandomLength = 8;

    public static string Next()
    {
        var chars = new char[RandomLength];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        return "KB-" + new string(chars);
    }
}
