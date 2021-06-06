using Express.Net.CodeAnalysis;
using Express.Net.CodeAnalysis.Diagnostics;
using Express.Net.CodeAnalysis.Syntax;
using Express.Net.CodeAnalysis.Syntax.Nodes;
using System;
using Xunit;
using CSharp = Microsoft.CodeAnalysis.CSharp;

namespace Express.Net.Tests
{
    public class ParserTests
    {
        [Fact]
        public void ParseTopLevelStatements()
        {
            var text = @"
service ""api/v1/todo"" TodoService;

using System;
using System.Collections.Generic;
using Foo = System.Collections.Generic;";

            var syntaxTree = SyntaxTree.Parse(text);

            var compilationUnit = syntaxTree.Root;

            Assert.Empty(syntaxTree.Diagnostics);
            Assert.NotNull(compilationUnit);

            Assert.Collection(compilationUnit.Members, new Action<MemberSyntax>[]
            {
                t =>
                {
                    Assert.Equal(SyntaxKind.ServiceDeclaration, t.Kind);
                    var sds = Assert.IsAssignableFrom<ServiceDeclarationSyntax>(t);

                    Assert.Equal("api/v1/todo", sds.Route.Value);
                    Assert.Equal("TodoService", sds.Identifier.Text);
                },
                t =>
                {
                    Assert.Equal(SyntaxKind.UsingDirective, t.Kind);
                    var us = Assert.IsAssignableFrom<UsingDirectiveSyntax>(t);

                    Assert.Equal(SyntaxKind.Namespace, us.Namespace.Kind);
                    Assert.Equal("System", us.Namespace.Name);
                    Assert.Null(us.Namespace.Alias);
                },
                t =>
                {
                    Assert.Equal(SyntaxKind.UsingDirective, t.Kind);
                    var us = Assert.IsAssignableFrom<UsingDirectiveSyntax>(t);

                    Assert.Equal(SyntaxKind.Namespace, us.Namespace.Kind);
                    Assert.Equal("System.Collections.Generic", us.Namespace.Name);
                    Assert.Null(us.Namespace.Alias);
                },
                t =>
                {
                    Assert.Equal(SyntaxKind.UsingDirective, t.Kind);
                    var us = Assert.IsAssignableFrom<UsingDirectiveSyntax>(t);

                    Assert.Equal(SyntaxKind.Namespace, us.Namespace.Kind);
                    Assert.Equal("System.Collections.Generic", us.Namespace.Name);
                    Assert.NotNull(us.Namespace.Alias);

                    Assert.Equal(SyntaxKind.IdentifierToken, us.Namespace.Alias.Kind);
                    Assert.Equal("Foo", us.Namespace.Alias.Text);
                }
            });
        }

        [Fact]
        public void ParseCSharpCodeBlock()
        {
            var text = @"
csharp
{
    private readonly IList<TodoItem> todoItems;

    TodoService()
    {
        todoItems = new List<TodoItem>();
    }
}";

            var syntaxTree = SyntaxTree.Parse(text);

            var compilationUnit = syntaxTree.Root;

            Assert.Empty(syntaxTree.Diagnostics);
            Assert.NotNull(compilationUnit);

            var member = Assert.Single(compilationUnit.Members);

            Assert.Equal(SyntaxKind.CSharpBlock, member.Kind);
            var chsarpBlock = Assert.IsAssignableFrom<CSharpBlockSyntax>(member);

            Assert.Equal(SyntaxKind.CodeBlock, chsarpBlock.CodeBlock.Kind);

            var codeText = text.Substring(chsarpBlock.CodeBlock.Span.Start, chsarpBlock.CodeBlock.Span.Length);
            Assert.Equal(codeText, chsarpBlock.CodeBlock.Text);

            Assert.Collection(chsarpBlock.Members, new Action<CSharpSyntax>[]
            {
                t =>
                {
                    Assert.Equal(CSharp.SyntaxKind.FieldDeclaration, t.CSharpStatement.Kind());
                    Assert.Equal("private readonly IList<TodoItem> todoItems;", syntaxTree.Text.ToString(t.Span));
                },
                t =>
                {
                    Assert.Equal(CSharp.SyntaxKind.ConstructorDeclaration, t.CSharpStatement.Kind());
                }
            });
        }

        [Fact]
        public void CSharpCodeBlockDiagnostics()
        {
            var text = @"
csharp
{
    private readonly IList<TodoItem> todoItems

    TodoService()
    {
        todoItems = new List<TodoItem>();
    }
}";

            var syntaxTree = SyntaxTree.Parse(text);

            var compilationUnit = syntaxTree.Root;

            Assert.Collection(syntaxTree.Diagnostics, new Action<Diagnostic>[]
            {
                t =>
                {
                    Assert.Equal(DiagnosticType.Error, t.DiagnosticType);
                    Assert.Equal(59, t.Location.Span.Start);
                    Assert.Equal(0, t.Location.Span.Length);
                    Assert.Equal("; expected", t.Message);
                }
            });
        }

        [Fact]
        public void ParseSimpleEndpointDeclaration()
        {
            var text = @"
get OkObjectResult ()
{
    return Ok(""Hello World"");
}";

            var syntaxTree = SyntaxTree.Parse(text);
            Assert.Empty(syntaxTree.Diagnostics);

            var compilationUnit = syntaxTree.Root;
            var member = Assert.Single(compilationUnit.Members);

            Assert.Equal(SyntaxKind.EndpointDeclaration, member.Kind);

            var endpointDefintion = Assert.IsAssignableFrom<EndpointDeclarationSyntax>(member);

            Assert.Equal(SyntaxKind.GetKeyword, endpointDefintion.HttpVerbKeyword.Kind);
            Assert.Equal("", endpointDefintion.Route.Value);
        }

        [Fact]
        public void ParseEndpointDeclaration()
        {
            var text = @"
/*
    @description: Returns all the todo items.
    #limit: Maximum number of elements to return.
    #skip: Number of elements to exclude from the beginning.
*/
get ""/"" ObjectResponse<TodoItem[]> ([Test] query int limit = 10, query int skip = 0, service CancellationToken cancellationToken = default)
{
    //This returns the filtered items
    return items.Skip(skip).Take(limit).ToArray();
}";

            var syntaxTree = SyntaxTree.Parse(text);

            var compilationUnit = syntaxTree.Root;

            Assert.Empty(syntaxTree.Diagnostics);
            Assert.NotNull(compilationUnit);

            var member = Assert.Single(compilationUnit.Members);

            Assert.Equal(SyntaxKind.EndpointDeclaration, member.Kind);

            var endpointDefintion = Assert.IsAssignableFrom<EndpointDeclarationSyntax>(member);

            Assert.Equal(SyntaxKind.GetKeyword, endpointDefintion.HttpVerbKeyword.Kind);
            Assert.Equal("/", endpointDefintion.Route.Value);

            var returnType = Assert.Single(endpointDefintion.ReturnTypes.Types);
            Assert.Equal("ObjectResponse<TodoItem[]>", returnType.TypeName);

            Assert.Collection(endpointDefintion.ParametersList.Parameters, new Action<EndpointParameterSyntax>[]
            {
                p =>
                {
                    Assert.Equal(SyntaxKind.QueryKeyword, p.BindLocation.Kind);
                    Assert.Equal("int", p.Type.TypeName);
                    Assert.Equal(SyntaxKind.IdentifierToken, p.Identifer.Kind);
                    Assert.Equal("limit", p.Identifer.Text);

                    Assert.NotNull(p.DefaultValue);
                    Assert.Equal(SyntaxKind.NumberToken, p.DefaultValue.Kind);
                    Assert.Equal(10, p.DefaultValue.Value);

                    var attribute = Assert.Single(p.AttributeList.Attributes);
                    Assert.Equal("Test",attribute.Identifier.Text);
                },
                p =>
                {
                    Assert.Equal(SyntaxKind.QueryKeyword, p.BindLocation.Kind);
                    Assert.Equal("int", p.Type.TypeName);
                    Assert.Equal(SyntaxKind.IdentifierToken, p.Identifer.Kind);
                    Assert.Equal("skip", p.Identifer.Text);

                    Assert.NotNull(p.DefaultValue);
                    Assert.Equal(SyntaxKind.NumberToken, p.DefaultValue.Kind);
                    Assert.Equal(0, p.DefaultValue.Value);
                },
                p =>
                {
                    Assert.Equal(SyntaxKind.ServiceKeyword, p.BindLocation.Kind);
                    Assert.Equal("CancellationToken", p.Type.TypeName);
                    Assert.Equal(SyntaxKind.IdentifierToken, p.Identifer.Kind);
                    Assert.Equal("cancellationToken", p.Identifer.Text);

                    Assert.NotNull(p.DefaultValue);
                    Assert.Equal(SyntaxKind.DefaultKeyword, p.DefaultValue.Kind);
                }
            });

            Assert.Equal(SyntaxKind.CodeBlock, endpointDefintion.EndpointDeclarationBody.Kind);
            var statement = Assert.Single(endpointDefintion.Statements);

            Assert.Equal("return items.Skip(skip).Take(limit).ToArray();", syntaxTree.Text.ToString(statement.Span));
            Assert.Equal(CSharp.SyntaxKind.ReturnStatement, statement.CSharpStatement.Kind());
        }

        [Fact]
        public void ParseAttributes()
        {
            var text = @"
[Authorize]
[Authorize(""Administrator"")]
[Authorize(""Administrator"", Policy = ""RequireAdministratorRole"")]
service ""api/v1/todo"" TodoService;";

            var syntaxTree = SyntaxTree.Parse(text);

            var compilationUnit = syntaxTree.Root;

            Assert.Empty(syntaxTree.Diagnostics);
            Assert.NotNull(compilationUnit);

            var member = Assert.Single(compilationUnit.Members);

            Assert.Equal(SyntaxKind.ServiceDeclaration, member.Kind);

            var serviceDeclaration = Assert.IsAssignableFrom<ServiceDeclarationSyntax>(member);

            Assert.NotNull(serviceDeclaration.AttributeList);

            Assert.Collection(serviceDeclaration.AttributeList.Attributes, new Action<AttributeSyntax>[]
            {
                t =>
                {
                    Assert.Equal("Authorize", t.Identifier.Text);
                    Assert.Empty(t.ArgumentList.Arguments);
                },
                t =>
                {
                    Assert.Equal("Authorize", t.Identifier.Text);

                    var argument = Assert.Single(t.ArgumentList.Arguments);

                    Assert.Null(argument.Name);
                    Assert.Equal(SyntaxKind.StringToken, argument.Expression.Kind);
                    Assert.Equal("Administrator", argument.Expression.Value);
                },
                t =>
                {
                    Assert.Equal("Authorize", t.Identifier.Text);

                    Assert.Collection(t.ArgumentList.Arguments, new Action<AttributeArgumentSyntax>[]
                    {
                        a =>
                        {
                            Assert.Null(a.Name);
                            Assert.Equal(SyntaxKind.StringToken, a.Expression.Kind);
                            Assert.Equal("Administrator", a.Expression.Value);
                        },
                        a =>
                        {
                            Assert.Equal("Policy", a.Name.Text);
                            Assert.Equal(SyntaxKind.StringToken, a.Expression.Kind);
                            Assert.Equal("RequireAdministratorRole", a.Expression.Value);
                        }
                    });
                }
            });
        }

        [Fact]
        public void ParseSimpleServiceDeclaration()
        {
            var text = @"
service HelloWorldService;

get OkObjectResult ()
{
    return Ok(""Hello World"");
}";

            var syntaxTree = SyntaxTree.Parse(text);
            Assert.Empty(syntaxTree.Diagnostics);

            var compilationUnit = syntaxTree.Root;
            var serviceDeclaration = Assert.Single(compilationUnit.Members.OfType<ServiceDeclarationSyntax>());

            Assert.Equal(SyntaxKind.ServiceDeclaration, serviceDeclaration.Kind);
            Assert.Equal("", serviceDeclaration.Route.Value);

            var endpointDefintion = Assert.Single(compilationUnit.Members.OfType<EndpointDeclarationSyntax>());

            Assert.Equal(SyntaxKind.EndpointDeclaration, endpointDefintion.Kind);

            Assert.Equal(SyntaxKind.GetKeyword, endpointDefintion.HttpVerbKeyword.Kind);
            Assert.Equal("", endpointDefintion.Route.Value);
        }

        [Fact]
        public void ParseCompilationUnit()
        {
            var text = @"service ""api/v1/todo"" TodoService;

using System;
using System.Linq;
using System.Collections.Generic;

//Interop with C# language
csharp
{
    private readonly IList<TodoItem> todoItems;

    TodoService()
    {
        todoItems = new List<TodoItem>();
    }

    private int FindIndexById (Guid itemId)
    {
        var index = -1;
    
        for (var i = 0; i < todoItems.Count; i++)
        {
            if (todoItems[i].Id == itemId)
            {
                index = i;
                break;
            }
        }

        return index;
    }
}

/*
    @description: Returns all the todo items.
    #limit: Maximum number of elements to return.
    #skip: Number of elements to exclude from the beginning.
*/
get ""/"" ObjectResponse<TodoItem[]> (query int limit = 10, query int skip = 0)
{
    return items.Skip(skip).Take(limit).ToArray();
}

/*
    @description: Returns a todo item by its ID.
    #itemId: ID of the todo item.
*/
get ""/{itemId}"" ObjectResponse<TodoItem> | NotFoundResponse (route Guid itemId)
{
    var index = FindIndexById(itemId);
    
    if (index < 0)
    {
        // Item with the ID not found, return a not found error response.
        return NotFoundResponse($""TODO Item with ID: {itemId} not found."");
    }
    
    return todoItems[index];
}

/*
    #description: Adds a new todo item.
*/
[Authorize(Roles = ""Administrator"")]
post ""/"" SuccessResponse (body TodoItem newItem)
{
    todoItems.Add(newItem);
    
    return SuccessResponse();
}



/*
    @description: Updates a todo item.
    #itemId: ID of the todo item.
*/
[Authorize(Roles = ""Administrator"")]
put ""/{itemId}"" SuccessResponse | NotFoundResponse  (
    route Guid itemId,
    body TodoItem updateItem)
{
    var index = FindIndexById(itemId);
    
    if (index < 0)
    {
        // Item with the ID not found, return a not found error response.
        return NotFoundResponse($""TODO Item with ID: {itemId} not found."");
    }
    
    todoItems[index] = updateItem;
    
    return SuccessResponse();
}

/*
    @description: Deletes a todo item.
    #itemId: ID of the todo item.
*/
[Authorize(Roles = ""Administrator"")]
delete ""/{itemId}"" NoContentResponse | NotFoundResponse (route Guid itemId)
{
    var index = FindIndexById(itemId);
    
    if (index < 0)
    {
        // Item with the ID not found, return a not found error response.
        return NotFoundResponse($""TODO Item with ID: {itemId} not found."");
    }
    
    todoItems.RemoveAt(index);
    
    return NoContentResponse();
}";

            var syntaxTree = SyntaxTree.Parse(text);

            var compilationUnit = syntaxTree.Root;

            Assert.Empty(syntaxTree.Diagnostics);
            Assert.NotNull(compilationUnit);
        }
    }
}
