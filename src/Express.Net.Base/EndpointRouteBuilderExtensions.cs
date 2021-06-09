using Express.Net.System;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace Express.Net
{
    public static class EndpointRouteBuilderExtensions
    {
        public static void MapExpressController(this IEndpointRouteBuilder routes)
        {
            var assembly = Assembly.GetCallingAssembly();
            var baseType = typeof(ControllerBase);
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                if (type.IsAssignableTo(baseType))
                {
                    HttpHandlerBuilder.Build(type, routes);
                }
            }
        }
    }
}
