using System.Collections.Generic;
using System.Collections.Immutable;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        internal CompilationUnitSyntax(SyntaxTree syntaxTree, ImmutableArray<MemberSyntax> members, SyntaxToken endOfFileToken)
            : base(syntaxTree)
        {
            Members = members;
            EndOfFileToken = endOfFileToken;

            Kind = SyntaxKind.CompilationUnit;
        }

        public override SyntaxKind Kind { get; init; }

        public ImmutableArray<MemberSyntax> Members { get; init; }

        public SyntaxToken EndOfFileToken { get; init; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var child in Members)
                yield return child;

            yield return EndOfFileToken;
        }

        public override string ToString() => "Compilation Unit";
    }
}
