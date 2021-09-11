using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Express.Net.CodeAnalysis.Syntax.Nodes;

public sealed class SyntaxToken : SyntaxNode
{
    internal SyntaxToken(SyntaxTree syntaxTree, SyntaxKind kind)
        : base(syntaxTree)
    {
        Kind = kind;
    }

    public override SyntaxKind Kind { get; init; }

    public int Position { get; init; }

    public string? Text { get; init; }

    public object? Value { get; init; }

    public override TextSpan Span => new(Position, Text?.Length ?? 0);

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