using Express.Net.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public sealed class SyntaxToken : SyntaxNode
    {
        internal SyntaxToken(
            SyntaxTree syntaxTree,
            SyntaxKind kind,
            int position,
            string? text,
            object? value,
            ImmutableArray<SyntaxTrivia> leadingTrivia,
            ImmutableArray<SyntaxTrivia> trailingTrivia)
            : base(syntaxTree)
        {
            Kind = kind;
            Position = position;
            Text = text ?? string.Empty;
            Value = value;
            LeadingTrivia = leadingTrivia;
            TrailingTrivia = trailingTrivia;
        }

        public override SyntaxKind Kind { get; init; }

        public int Position { get; init; }

        public string Text { get; init; }

        public object? Value { get; init; }

        public override TextSpan Span => new (Position, Text.Length);

        public bool IsMissing => string.IsNullOrEmpty(Text);

        public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; }

        public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; }

        public override TextSpan FullSpan
        {
            get
            {
                var start = LeadingTrivia.Length == 0 ?
                    Span.Start :
                    LeadingTrivia.First().Span.Start;

                var end = TrailingTrivia.Length == 0 ?
                    Span.End :
                    TrailingTrivia.Last().Span.End;

                return TextSpan.FromBounds(start, end);
            }
        }

        public override IEnumerable<SyntaxNode> GetChildren() => Array.Empty<SyntaxNode>();
    }
}
