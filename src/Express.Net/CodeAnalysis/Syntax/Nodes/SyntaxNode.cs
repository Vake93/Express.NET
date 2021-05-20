using Express.Net.CodeAnalysis.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public abstract class SyntaxNode
    {
        private protected SyntaxNode(SyntaxTree syntaxTree)
        {
            SyntaxTree = syntaxTree;
        }

        public SyntaxTree SyntaxTree { get; init; }

        public SyntaxNode? Parent => SyntaxTree.GetParent(this);

        public abstract SyntaxKind Kind { get; init; }

        public virtual TextSpan Span
        {
            get
            {
                var first = GetChildren().First().Span;
                var last = GetChildren().Last().Span;
                return TextSpan.FromBounds(first.Start, last.End);
            }
        }

        public virtual TextSpan FullSpan
        {
            get
            {
                var first = GetChildren().First().FullSpan;
                var last = GetChildren().Last().FullSpan;
                return TextSpan.FromBounds(first.Start, last.End);
            }
        }

        public TextLocation Location => new (SyntaxTree.Text, Span);

        public abstract IEnumerable<SyntaxNode> GetChildren();

        public void WriteTo(TextWriter writer)
        {
            PrettyPrint(writer, this);
        }

        public override string ToString()
        {
            using var writer = new StringWriter();
            WriteTo(writer);

            return writer.ToString();
        }

        private static void PrettyPrint(TextWriter writer, SyntaxNode node)
        {
            var token = node as SyntaxToken;

            if (token is not null)
            {
                foreach (var trivia in token.LeadingTrivia)
                {
                    writer.Write($"L: {trivia.Kind} ");
                }
            }

            writer.Write(node.Kind);

            if (token != null && token.Value != null)
            {
                writer.Write(" ");
                writer.Write(token.Value);
            }

            writer.WriteLine();

            if (token is not null)
            {
                foreach (var trivia in token.TrailingTrivia)
                {
                    writer.WriteLine($"T: {trivia.Kind} ");
                }
            }

            foreach (var child in node.GetChildren())
            {
                PrettyPrint(writer, child);
            }
        }

    }
}
