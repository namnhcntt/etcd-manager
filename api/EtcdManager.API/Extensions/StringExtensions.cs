namespace EtcdManager.API.Extensions
{
    public static class StringExtensions
    {
        public static string ToCamelCaseString(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            return Char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}