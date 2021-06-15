using Express.Net.Models;
using System;
using System.IO;
using System.Text.Json;

namespace Express.Net.Build.Services
{
    internal static class ProjectFileHandler
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new ()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };

        public static Project? ReadProjectFile(
            string projectFilePath)
        {
            var projectJson = File.ReadAllText(projectFilePath);
            return JsonSerializer.Deserialize<Project>(projectJson, _jsonSerializerOptions);
        }

        public static void BuildProjectFile(
            string projectName,
            string outputDirectory)
        {
            var project = new Project(
                Array.Empty<PackageReference>(),
                Array.Empty<LibraryReference>(),
                true,
                true);

            var jsonText = JsonSerializer.Serialize(project, _jsonSerializerOptions);
            var fileName = $"{projectName}.enproj";
            var path = Path.Combine(outputDirectory, fileName);

            File.WriteAllText(path, jsonText);
        }

        public static void BuildServiceFile(
            string projectName,
            string outputDirectory)
        {
            var source = $@"
service {projectName};

get Ok ()
{{
    return Ok(""Hello World"");
}}".Trim();
            var fileName = $"HelloWorldService.en";
            var path = Path.Combine(outputDirectory, fileName);

            File.WriteAllText(path, source);
        }
    }
}
