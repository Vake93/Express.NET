using Express.Net.CodeAnalysis.Diagnostics;
using Express.Net.CodeAnalysis.Syntax;
using Express.Net.CodeAnalysis.Syntax.Nodes;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using CSharp = Microsoft.CodeAnalysis.CSharp;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxSyntaxNode = Microsoft.CodeAnalysis.SyntaxNode;
using CSharpSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;
using SyntaxNode = Express.Net.CodeAnalysis.Syntax.Nodes.SyntaxNode;

namespace Express.Net.CodeAnalysis
{
    internal sealed class Transformer
    {
        private readonly SyntaxTree _syntaxTree;
        private readonly string _projectName;
        private readonly bool _addDebugInfo;

        public Transformer(SyntaxTree syntaxTree, string projectName, bool addDebugInfo = false)
        {
            Diagnostics = new DiagnosticBag();
            _addDebugInfo = addDebugInfo;
            _projectName = projectName;
            _syntaxTree = syntaxTree;
        }

        public DiagnosticBag Diagnostics { get; init; }

        public CSharpSyntaxTree Transform()
        {
            var compilationUnit = CSharp.SyntaxFactory.CompilationUnit();

            compilationUnit = TransformUsingDirectiveSyntax(compilationUnit);
            compilationUnit = TransformServiceDeclarationSyntax(compilationUnit);
            compilationUnit = NormalizeWhitespace(compilationUnit);

            return CSharp.SyntaxFactory.SyntaxTree(compilationUnit);
        }

        private static SyntaxList<CSharpSyntax.AttributeListSyntax> BuildAttributeLists(AttributeListSyntax? attributeList, SyntaxKind syntaxKind, string? route = null)
        {
            var csharpAttributeSyntax = new List<CSharpSyntax.AttributeSyntax>();

            if (syntaxKind == SyntaxKind.ServiceDeclaration)
            {
                if (!string.IsNullOrEmpty(route))
                {
                    csharpAttributeSyntax.Add(
                        CSharp.SyntaxFactory.Attribute(CSharp.SyntaxFactory
                        .IdentifierName(Constants.RouteAttribute))
                        .WithArgumentList(
                            CSharp.SyntaxFactory.AttributeArgumentList(
                                CSharp.SyntaxFactory.SingletonSeparatedList(
                                    CSharp.SyntaxFactory.AttributeArgument(
                                        CSharp.SyntaxFactory.ParseExpression(route))))));
                }
            }
            else
            {
                var identifierName = syntaxKind switch
                {
                    SyntaxKind.GetKeyword    => Constants.HttpGetAttribute,
                    SyntaxKind.PostKeyword   => Constants.HttpPostAttribute,
                    SyntaxKind.PutKeyword    => Constants.HttpPutAttribute,
                    SyntaxKind.PatchKeyword  => Constants.HttpPatchAttribute,
                    SyntaxKind.DeleteKeyword => Constants.HttpDeleteAttribute,
                    SyntaxKind.HeadKeyword   => Constants.HttpHeadAttribute,
                    _ => Constants.Empty,
                };

                if (!string.IsNullOrEmpty(identifierName))
                {
                    var attribute = CSharp.SyntaxFactory.Attribute(CSharp.SyntaxFactory.IdentifierName(identifierName));

                    if (!string.IsNullOrEmpty(route) && route != Constants.EmptyStringValue)
                    {
                        attribute = attribute.WithArgumentList(
                            CSharp.SyntaxFactory.AttributeArgumentList(
                                CSharp.SyntaxFactory.SingletonSeparatedList(
                                    CSharp.SyntaxFactory.AttributeArgument(
                                        CSharp.SyntaxFactory.ParseExpression(route)))));
                    }

                    csharpAttributeSyntax.Add(attribute);
                }    
            }

            var csharpAttributeList = new List<CSharpSyntax.AttributeListSyntax>();

            if (csharpAttributeSyntax.Any())
            {
                csharpAttributeList.Add(
                    CSharp.SyntaxFactory.AttributeList(
                        CSharp.SyntaxFactory.SeparatedList(csharpAttributeSyntax)));
            }

            if (attributeList is AttributeListSyntax && attributeList.Attributes.Length > 0)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (Constants.SkipAttributes.Contains(attribute.Identifier.Text))
                    {
                        continue;
                    }

                    var csIdentifier = CSharp.SyntaxFactory.IdentifierName(attribute.Identifier.Text);
                    var csharpAttribute = CSharp.SyntaxFactory.Attribute(csIdentifier);
                    var arguments = attribute.ArgumentList.Arguments;

                    if (arguments.Any())
                    {
                        var csharpAttributeArguments = new List<CSharpSyntax.AttributeArgumentSyntax>();

                        foreach (var argument in arguments)
                        {
                            var nameEquals = argument.Name is null ? null : CSharp.SyntaxFactory.NameEquals(argument.Name.Text);
                            var expression = CSharp.SyntaxFactory.ParseExpression(argument.Expression.Text);
                            var attributeArgument = CSharp.SyntaxFactory.AttributeArgument(nameEquals, nameColon: null, expression);

                            csharpAttributeArguments.Add(attributeArgument);
                        }

                        csharpAttribute = csharpAttribute.WithArgumentList(
                            CSharp.SyntaxFactory.AttributeArgumentList(
                                CSharp.SyntaxFactory.SeparatedList(csharpAttributeArguments)));
                    }

                    csharpAttributeList.Add(
                        CSharp.SyntaxFactory.AttributeList(
                            CSharp.SyntaxFactory.SingletonSeparatedList(csharpAttribute)));
                }
            }

            return CSharp.SyntaxFactory.List(csharpAttributeList);
        }

        private static SyntaxList<CSharpSyntax.AttributeListSyntax> BuildResponseTypeAttributeLists(
            SyntaxList<CSharpSyntax.AttributeListSyntax> attributeLists,
            EndpointReturnTypesSyntax returnTypes)
        {
            foreach (var returnType in returnTypes.Types)
            {
                var responseType = ResponseTypeResolver.GetResponseType(returnType);
                var responseCode = ResponseTypeResolver.GetResponseCode(returnType);

                var attribute = CSharp.SyntaxFactory.Attribute(CSharp.SyntaxFactory
                    .IdentifierName(Constants.ProducesResponseTypeAttribute));

                var responseCodeArgument = CSharp.SyntaxFactory.AttributeArgument(responseCode);

                if (responseType == null)
                {
                    var argumentList = CSharp.SyntaxFactory.AttributeArgumentList(
                        CSharp.SyntaxFactory.SingletonSeparatedList(responseCodeArgument));

                    attribute = attribute.WithArgumentList(argumentList);
                }
                else
                {
                    var responseTypeArgument = CSharp.SyntaxFactory.AttributeArgument(
                        CSharp.SyntaxFactory.TypeOfExpression(responseType));

                    var argumentSeperator = CSharp.SyntaxFactory.Token(
                        CSharp.SyntaxKind.CommaToken);

                    var argumentList = CSharp.SyntaxFactory.AttributeArgumentList(
                        CSharp.SyntaxFactory.SeparatedList<CSharpSyntax.AttributeArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                responseTypeArgument,
                                argumentSeperator,
                                responseCodeArgument
                            }));

                    attribute = attribute.WithArgumentList(argumentList);
                }

                attributeLists = attributeLists.Add(
                    CSharp.SyntaxFactory.AttributeList(
                        CSharp.SyntaxFactory.SingletonSeparatedList(attribute)));
            }

            return attributeLists;
        }

        private static CSharpSyntax.ParameterListSyntax BuildParameterList(EndpointParameterListSyntax parameterList)
        {
            var parameters = new List<CSharpSyntax.ParameterSyntax>();

            foreach (var parameter in parameterList.Parameters)
            {
                var typeName = parameter.Type.TypeName;
                var identifer = parameter.Identifer.Text;
                var bindLocation = parameter.BindLocation.Kind;

                var attributeName = bindLocation switch
                {
                    SyntaxKind.QueryKeyword   => Constants.FromQueryAttribute,
                    SyntaxKind.RouteKeyword   => Constants.FromRouteAttribute,
                    SyntaxKind.HeaderKeyword  => Constants.FromHeaderAttribute,
                    SyntaxKind.ServiceKeyword => Constants.FromServicesAttribute,
                    _ => Constants.FromBodyAttribute
                };

                var attributeList = BuildAttributeLists(parameter.AttributeList, parameter.Kind).Add(
                    CSharp.SyntaxFactory.AttributeList(
                        CSharp.SyntaxFactory.SingletonSeparatedList(
                            CSharp.SyntaxFactory.Attribute(
                                CSharp.SyntaxFactory.IdentifierName(attributeName)))));

                var csharpParameter = CSharp.SyntaxFactory
                    .Parameter(CSharp.SyntaxFactory.ParseToken(identifer))
                    .WithType(CSharp.SyntaxFactory.ParseTypeName(typeName))
                    .WithAttributeLists(CSharp.SyntaxFactory.List(attributeList));

                if (parameter.DefaultValue is not null)
                {
                    var literalExpression = parameter.DefaultValue.Kind switch
                    {
                        SyntaxKind.NumberToken => CSharp.SyntaxFactory.LiteralExpression(
                            CSharp.SyntaxKind.NumericLiteralExpression,
                            CSharp.SyntaxFactory.Literal((int)parameter.DefaultValue.Value!)),

                        SyntaxKind.StringToken => CSharp.SyntaxFactory.LiteralExpression(
                            CSharp.SyntaxKind.StringLiteralExpression,
                            CSharp.SyntaxFactory.Literal((string)parameter.DefaultValue.Value!)),

                        SyntaxKind.NullKeyword => CSharp.SyntaxFactory.LiteralExpression(
                            CSharp.SyntaxKind.NullLiteralExpression),

                        SyntaxKind.DefaultKeyword => CSharp.SyntaxFactory.LiteralExpression(
                            CSharp.SyntaxKind.DefaultLiteralExpression,
                            CSharp.SyntaxFactory.Token(CSharp.SyntaxKind.DefaultKeyword)),

                        _ => null
                    };

                    if (literalExpression is not null)
                    {
                        csharpParameter = csharpParameter.WithDefault(
                            CSharp.SyntaxFactory.EqualsValueClause(literalExpression));
                    }
                }

                parameters.Add(csharpParameter);
            }

            return CSharp.SyntaxFactory.ParameterList(CSharp.SyntaxFactory.SeparatedList(parameters));
        }

        private static SyntaxTokenList BuildTokenList(params CSharp.SyntaxKind[] syntaxKind)
        {
            static Microsoft.CodeAnalysis.SyntaxToken ToToken(CSharp.SyntaxKind syntaxKind) =>
                CSharp.SyntaxFactory.Token(syntaxKind);

            return CSharp.SyntaxFactory.TokenList(syntaxKind.Select(ToToken));
        }

        private static CSharpSyntax.BaseListSyntax BuildBaseClassList(params string[] classNames)
        {
            static CSharpSyntax.BaseTypeSyntax ToSimpleBaseType(string identifierName) =>
                CSharp.SyntaxFactory.SimpleBaseType(CSharp.SyntaxFactory.ParseTypeName(identifierName));

            return CSharp.SyntaxFactory.BaseList(CSharp.SyntaxFactory.SeparatedList(classNames.Select(ToSimpleBaseType)));
        }

        private static string BuildMethodName(EndpointDeclarationSyntax endpointDeclaration, int index)
        {
            var route = endpointDeclaration.Route.Value as string;

            var sufix = endpointDeclaration.AsyncCode ? Constants.Async : Constants.Empty;

            var methodName = string.IsNullOrEmpty(route) ?
                $"{endpointDeclaration.HttpVerbKeyword.Text}" :
                $"{endpointDeclaration.HttpVerbKeyword.Text} {route}";

            methodName = Constants.NameRegex.Replace(methodName, Constants.Space);
            methodName = methodName.Replace(Constants.Space, Constants.Empty).Trim();

            return $"__{methodName}{index}{sufix}";
        }

        private static string BuildServiceNamespaceName(string projectName)
        {
            projectName = Constants.NameRegex.Replace(projectName, Constants.Space);
            projectName = projectName.Replace(Constants.Space, Constants.Empty).Trim();

            return $"{projectName}.{Constants.ControllerNamespace}";
        }

        private static CSharpSyntax.CompilationUnitSyntax NormalizeWhitespace(CSharpSyntax.CompilationUnitSyntax compilationUnit)
        {
            return compilationUnit.NormalizeWhitespace();
        }

        private CSharpSyntax.CompilationUnitSyntax TransformUsingDirectiveSyntax(CSharpSyntax.CompilationUnitSyntax compilationUnit)
        {
            var namespaceNames = new HashSet<string>();

            var usingDirectives = _syntaxTree.Root.Members.OfType<UsingDirectiveSyntax>();
            var csharpUsingDirectives = new List<CSharpSyntax.UsingDirectiveSyntax>();

            foreach (var usingDirective in usingDirectives)
            {
                var name = usingDirective.Namespace.Name;
                namespaceNames.Add(name);

                csharpUsingDirectives.Add(
                    CSharp.SyntaxFactory.UsingDirective(
                        CSharp.SyntaxFactory.ParseName(name)));
            }

            AddMissingNamespace(Constants.ExpressNamespace);
            AddMissingNamespace(Constants.TaskNamespace);

            return compilationUnit.WithUsings(CSharp.SyntaxFactory.List(csharpUsingDirectives));

            void AddMissingNamespace(string namespaceName)
            {
                if (namespaceNames?.Contains(namespaceName) == false)
                {
                    csharpUsingDirectives?.Add(
                        CSharp.SyntaxFactory.UsingDirective(
                            CSharp.SyntaxFactory.ParseName(namespaceName)));
                }
            }
        }

        private CSharpSyntax.CompilationUnitSyntax TransformServiceDeclarationSyntax(CSharpSyntax.CompilationUnitSyntax compilationUnit)
        {
            var serviceDeclaration = _syntaxTree.Root.Members.OfType<ServiceDeclarationSyntax>().First();

            var serviceAttributeList = serviceDeclaration.AttributeList;
            var serviceRoute = serviceDeclaration.Route.Text;

            var classDeclaration = CSharp.SyntaxFactory
                .ClassDeclaration(serviceDeclaration.Identifier.Text)
                .WithModifiers(BuildTokenList(CSharp.SyntaxKind.PublicKeyword))
                .WithBaseList(BuildBaseClassList(Constants.ControllerBaseClass));

            if (_addDebugInfo)
            {
                classDeclaration = AddDebugInfo(serviceDeclaration, classDeclaration, addFile: true);
            }

            classDeclaration = classDeclaration
                .WithAttributeLists(BuildAttributeLists(serviceAttributeList, serviceDeclaration.Kind, serviceRoute))
                .WithMembers(BuildClassMembersList(_syntaxTree.Root));

            var namespaceDeclaration = (CSharpSyntax.MemberDeclarationSyntax)CSharp.SyntaxFactory
                .NamespaceDeclaration(CSharp.SyntaxFactory.ParseName(BuildServiceNamespaceName(_projectName)))
                .WithMembers(CSharp.SyntaxFactory.SingletonList<CSharpSyntax.MemberDeclarationSyntax>(classDeclaration));

            return compilationUnit.WithMembers(CSharp.SyntaxFactory.SingletonList(namespaceDeclaration));
        }

        private SyntaxList<CSharpSyntax.MemberDeclarationSyntax> BuildClassMembersList(CompilationUnitSyntax compilationUnit)
        {
            var members = new List<CSharpSyntax.MemberDeclarationSyntax>();

            var csharpCodeBlock = compilationUnit.Members.OfType<CSharpBlockSyntax>();
            var codeBlockMembers = csharpCodeBlock.SelectMany(cb => cb.Members);

            if (codeBlockMembers.Any())
            {
                foreach (var codeBlockMember in codeBlockMembers)
                {
                    if (codeBlockMember.CSharpStatement is not CSharpSyntax.MemberDeclarationSyntax csharpMemberDeclaration)
                    {
                        continue;
                    }

                    if (_addDebugInfo)
                    {
                        csharpMemberDeclaration = AddDebugInfo(codeBlockMember, csharpMemberDeclaration);
                    }

                    members.Add(csharpMemberDeclaration);
                }
            }

            var endpointDeclarations = compilationUnit.Members.OfType<EndpointDeclarationSyntax>();
            var index = 0;

            foreach (var endpointDeclaration in endpointDeclarations)
            {
                var httpKeyword = endpointDeclaration.HttpVerbKeyword;
                var kind = httpKeyword.Kind;
                var route = endpointDeclaration.Route.Text;
                var attributeList = endpointDeclaration.AttributeList;
                var parameterList = endpointDeclaration.ParametersList;

                var modifiers = endpointDeclaration.AsyncCode ?
                    BuildTokenList(CSharp.SyntaxKind.PublicKeyword, CSharp.SyntaxKind.AsyncKeyword) :
                    BuildTokenList(CSharp.SyntaxKind.PublicKeyword);

                var methodReturnType = endpointDeclaration.AsyncCode ?
                    CSharp.SyntaxFactory.ParseTypeName(Constants.AsyncEndpointDeclarationReturnType) :
                    CSharp.SyntaxFactory.ParseTypeName(Constants.SyncEndpointDeclarationReturnType);

                var methodDeclaration = CSharp.SyntaxFactory
                    .MethodDeclaration(methodReturnType, BuildMethodName(endpointDeclaration, index))
                    .WithModifiers(modifiers)
                    .WithParameterList(BuildParameterList(parameterList));

                if (_addDebugInfo)
                {
                    methodDeclaration = AddDebugInfo(endpointDeclaration, methodDeclaration);
                }

                var statements = new List<CSharpSyntax.StatementSyntax>();

                foreach (var statement in endpointDeclaration.Statements)
                {
                    if (statement.CSharpStatement is not CSharpSyntax.StatementSyntax csharpStatement)
                    {
                        continue;
                    }

                    if (_addDebugInfo)
                    {
                        csharpStatement = AddDebugInfo(statement, csharpStatement);
                    }

                    statements.Add(csharpStatement);
                }

                var attributes = BuildAttributeLists(attributeList, kind, route);
                attributes = BuildResponseTypeAttributeLists(attributes, endpointDeclaration.ReturnTypes);

                methodDeclaration = methodDeclaration
                    .WithAttributeLists(attributes)
                    .WithBody(CSharp.SyntaxFactory.Block(statements));

                members.Add(methodDeclaration);
                index++;
            }

            return CSharp.SyntaxFactory.List(members);
        }

        private T AddDebugInfo<T>(SyntaxNode syntaxNode, T csharpSyntaxSyntaxNode, bool addFile = false)
            where T : CSharpSyntaxSyntaxNode
        {
            var endOfLineTriviaRawKind = (int)CSharp.SyntaxKind.EndOfLineTrivia;

            // Line directive index starts at 1
            var lineNumber = syntaxNode.Location.StartLine + 1;

            var leadingTrivia = addFile ?
                CSharp.SyntaxFactory.Trivia(
                    CSharp.SyntaxFactory.LineDirectiveTrivia(
                        CSharp.SyntaxFactory.Literal(lineNumber),
                        CSharp.SyntaxFactory.Literal(_syntaxTree.FileName ?? string.Empty),
                        true)) :
                CSharp.SyntaxFactory.Trivia(
                    CSharp.SyntaxFactory.LineDirectiveTrivia(
                        CSharp.SyntaxFactory.Literal(lineNumber), true));

            csharpSyntaxSyntaxNode = csharpSyntaxSyntaxNode.WithLeadingTrivia(leadingTrivia);

            if (syntaxNode.Location.StartLine != syntaxNode.Location.EndLine)
            {
                var count = csharpSyntaxSyntaxNode
                    .DescendantTokens()
                    .Where(t => t.TrailingTrivia.Any(tt => tt.RawKind == endOfLineTriviaRawKind))
                    .Select(t => t.GetNextToken())
                    .Count();

                for (var i = 0; i < count; i++)
                {
                    var token = csharpSyntaxSyntaxNode
                        .DescendantTokens()
                        .Where(t => t.TrailingTrivia.Any(tt => tt.RawKind == endOfLineTriviaRawKind))
                        .Select(t => (t.GetNextToken()))
                        .Skip(i)
                        .First();

                    lineNumber += token.LeadingTrivia.Count(tt => tt.RawKind == endOfLineTriviaRawKind);

                    var newToken = token.WithLeadingTrivia(
                        CSharp.SyntaxFactory.Trivia(
                            CSharp.SyntaxFactory.LineDirectiveTrivia(
                                CSharp.SyntaxFactory.Literal(++lineNumber), true)));

                    csharpSyntaxSyntaxNode = csharpSyntaxSyntaxNode.ReplaceToken(token, newToken);
                }
            }

            return csharpSyntaxSyntaxNode;
        }
    }
}
