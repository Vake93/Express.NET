using System.Collections.Generic;
using System.Collections.Immutable;

namespace Express.Net.CodeAnalysis.Syntax.Nodes;

public record AttributeListSyntax(
    SyntaxTree SyntaxTree,
    ImmutableArray<AttributeSyntax> Attributes) : SyntaxNode(SyntaxTree)
{
    public override SyntaxKind Kind { get; init; } = SyntaxKind.AttributeList;

    public override IEnumerable<SyntaxNode> GetChildren() => Attributes;
}

public record AttributeSyntax(
    SyntaxTree SyntaxTree,
    SyntaxToken Identifier,
    AttributeArgumentListSyntax ArgumentList) : SyntaxNode(SyntaxTree)
{
    public override SyntaxKind Kind { get; init; } = SyntaxKind.Attribute;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Identifier;
        yield return ArgumentList;
    }
}

public record AttributeArgumentListSyntax(
    SyntaxTree SyntaxTree,
    ImmutableArray<AttributeArgumentSyntax> Arguments) : SyntaxNode(SyntaxTree)
{
    public override SyntaxKind Kind { get; init; } = SyntaxKind.AttributeArgumentList;

    public override IEnumerable<SyntaxNode> GetChildren() => Arguments;
}

public record AttributeArgumentSyntax(
    SyntaxTree SyntaxTree,
    SyntaxToken Expression,
    SyntaxToken? Name = null) : SyntaxNode(SyntaxTree)
{
    public override SyntaxKind Kind { get; init; } = SyntaxKind.AttributeArgument;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        if (Name is not null)
        {
            yield return Name;
        }

        yield return Expression;
    }
}
