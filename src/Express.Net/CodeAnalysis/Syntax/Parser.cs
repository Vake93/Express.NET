using Express.Net.CodeAnalysis.Diagnostics;
using Express.Net.CodeAnalysis.Syntax.Nodes;
using Express.Net.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharp = Microsoft.CodeAnalysis.CSharp;
using CSharpDiagnostic = Microsoft.CodeAnalysis.Diagnostic;
using CSharpDiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace Express.Net.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private readonly SyntaxTree _syntaxTree;

        public Parser(SyntaxTree syntaxTree)
        {
            _syntaxTree = syntaxTree;

            var lexer = new Lexer(_syntaxTree);
            var tokens = new List<SyntaxToken>();
            var badTokens = new List<SyntaxToken>();

            var token = lexer.Lex();

            do
            {
                if (token.Kind == SyntaxKind.BadToken)
                {
                    badTokens.Add(token);
                    token = lexer.Lex();
                    continue;
                }

                if (badTokens.Count > 0)
                {
                    // If there is any bad tokens before the current token
                    // we add the bad tokens to leading trivia with kind SkippedTextTrivia
                    // of the current token
                    var leadingTrivia = token.LeadingTrivia.ToBuilder();
                    var index = 0;

                    foreach (var badToken in badTokens)
                    {
                        foreach (var lt in badToken.LeadingTrivia)
                            leadingTrivia.Insert(index++, lt);

                        var trivia = new SyntaxTrivia(SyntaxKind.SkippedTextTrivia, badToken.Position, badToken.Text);
                        leadingTrivia.Insert(index++, trivia);

                        foreach (var tt in badToken.TrailingTrivia)
                            leadingTrivia.Insert(index++, tt);
                    }

                    badTokens.Clear();

                    token = new SyntaxToken(
                        _syntaxTree,
                        token.Kind,
                        token.Position,
                        token.Text,
                        token.Value,
                        leadingTrivia.ToImmutable(),
                        token.TrailingTrivia);
                }

                tokens.Add(token);

                token = lexer.Lex();

            } while (token.Kind != SyntaxKind.EndOfFileToken);

            //Add the EndOfFileToken
            tokens.Add(token);

            _tokens = tokens.ToImmutableArray();

            Diagnostics = new DiagnosticBag(lexer.Diagnostics);
        }

        public DiagnosticBag Diagnostics { get; init; }

        private int Position { get; set; }

        private SyntaxToken Current => Peek(0);

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var members = ParseMembers();
            var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new CompilationUnitSyntax(_syntaxTree, members, endOfFileToken);
        }

        private ImmutableArray<MemberSyntax> ParseMembers()
        {
            var members = ImmutableArray.CreateBuilder<MemberSyntax>();

            while (Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var startToken = Current;

                var attributes = ParseAttributes();
                var member = ParseMember(attributes);
                members.Add(member);

                if (Current == startToken)
                    NextToken();
            }

            return members.ToImmutable();
        }

        private ImmutableArray<AttributeSyntax>? ParseAttributes()
        {
            if (Current.Kind != SyntaxKind.OpenSquareBracket)
                return null;

            var attributes = new List<AttributeSyntax>();

            while (Current.Kind == SyntaxKind.OpenSquareBracket)
            {
                _ = MatchToken(SyntaxKind.OpenSquareBracket);
                var identifier = MatchToken(SyntaxKind.IdentifierToken);

                var hasParams = Current.Kind == SyntaxKind.OpenParenthesisToken;
                var attributeArguments = ImmutableArray.CreateBuilder<AttributeArgumentSyntax>();

                if (hasParams)
                {
                    _ = MatchToken(SyntaxKind.OpenParenthesisToken);
                    attributeArguments.Add(ParseAttributeArgument());

                    while (Current.Kind == SyntaxKind.CommaToken)
                    {
                        _ = MatchToken(SyntaxKind.CommaToken);
                        attributeArguments.Add(ParseAttributeArgument());
                    }

                    _ = MatchToken(SyntaxKind.CloseParenthesisToken);
                }

                _ = MatchToken(SyntaxKind.CloseSquareBracket);

                var argumentList = new AttributeArgumentListSyntax(_syntaxTree, attributeArguments.ToImmutable());
                attributes.Add(new AttributeSyntax(_syntaxTree, identifier, argumentList));
            }

            return attributes.ToImmutableArray();

            AttributeArgumentSyntax ParseAttributeArgument()
            {
                var name = Current.Kind == SyntaxKind.IdentifierToken ?
                            MatchToken(SyntaxKind.IdentifierToken) :
                            null;

                _ = Current.Kind == SyntaxKind.EqualsToken ?
                    MatchToken(SyntaxKind.EqualsToken) :
                    null;

                var expression = Current.Kind switch
                {
                    SyntaxKind.NumberToken => MatchToken(SyntaxKind.NumberToken),
                    SyntaxKind.StringToken => MatchToken(SyntaxKind.StringToken),
                    SyntaxKind.NullKeyword => MatchToken(SyntaxKind.NullKeyword),
                    _ => MatchToken(SyntaxKind.AttributeArgument)
                };

                return new AttributeArgumentSyntax(_syntaxTree, expression, name);
            }
        }

        private MemberSyntax ParseMember(ImmutableArray<AttributeSyntax>? attributes) => Current.Kind switch
        {
            SyntaxKind.ServiceKeyword => ParseServiceDeclaration(attributes),
            SyntaxKind.UsingKeyword => ParseUsingStatement(),
            SyntaxKind.CSharpKeyword => ParseCSharpCodeBlock(),
            SyntaxKind.GetKeyword => ParseEndpointDeclaration(attributes),
            SyntaxKind.PostKeyword => ParseEndpointDeclaration(attributes),
            SyntaxKind.PutKeyword => ParseEndpointDeclaration(attributes),
            SyntaxKind.PatchKeyword => ParseEndpointDeclaration(attributes),
            SyntaxKind.DeleteKeyword => ParseEndpointDeclaration(attributes),
            SyntaxKind.HeadKeyword => ParseEndpointDeclaration(attributes),
            _ => ParseUnknownStatement()
        };

        private MemberSyntax ParseServiceDeclaration(ImmutableArray<AttributeSyntax>? attributes)
        {
            var serviceKeyword = MatchToken(SyntaxKind.ServiceKeyword);
            var route = ParseRoute();
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var attributeList = attributes.HasValue ? new AttributeListSyntax(_syntaxTree, attributes.Value) : null;

            _ = MatchToken(SyntaxKind.SemicolonToken);

            return new ServiceDeclarationSyntax(_syntaxTree, serviceKeyword, route, identifier, attributeList);
        }

        private MemberSyntax ParseUsingStatement()
        {
            var usingKeyword = MatchToken(SyntaxKind.UsingKeyword);
            var identifierBuilder = ImmutableArray.CreateBuilder<SyntaxToken>();

            var isAlias = Peek(1).Kind == SyntaxKind.EqualsToken;

            var alias = (SyntaxToken?)null;

            if (isAlias)
            {
                alias = MatchToken(SyntaxKind.IdentifierToken);
                _ = MatchToken(SyntaxKind.EqualsToken);
            }

            identifierBuilder.Add(MatchToken(SyntaxKind.IdentifierToken));

            while (Current.Kind == SyntaxKind.DotToken)
            {
                _ = MatchToken(SyntaxKind.DotToken);
                identifierBuilder.Add(MatchToken(SyntaxKind.IdentifierToken));
            }

            _ = MatchToken(SyntaxKind.SemicolonToken);

            var namespaceSyntax = new NamespaceSyntax(_syntaxTree, identifierBuilder.ToImmutable(), alias);

            return new UsingDirectiveSyntax(_syntaxTree, usingKeyword, namespaceSyntax);
        }

        private MemberSyntax ParseUnknownStatement()
        {
            var badTokens = ImmutableArray.CreateBuilder<SyntaxToken>();

            badTokens.Add(NextToken());

            while (Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var nextToken = NextToken();
                badTokens.Add(nextToken);

                if (nextToken.TrailingTrivia.Any(tt => tt.Kind == SyntaxKind.LineBreakTrivia))
                {
                    break;
                }
            }

            var span = new TextSpan(badTokens.First().Span.Start, badTokens.Last().Span.End);
            Diagnostics.ReportUnknownStatement(new TextLocation(_syntaxTree.Text, span));

            return new UnknownStatementSyntax(_syntaxTree, badTokens.ToImmutable());
        }

        private CSharpBlockSyntax ParseCSharpCodeBlock()
        {
            var csharpKeyword = MatchToken(SyntaxKind.CSharpKeyword);
            var codeBlock = MatchToken(SyntaxKind.CodeBlock);
            var members = SplitCSharpSyntax(codeBlock, SyntaxKind.CSharpBlock);

            return new CSharpBlockSyntax(_syntaxTree, csharpKeyword, codeBlock, members);
        }

        private EndpointDeclarationSyntax ParseEndpointDeclaration(ImmutableArray<AttributeSyntax>? attributes)
        {
            var kind = Current.Kind;
            var attributeList = attributes.HasValue ? new AttributeListSyntax(_syntaxTree, attributes.Value) : null;
            var httpVerbKeyword = MatchToken(kind);
            var route = ParseRoute();
            var returnTypes = ParseEndpointReturnTypes();
            var parameterList = ParseEndpointParameterList();
            var codeBlock = MatchToken(SyntaxKind.CodeBlock);

            var statements = SplitCSharpSyntax(codeBlock, kind);
            var asyncCode = ContainsAsyncCode(statements);

            return new EndpointDeclarationSyntax(
                _syntaxTree,
                httpVerbKeyword,
                route,
                returnTypes,
                parameterList,
                codeBlock,
                statements,
                asyncCode,
                attributeList);

            static bool ContainsAsyncCode(ImmutableArray<CSharpSyntax> statements)
            {
                var csharpStatements = statements.Select(s => s.CSharpStatement).OfType<CSharp.Syntax.StatementSyntax>();
                var asyncCode = false;

                foreach (var csharpStatement in csharpStatements)
                {
                    asyncCode = csharpStatement.DescendantNodes().OfType<CSharp.Syntax.AwaitExpressionSyntax>().Any();

                    if (asyncCode)
                    {
                        break;
                    }
                }

                return asyncCode;
            }
        }

        private ImmutableArray<CSharpSyntax> SplitCSharpSyntax(SyntaxToken codeBlock, SyntaxKind ParentKind)
        {
            if (codeBlock.IsMissing)
            {
                return ImmutableArray<CSharpSyntax>.Empty;
            }

            var text = codeBlock.Value as string;

            if (string.IsNullOrEmpty(text))
            {
                return ImmutableArray<CSharpSyntax>.Empty;
            }

            return ParentKind == SyntaxKind.CSharpBlock ?
                SplitCSharpCodeBlockSyntax(codeBlock, text) :
                SplitEndpointDefinitionBlockSyntax(codeBlock, text);
        }

        private ImmutableArray<CSharpSyntax> SplitEndpointDefinitionBlockSyntax(SyntaxToken codeBlock, string text)
        {
            var builder = ImmutableArray.CreateBuilder<CSharpSyntax>();

            var prefixLenght = text.IndexOf('{') + 1;
            var sufixLenght = text.LastIndexOf('}') - 1;

            text = text.Substring(prefixLenght, sufixLenght);

            var csharpSyntaxTree = CSharp.CSharpSyntaxTree.ParseText(text);
            var diagnostics = csharpSyntaxTree.GetDiagnostics();

            foreach (var diagnostic in diagnostics)
            {
                var span = TextSpan.FromCodeAnalysisTextSpan(diagnostic.Location.SourceSpan) + codeBlock.FullSpan + prefixLenght;
                var location = new TextLocation(_syntaxTree.Text, span);
                var message = diagnostic.GetMessage();

                ReportDiagnostic(diagnostic, location, message);
            }

            var compilationUnitSyntax = (CSharp.Syntax.CompilationUnitSyntax)csharpSyntaxTree.GetRoot();
            var statements = compilationUnitSyntax.Members
                .OfType<CSharp.Syntax.GlobalStatementSyntax>()
                .Select(gss => gss.Statement);

            foreach (var csharpStatement in statements)
            {
                var span = TextSpan.FromCodeAnalysisTextSpan(csharpStatement.Span) + codeBlock.Span + prefixLenght;
                var fullSpan = TextSpan.FromCodeAnalysisTextSpan(csharpStatement.FullSpan) + codeBlock.FullSpan + prefixLenght;

                builder.Add(new CSharpSyntax(_syntaxTree, csharpStatement, span, fullSpan));
            }

            return builder.ToImmutable();
        }

        private ImmutableArray<CSharpSyntax> SplitCSharpCodeBlockSyntax(SyntaxToken codeBlock, string text)
        {
            var builder = ImmutableArray.CreateBuilder<CSharpSyntax>();

            var prefixLenght = Constants.SyntaxParserTempClass.Length;

            var csharpSyntaxTree = CSharp.CSharpSyntaxTree.ParseText($"{Constants.SyntaxParserTempClass}{text}");
            var diagnostics = csharpSyntaxTree.GetDiagnostics();

            foreach (var diagnostic in diagnostics)
            {
                var span = TextSpan.FromCodeAnalysisTextSpan(diagnostic.Location.SourceSpan) + codeBlock.FullSpan - prefixLenght;
                var location = new TextLocation(_syntaxTree.Text, span);
                var message = diagnostic.GetMessage();

                ReportDiagnostic(diagnostic, location, message);
            }

            var compilationUnitSyntax = (CSharp.Syntax.CompilationUnitSyntax)csharpSyntaxTree.GetRoot();
            var classDeclaration = (CSharp.Syntax.ClassDeclarationSyntax)compilationUnitSyntax.Members[0];

            foreach (var csharpStatement in classDeclaration.Members)
            {
                var span = TextSpan.FromCodeAnalysisTextSpan(csharpStatement.Span) + codeBlock.Span - prefixLenght;
                var fullSpan = TextSpan.FromCodeAnalysisTextSpan(csharpStatement.FullSpan) + codeBlock.FullSpan - prefixLenght;

                builder.Add(new CSharpSyntax(_syntaxTree, csharpStatement, span, fullSpan));
            }

            return builder.ToImmutable();
        }

        private void ReportDiagnostic(CSharpDiagnostic diagnostic, TextLocation location, string message)
        {
            switch (diagnostic.Severity)
            {
                case CSharpDiagnosticSeverity.Warning:
                    Diagnostics.ReportWarning(location, message);
                    break;

                case CSharpDiagnosticSeverity.Error:
                    Diagnostics.ReportError(location, message);
                    break;

                default:
                    Diagnostics.ReportInformation(location, message);
                    break;
            }
        }

        private SyntaxToken ParseRoute()
        {
            if (Current.Kind == SyntaxKind.StringToken)
            {
                return MatchToken(SyntaxKind.StringToken);
            }

            return new SyntaxToken(
                _syntaxTree,
                SyntaxKind.StringToken,
                Position,
                Constants.EmptyStringValue,
                Constants.Empty,
                ImmutableArray<SyntaxTrivia>.Empty,
                ImmutableArray<SyntaxTrivia>.Empty);
        }

        private EndpointReturnTypesSyntax ParseEndpointReturnTypes()
        {
            var returnTypeBuilder = ImmutableArray.CreateBuilder<TypeClauseSyntax>();

            while (Current.Kind != SyntaxKind.EndOfFileToken)
            {
                returnTypeBuilder.Add(ParseTypeClause());

                if (Current.Kind != SyntaxKind.PipeToken)
                {
                    break;
                }

                _ = MatchToken(SyntaxKind.PipeToken);
            }

            return new EndpointReturnTypesSyntax(_syntaxTree, returnTypeBuilder.ToImmutable());
        }

        private EndpointParameterListSyntax ParseEndpointParameterList()
        {
            var parameters = ImmutableArray.CreateBuilder<EndpointParameterSyntax>();

            MatchToken(SyntaxKind.OpenParenthesisToken);

            if (Current.Kind != SyntaxKind.CloseParenthesisToken)
            {
                while (Current.Kind != SyntaxKind.EndOfFileToken)
                {
                    var attributes = ParseAttributes();
                    var bindLocation = ParseParameterLocation();
                    var type = ParseTypeClause();
                    var identifer = MatchToken(SyntaxKind.IdentifierToken);

                    var attributeList = attributes.HasValue ? new AttributeListSyntax(_syntaxTree, attributes.Value) : null;

                    var defaultValue = (SyntaxToken?)null;

                    if (Current.Kind == SyntaxKind.EqualsToken)
                    {
                        _ = MatchToken(SyntaxKind.EqualsToken);

                        switch (Current.Kind)
                        {
                            case SyntaxKind.NumberToken:
                            case SyntaxKind.StringToken:
                            case SyntaxKind.NullKeyword:
                            case SyntaxKind.DefaultKeyword:
                                defaultValue = MatchToken(Current.Kind);
                                break;

                            default:
                                _ = MatchToken(Current.Kind);
                                Diagnostics.ReportInvalidDefaultValue(Current.Location, identifer);
                                break;
                        }
                    }

                    parameters.Add(new EndpointParameterSyntax(_syntaxTree, bindLocation, type, identifer, defaultValue, attributeList));

                    if (Current.Kind != SyntaxKind.CommaToken)
                    {
                        break;
                    }

                    _ = MatchToken(SyntaxKind.CommaToken);
                }
            }

            MatchToken(SyntaxKind.CloseParenthesisToken);

            return new EndpointParameterListSyntax(_syntaxTree, parameters.ToImmutable());

            SyntaxToken ParseParameterLocation() => Current.Kind switch
            {
                SyntaxKind.BodyKeyword => MatchToken(SyntaxKind.BodyKeyword),
                SyntaxKind.QueryKeyword => MatchToken(SyntaxKind.QueryKeyword),
                SyntaxKind.RouteKeyword => MatchToken(SyntaxKind.RouteKeyword),
                SyntaxKind.HeaderKeyword => MatchToken(SyntaxKind.HeaderKeyword),
                SyntaxKind.ServiceKeyword => MatchToken(SyntaxKind.ServiceKeyword),
                _ => MatchToken(SyntaxKind.BodyKeyword)
            };
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            var builder = ImmutableArray.CreateBuilder<SyntaxToken>();

            ParseTypeClauseIdentifiers(builder);

            return new TypeClauseSyntax(_syntaxTree, builder.ToImmutable());

            void ParseTypeClauseIdentifiers(ImmutableArray<SyntaxToken>.Builder builder)
            {
                builder.Add(MatchToken(SyntaxKind.IdentifierToken));

                var arrayType = Current.Kind == SyntaxKind.OpenSquareBracket;
                var genericType = Current.Kind == SyntaxKind.LessThanToken;

                if (TryParseNullableType(builder))
                {
                    return;
                }

                if (arrayType)
                {
                    builder.Add(MatchToken(SyntaxKind.OpenSquareBracket));
                    builder.Add(MatchToken(SyntaxKind.CloseSquareBracket));
                    TryParseNullableType(builder);

                    return;
                }

                if (genericType)
                {
                    builder.Add(MatchToken(SyntaxKind.LessThanToken));
                    ParseTypeClauseIdentifiers(builder);
                    builder.Add(MatchToken(SyntaxKind.GreaterThanToken));
                    TryParseNullableType(builder);

                    return;
                }
            }

            bool TryParseNullableType(ImmutableArray<SyntaxToken>.Builder builder)
            {
                var nullableType = Current.Kind == SyntaxKind.QuestionMarkToken;

                if (nullableType)
                {
                    builder.Add(MatchToken(SyntaxKind.QuestionMarkToken));
                }

                return nullableType;
            }
        }

        private SyntaxToken Peek(int offset)
        {
            var index = Position + offset;

            if (index >= _tokens.Length)
            {
                return _tokens[^1];
            }

            return _tokens[index];
        }

        private SyntaxToken NextToken()
        {
            var current = Current;
            Position++;
            return current;
        }

        private SyntaxToken MatchToken(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            Diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind, kind);

            return new SyntaxToken(
                _syntaxTree,
                kind,
                Current.Position,
                null,
                null,
                ImmutableArray<SyntaxTrivia>.Empty,
                ImmutableArray<SyntaxTrivia>.Empty);
        }
    }
}
