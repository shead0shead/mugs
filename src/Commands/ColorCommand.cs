// Mugs/Commands/ColorCommand.cs

using Mugs.Models;
using Mugs.Services;
using Mugs.Interfaces;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Mugs.Commands
{
    public class ColorCommand : ICommand
    {
        public string Name => "color";
        public string Description => LocalizationService.GetString("color_description");
        public IEnumerable<string> Aliases => new[] { "colors" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "color set response green\ncolor get error\ncolor list";

        private static readonly Dictionary<string, Func<ConsoleColor>> ColorGetters = new()
        {
            ["response"] = () => AppSettings.ResponseColor,
            ["error"] = () => AppSettings.ErrorColor,
            ["warning"] = () => AppSettings.WarningColor,
            ["success"] = () => AppSettings.SuccessColor,
            ["info"] = () => AppSettings.InfoColor,
            ["debug"] = () => AppSettings.DebugColor
        };

        private static readonly Dictionary<string, Action<ConsoleColor>> ColorSetters = new()
        {
            ["response"] = (color) => AppSettings.ResponseColor = color,
            ["error"] = (color) => AppSettings.ErrorColor = color,
            ["warning"] = (color) => AppSettings.WarningColor = color,
            ["success"] = (color) => AppSettings.SuccessColor = color,
            ["info"] = (color) => AppSettings.InfoColor = color,
            ["debug"] = (color) => AppSettings.DebugColor = color
        };

        public Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0 || args[0] == "list")
            {
                return ListColors();
            }

            switch (args[0].ToLower())
            {
                case "get" when args.Length > 1:
                    return GetColor(args[1]);
                case "set" when args.Length > 2:
                    return SetColor(args[1], args[2]);
                default:
                    OutputService.WriteError("color_invalid_command");
                    return Task.CompletedTask;
            }
        }

        private Task ListColors()
        {
            var response = new System.Text.StringBuilder(
                LocalizationService.GetString("color_available_settings") + "\n");

            foreach (var colorType in ColorGetters.Keys)
            {
                var color = ColorGetters[colorType]();
                var displayType = LocalizationService.GetString($"color_type_{colorType}");
                response.AppendLine($"  {displayType,-9}- {color}");
            }

            OutputService.WriteResponse(response.ToString().TrimEnd());
            return Task.CompletedTask;
        }

        private Task GetColor(string colorType)
        {
            if (!ColorGetters.ContainsKey(colorType))
            {
                OutputService.WriteError("color_invalid_type", colorType);
                return Task.CompletedTask;
            }

            var color = ColorGetters[colorType]();
            var displayType = LocalizationService.GetString($"color_type_{colorType}");
            OutputService.WriteResponse("color_current_value", displayType, color);
            return Task.CompletedTask;
        }

        private Task SetColor(string colorType, string colorName)
        {
            if (!ColorSetters.ContainsKey(colorType))
            {
                OutputService.WriteError("color_invalid_type", colorType);
                return Task.CompletedTask;
            }

            if (!Enum.TryParse<ConsoleColor>(colorName, true, out var colorValue))
            {
                OutputService.WriteError("color_invalid_name", colorName);
                var colors = string.Join(", ", Enum.GetNames(typeof(ConsoleColor)));
                OutputService.WriteResponse("color_available_colors", colors);
                return Task.CompletedTask;
            }

            ColorSetters[colorType](colorValue);
            var displayType = LocalizationService.GetString($"color_type_{colorType}");
            OutputService.WriteSuccess("color_changed", displayType, colorValue);
            return Task.CompletedTask;
        }
    }
}