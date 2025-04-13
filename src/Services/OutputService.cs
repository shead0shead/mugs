// Mugs/Services/OutputService.cs

namespace Mugs.Services
{
    public static class OutputService
    {
        private const char BorderChar = '▌';

        public static void WriteResponse(string messageKey, params object[] args) => 
            Write(messageKey, ConsoleColor.DarkGray, "Response", args);

        public static void WriteError(string messageKey, params object[] args) => 
            Write(messageKey, ConsoleColor.DarkRed, "Error", args);

        public static void WriteWarning(string messageKey, params object[] args) => 
            Write(messageKey, ConsoleColor.DarkYellow, "Warning", args);

        public static void WriteSuccess(string messageKey, params object[] args) => 
            Write(messageKey, ConsoleColor.DarkGreen, "Success", args);

        public static void WriteInfo(string messageKey, params object[] args) => 
            Write(messageKey, ConsoleColor.DarkCyan, "Info", args);

        //public static void WriteError(string messageKey, params object[] args)
        //{
        //    var message = LocalizationService.GetString(messageKey, args);
        //    var lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        //    LoggerService.LogError($"Error: {message}");

        //    foreach (var line in lines)
        //    {
        //        Console.ForegroundColor = ConsoleColor.Red;
        //        Console.Write($"{BorderChar} ");
        //        Console.WriteLine(line);
        //        Console.ResetColor();
        //    }
        //    Console.WriteLine();
        //}

        public static void WriteDebug(string message)
        {
            var lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            LoggerService.LogDebug($"Debug: {message}");

            foreach (var line in lines)
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write($"{BorderChar} ");
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("[DEBUG] ");
                Console.ResetColor();
                Console.WriteLine(line);
            }
        }

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
            LoggerService.LogInfo($"{logPrefix}: {message}");

            foreach (var line in lines)
            {
                Console.ForegroundColor = borderColor;
                Console.Write($"{BorderChar} ");
                Console.ResetColor();
                Console.WriteLine(line);
            }
            Console.WriteLine();
        }
    }
}