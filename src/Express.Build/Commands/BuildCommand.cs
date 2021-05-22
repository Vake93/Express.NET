using Express.Build.Services;
using Express.Net;
using Express.Net.CodeAnalysis;
using Express.Net.Emit;
using Express.Net.Emit.Bootstrapping;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Express.Build.Commands
{
    internal sealed class BuildCommand : Command<BuildCommand.BuildSettings>
    {
        public sealed class BuildSettings : CommandSettings
        {
            [Description("The configuration to use for building the project. Can be 'Debug' or 'Release'.")]
            [CommandOption("-c|--configuration <CONFIGURATION>")]
            public string? Configuration { get; init; }

            [Description("The output directory to place built artifacts in.")]
            [CommandOption("-o|--output <OUTPUT_DIR>")]
            public string? Output { get; init; }

            [Description("The input directory where source files are placed.")]
            [CommandOption("-i|--input <INPUT_DIR>")]
            public string? ProjectFolder { get; init; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] BuildSettings settings)
        {
            var projectFolder = string.IsNullOrEmpty(settings.ProjectFolder) ?
                Directory.GetCurrentDirectory() :
                settings.ProjectFolder.TrimEnd('\\', '/').Trim();

            var configuration = string.IsNullOrEmpty(settings.Configuration) ?
                "Debug" :
                settings.Configuration;

            var output = string.IsNullOrEmpty(settings.Output) ?
                Path.Combine(projectFolder, "bin", configuration) :
                settings.Output;

            var projectFile = SourceFileDiscovery.GetProjectFileInDirectory(projectFolder);

            if (string.IsNullOrEmpty(projectFile))
            {
                AnsiConsole.WriteLine("No project file found.");
                return -1;
            }

            output = Path.GetFullPath(output);

            if (!Directory.Exists(output))
            {
                Directory.CreateDirectory(output);
            }

            var projectName = Path.GetFileNameWithoutExtension(projectFile);
            var sourceFiles = SourceFileDiscovery.GetSourceFilesInDirectory(projectFolder);

            AnsiConsole.WriteLine($"Build starting for {projectName}");

            var compilation = new ExpressNetCompilation(projectName, output, configuration)
                .SetTargetFrameworks(TargetFrameworks.NetCore50, TargetFrameworks.AspNetCore50)
                .SetBootstrapper(BasicBootstrapper.Instance);

            var syntaxTrees = new SyntaxTree[sourceFiles.Length];

            for (var i = 0; i < sourceFiles.Length; i++)
            {
                var sourceFile = sourceFiles[i];
                AnsiConsole.WriteLine($"Parsing file {sourceFile}");
                syntaxTrees[i] = SyntaxTree.FromFile(sourceFile);
            }

            AnsiConsole.WriteLine($"Building Runtime Config");

            RuntimeConfigBuilder.BuildRuntimeConfig(
                projectName,
                output,
                TargetFrameworks.AspNetCore50);

            AnsiConsole.WriteLine($"Emiting IL");

            var result = compilation
                .SetSyntaxTrees(syntaxTrees)
                .Emit();

            if (result.Success)
            {
                AnsiConsole.WriteLine("Build completed successfully.");
            }
            else
            {
                AnsiConsole.WriteLine("Build Error.");

                foreach (var diagnostic in result.Diagnostics)
                {
                    AnsiConsole.WriteLine(diagnostic.Message);
                }
            }

            return 0;
        }
    }
}
