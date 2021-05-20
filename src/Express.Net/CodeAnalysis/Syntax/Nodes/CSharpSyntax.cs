using Express.Net.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using CSharpSyntaxNode = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxNode;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public class CSharpSyntax : SyntaxNode
    {
        public CSharpSyntax(SyntaxTree syntaxTree, CSharpSyntaxNode csharpStatement, TextSpan span, TextSpan fullSpan)
            : base(syntaxTree)
        {
            Kind = SyntaxKind.CSharpSyntax;

            CSharpStatement = csharpStatement;
            FullSpan = fullSpan;
            Span = span;
        }

        public override SyntaxKind Kind { get; init; }

        public override TextSpan Span { get; }

        public override TextSpan FullSpan { get; }

        public CSharpSyntaxNode CSharpStatement { get; init; }

        public override IEnumerable<SyntaxNode> GetChildren() => Array.Empty<SyntaxNode>();
    }
}
