// Mugs/Models/LanguageData.cs

namespace Mugs.Models
{
    public class LanguageData
    {
        public string LanguageName { get; set; } = "English";
        public Dictionary<string, string> Translations { get; set; } = new Dictionary<string, string>();
    }
}