using Express.Net.Build.Services;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Express.Net.Build.Commands
{
    internal sealed class InspectCommand : Command<InspectCommand.InspectSettings>
    {
        public sealed class InspectSettings : CommandSettings
        {
            [Description("The output directory to place built artifacts in.")]
            [CommandOption("-o|--output <OUTPUT_DIR>")]
            public string? Output { get; init; }

            [Description("The input directory where source files are placed.")]
            [CommandOption("-i|--input <INPUT_DIR>")]
            public string? ProjectFolder { get; init; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] InspectSettings settings)
        {
            try
            {
                var result = BuildService.BuildProject(
                    settings.ProjectFolder,
                    settings.Output,
                    "Release",
                    logger: Logger.LogInfo<BuildService>,
                    dumpGeneratedFiles: true);

                if (result.Success)
                {
                    return 0;
                }

                Logger.LogError<InspectCommand>("Build Error.");

                foreach (var diagnostic in result.Diagnostics)
                {
                    Logger.WriteLine(diagnostic.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError<InspectCommand>("Build Failed.");
                Logger.WriteException(ex);
            }

            return -1;
        }
    }
}
