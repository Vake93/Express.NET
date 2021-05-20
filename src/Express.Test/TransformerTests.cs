using Xunit;
using SyntaxTree = Express.Net.CodeAnalysis.SyntaxTree;

namespace Express.Net.Tests
{
    public class TransformerTests
    {
        [Fact]
        public void ValidateSyncEndpointTransformation()
        {
            var code = @"
service HelloWorldService;

get OkObjectResult ()
{
    return Ok(""Hello World!"");
}".Trim();

            var csharpCode = @"
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ProjectName.Controllers
{
    [ApiController, Route("""")]
    public class HelloWorldService : ControllerBase
    {
        [HttpGet]
        public IActionResult __Get0()
        {
            return Ok(""Hello World!"");
        }
    }
}".Trim();
            var syntaxTree = SyntaxTree.Parse(code);

            Assert.Empty(syntaxTree.Diagnostics);

            var transformedSyntaxTree = syntaxTree.Transform(out _);

            Assert.Empty(syntaxTree.Diagnostics);
            Assert.Empty(transformedSyntaxTree.GetDiagnostics());

            var generatedCode = transformedSyntaxTree.ToString();

            Assert.Equal(csharpCode, generatedCode);
        }

        [Fact]
        public void ValidateAsyncEndpointTransformation()
        {
            var code = @"
service HelloWorldService;

get OkObjectResult ()
{
    await Task.Delay(10);
    return Ok(""Hello World!"");
}".Trim();

            var csharpCode = @"
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ProjectName.Controllers
{
    [ApiController, Route("""")]
    public class HelloWorldService : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> __Get0Async()
        {
            await Task.Delay(10);
            return Ok(""Hello World!"");
        }
    }
}".Trim();
            var syntaxTree = SyntaxTree.Parse(code);

            Assert.Empty(syntaxTree.Diagnostics);

            var transformedSyntaxTree = syntaxTree.Transform(out _);

            Assert.Empty(syntaxTree.Diagnostics);
            Assert.Empty(transformedSyntaxTree.GetDiagnostics());

            var generatedCode = transformedSyntaxTree.ToString();

            Assert.Equal(csharpCode, generatedCode);
        }

        [Fact]
        public void TestFullTransformation()
        {
            var text = @"
service ""api/v1/todo"" TodoService;

using System;
using System.Linq;
using System.Collections.Generic;

//Interop with C# language
csharp
{
    public record TodoItem(Guid Id, string Description);

    private static readonly IList<TodoItem> todoItems = new List<TodoItem>()
    {
        new (Guid.NewGuid(), ""Test Item 1""),
        new (Guid.NewGuid(), ""Test Item 2""),
        new (Guid.NewGuid(), ""Test Item 3""),
        new (Guid.NewGuid(), ""Test Item 4""),
        new (Guid.NewGuid(), ""Test Item 5""),
    };

    private static int FindIndexById(Guid itemId)
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
get OkObjectResult (query int limit = 10, query int skip = 0)
{
    return Ok(todoItems.Skip(skip).Take(limit));
}

/*
    @description: Returns a todo item by its ID.
    #itemId: ID of the todo item.
*/
get ""{itemId}"" OkObjectResult | NotFoundObjectResult (route Guid itemId)
{
    var index = FindIndexById(itemId);
    
    if (index < 0)
    {
        // Item with the ID not found, return a not found error response.
        return NotFound($""TODO Item with ID: {itemId} not found."");
    }
    
    return Ok(todoItems[index]);
}

/*
    #description: Adds a new todo item.
*/
post OkObjectResult (body TodoItem newItem)
{
    todoItems.Add(newItem);
    
    return Ok();
}

/*
    @description: Updates a todo item.
    #itemId: ID of the todo item.
*/
put ""{itemId}"" OkObjectResult | NotFoundObjectResult  (
    route Guid itemId,
    body TodoItem updateItem)
{
    var index = FindIndexById(itemId);
    
    if (index < 0)
    {
        // Item with the ID not found, return a not found error response.
        return NotFound($""TODO Item with ID: {itemId} not found."");
    }
    
    todoItems[index] = updateItem with { Id = itemId };
    
    return Ok();
}

/*
    @description: Deletes a todo item.
    #itemId: ID of the todo item.
*/
delete ""{itemId}"" NoContentResult | NotFoundObjectResult (route Guid itemId)
{
    var index = FindIndexById(itemId);
    
    if (index < 0)
    {
        // Item with the ID not found, return a not found error response.
        return NotFound($""TODO Item with ID: {itemId} not found."");
    }
    
    todoItems.RemoveAt(index);
    
    return NoContent();
}".Trim();

            var syntaxTree = SyntaxTree.Parse(text);

            Assert.Empty(syntaxTree.Diagnostics);

            var transformedSyntaxTree = syntaxTree.Transform(out _);

            Assert.Empty(syntaxTree.Diagnostics);
            Assert.Empty(transformedSyntaxTree.GetDiagnostics());
        }
    }

}
