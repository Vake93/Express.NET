using System.Collections.Generic;
using System.Collections.Immutable;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public sealed class CSharpBlockSyntax : MemberSyntax
    {
        public CSharpBlockSyntax(SyntaxTree syntaxTree, SyntaxToken csharpKeyword, SyntaxToken codeBlock, ImmutableArray<CSharpSyntax> members)
            : base(syntaxTree)
        {
            CSharpKeyword = csharpKeyword;
            CodeBlock = codeBlock;
            Members = members;

            Kind = SyntaxKind.CSharpBlock;
        }

        public override SyntaxKind Kind { get; init; }

        public SyntaxToken CSharpKeyword { get; init; }

        public SyntaxToken CodeBlock { get; init; }

        public ImmutableArray<CSharpSyntax> Members { get; init; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return CSharpKeyword;
            yield return CodeBlock;
        }

        public override string ToString() => "C# Code Block";
    }
}
