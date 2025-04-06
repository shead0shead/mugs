// Mugs/Commands/NewCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

namespace Mugs.Commands
{
    public class NewCommand : ICommand
    {
        private readonly string _extensionsPath;

        public NewCommand(string extensionsPath) => _extensionsPath = extensionsPath;
        public string Name => "new";
        public string Description => LocalizationService.GetString("new_description");
        public IEnumerable<string> Aliases => new[] { "template" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "new mycommand";

        public Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleHelperService.WriteError("missing_command_name");
                return Task.CompletedTask;
            }

            var commandName = args[0].ToLowerInvariant();
            var fileName = $"{commandName}.csx";
            var filePath = Path.Combine(_extensionsPath, fileName);

            if (File.Exists(filePath))
            {
                ConsoleHelperService.WriteError("file_exists", fileName);
                return Task.CompletedTask;
            }

            var template = $@"// Example extension script for command '{commandName}'
                // Remove comments and implement your command

                public class {char.ToUpper(commandName[0]) + commandName.Substring(1)}Command : ICommand
                {{
                    public string Name => ""{commandName}"";
                    public string Description => ""Description of {commandName} command"";
                    public IEnumerable<string> Aliases => new[] {{ ""{commandName[0]}"", ""{commandName.Substring(0, Math.Min(3, commandName.Length))}"" }};
                    public string Author => ""Your Name"";
                    public string Version => ""1.0"";
                    public string? UsageExample => ""{commandName} arg1 arg2\n{commandName} --option"";

                    public async Task ExecuteAsync(string[] args)
                    {{
                        // Your code here
                        Print(""Command '{commandName}' executed!"");
        
                        // Example argument handling
                        if (args.Length > 0)
                        {{
                            Print($""Received arguments: {{string.Join("", "", args)}}"");
                        }}
                    }}
                }}

                // Return command instance
                new {char.ToUpper(commandName[0]) + commandName.Substring(1)}Command()";

            File.WriteAllText(filePath, template);
            ConsoleHelperService.WriteResponse("template_created", fileName);
            ConsoleHelperService.WriteResponse("reload_usage");

            return Task.CompletedTask;
        }
    }
}