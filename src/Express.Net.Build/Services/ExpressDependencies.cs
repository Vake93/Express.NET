using Express.Net.Models;
using Express.Net.Models.NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Express.Net.Build.Services
{
    internal static class ExpressDependencies
    {
        public static bool CopyFrameworkAssemblies(Project project, string outputPath)
        {
            WriteResourceToFile("Express.Net.Base", outputPath);

            if (project.GenerateSwaggerDoc)
            {
                WriteResourceToFile("Microsoft.OpenApi", outputPath);
                WriteResourceToFile("Swashbuckle.AspNetCore.Swagger", outputPath);
                WriteResourceToFile("Swashbuckle.AspNetCore.SwaggerGen", outputPath);

                if (project.AddSwaggerUI)
                {
                    WriteResourceToFile("Swashbuckle.AspNetCore.SwaggerUI", outputPath);
                }
            }

            return true;
        }

        public static bool CopyPackageAssemblies(IEnumerable<PackageAssembly> packageAssemblies, string projectFolder, string outputPath)
        {
            var assemblyFiles = packageAssemblies.SelectMany(pa => pa.PackageFiles);

            foreach (var assemblyFile in assemblyFiles)
            {
                var name = Path.GetFileName(assemblyFile);
                var from = Path.Combine(projectFolder, assemblyFile);
                var to = Path.Combine(outputPath, name);

                File.Copy(from, to, overwrite: true);
            }

            return true;
        }

        private static bool WriteResourceToFile(string name, string outputPath)
        {
            var filePath = Path.Combine(outputPath, $"{name}.dll");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var assembly = typeof(ExpressDependencies).GetTypeInfo().Assembly;

            using var stream = assembly.GetManifestResourceStream(name);

            if (stream == null)
            {
                throw new InvalidOperationException($"Resource '{name}' not found in {assembly.FullName}.");
            }

            using var fileStream = File.OpenWrite(filePath);

            stream.CopyTo(fileStream);
            fileStream.Flush();

            return true;
        }
    }
}
