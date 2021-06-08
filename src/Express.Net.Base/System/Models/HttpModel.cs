using System;
using System.Collections.Generic;
using System.Reflection;

namespace Express.Net.System.Models
{
    internal class HttpModel
    {
        public HttpModel(Type handlerType)
        {
            HandlerType = handlerType;
        }

        public List<MethodModel> Methods { get; } = new List<MethodModel>();

        public Type HandlerType { get; }

        public static HttpModel FromType(Type type, Assembly assembly)
        {
            var model = new HttpModel(type);

            var routeAttributeType = assembly.GetType<RouteAttribute>();
            var httpMethodAttributeType = assembly.GetType<HttpMethodAttribute>();
            var fromQueryAttributeType = assembly.GetType<FromQueryAttribute>();
            var fromHeaderAttributeType = assembly.GetType<FromHeaderAttribute>();
            var fromFormAttributeType = assembly.GetType<FromFormAttribute>();
            var fromBodyAttributeType = assembly.GetType<FromBodyAttribute>();
            var fromRouteAttributeType = assembly.GetType<FromRouteAttribute>();
            var fromCookieAttributeType = assembly.GetType<FromCookieAttribute>();
            var fromServicesAttributeType = assembly.GetType<FromServicesAttribute>();

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

            var routeAttribute = type.GetCustomAttributeData(routeAttributeType);
            var methodNames = new Dictionary<string, int>();

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttributeData(httpMethodAttributeType);
                var template = CombineRoute(
                    routeAttribute?.GetConstructorArgument<string>(0),
                    attribute?.GetConstructorArgument<string>(0) ?? method.GetCustomAttributeData(routeAttributeType)?.GetConstructorArgument<string>(0));

                if (template == null)
                {
                    continue;
                }

                var methodName = method.Name;

                if (!methodNames.TryGetValue(method.Name, out var count))
                {
                    methodNames[method.Name] = 1;
                }
                else
                {
                    methodNames[method.Name] = count + 1;
                    methodName += $"_{count}";
                }

                var parameters = new List<ParameterModel>();
                var metadata = new List<object>();
                var methodModel = new MethodModel(methodName, method, template, parameters, metadata);

                foreach (var customAttribute in method.CustomAttributes)
                {
                    if (customAttribute.AttributeType.Namespace == "System.Runtime.CompilerServices" ||
                        customAttribute.AttributeType.Name == "DebuggerStepThroughAttribute")
                    {
                        continue;
                    }

                    metadata.Add(customAttribute);
                }

                foreach (var parameter in method.GetParameters())
                {
                    var fromQuery = parameter.GetCustomAttributeData(fromQueryAttributeType);
                    var fromHeader = parameter.GetCustomAttributeData(fromHeaderAttributeType);
                    var fromForm = parameter.GetCustomAttributeData(fromFormAttributeType);
                    var fromBody = parameter.GetCustomAttributeData(fromBodyAttributeType);
                    var fromRoute = parameter.GetCustomAttributeData(fromRouteAttributeType);
                    var fromCookie = parameter.GetCustomAttributeData(fromCookieAttributeType);
                    var fromService = parameter.GetCustomAttributeData(fromServicesAttributeType);

                    parameters.Add(new ParameterModel(
                        parameter.Name,
                        parameter.ParameterType,
                        fromQuery == null ? null : fromQuery?.GetConstructorArgument<string>(0) ?? parameter.Name,
                        fromHeader == null ? null : fromHeader?.GetConstructorArgument<string>(0) ?? parameter.Name,
                        fromForm == null ? null : fromForm?.GetConstructorArgument<string>(0) ?? parameter.Name,
                        fromRoute == null ? null : fromRoute?.GetConstructorArgument<string>(0) ?? parameter.Name,
                        fromCookie == null ? null : fromCookie?.GetConstructorArgument<string>(0),
                        fromBody != null,
                        fromService != null,
                        parameter.DefaultValue));
                }

                model.Methods.Add(methodModel);
            }

            return model;
        }

        private static string? CombineRoute(string? prefix, string? template)
        {
            if (prefix == null)
            {
                return template;
            }

            if (template == null)
            {
                return prefix;
            }

            return prefix + '/' + template.TrimStart('/');
        }
    }
}
