using System.Collections.Generic;
using System.Collections.Immutable;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public class AttributeListSyntax : SyntaxNode
    {
        public AttributeListSyntax(SyntaxTree syntaxTree, ImmutableArray<AttributeSyntax> attributes)
            : base(syntaxTree)
        {
            Kind = SyntaxKind.AttributeList;
            Attributes = attributes;
        }

        public override SyntaxKind Kind { get; init; }

        public ImmutableArray<AttributeSyntax> Attributes { get; init; }

        public override IEnumerable<SyntaxNode> GetChildren() => Attributes;
    }
}
