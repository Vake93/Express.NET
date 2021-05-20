using System.Collections.Generic;
using System.Collections.Immutable;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public class UnknownStatementSyntax : MemberSyntax
    {
        internal UnknownStatementSyntax(SyntaxTree syntaxTree, ImmutableArray<SyntaxToken> tokens)
            : base(syntaxTree)
        {
            Kind = SyntaxKind.UnknownStatement;
            Tokens = tokens;
        }

        public override SyntaxKind Kind { get; init; }

        public ImmutableArray<SyntaxToken> Tokens { get; init; }

        public override IEnumerable<SyntaxNode> GetChildren() => Tokens;
    }
}
