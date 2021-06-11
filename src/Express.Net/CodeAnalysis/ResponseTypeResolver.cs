using Express.Net.CodeAnalysis.Syntax.Nodes;

namespace Express.Net.CodeAnalysis
{
    public static class ResponseTypeResolver
    {
        public static string GetResponseType(TypeClauseSyntax type)
        {
            if (type.TypeName.StartsWith("Ok") || 
                type.TypeName.StartsWith("Created") ||
                type.TypeName.StartsWith("NotFound") ||
                type.TypeName.StartsWith("BadRequest"))
            {
                var startIndex = type.TypeName.IndexOf('<');
                var endIndex = type.TypeName.LastIndexOf('>');

                if (startIndex > 0 && endIndex > 0 && endIndex > startIndex)
                {
                    return type.TypeName.Substring(startIndex + 1, endIndex - startIndex - 1);
                }
            }

            return string.Empty;
        }

        public static int GetResponseCode(TypeClauseSyntax type)
        {
            if (type.TypeName.StartsWith("Created"))
            {
                return 201;
            }

            if (type.TypeName.StartsWith("NotFound"))
            {
                return 404;
            }

            if (type.TypeName.StartsWith("BadRequest"))
            {
                return 404;
            }

            if (type.TypeName.StartsWith("Redirect"))
            {
                return 301;
            }

            if (type.TypeName.StartsWith("NoContent"))
            {
                return 204;
            }

            return 200;
        }
    }
}
