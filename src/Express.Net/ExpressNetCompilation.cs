using Express.Net.CodeAnalysis.Diagnostics;
using Express.Net.CodeAnalysis.Text;
using Express.Net.Emit;
using Express.Net.Emit.Bootstrapping;
using Express.Net.Models.NuGet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using CSharpSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;
using Diagnostic = Express.Net.CodeAnalysis.Diagnostics.Diagnostic;
using SyntaxTree = Express.Net.CodeAnalysis.SyntaxTree;

namespace Express.Net
{
    public sealed class ExpressNetCompilation
    {
        private readonly string _projectName;
        private readonly string _projectPath;
        private readonly string _output;
        private readonly string _configuration;

        private SyntaxTree[]? _syntaxTrees;
        private TargetFramework[]? _targetFrameworks;
        private Bootstrapper? _bootstrapper;
        private PackageAssembly[]? _packageAssemblies;

        public ExpressNetCompilation(string projectName, string projectPath, string output, string configuration)
        {
            _output = output;
            _projectName = projectName;
            _projectPath = projectPath;
            _configuration = configuration;
        }

        public ExpressNetCompilation SetSyntaxTrees(params SyntaxTree[] syntaxTrees)
        {
            _syntaxTrees = syntaxTrees;
            return this;
        }

        public ExpressNetCompilation SetBootstrapper(Bootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
            return this;
        }

        public ExpressNetCompilation SetTargetFrameworks(params TargetFramework[] targetFrameworks)
        {
            _targetFrameworks = targetFrameworks;
            return this;
        }

        public ExpressNetCompilation SetPackageAssemblies(params PackageAssembly[] packageAssemblies)
        {
            _packageAssemblies = packageAssemblies;
            return this;
        }

        public EmitResult Emit()
        {
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            var validateOk = ValidateCompilation(diagnostics);

            if (!validateOk)
            {
                return new EmitResult(false, diagnostics.ToImmutable());
            }

            if (_bootstrapper is null)
            {
                throw new ArgumentNullException(nameof(_bootstrapper));
            }

            var bootstrapper = _bootstrapper.GetBootstrapper();
            var configuration = ParseConfiguration();

            var syntaxTrees = TransformSyntaxTrees(diagnostics, configuration)
                .Append(bootstrapper);

            if (diagnostics.Where(d => d.DiagnosticType == DiagnosticType.Error).Any())
            {
                return new EmitResult(false, diagnostics.ToImmutable());
            }

            var references = BuildReferences();
            var assemblyName = $"{_projectName}.dll";
            var pdbName = $"{_projectName}.pdb";

            var compilation = CSharpCompilation
                .Create(_projectName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication)
                .WithOptimizationLevel(configuration)
                .WithPlatform(Platform.AnyCpu))
                .WithReferences(references)
                .AddSyntaxTrees(syntaxTrees);

            var result = compilation.Emit(
                Path.Combine(_output, assemblyName),
                Path.Combine(_output, pdbName));

            foreach (var diagnostic in result.Diagnostics)
            {
                diagnostics.Add(Diagnostic.FromCSharpDiagnostic(diagnostic));
            }

            return new EmitResult(result.Success, diagnostics.ToImmutable(), _output, assemblyName, syntaxTrees.ToImmutableArray());
        }

        private bool ValidateCompilation(ImmutableArray<Diagnostic>.Builder diagnostics)
        {
            var error = false;

            if (!Directory.Exists(_output))
            {
                diagnostics.Add(Diagnostic.Error(new TextLocation(), "Invalid output directory."));
                error = true;
            }

            if (string.IsNullOrEmpty(_projectName))
            {
                diagnostics.Add(Diagnostic.Error(new TextLocation(), "Invalid assembly name."));
                error = true;
            }

            if (_syntaxTrees is null || _syntaxTrees.Length == 0)
            {
                diagnostics.Add(Diagnostic.Error(new TextLocation(), "At least one syntax tree is required."));
                error = true;
            }

            if (_targetFrameworks is null || _targetFrameworks.Length == 0)
            {
                diagnostics.Add(Diagnostic.Error(new TextLocation(), "At least one target framework is required."));
                error = true;
            }

            if (_bootstrapper is null)
            {
                diagnostics.Add(Diagnostic.Error(new TextLocation(), "Bootstrapper not set."));
                error = true;
            }

            return !error;
        }

        private CSharpSyntaxTree[] TransformSyntaxTrees(ImmutableArray<Diagnostic>.Builder diagnostics, OptimizationLevel configuration)
        {
            var csharpSyntaxTrees = new CSharpSyntaxTree[_syntaxTrees!.Length];

            for (var i = 0; i < csharpSyntaxTrees.Length; i++)
            {
                var _syntaxTree = _syntaxTrees[i];
                var addDebugInfo = configuration == OptimizationLevel.Debug;
                csharpSyntaxTrees[i] = _syntaxTree.Transform(_projectName, addDebugInfo, out var transformDiagnostics);

                if (transformDiagnostics.Any())
                {
                    diagnostics.AddRange(transformDiagnostics);
                }
            }

            return csharpSyntaxTrees;
        }

        private OptimizationLevel ParseConfiguration()
        {
            _ = Enum.TryParse<OptimizationLevel>(_configuration, out var optimizationLevel);
            return optimizationLevel;
        }

        private IEnumerable<MetadataReference> BuildReferences()
        {
            var references = _targetFrameworks!.SelectMany(t => t).ToList();

            if (_packageAssemblies?.Any() ?? false)
            {
                var assemblyFiles = _packageAssemblies.SelectMany(pa => pa.PackageFiles);

                foreach (var assemblyFile in assemblyFiles)
                {
                    references.Add(MetadataReference.CreateFromFile(Path.Combine(_projectPath, assemblyFile)));
                }
            }

            return references;
        }
    }
}
