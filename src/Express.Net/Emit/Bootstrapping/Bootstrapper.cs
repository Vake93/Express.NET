using CSharpSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;

namespace Express.Net.Emit.Bootstrapping
{
    public abstract class Bootstrapper
    {
        public abstract CSharpSyntaxTree GetBootstrapper();
    }
}
