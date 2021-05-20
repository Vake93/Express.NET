using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public class TypeClauseSyntax : SyntaxNode
    {
        public TypeClauseSyntax(SyntaxTree syntaxTree, ImmutableArray<SyntaxToken> identifiers)
            : base(syntaxTree)
        {
            Kind = SyntaxKind.TypeClause;
            Identifiers = identifiers;

            var typeNameBuilder = new StringBuilder();

            foreach (var identifier in identifiers)
            {
                typeNameBuilder.Append(identifier.Text);
            }

            TypeName = typeNameBuilder.ToString();
        }

        public override SyntaxKind Kind { get; init; }

        public ImmutableArray<SyntaxToken> Identifiers { get; init; }

        public string TypeName { get; init; }

        public override IEnumerable<SyntaxNode> GetChildren() => Identifiers;

        public override string ToString() => $"Type {TypeName}";
    }
}
