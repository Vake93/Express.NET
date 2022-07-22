using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Express.Net.CodeAnalysis.Syntax.Nodes;

public record TypeClauseSyntax(
    SyntaxTree SyntaxTree,
    ImmutableArray<SyntaxToken> Identifiers) : SyntaxNode(SyntaxTree)
{
    private string? typeName;

    public override SyntaxKind Kind { get; init; } = SyntaxKind.TypeClause;

    public string TypeName
    {
        get
        {
            if (string.IsNullOrEmpty(typeName))
            {
                var typeNameBuilder = new StringBuilder();

                foreach (var identifier in Identifiers)
                {
                    typeNameBuilder.Append(identifier.Text);
                }

                typeName = typeNameBuilder.ToString();
            }

            return typeName;
        }
    }

    public override IEnumerable<SyntaxNode> GetChildren() => Identifiers;

    public override string ToString() => $"Type {TypeName}";
}