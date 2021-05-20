using System.Collections.Generic;
using System.Collections.Immutable;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public class EndpointReturnTypesSyntax : SyntaxNode
    {
        public EndpointReturnTypesSyntax(SyntaxTree syntaxTree, ImmutableArray<TypeClauseSyntax> types)
            :base(syntaxTree)
        {
            Kind = SyntaxKind.EndpointReturnType;
            Types = types;
        }

        public override SyntaxKind Kind { get; init; }

        public ImmutableArray<TypeClauseSyntax> Types { get; init; }

        public override IEnumerable<SyntaxNode> GetChildren() => Types;

        public override string ToString() => "Endpoint Return Types";
    }
}
