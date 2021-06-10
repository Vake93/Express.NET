using Express.Net.Emit.Bootstrapping;
using Express.Net.Models;
using Express.Net.Packages;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Express.Net.Tests
{
    public class PackageTests
    {
        [Fact]
        public async Task ProjectDependenciesTestAsync()
        {
            var configuration = "Debug";
            var objDirectoryName = "obj";

            var packageReferences = new PackageReference[]
            {
                new ("StackExchange.Redis", "2.2.4"),
            };

            var project = new Project(packageReferences, LibraryReferences: null, GenerateSwaggerDoc: false, AddSwaggerUI: false);
            var bootstrapper = new BasicBootstrapper("TestProject", addSwagger: false, addSwaggerUI: false);
            var tempPath = Path.GetTempPath();
            var nuGetClient = new NuGetClient(project, bootstrapper, configuration, tempPath);

            var packageAssemblies = await nuGetClient
                .RestoreProjectDependenciesAsync()
                .ConfigureAwait(false);

            var packagePath = Path.Combine(tempPath, objDirectoryName, configuration);
            var packageAssembliesFileName = Path.Combine(packagePath, "packageAssemblies.json");

            Assert.True(File.Exists(packageAssembliesFileName), $"{packageAssembliesFileName} not found");

            var files = packageAssemblies.SelectMany(pa => pa.PackageFiles);

            foreach (var file in files)
            {
                var filePath = Path.Combine(tempPath, file);
                Assert.True(File.Exists(filePath), $"{filePath} not found");
            }
        }
    }
}
