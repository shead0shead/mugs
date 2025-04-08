// Mugs/Commands/LanguageCommand.cs

using Mugs.Services;
using Mugs.Interfaces;
using Mugs.Models;

using System.Text;

namespace Mugs.Commands
{
    public class LanguageCommand : ICommand
    {
        public string Name => "language";
        public string Description => LocalizationService.GetString("language_description");
        public IEnumerable<string> Aliases => new[] { "lang" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "language en, language ru";

        public Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                var response = new StringBuilder();
                response.AppendLine(LocalizationService.GetString("current_language",
                    $"{LocalizationService.GetLanguageName(LocalizationService.CurrentLanguage)} ({LocalizationService.CurrentLanguage})"));

                var availableLangs = LocalizationService.GetAvailableLanguages()
                    .Select(lang => $"{LocalizationService.GetLanguageName(lang)} ({lang})");

                response.AppendLine(LocalizationService.GetString("available_languages", string.Join(", ", availableLangs)));

                OutputService.WriteResponse(response.ToString().TrimEnd());
            }
            else
            {
                var langCode = args[0].ToLower();
                if (LocalizationService.GetAvailableLanguages().Contains(langCode))
                {
                    LocalizationService.SetLanguage(langCode);
                    AppSettings.Language = langCode;
                    OutputService.WriteResponse("language_changed",
                        $"{LocalizationService.GetLanguageName(langCode)} ({langCode})");
                }
                else
                {
                    OutputService.WriteError("invalid_language", langCode);
                }
            }

            return Task.CompletedTask;
        }
    }
}