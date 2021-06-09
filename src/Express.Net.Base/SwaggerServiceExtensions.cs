using Express.Net.System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;

namespace Express.Net
{
    public static class SwaggerServiceExtensions
    {
        private class EmptyActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
        {
            public EmptyActionDescriptorCollectionProvider()
            {
                ActionDescriptors = new ActionDescriptorCollection(Array.Empty<ActionDescriptor>(), 1);
            }

            public ActionDescriptorCollection ActionDescriptors { get; }
        }

        public static IServiceCollection AddExpressSwagger(this IServiceCollection services, string projectName, string version)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, ExpressApiDescriptionProvider>());

            services.AddSingleton<IApiDescriptionGroupCollectionProvider>(serviceProvider =>
            {
                var apiDescriptionProvider = serviceProvider.GetServices<IApiDescriptionProvider>();

                return new ApiDescriptionGroupCollectionProvider(
                    new EmptyActionDescriptorCollectionProvider(),
                    apiDescriptionProvider);
            });

            services.AddSwaggerGen(o => o.SwaggerDoc("documents", new OpenApiInfo
            {
                Title = projectName,
                Version = version,
            }));

            return services;
        }

        public static IEndpointRouteBuilder MapExpressSwagger(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapSwagger();
        }

        public static IApplicationBuilder UseExpressSwaggerUI(this IApplicationBuilder app, string projectName)
        {
            var options = new SwaggerUIOptions
            {
                DocumentTitle = projectName,
                ConfigObject = new ConfigObject
                {
                    Urls = new []
                    {
                        new UrlDescriptor()
                        {
                            Name = projectName,
                            Url = "/swagger/documents/swagger.json"
                        }
                    }
                }
            };

            return app.UseSwaggerUI(options);
        }
    }
}
