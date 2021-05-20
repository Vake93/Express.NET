using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public class NamespaceSyntax : SyntaxNode
    {
        public NamespaceSyntax(SyntaxTree syntaxTree, ImmutableArray<SyntaxToken> qualifiedNameParts, SyntaxToken? alias = null)
            : base(syntaxTree)
        {
            Alias = alias;
            Kind = SyntaxKind.Namespace;
            QualifiedNameParts = qualifiedNameParts;
        }

        public override SyntaxKind Kind { get; init; }

        public SyntaxToken? Alias { get; init; }

        public ImmutableArray<SyntaxToken> QualifiedNameParts { get; init; }

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

        public override string ToString() => $"Namespace {Name}";
    }
}
