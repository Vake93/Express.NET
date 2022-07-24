using Express.Net.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Express.Net.CodeAnalysis.Syntax.Nodes;

public sealed record SyntaxToken(
    SyntaxTree SyntaxTree,
    SyntaxKind Kind,
    int Position,
    string? Text,
    object? Value,
    ImmutableArray<SyntaxTrivia> LeadingTrivia,
    ImmutableArray<SyntaxTrivia> TrailingTrivia) : SyntaxNode(SyntaxTree)
{
    public SyntaxToken(
        SyntaxTree syntaxTree,
        SyntaxKind kind,
        int position) : this(syntaxTree, kind, position, null, null, ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty)
    {
    }

    public override TextSpan Span => new(Position, Text?.Length ?? 0);

    public bool IsMissing => string.IsNullOrEmpty(Text);

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