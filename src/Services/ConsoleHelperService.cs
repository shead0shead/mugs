// Mugs/Services/ConsoleHelperService.cs

using Mugs.Models;

namespace Mugs.Services
{
    public static class ConsoleHelperService
    {
        private static int inputRow = -1;
        private static List<string> commandHistory = new List<string>();
        private static int historyIndex = -1;
        private const char BorderChar = '▌';
        private static readonly ConsoleColor BorderColor = ConsoleColor.DarkGray;

        public static void Initialize()
        {
            inputRow = Console.WindowHeight - 1;
            ClearInputLine();
        }

        public static string ReadLineWithColorHighlighting(CommandManager manager)
        {
            if (inputRow == -1) Initialize();

            var input = string.Empty;
            var position = 0;
            var suggestions = new List<string>();
            var suggestionIndex = -1;

            while (true)
            {
                RedrawInputLine(manager, input, position);

                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    if (!string.IsNullOrEmpty(input))
                    {
                        commandHistory.Add(input);
                        historyIndex = commandHistory.Count;
                    }
                    ClearInputLine();
                    return input;
                }
                else if (key.Key == ConsoleKey.Tab && suggestions.Any())
                {
                    suggestionIndex = (suggestionIndex + 1) % suggestions.Count;
                    input = suggestions[suggestionIndex];
                    position = input.Length;
                }
                else if (key.Key == ConsoleKey.Backspace && position > 0)
                {
                    input = input.Remove(position - 1, 1);
                    position--;
                    suggestions.Clear();
                    suggestionIndex = -1;
                }
                else if (key.Key == ConsoleKey.LeftArrow && position > 0)
                {
                    position--;
                }
                else if (key.Key == ConsoleKey.RightArrow && position < input.Length)
                {
                    position++;
                }
                else if (key.Key == ConsoleKey.UpArrow && commandHistory.Any())
                {
                    if (historyIndex > 0) historyIndex--;
                    if (historyIndex >= 0 && historyIndex < commandHistory.Count)
                    {
                        input = commandHistory[historyIndex];
                        position = input.Length;
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow && commandHistory.Any())
                {
                    if (historyIndex < commandHistory.Count - 1)
                    {
                        historyIndex++;
                        input = commandHistory[historyIndex];
                        position = input.Length;
                    }
                    else
                    {
                        historyIndex = commandHistory.Count;
                        input = string.Empty;
                        position = 0;
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    input = input.Insert(position, key.KeyChar.ToString());
                    position++;

                    var currentWord = input.Split(' ').FirstOrDefault() ?? string.Empty;
                    suggestions = manager.GetCommandNamesStartingWith(currentWord).ToList();
                    suggestionIndex = -1;
                }
            }
        }

        private static void RedrawInputLine(CommandManager manager, string input, int cursorPosition)
        {
            Console.SetCursorPosition(0, inputRow);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, inputRow);
            Console.Write("> ");

            var isValid = manager.IsValidCommand(input);
            Console.ForegroundColor = isValid ? ConsoleColor.Gray : ConsoleColor.Red;
            Console.Write(input);

            if (AppSettings.EnableSuggestions && !string.IsNullOrEmpty(input))
            {
                var suggestion = manager.GetCommandSuggestion(input.Split(' ')[0]);
                if (suggestion != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(suggestion);
                }
            }

            Console.ResetColor();
            Console.SetCursorPosition(Math.Min(cursorPosition + 2, Console.WindowWidth - 1), inputRow);
        }

        public static void ClearInputLine()
        {
            Console.SetCursorPosition(0, inputRow);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, inputRow);
        }

        public static List<string> GetCommandHistory()
        {
            return new List<string>(commandHistory);
        }

        public static void WriteResponse(string messageKey, params object[] args)
        {
            var message = LocalizationService.GetString(messageKey, args);
            var lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

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

            foreach (var line in lines)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write($"[DEBUG] ");
                Console.ResetColor();
                Console.WriteLine(line);
            }
        }
    }
}