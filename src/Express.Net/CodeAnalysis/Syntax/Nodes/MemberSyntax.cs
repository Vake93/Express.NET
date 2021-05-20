using Express.Net.CodeAnalysis.Text;

namespace Express.Net.CodeAnalysis.Syntax.Nodes
{
    public abstract class MemberSyntax : SyntaxNode
    {
        private protected MemberSyntax(SyntaxTree syntaxTree)
            : base(syntaxTree)
        {
        }
    }
}
