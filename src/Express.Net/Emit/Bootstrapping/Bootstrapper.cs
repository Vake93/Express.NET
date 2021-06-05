using Express.Net.Models;
using System.Collections.Generic;
using CSharpSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;

namespace Express.Net.Emit.Bootstrapping
{
    public abstract class Bootstrapper
    {
        public abstract IEnumerable<PackageReference> PackageReferences { get; }

        public abstract CSharpSyntaxTree GetBootstrapper();
    }
}
