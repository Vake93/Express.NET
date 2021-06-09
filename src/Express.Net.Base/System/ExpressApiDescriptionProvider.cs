using Express.Net.System.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Express.Net.System
{
    internal class ExpressApiDescriptionProvider : IApiDescriptionProvider
    {
        private readonly EndpointDataSource _endpointDataSource;

        public ExpressApiDescriptionProvider(EndpointDataSource endpointDataSource)
        {
            _endpointDataSource = endpointDataSource;
        }

        public int Order => 0;

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            var endpoints = _endpointDataSource.Endpoints;

            foreach (var endpoint in endpoints)
            {
                if (IsValidEndpoint(endpoint, out var routeEndpoint, out var methodModel, out var httpMethodAttribute))
                {
                    var apiDescription = CreateApiDescription(routeEndpoint, methodModel, httpMethodAttribute);

                    context.Results.Add(apiDescription);
                }
            }
        }

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
        }

        private static ApiDescription CreateApiDescription(
            RouteEndpoint routeEndpoint,
            MethodModel methodModel,
            HttpMethodAttribute httpMethodAttribute)
        {
            var apiDescription = new ApiDescription
            {
                HttpMethod = httpMethodAttribute.Method,
                ActionDescriptor = new ActionDescriptor
                {
                    RouteValues = new Dictionary<string, string>
                    {
                        // Swagger uses this to group endpoints together.
                        // Group methods together using the service name.
                        ["controller"] = methodModel.MethodInfo.DeclaringType?.Name ?? string.Empty
                    }
                },
                RelativePath = routeEndpoint.RoutePattern.RawText?.TrimStart('/') ?? string.Empty
            };

            apiDescription.SupportedRequestFormats.Add(new ApiRequestFormat
            { 
                MediaType = "application/json"
            });

            var responseTypes = methodModel.MethodInfo
                .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), false)
                .OfType<ProducesResponseTypeAttribute>();

            var addDefaultResponseType = true;

            foreach (var responseType in responseTypes)
            {
                addDefaultResponseType = false;

                apiDescription.SupportedResponseTypes.Add(new ApiResponseType
                {
                    ApiResponseFormats = { new ApiResponseFormat { MediaType = "application/json" } },
                    ModelMetadata = responseType.Type == null ? null : new ExpressModelMetadata(responseType.Type),
                    StatusCode = responseType.StatusCode,
                });
            }

            if (addDefaultResponseType)
            {
                apiDescription.SupportedResponseTypes.Add(new ApiResponseType
                {
                    ApiResponseFormats = { new ApiResponseFormat { MediaType = "application/json" } },
                    StatusCode = 200,
                }); 
            }

            foreach (var parameter in methodModel.Parameters)
            {
                apiDescription.ParameterDescriptions.Add(new ApiParameterDescription
                {
                    Name = parameter.Name ?? "Input",
                    ModelMetadata = new ExpressModelMetadata(parameter.ParameterType),
                    Source = parameter.GetBindingSource()
                });
            }

            return apiDescription;
        }

        private static bool IsValidEndpoint(
            Endpoint endpoint,
            [NotNullWhen(true)] out RouteEndpoint? routeEndpoint,
            [NotNullWhen(true)] out MethodModel? methodModel,
            [NotNullWhen(true)] out HttpMethodAttribute? httpMethodAttribute)
        {
            if (endpoint is RouteEndpoint re)
            {
                methodModel = re.Metadata.OfType<MethodModel>().FirstOrDefault();
                httpMethodAttribute = methodModel?.MethodInfo
                    .GetCustomAttributes(typeof(HttpMethodAttribute), false)
                    .OfType<HttpMethodAttribute>()
                    .FirstOrDefault();

                if (methodModel is not null && httpMethodAttribute is not null)
                {
                    routeEndpoint = re;
                    return true;
                }
            }

            httpMethodAttribute = null;
            routeEndpoint = null;
            methodModel = null;
            return false;
        }
    }
}
