using Express.Net.CodeAnalysis.Text;

namespace Express.Net.CodeAnalysis.Syntax;

public sealed record SyntaxTrivia(SyntaxKind Kind, int Position, string Text)
{
    public TextSpan Span => new(Position, Text.Length);
}