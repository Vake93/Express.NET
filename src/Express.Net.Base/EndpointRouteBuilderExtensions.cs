using Express.Net.System;
using Microsoft.AspNetCore.Routing;

namespace Express.Net
{
    public static class EndpointRouteBuilderExtensions
    {
        public static void MapHttpHandler<THttpHandler>(this IEndpointRouteBuilder builder)
        {
            HttpHandlerBuilder.Build<THttpHandler>(builder);
        }
    }
}
