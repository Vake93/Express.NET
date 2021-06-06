using Express.Net.Build.Services;
using Express.Net.CodeAnalysis;
using Express.Net.Emit;
using Express.Net.Emit.Bootstrapping;
using Express.Net.Models;
using Express.Net.Models.NuGet;
using Express.Net.Packages;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;

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
            var projectFolder = string.IsNullOrEmpty(settings.ProjectFolder) ?
                Directory.GetCurrentDirectory() :
                settings.ProjectFolder.TrimEnd('\\', '/').Trim();

            projectFolder = Path.GetFullPath(projectFolder);

            var configuration = "Debug";

            var output = Path.Combine(projectFolder, "bin", configuration);

            output = Path.GetFullPath(output);

            var projectFile = SourceFileDiscovery.GetProjectFileInDirectory(projectFolder);

            if (string.IsNullOrEmpty(projectFile))
            {
                AnsiConsole.WriteLine("No project file found.");
                return -1;
            }

            var projectJson = File.ReadAllText(projectFile);
            var project = JsonSerializer.Deserialize<Project>(projectJson);

            if (project is null)
            {
                AnsiConsole.WriteLine("Unable to read project file.");
                return -1;
            }

            if (!Directory.Exists(output))
            {
                Directory.CreateDirectory(output);
            }

            var projectName = Path.GetFileNameWithoutExtension(projectFile);
            var bootstrapper = new BasicBootstrapper(project.GenerateSwaggerDoc, project.AddSwaggerUI);
            var compilation = new ExpressNetCompilation(projectName, projectFolder, output, configuration)
                .SetTargetFrameworks(TargetFrameworks.NetCore50, TargetFrameworks.AspNetCore50)
                .SetBootstrapper(bootstrapper);

            var packageAssemblies = Enumerable.Empty<PackageAssembly>();

            if ((project.PackageReferences != null && project.PackageReferences.Any()) || bootstrapper.PackageReferences.Any())
            {
                AnsiConsole.WriteLine($"Restore NuGet Packages for {projectName}");

                var nugetClient = new NuGetClient(project, bootstrapper, configuration, projectFolder);
                packageAssemblies = nugetClient
                    .RestoreProjectDependenciesAsync()
                    .GetAwaiter()
                    .GetResult();

                compilation = compilation.SetPackageAssemblies(packageAssemblies.ToArray());
            }

            AnsiConsole.WriteLine($"Build starting for {projectName}");

            var sourceFiles = SourceFileDiscovery.GetSourceFilesInDirectory(projectFolder);

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

            if (!result.Success)
            {
                AnsiConsole.WriteLine("Build Error.");

                foreach (var diagnostic in result.Diagnostics)
                {
                    AnsiConsole.WriteLine(diagnostic.Message);
                }

                return -1;
            }

            if (packageAssemblies.Any())
            {
                AnsiConsole.WriteLine("Copying package assemblies");

                var assemblyFiles = packageAssemblies.SelectMany(pa => pa.PackageFiles);

                foreach (var assemblyFile in assemblyFiles)
                {
                    var name = Path.GetFileName(assemblyFile);
                    var from = Path.Combine(projectFolder, assemblyFile);
                    var to = Path.Combine(output, name);

                    File.Copy(from, to, overwrite: true);
                }
            }

            using var process = new Process();
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = $"{projectName}.dll";
            process.StartInfo.WorkingDirectory = output;

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = false;
            process.StartInfo.RedirectStandardInput = false;
            process.StartInfo.RedirectStandardOutput = false;

            AnsiConsole.WriteLine($"Starting {projectName}");
            AnsiConsole.WriteLine();

            process.Start();

            process.WaitForExit();

            return process.ExitCode;
        }

    }
}
