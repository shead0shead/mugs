// Mugs/Program.cs

using Mugs.Models;
using Mugs.Services;

public class Program
{
    private const string ExtensionsFolder = "Extensions";

    public static async Task Main(string[] args)
    {
        MetadataCacheService.Initialize();
        AliasManagerService.Initialize();
        AppSettings.Initialize();
        ConsoleHelperService.Initialize();
        LocalizationService.Initialize();

        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = LocalizationService.GetString("app_title");

        if (args.All(a => a != "--updated"))
        {
            ConsoleHelperService.WriteResponse("checking_updates");
            await UpdateCheckerService.CheckForUpdatesAsync();
        }
        else
        {
            ConsoleHelperService.WriteResponse("update_success");
        }

        var manager = new CommandManager(ExtensionsFolder);
        await manager.LoadCommandsAsync();

        ConsoleHelperService.WriteResponse("welcome_message");

        while (true)
        {
            var input = ConsoleHelperService.ReadLineWithColorHighlighting(manager);

            if (string.IsNullOrEmpty(input)) continue;

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleHelperService.WriteResponse("exit_confirmation");
                var confirm = Console.ReadLine();
                if (confirm.Equals("y", StringComparison.OrdinalIgnoreCase))
                    break;
                continue;
            }

            var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var commandName = parts[0];
            var commandArgs = parts.Length > 1 ? parts.Skip(1).ToArray() : Array.Empty<string>();

            var command = manager.GetCommand(commandName);
            if (command == null)
            {
                ConsoleHelperService.WriteError("command_not_found", commandName);
                continue;
            }

            try
            {
                await command.ExecuteAsync(commandArgs);
            }
            catch (Exception ex)
            {
                ConsoleHelperService.WriteError("command_error", ex.Message);
            }
        }
    }
}