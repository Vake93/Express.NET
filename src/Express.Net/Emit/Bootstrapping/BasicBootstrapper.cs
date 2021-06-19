using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;

namespace Express.Net.Emit.Bootstrapping
{
    public sealed class BasicBootstrapper : Bootstrapper
    {
        private const string _code = @"
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Express.Net;

const string projectName = ""{ProjectName}"";
const string projectVersion = ""V1"";

static IHostBuilder CreateHostBuilder(string[] args) => Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHost(webBuilder =>
        {
            webBuilder.UseKestrel();

            webBuilder.ConfigureServices(services =>
            {
                services.AddRouting();
#if Swagger
                services.AddExpressSwagger(projectName, projectVersion);
#endif
            });

            webBuilder.Configure(app =>
            {
                app.UseRouting();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapExpressController();
#if Swagger
                    endpoints.MapExpressSwagger();
#endif
                });

#if SwaggerUI
                app.UseExpressSwaggerUI(projectName);
#endif
            });
        });

CreateHostBuilder(args).Build().Run();";

        private readonly string _projectName;
        private readonly bool _addSwagger;
        private readonly bool _addSwaggerUI;

        public BasicBootstrapper(string projectName, bool addSwagger, bool addSwaggerUI)
        {
            _projectName = projectName;
            _addSwagger = addSwagger;
            _addSwaggerUI = addSwaggerUI;
        }

        public override SyntaxTree GetBootstrapper()
        {
            var codeBuilder = new StringBuilder();

            if (_addSwagger)
            {
                codeBuilder.AppendLine("#define Swagger");
            }

            if (_addSwaggerUI)
            {
                codeBuilder.AppendLine("#define SwaggerUI");
            }

            codeBuilder.AppendLine(_code.Replace("{ProjectName}", _projectName));

            return SyntaxFactory.ParseSyntaxTree(codeBuilder.ToString(), path: "Program.cs", encoding: Encoding.UTF8);
        }
    }
}
