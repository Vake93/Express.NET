using Express.Net.Build.Services;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace Express.Net.Build.Commands
{
    public class WatchCommand : Command<WatchCommand.WatchSettings>
    {
        private static readonly AutoResetEvent autoEvent = new (false);

        public sealed class WatchSettings : CommandSettings
        {
            [Description("The input directory where source files are placed.")]
            [CommandOption("-i|--input <INPUT_DIR>")]
            public string? ProjectFolder { get; init; }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] WatchSettings settings)
        {
            using var watcher = SetupFileSystemWatcher(settings);

            while (true)
            {
                autoEvent.Reset();
                watcher.EnableRaisingEvents = true;
                RunProject(settings);
                watcher.EnableRaisingEvents = false;

                Logger.WriteHeader();
            }
        }

        private FileSystemWatcher SetupFileSystemWatcher(WatchSettings settings)
        {
            var projectFolder = Path.GetFullPath(string.IsNullOrEmpty(settings.ProjectFolder) ?
               Directory.GetCurrentDirectory() :
               settings.ProjectFolder.TrimEnd('\\', '/').Trim());

            var watcher = new FileSystemWatcher(projectFolder);

            Logger.LogInfo<WatchCommand>($"Watching: {projectFolder}");

            watcher.Filters.Add("*.en");
            watcher.Filters.Add("*.cs");
            watcher.Filters.Add("*.enproj");

            watcher.Created += FileUpdate;
            watcher.Changed += FileUpdate;
            watcher.Deleted += FileUpdate;
            watcher.Renamed += FileUpdate;

            watcher.IncludeSubdirectories = true;

            return watcher;
        }

        private void FileUpdate(object sender, FileSystemEventArgs e)
        {
            autoEvent.Set();
        }

        private static int RunProject(WatchSettings settings)
        {
            try
            {
                var result = BuildService.BuildProject(
                    settings.ProjectFolder,
                    logger: Logger.LogInfo<BuildService>);

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

                    Logger.LogInfo<Process>("Starting .NET Runtime");
                    Logger.LogInfo<Process>(string.Empty);

                    process.Start();

                    autoEvent.WaitOne();

                    process.Kill(true);

                    return process.ExitCode;
                }

                Logger.LogError<WatchCommand>("Build Error.");

                foreach (var diagnostic in result.Diagnostics)
                {
                    Logger.WriteLine(diagnostic.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError<WatchCommand>("Build Failed.");
                Logger.WriteException(ex);
            }

            autoEvent.WaitOne();
            return -1;
        }
    }
}
