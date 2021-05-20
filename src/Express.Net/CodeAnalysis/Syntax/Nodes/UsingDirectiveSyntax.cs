using System.Collections.Generic;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public class UsingDirectiveSyntax : MemberSyntax
    {
        internal UsingDirectiveSyntax(SyntaxTree syntaxTree, SyntaxToken usingKeyword, NamespaceSyntax namespaceSyntax)
            : base(syntaxTree)
        {
            Kind = SyntaxKind.UsingDirective;
            Namespace = namespaceSyntax;
            UsingKeyword = usingKeyword;
        }

        public override SyntaxKind Kind { get; init; }

        public SyntaxToken UsingKeyword { get; init; }

        public NamespaceSyntax Namespace { get; init; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return UsingKeyword;
            yield return Namespace;
        }

        public override string ToString() => $"Using Statement {Namespace}";
    }
}
