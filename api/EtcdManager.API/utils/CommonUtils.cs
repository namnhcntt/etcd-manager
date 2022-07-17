namespace EtcdManager.API.utils
{
    public class CommonUtils
    {
        public static string GetEtcdResponseErrorMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return string.Empty;
            var arr = message.Split("Detail=\"");
            if (arr.Length > 1)
            {
                return arr[1].Split("\"")[0];
            }
            return string.Empty;
        }
    }
}