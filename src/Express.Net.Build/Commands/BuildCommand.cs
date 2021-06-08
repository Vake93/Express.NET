﻿using Express.Net.Build.Services;
using Express.Net.CodeAnalysis;
using Express.Net.Emit;
using Express.Net.Emit.Bootstrapping;
using Express.Net.Models;
using Express.Net.Models.NuGet;
using Express.Net.Packages;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;

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
            var projectFolder = string.IsNullOrEmpty(settings.ProjectFolder) ?
                Directory.GetCurrentDirectory() :
                settings.ProjectFolder.TrimEnd('\\', '/').Trim();

            projectFolder = Path.GetFullPath(projectFolder);

            var configuration = string.IsNullOrEmpty(settings.Configuration) ?
                "Debug" :
                settings.Configuration;

            var output = string.IsNullOrEmpty(settings.Output) ?
                Path.Combine(projectFolder, "bin", configuration) :
                settings.Output;

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

                    File.Copy(from, to);
                }
            }

            AnsiConsole.WriteLine("Build completed successfully.");

            return 0;
        }
    }
}