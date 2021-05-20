using Express.Net.CodeAnalysis.Syntax;
using Express.Net.CodeAnalysis.Syntax.Nodes;
using Express.Net.CodeAnalysis.Text;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Express.Net.CodeAnalysis.Diagnostics
{
    public sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics;

        public DiagnosticBag()
        {
            _diagnostics = new List<Diagnostic>();
        }

        public DiagnosticBag(IEnumerable<Diagnostic> diagnostics)
        {
            _diagnostics = new List<Diagnostic>(diagnostics);
        }

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void ReportUnterminatedMultiLineComment(TextLocation location)
        {
            var message = "Unterminated multi-line comment.";
            ReportError(location, message);
        }

        public void ReportBadCharacter(TextLocation location, char character)
        {
            var message = $"Bad character input: '{character}'.";
            ReportError(location, message);
        }

        public void ReportUnterminatedString(TextLocation location)
        {
            var message = "Unterminated string literal.";
            ReportError(location, message);
        }

        public void ReportUnterminatedCodeBlock(TextLocation location)
        {
            var message = "Unterminated code block.";
            ReportError(location, message);
        }

        public void ReportUnexpectedToken(TextLocation location, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            var message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>.";
            ReportError(location, message);
        }

        public void ReportUnknownStatement(TextLocation location)
        {
            var message = $"Unknown statement.";
            ReportError(location, message);
        }

        public void ReportInvalidNumber(TextLocation location, string text, Type type)
        {
            var message = $"The number {text} isn't valid {type.Name}.";
            ReportError(location, message);
        }

        public void ReportInvalidDefaultValue(TextLocation location, SyntaxToken identifier)
        {
            var message = $"Default value provided for parameter {identifier.Text} is invalid.";
            ReportError(location, message);
        }

        public void ReportError(TextLocation location, string message)
        {
            var diagnostic = Diagnostic.Error(location, message);
            _diagnostics.Add(diagnostic);
        }

        public void ReportWarning(TextLocation location, string message)
        {
            var diagnostic = Diagnostic.Warning(location, message);
            _diagnostics.Add(diagnostic);
        }

        public void ReportInformation(TextLocation location, string message)
        {
            var diagnostic = Diagnostic.Information(location, message);
            _diagnostics.Add(diagnostic);
        }
    }
}
