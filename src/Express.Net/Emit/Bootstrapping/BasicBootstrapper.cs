using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

        private static readonly BasicBootstrapper _instance = new ();

        public static BasicBootstrapper Instance => _instance;

        private BasicBootstrapper()
        {
        }

        public override SyntaxTree GetBootstrapper() => SyntaxFactory.ParseSyntaxTree(_code);
    }
}
