using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Express.Net.CodeAnalysis.Syntax.Nodes;

public record NamespaceSyntax(
    SyntaxTree SyntaxTree,
    ImmutableArray<SyntaxToken> QualifiedNameParts,
    SyntaxToken? Alias = null) : SyntaxNode(SyntaxTree)
{
    public override SyntaxKind Kind { get; init; } = SyntaxKind.Namespace;

    public string Name => string.Join('.', QualifiedNameParts.Select(i => i.Text));

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        if (Alias is not null)
        {
            yield return Alias;
        }

        foreach (var qualifiedNamePart in QualifiedNameParts)
        {
            yield return qualifiedNamePart;
        }
    }

    public override string ToString() => "Namespace";
}
