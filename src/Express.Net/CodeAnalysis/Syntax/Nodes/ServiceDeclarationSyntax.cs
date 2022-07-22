using System.Collections.Generic;

namespace Express.Net.CodeAnalysis.Syntax.Nodes;

public record ServiceDeclarationSyntax(
    SyntaxTree SyntaxTree,
    SyntaxToken Keyword,
    SyntaxToken Route,
    SyntaxToken Identifier,
    AttributeListSyntax? AttributeList) : MemberSyntax(SyntaxTree)
{
    public override SyntaxKind Kind { get; init; } = SyntaxKind.ServiceDeclaration;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        if (AttributeList is not null)
        {
            yield return AttributeList;
        }

        yield return Keyword;

        yield return Route;

        yield return Identifier;
    }

    public override string ToString() => "Service Declaration";
}
