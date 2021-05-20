using System.Collections.Generic;
using System.Collections.Immutable;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public class EndpointParameterListSyntax : SyntaxNode
    {
        public EndpointParameterListSyntax(SyntaxTree syntaxTree, ImmutableArray<EndpointParameterSyntax> parameters)
            : base(syntaxTree)
        {
            Kind = SyntaxKind.EndpointParameterList;
            Parameters = parameters;
        }

        public override SyntaxKind Kind { get; init; }

        public ImmutableArray<EndpointParameterSyntax> Parameters { get; init; }

        public override IEnumerable<SyntaxNode> GetChildren() => Parameters;

        public override string ToString() => "Endpoint Parameter List";
    }
}
