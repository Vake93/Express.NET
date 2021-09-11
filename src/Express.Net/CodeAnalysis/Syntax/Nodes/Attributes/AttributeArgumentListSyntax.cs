using System.Collections.Immutable;

namespace Express.Net.CodeAnalysis.Syntax.Nodes.Attributes;

public class AttributeArgumentListSyntax : SyntaxNode
{
    public AttributeArgumentListSyntax(SyntaxTree syntaxTree, IEnumerable<AttributeArgumentSyntax> arguments)
        : base(syntaxTree)
    {
        Kind = SyntaxKind.AttributeArgumentList;

        var builder = ImmutableArray.CreateBuilder<AttributeArgumentSyntax>();
        builder.AddRange(arguments);

        Arguments = builder.ToImmutable();
    }

    public override SyntaxKind Kind { get; init; }

    public ImmutableArray<AttributeArgumentSyntax> Arguments { get; init; }

    public override IEnumerable<SyntaxNode> GetChildren() => Arguments;
}