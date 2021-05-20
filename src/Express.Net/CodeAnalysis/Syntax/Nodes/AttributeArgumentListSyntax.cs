using System.Collections.Generic;
using System.Collections.Immutable;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public class AttributeArgumentListSyntax : SyntaxNode
    {
        public AttributeArgumentListSyntax(SyntaxTree syntaxTree, ImmutableArray<AttributeArgumentSyntax> arguments)
            : base(syntaxTree)
        {
            Kind = SyntaxKind.AttributeArgumentList;
            Arguments = arguments;
        }

        public override SyntaxKind Kind { get; init; }

        public ImmutableArray<AttributeArgumentSyntax> Arguments { get; init; }

        public override IEnumerable<SyntaxNode> GetChildren() => Arguments;
    }
}
