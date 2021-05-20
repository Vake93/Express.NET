using Express.Net.CodeAnalysis.Text;
using Express.Net.Emit;
using Express.Net.Emit.Bootstrapping;
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
        private readonly string _output;
        private readonly string _configuration;

        private SyntaxTree[]? _syntaxTrees;
        private TargetFramework[]? _targetFrameworks;
        private Bootstrapper? _bootstrapper;

        public ExpressNetCompilation(string projectName, string output, string configuration)
        {
            _output = output;
            _projectName = projectName;
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

        public EmitResult Emit()
        {
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            var exit = ValidateCompilation(diagnostics);

            if (exit)
            {
                return new EmitResult(false, diagnostics.ToImmutable());
            }

            var csharpSyntaxTrees = TransformSyntaxTrees(diagnostics, ref exit);

            if (exit)
            {
                return new EmitResult(false, diagnostics.ToImmutable());
            }

            var configuration = ParseConfiguration();
            var references = BuildReferences();
            var assemblyName = $"{_projectName}.dll";
            var pdbName = $"{_projectName}.pdb";

            var compilation = CSharpCompilation
                .Create(assemblyName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication)
                .WithOptimizationLevel(configuration)
                .WithPlatform(Platform.AnyCpu))
                .WithReferences(references)
                .AddSyntaxTrees(csharpSyntaxTrees)
                .AddSyntaxTrees(_bootstrapper!.GetBootstrapper());

            var result = compilation.Emit(
                Path.Combine(_output, assemblyName),
                Path.Combine(_output, pdbName));


            foreach (var diagnostic in result.Diagnostics)
            {
                diagnostics.Add(Diagnostic.FromCSharpDiagnostic(diagnostic));
            }

            return new EmitResult(result.Success, diagnostics.ToImmutable());
        }

        private bool ValidateCompilation(ImmutableArray<Diagnostic>.Builder diagnostics)
        {
            var exit = false;

            if (!Directory.Exists(_output))
            {
                diagnostics.Add(Diagnostic.Error(new TextLocation(), "Invalid output directory."));
                exit = true;
            }

            if (string.IsNullOrEmpty(_projectName))
            {
                diagnostics.Add(Diagnostic.Error(new TextLocation(), "Invalid assembly name."));
                exit = true;
            }

            if (_syntaxTrees is null || _syntaxTrees.Length == 0)
            {
                diagnostics.Add(Diagnostic.Error(new TextLocation(), "At least one syntax tree is required."));
                exit = true;
            }

            if (_targetFrameworks is null || _targetFrameworks.Length == 0)
            {
                diagnostics.Add(Diagnostic.Error(new TextLocation(), "At least one target framework is required."));
                exit = true;
            }

            if (_bootstrapper is null)
            {
                diagnostics.Add(Diagnostic.Error(new TextLocation(), "Bootstrapper not set."));
                exit = true;
            }

            return exit;
        }

        private CSharpSyntaxTree[] TransformSyntaxTrees(ImmutableArray<Diagnostic>.Builder diagnostics, ref bool exit)
        {
            var csharpSyntaxTrees = new CSharpSyntaxTree[_syntaxTrees!.Length];

            for (var i = 0; i < csharpSyntaxTrees.Length; i++)
            {
                var _syntaxTree = _syntaxTrees[i];
                csharpSyntaxTrees[i] = _syntaxTree.Transform(out var transformDiagnostics);

                if (transformDiagnostics.Any())
                {
                    diagnostics.AddRange(transformDiagnostics);

                    exit = true;
                    break;
                }
            }

            return csharpSyntaxTrees;
        }

        private OptimizationLevel ParseConfiguration()
        {
            _ = Enum.TryParse<OptimizationLevel>(_configuration, out var optimizationLevel);
            return optimizationLevel;
        }

        private IEnumerable<MetadataReference> BuildReferences() => _targetFrameworks!.SelectMany(t => t);
    }
}
