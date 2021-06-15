using Express.Net.CodeAnalysis.Syntax.Nodes;
using CSharp = Microsoft.CodeAnalysis.CSharp;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Express.Net.CodeAnalysis
{
    public static class ResponseTypeResolver
    {
        public static CSharpSyntax.TypeSyntax? GetResponseType(TypeClauseSyntax type)
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
                    var responseTypeName = type.TypeName.Substring(startIndex + 1, endIndex - startIndex - 1);
                    return CSharp.SyntaxFactory.ParseTypeName(responseTypeName);
                }
            }

            return null;
        }

        public static CSharpSyntax.LiteralExpressionSyntax GetResponseCode(TypeClauseSyntax type)
        {
            var responseCode = 200;

            if (type.TypeName.StartsWith("Created"))
            {
                responseCode = 201;
            }

            if (type.TypeName.StartsWith("NotFound"))
            {
                responseCode = 404;
            }

            if (type.TypeName.StartsWith("BadRequest"))
            {
                responseCode = 404;
            }

            if (type.TypeName.StartsWith("Redirect"))
            {
                responseCode = 301;
            }

            if (type.TypeName.StartsWith("NoContent"))
            {
                responseCode = 204;
            }

            return CSharp.SyntaxFactory.LiteralExpression(
                CSharp.SyntaxKind.NumericLiteralExpression,
                CSharp.SyntaxFactory.Literal(responseCode));
        }
    }
}
