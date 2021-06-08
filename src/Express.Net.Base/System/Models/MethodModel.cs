using System.Collections.Generic;
using System.Reflection;

namespace Express.Net.System.Models
{
    internal class MethodModel
    {
        public MethodModel(
            string uniqueName,
            MethodInfo methodInfo,
            string routePattern,
            IReadOnlyList<ParameterModel> parameters,
            IReadOnlyList<object> metadata)
        {
            UniqueName = uniqueName;
            MethodInfo = methodInfo;
            RoutePattern = routePattern;
            Parameters = parameters;
            Metadata = metadata;
        }

        public string UniqueName { get; init; }

        public MethodInfo MethodInfo { get; init; }

        public string RoutePattern { get; init; }

        public IReadOnlyList<ParameterModel> Parameters { get; init; }

        public IReadOnlyList<object> Metadata { get; init; }
    }
}
