// Mugs/Models/CommandMetadata.cs

namespace Mugs.Models
{
    public class CommandMetadata
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Aliases { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string FilePath { get; set; }
        public string Hash { get; set; }
        public DateTime LastModified { get; set; }
    }
}