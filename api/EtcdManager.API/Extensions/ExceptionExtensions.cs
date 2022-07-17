using System.Runtime.CompilerServices;
using EtcdManager.API.utils;

namespace EtcdManager.API.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetDebugMessage(this Exception ex)
        {
            string result = ex.ToString();
            if (ex.StackTrace != null)
                result += Environment.NewLine + "\t" + ex.StackTrace.ToString();
            result += Environment.NewLine + "\t" + ex.Message;
            return result;
        }


        public static void ShowDebugMessage(this Exception ex, string prefix = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            string message = ex.GetDebugMessage();
            string location = ErrorLogUtils.GetDetail(filePath, lineNumber, caller);
            Console.WriteLine($"-----> Exception detail {prefix}: {message}. {location}");
        }
    }
}