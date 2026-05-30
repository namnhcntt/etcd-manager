using System.Security.Cryptography;
using System.Text;

namespace EtcdManager.API.Core.Helpers;

public static class CommonHelper
{
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public static bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    [Obsolete("Use HashPassword instead. SHA256 without salt is insecure.")]
    public static string SHA256Hash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
