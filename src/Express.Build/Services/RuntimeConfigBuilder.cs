using Express.Net.Emit;
using Express.Net.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Express.Net.Build.Services
{
    internal static class RuntimeConfigBuilder
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new ()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        public static void BuildRuntimeConfig(
            string projectName,
            string output,
            TargetFramework targetFramework,
            Dictionary<string, object>? configProperties = null)
        {
            var framework = new Framework(targetFramework.Name, targetFramework.Version);

            var runtimeOptions = new
            {
                RuntimeOptions = new RuntimeOptions(
                    targetFramework.Tfm,
                    framework,
                    configProperties ?? new())
            };

            var jsonText = JsonSerializer.Serialize(runtimeOptions, _jsonSerializerOptions);
            var fileName = $"{projectName}.runtimeconfig.json";
            var path = Path.Combine(output, fileName);

            File.WriteAllText(path, jsonText);
        }
    }
}
