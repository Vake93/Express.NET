using Express.Net.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;

namespace Express.Net.CodeAnalysis.Syntax.Nodes;

public abstract record SyntaxNode(SyntaxTree SyntaxTree)
{
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

    public TextLocation Location => new(SyntaxTree.Text, Span);

    public abstract IEnumerable<SyntaxNode> GetChildren();

    public override string ToString() => $"{Kind}: {FullSpan}";
}