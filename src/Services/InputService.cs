// Mugs/Services/InputService.cs

using Mugs.Models;

namespace Mugs.Services
{
    public static class InputService
    {
        private static int _inputRow = -1;
        private static List<string> _commandHistory = new List<string>();
        private static int _historyIndex = -1;
        private static List<string> _suggestions = new List<string>();
        private static int _suggestionIndex = -1;

        public static void Initialize()
        {
            _inputRow = Console.WindowHeight - 1;
            ClearInputLine();
        }

        public static string ReadLineWithColorHighlighting(CommandManager manager)
        {
            if (_inputRow == -1) Initialize();

            var input = string.Empty;
            var position = 0;

            while (true)
            {
                RedrawInputLine(manager, input, position);

                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    if (!string.IsNullOrEmpty(input))
                    {
                        _commandHistory.Add(input);
                        _historyIndex = _commandHistory.Count;
                    }
                    ClearInputLine();
                    return input;
                }
                else if (key.Key == ConsoleKey.Tab && _suggestions.Any())
                {
                    _suggestionIndex = (_suggestionIndex + 1) % _suggestions.Count;
                    input = _suggestions[_suggestionIndex];
                    position = input.Length;
                }
                else if (key.Key == ConsoleKey.Backspace && position > 0)
                {
                    input = input.Remove(position - 1, 1);
                    position--;
                    _suggestions.Clear();
                    _suggestionIndex = -1;
                }
                else if (key.Key == ConsoleKey.LeftArrow && position > 0)
                {
                    position--;
                }
                else if (key.Key == ConsoleKey.RightArrow && position < input.Length)
                {
                    position++;
                }
                else if (key.Key == ConsoleKey.UpArrow && _commandHistory.Any())
                {
                    if (_historyIndex > 0) _historyIndex--;
                    if (_historyIndex >= 0 && _historyIndex < _commandHistory.Count)
                    {
                        input = _commandHistory[_historyIndex];
                        position = input.Length;
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow && _commandHistory.Any())
                {
                    if (_historyIndex < _commandHistory.Count - 1)
                    {
                        _historyIndex++;
                        input = _commandHistory[_historyIndex];
                        position = input.Length;
                    }
                    else
                    {
                        _historyIndex = _commandHistory.Count;
                        input = string.Empty;
                        position = 0;
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    input = input.Insert(position, key.KeyChar.ToString());
                    position++;

                    var currentWord = input.Split(' ').FirstOrDefault() ?? string.Empty;
                    _suggestions = manager.GetCommandNamesStartingWith(currentWord).ToList();
                    _suggestionIndex = -1;
                }
            }
        }

        private static void RedrawInputLine(CommandManager manager, string input, int cursorPosition)
        {
            Console.SetCursorPosition(0, _inputRow);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, _inputRow);
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
            Console.SetCursorPosition(Math.Min(cursorPosition + 2, Console.WindowWidth - 1), _inputRow);
        }

        public static void ClearInputLine()
        {
            Console.SetCursorPosition(0, _inputRow);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, _inputRow);
        }

        public static List<string> GetCommandHistory()
        {
            return new List<string>(_commandHistory);
        }
    }
}