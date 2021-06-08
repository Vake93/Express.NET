using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Express.Net.Reference.Assemblies
{
    internal static class BaseLibraryResources
    {
        private static byte[]? _ExpressNetBase;
        internal static byte[] ExpressNetBase => ResourceLoader.GetOrCreateResource(ref _ExpressNetBase, "Express.Net.Base");
    }

    public static class BaseLibrary
    {
        public static PortableExecutableReference ExpressNetBase { get; } = AssemblyMetadata.CreateFromImage(BaseLibraryResources.ExpressNetBase).GetReference(filePath: "Express.Net.Base.dll", display: "Express.Net.Base (Express.NET)");

        public static IEnumerable<PortableExecutableReference> All { get; } = new PortableExecutableReference[]
        {
            ExpressNetBase
        };
    }
}
