using Express.Net.CodeAnalysis;
using Express.Net.CodeAnalysis.Diagnostics;
using Express.Net.Emit;
using Express.Net.Emit.Bootstrapping;
using Express.Net.Models.NuGet;
using Express.Net.Packages;
using System;
using System.IO;
using System.Linq;
using GeneratedSyntaxTrees = System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.SyntaxTree>;
using CSharpSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;
using CSharp = Microsoft.CodeAnalysis.CSharp;
using System.Text;

namespace Express.Net.Build.Services
{
    internal class BuildService
    {
        public static EmitResult BuildProject(string? projectPath, string? outputPath = null, string? configuration = null, Action<string>? logger = null, bool dumpGeneratedFiles = false)
        {
            var projectFolder = Path.GetFullPath(string.IsNullOrEmpty(projectPath) ?
                Directory.GetCurrentDirectory() :
                projectPath.TrimEnd('\\', '/').Trim());

            var buildConfiguration = string.IsNullOrEmpty(configuration) ? "Debug" : configuration;

            var executableFolder = Path.GetFullPath(string.IsNullOrEmpty(outputPath) ?
                Path.Combine(projectFolder, "bin", buildConfiguration) :
                outputPath);

            var projectFile = SourceFileDiscovery.GetProjectFileInDirectory(projectFolder);
            var projectName = Path.GetFileNameWithoutExtension(projectFile);

            if (string.IsNullOrEmpty(projectFile))
            {
                throw new Exception("No project file found");
            }

            var project = ProjectFileHandler.ReadProjectFile(projectFile);

            if (project is null)
            {
                throw new Exception("Unable to read project file");
            }

            Directory.CreateDirectory(executableFolder);

            var compilation = new ExpressNetCompilation(projectName, projectFolder, executableFolder, buildConfiguration)
                .SetTargetFrameworks(TargetFrameworks.NetCore50, TargetFrameworks.AspNetCore50, TargetFrameworks.ExpressNet)
                .SetBootstrapper(new BasicBootstrapper(projectName, project.GenerateSwaggerDoc, project.AddSwaggerUI));

            var packageAssemblies = Enumerable.Empty<PackageAssembly>();

            if (project.PackageReferences != null && project.PackageReferences.Length > 0)
            {
                logger?.Invoke($"Restore NuGet Packages for {projectName}");

                var nugetClient = new NuGetClient(project, buildConfiguration, projectFolder);

                packageAssemblies = nugetClient
                    .RestoreProjectDependenciesAsync()
                    .GetAwaiter()
                    .GetResult();

                compilation = compilation.SetPackageAssemblies(packageAssemblies.ToArray());
            }

            logger?.Invoke($"Build starting for {projectName}");

            var sourceFiles = SourceFileDiscovery.GetSourceFilesInDirectory(projectFolder);

            if (sourceFiles.Length == 0)
            {
                throw new Exception("Unable locate any source files");
            }

            compilation = compilation.SetSyntaxTrees(ParseSourceFiles(sourceFiles, logger));

            var csharpSourceFiles = SourceFileDiscovery.GetCSharpSourceFilesInDirectory(projectFolder);

            if (csharpSourceFiles.Length > 0)
            {
                compilation = compilation.SetCSharpSyntaxTrees(ParseCSharpSourceFiles(csharpSourceFiles, logger));
            }

            logger?.Invoke($"Building Runtime Config");

            RuntimeConfigBuilder.BuildRuntimeConfig(projectName, executableFolder, TargetFrameworks.ExpressNet);

            logger?.Invoke($"Emiting IL");

            var result = compilation.Emit();

            if (result.Success)
            {
                logger?.Invoke("Emiting IL complete");

                logger?.Invoke("Copying express framework assemblies");
                ExpressDependencies.CopyFrameworkAssemblies(project, executableFolder);

                if (packageAssemblies.Any())
                {
                    logger?.Invoke("Copying package assemblies");
                    ExpressDependencies.CopyPackageAssemblies(packageAssemblies, projectFolder, executableFolder);
                }
            }

            if (dumpGeneratedFiles)
            {
                logger?.Invoke("Saving generated C# files");

                SaveGeneratedCSharpFiles(outputPath, projectFolder, result.GeneratedSyntaxTrees);
            }

            return result;
        }

        private static SyntaxTree[] ParseSourceFiles(string[] sourceFiles, Action<string>? logger)
        {
            var syntaxTrees = new SyntaxTree[sourceFiles.Length];

            for (var i = 0; i < sourceFiles.Length; i++)
            {
                var sourceFile = sourceFiles[i];
                var sourceFileName = Path.GetFileName(sourceFile);

                logger?.Invoke($"Parsing file {sourceFileName}");
                var syntaxTree = SyntaxTree.FromFile(sourceFile);

                var hasErrors = false;

                foreach (var diagnostic in syntaxTree.Diagnostics)
                {
                    hasErrors = diagnostic.DiagnosticType == DiagnosticType.Error;

                    logger?.Invoke($"{diagnostic.DiagnosticType}: {diagnostic.Message} @ {diagnostic.Location}");
                }

                if (hasErrors)
                {
                    throw new Exception($"Syntax errors in {sourceFileName}");
                }

                syntaxTrees[i] = syntaxTree;
            }

            return syntaxTrees;
        }

        private static CSharpSyntaxTree[] ParseCSharpSourceFiles(string[] sourceFiles, Action<string>? logger)
        {
            var syntaxTrees = new CSharpSyntaxTree[sourceFiles.Length];

            for (var i = 0; i < sourceFiles.Length; i++)
            {
                var sourceFile = sourceFiles[i];
                var sourceFileName = Path.GetFileName(sourceFile);

                logger?.Invoke($"Parsing file {sourceFileName}");
                var sourceText = File.ReadAllText(sourceFile);
                var syntaxTree = CSharp.CSharpSyntaxTree.ParseText(sourceText, path: sourceFile, encoding: Encoding.UTF8);

                var hasErrors = false;

                foreach (var diagnostic in syntaxTree.GetDiagnostics())
                {
                    hasErrors = diagnostic.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error;

                    logger?.Invoke($"{diagnostic.Severity}: {diagnostic.GetMessage()} @ {diagnostic.Location}");
                }

                if (hasErrors)
                {
                    throw new Exception($"Syntax errors in {sourceFileName}");
                }

                syntaxTrees[i] = syntaxTree;
            }

            return syntaxTrees;
        }

        private static void SaveGeneratedCSharpFiles(string? outputPath, string projectFolder, GeneratedSyntaxTrees? generatedSyntaxTrees)
        {
            if (!generatedSyntaxTrees.HasValue)
            {
                return;
            }

            var generatedSourceFolder = Path.GetFullPath(string.IsNullOrEmpty(outputPath) ?
                Path.Combine(projectFolder, "obj", "Generated") :
                Path.Combine(outputPath, "Generated"));

            Directory.CreateDirectory(generatedSourceFolder);

            foreach (var generatedSyntaxTree in generatedSyntaxTrees)
            {
                var fileName = Path.GetFileNameWithoutExtension(generatedSyntaxTree.FilePath);
                var filePath = Path.Combine(generatedSourceFolder, $"{fileName}.cs");
                var text = generatedSyntaxTree.ToString();

                File.WriteAllText(filePath, text);
            }
        }

    }
}
