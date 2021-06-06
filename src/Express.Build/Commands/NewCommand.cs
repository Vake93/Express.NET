using Express.Net.Build.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;

namespace Express.Net.Build.Commands
{
    public class NewCommand : Command<NewCommand.NewSettings>
    {
        private static readonly Regex NameRegex = new(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");

        public sealed class NewSettings : CommandSettings
        {
            [Description("Name of the new project")]
            [CommandOption("-n|--name <NAME>")]
            public string? Name { get; init; }

            [Description("The output directory to place project files in.")]
            [CommandOption("-o|--output <OUTPUT_DIR>")]
            public string? Output { get; init; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] NewSettings settings)
        {
            if (string.IsNullOrEmpty(settings.Name) || NameRegex.IsMatch(settings.Name))
            {
                AnsiConsole.WriteLine($"{settings.Name} is not a valid project name.");
                return -1;
            }

            var projectDirectory = string.IsNullOrEmpty(settings.Output) ?
                Path.Combine(Directory.GetCurrentDirectory(), settings.Name) :
                Path.Combine(settings.Output, settings.Name);

            if (Directory.Exists(projectDirectory))
            {
                AnsiConsole.WriteLine($"Directory {projectDirectory} already exists.");
                return -1;
            }

            AnsiConsole.WriteLine($"Creating project directory {projectDirectory}.");
            Directory.CreateDirectory(projectDirectory);

            AnsiConsole.WriteLine($"Creating project file.");
            ProjectFileBuilder.BuildProjectFile(settings.Name, projectDirectory);

            AnsiConsole.WriteLine($"Creating service file.");
            ProjectFileBuilder.BuildServiceFile(projectDirectory);

            AnsiConsole.WriteLine($"New project created.");
            return 0;
        }
    }
}
