using Express.Net.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Text;

namespace Express.Net.Emit.Bootstrapping
{
    public sealed class BasicBootstrapper : Bootstrapper
    {
        private const string _code = @"
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
            #if Swagger
                            services.AddSwaggerGen();
            #endif
                        });

                        webBuilder.Configure(app =>
                        {
                            app.UseRouting();

                            app.UseEndpoints(endpoints =>
                            {
                                endpoints.MapControllers();
            #if Swagger
                                endpoints.MapSwagger();
            #endif
                            });

            #if SwaggerUI
                            app.UseSwaggerUI();
            #endif
                        });
                    });

            CreateHostBuilder(args).Build().Run();";

        private readonly bool AddSwagger;
        private readonly bool AddSwaggerUI;

        public BasicBootstrapper(bool addSwagger, bool addSwaggerUI)
        {
            AddSwagger = addSwagger;
            AddSwaggerUI = addSwaggerUI;
        }

        public override SyntaxTree GetBootstrapper()
        {
            var codeBuilder = new StringBuilder(_code);

            if (AddSwagger)
            {
                codeBuilder.Insert(0, "#define Swagger\n");
            }

            if (AddSwaggerUI)
            {
                codeBuilder.Insert(0, "#define SwaggerUI\n");
            }

            return SyntaxFactory.ParseSyntaxTree(codeBuilder.ToString());
        }
    }
}
