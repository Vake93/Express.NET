using System.Collections.Generic;
using System.Collections.Immutable;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public class EndpointDeclarationSyntax : MemberSyntax
    {
        public EndpointDeclarationSyntax(
            SyntaxTree syntaxTree,
            SyntaxToken httpVerbKeyword,
            SyntaxToken route,
            EndpointReturnTypesSyntax returnTypes,
            EndpointParameterListSyntax parametersList,
            SyntaxToken endpointDeclarationBody,
            ImmutableArray<CSharpSyntax> statements,
            bool asyncCode = false,
            AttributeListSyntax? attributeList = null)
            : base(syntaxTree)
        {
            Kind = SyntaxKind.EndpointDeclaration;

            EndpointDeclarationBody = endpointDeclarationBody;
            HttpVerbKeyword = httpVerbKeyword;
            ParametersList = parametersList;
            AttributeList = attributeList;
            ReturnTypes = returnTypes;
            Statements = statements;
            AsyncCode = asyncCode;
            Route = route;
        }

        public override SyntaxKind Kind { get; init; }

        public SyntaxToken HttpVerbKeyword { get; init; }

        public SyntaxToken Route { get; }

        public EndpointReturnTypesSyntax ReturnTypes { get; init; }

        public EndpointParameterListSyntax ParametersList { get; init; }

        public SyntaxToken EndpointDeclarationBody { get; init; }

        public ImmutableArray<CSharpSyntax> Statements { get; init; }

        public AttributeListSyntax? AttributeList { get; init; }

        public bool AsyncCode { get; init; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            if (AttributeList is not null)
            {
                yield return AttributeList;
            }

            yield return HttpVerbKeyword;
            yield return Route;
            yield return ReturnTypes;
            yield return ParametersList;
            yield return EndpointDeclarationBody;
        }

        public override string ToString() => $"Endpoint Declaration [{HttpVerbKeyword.Text}] {Route.Text}";
    }
}
