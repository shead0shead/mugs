// Mugs/Program.cs

using Mugs.Models;
using Mugs.Services;

public class Program
{
    private const string ExtensionsFolder = "Extensions";

    public static async Task Main(string[] args)
    {
        try
        {
            AppSettings.Initialize();
            LocalizationService.Initialize();
            AliasManagerService.Initialize();
            MetadataCacheService.Initialize();
            InputService.Initialize();

            LoggerService.LogInfo("Services initialized");

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = LocalizationService.GetString("app_title");

            if (args.All(a => a != "--updated"))
            {
                OutputService.WriteResponse("checking_updates");
                using (SpinnerService.StartActivity())
                {
                    await UpdateCheckerService.CheckForUpdatesAsync();
                }
            }
            else
            {
                OutputService.WriteResponse("update_success");
            }

            var manager = new CommandManager(ExtensionsFolder);
            using (SpinnerService.StartActivity())
            {
                manager.LoadCommandsAsync().Wait();
            }

            LoggerService.LogInfo("Application started successfully");
            OutputService.WriteResponse("welcome_message");

            while (true)
            {
                var input = InputService.ReadLineWithColorHighlighting(manager);

                if (string.IsNullOrEmpty(input)) continue;

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    OutputService.WriteResponse("exit_confirmation");
                    using (SpinnerService.PauseForInput())
                    {
                        var confirm = Console.ReadLine();
                        if (confirm.Equals("y", StringComparison.OrdinalIgnoreCase))
                            break;
                    }
                    continue;
                }

                var parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var commandName = parts[0];
                var commandArgs = parts.Length > 1 ? parts.Skip(1).ToArray() : Array.Empty<string>();

                var command = manager.GetCommand(commandName);
                if (command == null)
                {
                    OutputService.WriteError("command_not_found", commandName);
                    continue;
                }

                try
                {
                    using (SpinnerService.StartActivity())
                    {
                        await command.ExecuteAsync(commandArgs);
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.LogError("Command processing error", ex);
                    OutputService.WriteError("command_error", ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            LoggerService.LogCritical("Fatal application error", ex);
            Environment.Exit(1);
        }
    }
}