using NuGet.Configuration;
using NuGet.Protocol.Core.Types;

namespace Express.Net.Packages.Services
{
    public class RepositoryProvider : SourceRepositoryProvider
    {
        public RepositoryProvider(IPackageSourceProvider packageSourceProvider)
            : base(packageSourceProvider, Repository.Provider.GetCoreV3())
        {
        }
    }
}
