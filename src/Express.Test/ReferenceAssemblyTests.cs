using Express.Reference.Assemblies;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Linq;
using Xunit;

namespace Express.Net.Tests
{
    public class ReferenceAssemblyTests
    {
        [Fact]
        public void Net50Compilation()
        {
            var source = @"
using System;

class Program
{
    static void Main() 
    {
        Console.WriteLine(""Hello World"");
    }
}";
            var compilation = CSharpCompilation.Create(
                "Example",
                new[] { CSharpSyntaxTree.ParseText(source) },
                references: Net50.All);

            Assert.Empty(compilation.GetDiagnostics());

            using var stream = new MemoryStream();
            var emitResult = compilation.Emit(stream);

            Assert.True(emitResult.Success);
            Assert.Empty(emitResult.Diagnostics);
        }

        [Fact]
        public void AspNet50Compilation()
        {
            var source = @"
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHost(webBuilder =>
        {
            webBuilder.UseKestrel();

            webBuilder.ConfigureServices(services =>
            {
                services.AddControllers();
            });

            webBuilder.Configure(app =>
            {
                app.UseRouting();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            });
        });

CreateHostBuilder(args).Build().Run();";

            var compilation = CSharpCompilation.Create(
                "Example",
                new[] { CSharpSyntaxTree.ParseText(source) },
                references: Net50.All.Union(AspNet50.All));

            Assert.Empty(compilation.GetDiagnostics());

            using var stream = new MemoryStream();
            var emitResult = compilation.Emit(stream);

            Assert.True(emitResult.Success);
            Assert.Empty(emitResult.Diagnostics);
        }
    }
}
