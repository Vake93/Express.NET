using System.Collections.Generic;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public class EndpointParameterSyntax : SyntaxNode
    {
        public EndpointParameterSyntax(
            SyntaxTree syntaxTree,
            SyntaxToken bindLocation,
            TypeClauseSyntax type,
            SyntaxToken identifer,
            SyntaxToken? defaultValue = null,
            AttributeListSyntax? attributeList = null)
            : base(syntaxTree)
        {
            Kind = SyntaxKind.EndpointParameter;
            AttributeList = attributeList;
            BindLocation = bindLocation;
            DefaultValue = defaultValue;
            Identifer = identifer;
            Type = type;
        }

        public override SyntaxKind Kind { get; init; }

        public SyntaxToken BindLocation { get; init; }

        public TypeClauseSyntax Type { get; init; }

        public SyntaxToken Identifer { get; init; }

        public SyntaxToken? DefaultValue { get; init; }

        public AttributeListSyntax? AttributeList { get; init; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return BindLocation;
            yield return Type;
            yield return Identifer;

            if (DefaultValue is not null)
            {
                yield return DefaultValue;
            }
        }

        public override string ToString() => $"Endpoint Parameter {Identifer.Text}";
    }
}
