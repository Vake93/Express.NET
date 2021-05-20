using System.Collections.Generic;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public class AttributeArgumentSyntax : SyntaxNode
    {
        public AttributeArgumentSyntax(SyntaxTree syntaxTree, SyntaxToken expression, SyntaxToken? name)
            : base(syntaxTree)
        {
            Kind = SyntaxKind.AttributeArgument;
            Expression = expression;
            Name = name;
        }

        public override SyntaxKind Kind { get; init; }

        public SyntaxToken? Name { get; init; }

        public SyntaxToken Expression { get; init; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            if (Name is not null)
            {
                yield return Name;
            }

            yield return Expression;
        }
    }
}
