using Express.Net.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Express.Net.Emit
{
    public sealed class EmitResult
    {
        internal EmitResult(bool success, ImmutableArray<Diagnostic> diagnostics, string? outputFolder = null, string? binaryFileName = null)
        {
            Success = success;
            Diagnostics = new DiagnosticBag(diagnostics);

            if (success)
            {
                OutputFolder = outputFolder ?? throw new ArgumentNullException(nameof(outputFolder));
                BinaryFileName = binaryFileName ?? throw new ArgumentNullException(nameof(binaryFileName));
            }
        }

        [MemberNotNullWhen(true, nameof(BinaryFileName), nameof(OutputFolder))]
        public bool Success { get; }

        public string? BinaryFileName { get; }

        public string? OutputFolder { get; }

        public DiagnosticBag Diagnostics { get; }
    }
}