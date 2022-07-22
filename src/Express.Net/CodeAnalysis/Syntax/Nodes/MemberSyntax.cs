using System.Collections.Generic;
using System.Collections.Immutable;

namespace Express.Net.CodeAnalysis.Syntax.Nodes;

public abstract record MemberSyntax(
    SyntaxTree SyntaxTree) : SyntaxNode(SyntaxTree);

public record UnknownStatementSyntax(
    SyntaxTree SyntaxTree,
    ImmutableArray<SyntaxToken> Tokens) : MemberSyntax(SyntaxTree)
{
    public override SyntaxKind Kind { get; init; } = SyntaxKind.UnknownStatement;

    public override IEnumerable<SyntaxNode> GetChildren() => Tokens;

    public override string ToString() => "Unknown Statement";
}

public record UsingDirectiveSyntax(
    SyntaxTree SyntaxTree) : MemberSyntax(SyntaxTree)
{
    public override SyntaxKind Kind { get; init; } = SyntaxKind.UsingDirective;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield break;
    }

    public override string ToString() => "Using Directive";
}