using Express.Net.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Express.Net.Emit
{
    public sealed class EmitResult
    {
        internal EmitResult(bool success, ImmutableArray<Diagnostic> diagnostics)
        {
            Success = success;
            Diagnostics = new DiagnosticBag(diagnostics);
        }

        public bool Success { get; }

        public DiagnosticBag Diagnostics { get; }
    }
}
