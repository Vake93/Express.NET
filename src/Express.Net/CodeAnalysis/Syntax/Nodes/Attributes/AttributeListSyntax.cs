using System.Collections.Immutable;

namespace Express.Net.CodeAnalysis.Syntax.Nodes.Attributes;

public class AttributeListSyntax : SyntaxNode
{
    public AttributeListSyntax(SyntaxTree syntaxTree, IEnumerable<AttributeSyntax> attributes)
        : base(syntaxTree)
    {
        Kind = SyntaxKind.AttributeList;

        var builder = ImmutableArray.CreateBuilder<AttributeSyntax>();
        builder.AddRange(attributes);

        Attributes = builder.ToImmutable();
    }

    public override SyntaxKind Kind { get; init; }

    public ImmutableArray<AttributeSyntax> Attributes { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => Attributes;
}