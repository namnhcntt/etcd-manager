using System.Security.Cryptography;
using System.Text;

namespace EtcdManager.API.Core.Helpers
{
    public static class CommonHelper
    {
        public static string SHA256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

    }
}
