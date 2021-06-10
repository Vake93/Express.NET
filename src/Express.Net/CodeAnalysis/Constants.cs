using System.Text.RegularExpressions;

namespace Express.Net.CodeAnalysis
{
    internal static class Constants
    {
        public static readonly Regex NameRegex = new (@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");

        public const string SyntaxParserTempClass = "class c__SyntaxParser";
        public const string EmptyStringValue = @"""""";

        public const string Empty = "";
        public const string Space = " ";
        public const string Async = "Async";

        public const string ExpressNamespace = "Express.Net";
        public const string TaskNamespace = "System.Threading.Tasks";
        public const string ControllerNamespace = "Controllers";

        public const string ControllerBaseClass = "ControllerBase";

        public const string RouteAttribute = "Route";
        public const string HttpGetAttribute = "HttpGet";
        public const string HttpPostAttribute = "HttpPost";
        public const string HttpPutAttribute = "HttpPut";
        public const string HttpPatchAttribute = "HttpPatch";
        public const string HttpDeleteAttribute = "HttpDelete";
        public const string HttpHeadAttribute = "HttpHead";
        public const string FromBodyAttribute = "FromBody";
        public const string FromQueryAttribute = "FromQuery";
        public const string FromRouteAttribute = "FromRoute";
        public const string FromHeaderAttribute = "FromHeader";
        public const string FromServicesAttribute = "FromServices";

        public const string AsyncEndpointDeclarationReturnType = "Task<IResult>";
        public const string SyncEndpointDeclarationReturnType = "IResult";

        public static readonly string[] SkipAttributes = new[]
        {
            RouteAttribute, HttpGetAttribute, HttpPostAttribute, HttpPutAttribute,
            HttpPatchAttribute, HttpDeleteAttribute, HttpHeadAttribute, FromBodyAttribute, FromQueryAttribute,
            FromRouteAttribute, FromHeaderAttribute, FromServicesAttribute
        };
    }
}
