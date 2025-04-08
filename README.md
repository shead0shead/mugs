# Mugs - Extensible Console Utility

Mugs is a powerful console utility with support for dynamic command loading through C# scripts. The program provides a convenient environment for executing built-in and custom commands with features like localization, automatic updates, and a security system.

![Screenshot](./assets/screenshot.png)

## Key Features

* **Dynamic command loading** from `.csx` and `.cs` files  
* **Built-in commands** for system management  
* **Localization support** with the ability to add new languages  
* **Automatic updates** via GitHub Release  
* **Security system** with script checks for dangerous code  
* **Command hints** with auto-completion  
* **Command history** with search functionality  
* **Command aliases** for quick access  
* **Batch execution** of commands from files  
* **Command debugging** with execution time output  
* **Metadata caching** for fast command loading  
* **Verified extensions** with hash verification  
* **Logging** of command execution and errors  

## Installation

1. Download the latest version from the [Releases section](https://github.com/shead0shead/mugs/releases)  
2. Extract the archive to a convenient directory  
3. Run `Mugs.exe`  

## Usage

After launching, you will see a welcome message. Type `help` to view the list of available commands.  

## Basic Commands

* `help` - Show command help  
* `list` - List all available commands  
* `reload` - Reload all commands  
* `clear` - Clear the console  
* `restart` - Fully restart the application  
* `time` - Show the current time  
* `update` - Check for and install updates  
* `new <name>` - Create a template for a new command  
* `enable/disable <command>` - Enable/disable an extension  
* `import <url>` - Install an extension from a URL  
* `language <code>` - Change the interface language  
* `logging` - Enable/disable console logging output  
* `script <file>` - Execute commands from a file  
* `suggestions` - Enable/disable command hints  
* `alias` - Manage command aliases  
* `scan <file>` - Check a script for dangerous code  
* `history` - Show command history  
* `version` - Show the application version  
* `debug <command>` - Run a command in debug mode  

## Creating Custom Commands

1. Use the command `new mycommand` to create a template  
2. Edit the file `Extensions/mycommand.csx`  
3. Run `reload` to apply the changes  

Example of a simple command:  

```csharp
public class MyCommand : ICommand
{
    public string Name => "mycommand";
    public string Description => "My custom command";
    public IEnumerable<string> Aliases => new[] { "mc" };
    public string Author => "Your Name";
    public string Version => "1.0";
    public string? UsageExample => "mycommand arg1 arg2";

    public async Task ExecuteAsync(string[] args)
    {
        // Output command arguments to console
        OutputService.WriteResponse($"Command executed with args: {string.Join(", ", args)}");
        
        // Use shared data between commands
        SetSharedData("mykey", "value");
        var data = GetSharedData<string>("mykey");
    }
}

// Required for script commands
new MyCommand()
```

## Creating and Using Libraries

Mugs allows you to create reusable libraries that can be shared between different commands. Here's how to create and use a library:

#### 1. Creating a Library (math_utils.csx)

```csharp
// math_utils.csx - Simple math operations library

public class MathUtils
{
    // Add two numbers
    public static double Add(double a, double b) => a + b;

    // Subtract two numbers
    public static double Subtract(double a, double b) => a - b;

    // Multiply two numbers
    public static double Multiply(double a, double b) => a * b;

    // Divide two numbers
    public static double Divide(double a, double b)
    {
        if (b == 0) throw new DivideByZeroException();
        return a / b;
    }

    // Calculate percentage
    public static double Percentage(double value, double percent) => value * percent / 100;
}
```
Key points about libraries:

* Place in `Extensions/` folder
* Can contain multiple utility classes
* Use `public` visibility for shared methods
* Can use platform interop (like Windows API)

#### 2. Using the Library (math_command.csx)

```csharp
// math_command.csx - Example command using math_utils library

#load "math_utils.csx"  // Load the library

public class MathCommand : ICommand
{
    // Command metadata
    public string Name => "math";
    public string Description => "Basic math operations";
    public IEnumerable<string> Aliases => new[] { "m" }; // Short alias
    public string Author => "Your Name";
    public string Version => "1.0";

    public async Task ExecuteAsync(string[] args)
    {
        // Validate argument count
        if (args.Length != 3)
        {
            OutputService.WriteResponse("Usage: math [add|sub|mul|div] x y");
            return;
        }

        try
        {
            // Parse input numbers
            double x = double.Parse(args[1]);
            double y = double.Parse(args[2]);
            
            // Calculate result using switch expression
            double result = args[0] switch
            {
                "add" => MathUtils.Add(x, y),
                "sub" => MathUtils.Subtract(x, y),
                "mul" => MathUtils.Multiply(x, y),
                "div" => MathUtils.Divide(x, y),
                _ => throw new ArgumentException("Invalid operation")
            };
            
            // Output formatted result
            OutputService.WriteResponse($"Result: {result}");
        }
        catch (Exception ex)
        {
            // Handle parsing and calculation errors
            OutputService.WriteError(ex.Message);
        }
    }
}

// Required instantiation for script commands
new MathCommand()
```
#### 3. Development Workflow

1. Create your library file (e.g., math_utils.csx)
2. Create command files that use the library with #load directive
3. Test your commands:

    ```
    reload               # Reload all commands
    math add 5 3         # Should output "Result: 8"
    math div 10 2        # Should output "Result: 5"
    ```

## Security

The program includes a script verification system:

* Automatic checks for dangerous calls (scan command)
* Verified extensions system (marked with a ✅ icon)
* Ability to disable suspicious scripts
* File hash verification for verified commands

## Localization

### Changing Language

```
language ru  # Switch to Russian
language en  # Switch back to English
```

### Adding New Languages

1. Create `Languages/<code>.json` (e.g., `es.json` for Spanish)
2. Use the English file as a template
3. Add translations for all keys

Example structure:

```
{
    "LanguageName": "Español",
    "Translations": {
        "app_title": "Mugs",
        "welcome_message": "Aplicación de consola...",
        ...
    }
}
```

## Development

### Architecture Overview

```
Mugs/
├── Commands/          # Built-in command implementations
├── Models/            # Data structures
├── Services/          # Core functionality
├── Interfaces/        # Public interfaces
└── Program.cs         # App entry point file
```

### Building from Source

1. Clone repository:

    ```bash
    git clone https://github.com/shead0shead/mugs.git
    ```
2. Build:

    ```bash
    dotnet build
    ```

## Contribution Guidelines

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Commit changes (`git commit -m 'Add some feature'`)
4. Push to branch (`git push origin feature/your-feature`)
5. Open a Pull Request

### Reporting Issues

Include:

* Mugs version (`version` command output)
* Steps to reproduce
* Expected vs actual behavior
* Screenshots if applicable

## FAQ

**Q: How do I update Mugs?**<br>
A: Run `update install` when a new version is available.

**Q: My command isn't loading after changes**<br>
A: Use `reload` to refresh all commands.

**Q: How do I check if a command is safe?**<br>
A: Verified commands show ✅ icon. Use `scan` for detailed analysis.

**Q: Can I use NuGet packages in commands?**<br>
A: Currently only built-in assemblies are supported.

## License

The project is distributed under the MIT license. For details, see the LICENSE file.

## Support and Contributions

Bug reports and feature requests can be submitted in Issues. Pull requests are welcome!

## System Requirements

* .NET 6.0 or higher
* Windows 7/10/11 or Linux/macOS (with Mono)
* 50 MB of free disk space