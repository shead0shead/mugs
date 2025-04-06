// Mugs/Interfaces/ICommand.cs

namespace Mugs.Interfaces
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        IEnumerable<string> Aliases { get; }
        string Author { get; }
        string Version { get; }
        string? UsageExample { get; }
        Task ExecuteAsync(string[] args);
    }
}