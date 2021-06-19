using Express.Net.Build.Services;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Express.Net.Build.Commands
{
    internal sealed class RunCommand : Command<RunCommand.RunSettings>
    {
        public sealed class RunSettings : CommandSettings
        {
            [Description("The input directory where source files are placed.")]
            [CommandOption("-i|--input <INPUT_DIR>")]
            public string? ProjectFolder { get; init; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] RunSettings settings)
        {
            try
            {
                var result = BuildService.BuildProject(
                    settings.ProjectFolder,
                    logger: Logger.LogInfo);

                if (result.Success)
                {
                    using var process = new Process();
                    process.StartInfo.FileName = "dotnet";
                    process.StartInfo.Arguments = result.BinaryFileName;
                    process.StartInfo.WorkingDirectory = result.OutputFolder;

                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = false;
                    process.StartInfo.RedirectStandardInput = false;
                    process.StartInfo.RedirectStandardOutput = false;

                    Logger.LogInfo("Starting .NET Runtime");
                    Logger.LogInfo(string.Empty);

                    Logger.LogInfo($"[bold]dotnet [italic]{result.BinaryFileName}[/][/]");

                    process.Start();

                    process.WaitForExit();

                    return process.ExitCode;
                }

                Logger.LogError("Build Error.");

                foreach (var diagnostic in result.Diagnostics)
                {
                    Logger.WriteLine(diagnostic.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Build Failed.");
                Logger.WriteException(ex);
            }

            return -1;
        }
    }
}
