using System.Collections.Generic;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public class AttributeSyntax : SyntaxNode
    {
        public AttributeSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, AttributeArgumentListSyntax argumentList)
            : base(syntaxTree)
        {
            Kind = SyntaxKind.Attribute;

            Identifier = identifier;
            ArgumentList = argumentList;
        }

        public override SyntaxKind Kind { get; init; }

        public SyntaxToken Identifier { get; init; }

        public AttributeArgumentListSyntax ArgumentList { get; init; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            yield return ArgumentList;
        }
    }
}
