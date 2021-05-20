using System.Collections.Generic;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public class ServiceDeclarationSyntax : MemberSyntax
    {
        internal ServiceDeclarationSyntax(
            SyntaxTree syntaxTree,
            SyntaxToken keyword,
            SyntaxToken route,
            SyntaxToken identifier,
            AttributeListSyntax? attributeList = null)
            : base(syntaxTree)
        {
            Kind = SyntaxKind.ServiceDeclaration;
            AttributeList = attributeList;
            Identifier = identifier;
            Keyword = keyword;
            Route = route;
        }

        public override SyntaxKind Kind { get; init; }

        public SyntaxToken Keyword { get; init; }

        public SyntaxToken Route { get; init; }

        public SyntaxToken Identifier { get; init; }

        public AttributeListSyntax? AttributeList { get; init; }

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

        public override string ToString() => $"Service Declaration {Identifier.Text} @ {Route.Text}";
    }
}
