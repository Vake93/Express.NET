using System;

namespace Express.Net.System.Models
{
    internal class ParameterModel
    {
        public ParameterModel(
            string? name,
            Type parameterType,
            string? fromQuery,
            string? fromHeader,
            string? fromForm,
            string? fromRoute,
            string? fromCookie,
            bool fromBody,
            bool fromServices,
            object? defaultValue)
        {
            Name = name;
            ParameterType = parameterType;
            FromQuery = fromQuery;
            FromHeader = fromHeader;
            FromForm = fromForm;
            FromRoute = fromRoute;
            FromCookie = fromCookie;
            FromBody = fromBody;
            FromServices = fromServices;
            DefaultValue = defaultValue;
        }

        public string? Name { get; init; }

        public Type ParameterType { get; init; }

        public string? FromQuery { get; init; }

        public string? FromHeader { get; init; }

        public string? FromForm { get; init; }

        public string? FromRoute { get; init; }

        public string? FromCookie { get; init; }

        public bool FromBody { get; init; }

        public bool FromServices { get; init; }

        public object? DefaultValue { get; init; }

        public bool HasBindingSource =>
            FromBody || FromServices ||
            FromCookie != null || FromForm != null ||
            FromQuery != null || FromHeader != null ||
            FromRoute != null;
    }
}