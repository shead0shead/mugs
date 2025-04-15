// Mugs/Services/OutputService.cs

namespace Mugs.Services
{
    public static class OutputService
    {
        private const char BorderChar = '▌';

        public static void WriteResponse(string messageKey, params object[] args) => 
            Write(messageKey, AppSettings.ResponseColor, "Response", args);

        public static void WriteError(string messageKey, params object[] args) => 
            Write(messageKey, AppSettings.ErrorColor, "Error", args);

        public static void WriteWarning(string messageKey, params object[] args) => 
            Write(messageKey, AppSettings.WarningColor, "Warning", args);

        public static void WriteSuccess(string messageKey, params object[] args) => 
            Write(messageKey, AppSettings.SuccessColor, "Success", args);

        public static void WriteInfo(string messageKey, params object[] args) => 
            Write(messageKey, AppSettings.InfoColor, "Info", args);

        public static void WriteDebug(string message) =>
            Write(message, AppSettings.DebugColor, "Debug");

        public static void WriteLog(string message, ConsoleColor color)
        {
            var lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                Console.ForegroundColor = color;
                Console.Write($"{BorderChar} ");
                Console.ResetColor();
                Console.WriteLine(line);
            }
            Console.WriteLine();
        }

        private static void Write(string messageKey, ConsoleColor borderColor, string logPrefix, params object[] args)
        {
            var message = LocalizationService.GetString(messageKey, args);
            var lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            switch (logPrefix)
            {
                case "Error":
                    LoggerService.LogError($"Error: {message}");
                    break;
                case "Warning":
                    LoggerService.LogWarning($"Warning: {message}");
                    break;
                case "Debug":
                    LoggerService.LogDebug($"Debug: {message}");
                    break;
                default:
                    LoggerService.LogInfo($"{logPrefix}: {message}");
                    break;
            }

            foreach (var line in lines)
            {
                Console.ForegroundColor = borderColor;
                Console.Write($"{BorderChar} ");
                if (logPrefix == "Debug") Console.Write("[DEBUG] ");
                if (logPrefix != "Error") Console.ResetColor();
                Console.WriteLine(line);
                Console.ResetColor();
            }
            Console.WriteLine();
        }
    }
}