using Express.Net.Emit.Bootstrapping;
using Xunit;

namespace Express.Net.Tests
{
    public class BasicBootstrapperTests
    {
        [Fact]
        public void TestBasicBootstrapperCodegenNoSwagger()
        {
            var _code = @"
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Express.Net;

const string projectName = ""Test Project"";
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

CreateHostBuilder(args).Build().Run();".Trim();

            var bootstrapper = new BasicBootstrapper("Test Project", addSwagger: false, addSwaggerUI: false);
            var bootstrapperSyntaxTree = bootstrapper.GetBootstrapper();
            var bootstrapperCode = bootstrapperSyntaxTree.ToString().Trim();

            Assert.Equal(_code, bootstrapperCode);
        }

        [Fact]
        public void TestBasicBootstrapperCodegenSwagger()
        {
            var _code = @"
#define Swagger
#define SwaggerUI

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Express.Net;

const string projectName = ""Test Project"";
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

CreateHostBuilder(args).Build().Run();".Trim();

            var bootstrapper = new BasicBootstrapper("Test Project", addSwagger: true, addSwaggerUI: true);
            var bootstrapperSyntaxTree = bootstrapper.GetBootstrapper();
            var bootstrapperCode = bootstrapperSyntaxTree.ToString().Trim();

            Assert.Equal(_code, bootstrapperCode);
        }
    }
}
