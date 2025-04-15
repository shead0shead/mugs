// Mugs/Services/OutputService.cs

using Mugs.Models;

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

        public static void WriteTable(string title, IEnumerable<string> items)
        {
            const char TopLeft = '╭';
            const char TopRight = '╮';
            const char BottomLeft = '╰';
            const char BottomRight = '╯';
            const char Horizontal = '─';
            const char Vertical = '│';
            const char Space = ' ';

            if (!items.Any()) return;

            int maxLength = Math.Max(
                items.Max(i => i.Length),
                title.Length
            ) + 4;

            var titleLine = $"{TopLeft}{Horizontal}{Space}{title}{Space}";
            while (titleLine.Length < maxLength - 1)
                titleLine += Horizontal;
            titleLine += TopRight;

            Console.WriteLine(titleLine);

            foreach (var item in items)
            {
                var content = $"{Vertical}{Space}{item}";
                while (content.Length < maxLength - 1)
                    content += Space;
                content += Vertical;
                Console.WriteLine(content);
            }

            var bottomLine = $"{BottomLeft}";
            while (bottomLine.Length < maxLength - 1)
                bottomLine += Horizontal;
            bottomLine += BottomRight;

            Console.WriteLine(bottomLine);
            Console.WriteLine();
        }

        public static void WriteTableColumns(string title, IEnumerable<IEnumerable<string>> columns, IEnumerable<string> columnHeaders = null)
        {
            const char TopLeft = '╭';
            const char TopRight = '╮';
            const char BottomLeft = '╰';
            const char BottomRight = '╯';
            const char Horizontal = '─';
            const char Vertical = '│';
            const char Cross = '├';
            const char MiddleCross = '┼';
            const char RightCross = '┤';
            const char Space = ' ';

            if (!columns.Any()) return;

            int consoleWidth = Math.Max(Console.WindowWidth, 20);
            int contentWidth = consoleWidth - 2;

            var columnWidths = new List<int>();
            int remainingWidth = contentWidth;

            var minWidths = columns.Select(col =>
                col.Concat(columnHeaders?.Take(1) ?? Enumerable.Empty<string>())
                   .Max(item => item?.Length ?? 0) + 2).ToList();

            int totalMinWidth = minWidths.Sum() + minWidths.Count - 1;
            if (totalMinWidth >= contentWidth)
            {
                double ratio = (double)contentWidth / totalMinWidth;
                columnWidths = minWidths.Select(w => (int)(w * ratio)).ToList();
            }
            else
            {
                int extraSpace = contentWidth - totalMinWidth;
                columnWidths = minWidths.Select(w => w + extraSpace / minWidths.Count).ToList();
            }

            columnWidths = columnWidths.Select(w => Math.Max(w, 3)).ToList();

            int tableWidth = columnWidths.Sum() + columnWidths.Count - 1;
            int widthDiff = contentWidth - tableWidth;

            if (widthDiff != 0 && columnWidths.Count > 0)
            {
                columnWidths[columnWidths.Count - 1] += widthDiff;
            }

            string titlePart = $" {title} ";
            int titleLeftPadding = (contentWidth - titlePart.Length) / 2;
            if (titleLeftPadding < 0) titleLeftPadding = 0;

            string topLine = $"{TopLeft}{new string(Horizontal, titleLeftPadding)}{titlePart}";
            while (topLine.Length < consoleWidth - 1) topLine += Horizontal;
            topLine += TopRight;
            Console.WriteLine(topLine);

            if (columnHeaders != null)
            {
                string headerLine = Vertical.ToString();
                for (int i = 0; i < columnHeaders.Count(); i++)
                {
                    string header = columnHeaders.ElementAt(i);
                    int width = i < columnWidths.Count ? columnWidths[i] : 10;
                    headerLine += $"{Space}{header.Truncate(width - 2).PadRight(width - 1)}";
                    if (i < columnHeaders.Count() - 1) headerLine += Vertical;
                }
                headerLine += Vertical;
                Console.WriteLine(headerLine);

                string separator = Cross.ToString();
                for (int i = 0; i < columnWidths.Count; i++)
                {
                    separator += new string(Horizontal, columnWidths[i]);
                    if (i < columnWidths.Count - 1) separator += MiddleCross;
                }
                separator += RightCross;
                Console.WriteLine(separator);
            }

            int rowCount = columns.First().Count();
            for (int row = 0; row < rowCount; row++)
            {
                string rowLine = Vertical.ToString();
                for (int col = 0; col < columns.Count(); col++)
                {
                    var column = columns.ElementAt(col);
                    var cell = row < column.Count() ? column.ElementAt(row) : "";
                    int width = col < columnWidths.Count ? columnWidths[col] : 10;
                    rowLine += $"{Space}{cell.Truncate(width - 2).PadRight(width - 1)}";
                    if (col < columns.Count() - 1) rowLine += Vertical;
                }
                rowLine += Vertical;
                Console.WriteLine(rowLine);
            }

            string bottomLine = $"{BottomLeft}{new string(Horizontal, contentWidth)}{BottomRight}";
            Console.WriteLine(bottomLine);
            Console.WriteLine();
        }

        public static void WriteTableColumnsHighlight(string title, IEnumerable<IEnumerable<string>> columns, IEnumerable<string> columnHeaders = null)
        {
            const char TopLeft = '╭';
            const char TopRight = '╮';
            const char BottomLeft = '╰';
            const char BottomRight = '╯';
            const char Horizontal = '─';
            const char Vertical = '│';
            const char Cross = '├';
            const char MiddleCross = '┼';
            const char RightCross = '┤';
            const char Space = ' ';

            const ConsoleColor EvenRowBackground = ConsoleColor.DarkGray;
            const ConsoleColor EvenRowTextColor = ConsoleColor.Black;
            const ConsoleColor OddRowBackground = ConsoleColor.Black;
            const ConsoleColor OddRowTextColor = ConsoleColor.Gray;
            const ConsoleColor BorderColor = ConsoleColor.Gray;

            if (!columns.Any()) return;

            int consoleWidth = Math.Max(Console.WindowWidth, 20);
            int contentWidth = consoleWidth - 2;

            var columnWidths = new List<int>();
            int remainingWidth = contentWidth;

            var minWidths = columns.Select(col =>
                col.Concat(columnHeaders?.Take(1) ?? Enumerable.Empty<string>())
                   .Max(item => item?.Length ?? 0) + 2).ToList();

            int totalMinWidth = minWidths.Sum() + minWidths.Count - 1;
            if (totalMinWidth >= contentWidth)
            {
                double ratio = (double)contentWidth / totalMinWidth;
                columnWidths = minWidths.Select(w => (int)(w * ratio)).ToList();
            }
            else
            {
                int extraSpace = contentWidth - totalMinWidth;
                columnWidths = minWidths.Select(w => w + extraSpace / minWidths.Count).ToList();
            }

            columnWidths = columnWidths.Select(w => Math.Max(w, 3)).ToList();

            int tableWidth = columnWidths.Sum() + columnWidths.Count - 1;
            int widthDiff = contentWidth - tableWidth;

            if (widthDiff != 0 && columnWidths.Count > 0)
            {
                columnWidths[columnWidths.Count - 1] += widthDiff;
            }

            string titlePart = $" {title} ";
            int titleLeftPadding = (contentWidth - titlePart.Length) / 2;
            if (titleLeftPadding < 0) titleLeftPadding = 0;

            string topLine = $"{TopLeft}{new string(Horizontal, titleLeftPadding)}{titlePart}";
            while (topLine.Length < consoleWidth - 1) topLine += Horizontal;
            topLine += TopRight;
            Console.WriteLine(topLine);

            if (columnHeaders != null)
            {
                string headerLine = Vertical.ToString();
                for (int i = 0; i < columnHeaders.Count(); i++)
                {
                    string header = columnHeaders.ElementAt(i);
                    int width = i < columnWidths.Count ? columnWidths[i] : 10;
                    headerLine += $"{Space}{header.Truncate(width - 2).PadRight(width - 1)}";
                    if (i < columnHeaders.Count() - 1) headerLine += Vertical;
                }
                headerLine += Vertical;
                Console.WriteLine(headerLine);

                string separator = Cross.ToString();
                for (int i = 0; i < columnWidths.Count; i++)
                {
                    separator += new string(Horizontal, columnWidths[i]);
                    if (i < columnWidths.Count - 1) separator += MiddleCross;
                }
                separator += RightCross;
                Console.WriteLine(separator);
            }

            int rowCount = columns.First().Count();
            for (int row = 0; row < rowCount; row++)
            {
                Console.ForegroundColor = BorderColor;
                Console.Write(Vertical);
                Console.ResetColor();

                ConsoleColor bgColor = row % 2 == 0 ? EvenRowBackground : OddRowBackground;
                ConsoleColor txtColor = row % 2 == 0 ? EvenRowTextColor : OddRowTextColor;
                Console.BackgroundColor = bgColor;
                Console.ForegroundColor = txtColor;

                for (int col = 0; col < columns.Count(); col++)
                {
                    var column = columns.ElementAt(col);
                    var cell = row < column.Count() ? column.ElementAt(row) : "";
                    int width = col < columnWidths.Count ? columnWidths[col] : 10;

                    Console.Write($"{Space}{cell.Truncate(width - 2).PadRight(width - 1)}");

                    if (col < columns.Count() - 1)
                    {
                        Console.ResetColor();
                        Console.ForegroundColor = BorderColor;
                        Console.Write(Vertical);
                        Console.BackgroundColor = bgColor;
                        Console.ForegroundColor = txtColor;
                    }
                }

                Console.ResetColor();
                Console.ForegroundColor = BorderColor;
                Console.WriteLine(Vertical);
                Console.ResetColor();
            }

            string bottomLine = $"{BottomLeft}{new string(Horizontal, contentWidth)}{BottomRight}";
            Console.WriteLine(bottomLine);
            Console.WriteLine();
        }

        private static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 1) + "…";
        }
    }
}