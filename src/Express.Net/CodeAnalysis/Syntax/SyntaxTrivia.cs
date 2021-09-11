using Microsoft.CodeAnalysis.Text;

namespace Express.Net.CodeAnalysis.Syntax;

public sealed class SyntaxTrivia
{
    internal SyntaxTrivia(SyntaxKind kind, int position, string text)
    {
        Kind = kind;
        Position = position;
        Text = text;
    }

    public SyntaxKind Kind { get; init; }

    public int Position { get; init; }

    public string Text { get; init; }

    public TextSpan Span => new(Position, Text.Length);
}
