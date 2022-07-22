namespace Express.Net.CodeAnalysis.Syntax;

internal static class SyntaxFacts
{
    public static SyntaxKind GetKeywordKind(string text) => text switch
    {
        "using" => SyntaxKind.UsingKeyword,
        "service" => SyntaxKind.ServiceKeyword,
        "csharp" => SyntaxKind.CSharpKeyword,
        "get" => SyntaxKind.GetKeyword,
        "post" => SyntaxKind.PostKeyword,
        "put" => SyntaxKind.PutKeyword,
        "patch" => SyntaxKind.PatchKeyword,
        "delete" => SyntaxKind.DeleteKeyword,
        "head" => SyntaxKind.HeadKeyword,
        "header" => SyntaxKind.HeaderKeyword,
        "query" => SyntaxKind.QueryKeyword,
        "body" => SyntaxKind.BodyKeyword,
        "route" => SyntaxKind.RouteKeyword,
        "default" => SyntaxKind.DefaultKeyword,
        "null" => SyntaxKind.NullKeyword,
        _ => SyntaxKind.IdentifierToken,
    };
}