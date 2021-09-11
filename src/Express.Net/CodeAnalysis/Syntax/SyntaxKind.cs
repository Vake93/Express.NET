namespace Express.Net.CodeAnalysis.Syntax;

public enum SyntaxKind
{
    BadToken,

    // Trivia
    LineBreakTrivia,
    WhitespaceTrivia,
    SingleLineCommentTrivia,
    MultiLineCommentTrivia,
    SkippedTextTrivia,

    // Tokens
    EndOfFileToken,
    IdentifierToken,
    OpenParenthesisToken,
    CloseParenthesisToken,
    OpenSquareBracket,
    CloseSquareBracket,
    LessThanToken,
    GreaterThanToken,
    PipeToken,
    CommaToken,
    SemicolonToken,
    QuestionMarkToken,
    DotToken,
    StringToken,
    NumberToken,
    EqualsToken,

    // Keywords
    DefaultKeyword,
    GetKeyword,
    PostKeyword,
    PutKeyword,
    PatchKeyword,
    DeleteKeyword,
    HeadKeyword,
    CSharpKeyword,
    UsingKeyword,
    ServiceKeyword,
    QueryKeyword,
    RouteKeyword,
    BodyKeyword,
    HeaderKeyword,
    NullKeyword,

    // Nodes
    CompilationUnit,
    CodeBlock,
    Namespace,
    TypeClause,
    EndpointReturnType,
    EndpointParameterList,
    EndpointParameter,
    AttributeList,
    Attribute,
    AttributeArgumentList,
    AttributeArgument,
    CSharpSyntax,

    // Members
    UnknownStatement,
    ServiceDeclaration,
    UsingDirective,
    CSharpBlock,
    EndpointDeclaration,
}