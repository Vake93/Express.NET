using Express.Net.Build.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Express.Net.Build.Commands
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
            try
            {
                var result = BuildService.BuildProject(
                    settings.ProjectFolder,
                    settings.Output,
                    settings.Configuration,
                    AnsiConsole.WriteLine);

                if (result.Success)
                {
                    return 0;
                }

                AnsiConsole.WriteLine("Build Error.");

                foreach (var diagnostic in result.Diagnostics)
                {
                    AnsiConsole.WriteLine(diagnostic.Message);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine("Build Failed.");
                AnsiConsole.WriteException(ex);
            }

            return -1;
        }
    }
}
