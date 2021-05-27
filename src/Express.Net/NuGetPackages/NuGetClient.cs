using Express.Net.Models;
using Express.Net.Models.NuGet;
using Express.Net.Packages.Services;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PackageReference = NuGet.Packaging.PackageReference;

namespace Express.Net.Packages
{
    public class NuGetClient
    {
        private const string frameworkName = "net5.0";
        private const string objDirectoryName = "obj";

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
        };

        private readonly PackageExtractionContext _packageExtractionContext;
        private readonly PackagePathResolver _packagePathResolver;
        private readonly PackageIdentityComparer _packageComparer;
        private readonly PackageSourceProvider _packageProvider;
        private readonly FrameworkReducer _frameworkReducer;
        private readonly RepositoryProvider _sourceProvider;
        private readonly NuGetFramework _framework;
        private readonly ISettings _nugetSettings;
        private readonly ILogger _logger;
        
        private readonly string _packagePath;
        private readonly Project _project;

        public NuGetClient(
            Project project,
            string configuration,
            string projectPath,
            ILogger? logger = null)
        {
            _packagePath = Path.Combine(projectPath, objDirectoryName, configuration);
            _project = project;

            var root = Path.GetPathRoot(_packagePath);

            _nugetSettings = Settings.LoadDefaultSettings(root);
            _packageProvider = new PackageSourceProvider(_nugetSettings);
            _packagePathResolver = new PackagePathResolver(_packagePath);
            _sourceProvider = new RepositoryProvider(_packageProvider);
            _packageComparer = PackageIdentityComparer.Default;
            _framework = NuGetFramework.Parse(frameworkName);
            _frameworkReducer = new FrameworkReducer();
            _logger = logger ?? NullLogger.Instance;

            _packageExtractionContext = new PackageExtractionContext(
                PackageSaveMode.Files,
                XmlDocFileSaveMode.None,
                ClientPolicyContext.GetClientPolicy(_nugetSettings, _logger),
                _logger);
        }

        private string GlobalPackagesFolder => SettingsUtility.GetGlobalPackagesFolder(_nugetSettings);

        public async Task<IEnumerable<PackageAssembly>> RestoreProjectDependenciesAsync(CancellationToken cancellationToken = default)
        {
            if (_project.PackageReferences is null || _project.PackageReferences.Length == 0)
            {
                return Array.Empty<PackageAssembly>();
            }

            var packageAssemblies = new List<PackageAssembly>();

            var packageAssembliesFileName = Path.Combine(_packagePath, "packageAssemblies.json");

            if (File.Exists(packageAssembliesFileName))
            {
                using var readStream = File.OpenRead(packageAssembliesFileName);

                packageAssemblies = await JsonSerializer
                    .DeserializeAsync<List<PackageAssembly>>(readStream, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (packageAssemblies is not null)
                {
                    return packageAssemblies;
                }

                packageAssemblies = new ();
            }

            var allDependencies = new HashSet<SourcePackageDependencyInfo>(_packageComparer);

            using var cacheContext = new SourceCacheContext
            {
                DirectDownload = true,
                SessionId = Guid.NewGuid(),
            };

            var repositories = _sourceProvider.GetRepositories();
            var packageSources = repositories.Select(s => s.PackageSource);
            var packageIds = new List<string>();
            var packageIdentities = new List<PackageIdentity>();
            var packageReferences = new List<PackageReference>();

            foreach (var packageReference in _project.PackageReferences)
            {
                var package = new PackageIdentity(packageReference.Name, NuGetVersion.Parse(packageReference.Version));

                packageIds.Add(packageReference.Name);
                packageIdentities.Add(package);
                packageReferences.Add(new (package, _framework));

                await GetPackageDependenciesAsync(
                    package,
                    cacheContext,
                    repositories,
                    allDependencies,
                    cancellationToken).ConfigureAwait(false);
            }

            var resolverContext = new PackageResolverContext(
                DependencyBehavior.Lowest,
                packageIds,
                packageIds,
                packageReferences,
                packageIdentities,
                allDependencies,
                packageSources,
                _logger);

            var requiredDependencies = new PackageResolver()
                .Resolve(resolverContext, cancellationToken)
                .Select(p => allDependencies
                .Single(k => _packageComparer.Equals(k, p)));

            var packageDownloadContext = new PackageDownloadContext(cacheContext, _packagePath, true);

            foreach (var requiredDependency in requiredDependencies)
            {
                var downloadResource = await requiredDependency.Source
                    .GetResourceAsync<DownloadResource>(cancellationToken)
                    .ConfigureAwait(false);

                var downloadResult = await downloadResource
                    .GetDownloadResourceResultAsync(
                        requiredDependency,
                        packageDownloadContext,
                        GlobalPackagesFolder,
                        _logger,
                        cancellationToken)
                    .ConfigureAwait(false);

                await PackageExtractor.ExtractPackageAsync(
                    downloadResult.PackageSource,
                    downloadResult.PackageStream,
                    _packagePathResolver,
                    _packageExtractionContext,
                    cancellationToken);

                var packageReader = downloadResult.PackageReader;

                var framework = _frameworkReducer.GetNearest(
                    _framework,
                    packageReader.GetLibItems().Select(f => f.TargetFramework));

                var frameworkSpecificGroup = packageReader
                    .GetReferenceItems()
                    .FirstOrDefault(f => f.TargetFramework == framework);

                if (frameworkSpecificGroup is not null)
                {
                    if (!frameworkSpecificGroup.Items.Any())
                    {
                        continue;
                    }

                    var packageReference = new Models.PackageReference(
                        requiredDependency.Id,
                        requiredDependency.Version.ToNormalizedString());

                    var packageFiles = new List<string>();

                    foreach (var item in frameworkSpecificGroup.Items)
                    {
                        var path = Path
                            .GetFullPath(Path
                            .Join(_packagePathResolver.GetInstallPath(requiredDependency), item));

                        var relativePath = Path.GetRelativePath(_packagePath, path);

                        packageFiles.Add(relativePath);
                    }

                    packageAssemblies.Add(new (packageReference, packageFiles.ToImmutableArray()));
                }
            }

            using var writeStream = File.OpenWrite(packageAssembliesFileName);

            await JsonSerializer.SerializeAsync(writeStream, packageAssemblies, _jsonSerializerOptions, cancellationToken);

            return packageAssemblies;
        }

        private async Task GetPackageDependenciesAsync(
            PackageIdentity package,
            SourceCacheContext cacheContext,
            IEnumerable<SourceRepository> repositories,
            ISet<SourcePackageDependencyInfo> availablePackages,
            CancellationToken cancellationToken)
        {
            if (availablePackages.Contains(package))
            {
                return;
            }

            foreach (var repository in repositories)
            {
                var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(cancellationToken);

                var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                    package,
                    _framework,
                    cacheContext,
                    _logger,
                    cancellationToken);

                if (dependencyInfo is null)
                {
                    continue;
                }

                availablePackages.Add(dependencyInfo);

                foreach (var dependency in dependencyInfo.Dependencies)
                {
                    var dependentPackage = new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion);

                    await GetPackageDependenciesAsync(
                        dependentPackage,
                        cacheContext,
                        repositories,
                        availablePackages,
                        cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
