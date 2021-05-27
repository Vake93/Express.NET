using System.Collections.Immutable;

namespace Express.Net.Models.NuGet
{
    public record PackageAssembly(PackageReference PackageReference, ImmutableArray<string> PackageFiles);
}
