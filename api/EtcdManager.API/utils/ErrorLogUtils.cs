using System.Runtime.CompilerServices;

namespace EtcdManager.API.utils
{
    public class ErrorLogUtils
    {
        public static string GetDetail([CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1, [CallerMemberName] string caller = "")
        {
            return $"Line: {lineNumber} - Method: {caller} - file: {filePath}";
        }

        public static void LogDetail([CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1, [CallerMemberName] string caller = "")
        {
            string location = GetDetail(filePath, lineNumber, caller);
            Console.WriteLine($"-----> Debug line info: {location}");
        }

        public static void LogMessage(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1, [CallerMemberName] string caller = "")
        {
            string location = GetDetail(filePath, lineNumber, caller);
            Console.WriteLine($"-----> Message: {message}\n-----> Line info: {location}");
        }
    }
}