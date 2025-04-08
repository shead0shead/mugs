// Mugs/Services/OutputService.cs

namespace Mugs.Services
{
    public static class OutputService
    {
        private const char BorderChar = '▌';
        private static readonly ConsoleColor BorderColor = ConsoleColor.DarkGray;

        public static void WriteResponse(string messageKey, params object[] args)
        {
            var message = LocalizationService.GetString(messageKey, args);
            var lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            LoggerService.LogInfo($"Response: {message}");

            foreach (var line in lines)
            {
                Console.ForegroundColor = BorderColor;
                Console.Write($"{BorderChar} ");
                Console.ResetColor();
                Console.WriteLine(line);
            }
            Console.WriteLine();
        }

        public static void WriteError(string messageKey, params object[] args)
        {
            var message = LocalizationService.GetString(messageKey, args);
            var lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            LoggerService.LogError($"Error: {message}");

            foreach (var line in lines)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"{BorderChar} ");
                Console.WriteLine(line);
                Console.ResetColor();
            }
            Console.WriteLine();
        }

        public static void WriteDebug(string message)
        {
            var lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            LoggerService.LogDebug($"Debug: {message}");

            foreach (var line in lines)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write($"[DEBUG] ");
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
    }
}