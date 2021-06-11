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

get Ok<string> ()
{
    return Ok(""Hello World!"");
}".Trim();

            var csharpCode = @"
using Express.Net;
using System.Threading.Tasks;

namespace ProjectName.Controllers
{
    [Route("""")]
    public class HelloWorldService : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
        public IResult __get0()
        {
            return Ok(""Hello World!"");
        }
    }
}".Trim();

            var syntaxTree = SyntaxTree.Parse(code);

            Assert.Empty(syntaxTree.Diagnostics);

            var transformedSyntaxTree = syntaxTree.Transform("ProjectName", false, out _);

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

get Ok<string> ()
{
    await Task.Delay(10);
    return Ok(""Hello World!"");
}".Trim();

            var csharpCode = @"
using Express.Net;
using System.Threading.Tasks;

namespace ProjectName.Controllers
{
    [Route("""")]
    public class HelloWorldService : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IResult> __get0Async()
        {
            await Task.Delay(10);
            return Ok(""Hello World!"");
        }
    }
}".Trim();

            var syntaxTree = SyntaxTree.Parse(code);

            Assert.Empty(syntaxTree.Diagnostics);

            var transformedSyntaxTree = syntaxTree.Transform("ProjectName", false, out _);

            Assert.Empty(syntaxTree.Diagnostics);
            Assert.Empty(transformedSyntaxTree.GetDiagnostics());

            var generatedCode = transformedSyntaxTree.ToString();

            Assert.Equal(csharpCode, generatedCode);
        }

        [Fact]
        public void TestBasicDebugInfomation()
        {
            var text = @"
service HelloWorldService;

get Ok<string> ()
{
    return Ok(""Hello World"");
}

get ""{name}"" Ok<string> (route string name)
{
	var message = $""Hello {name}"";
    return Ok(message);
}".Trim();

            var csharpCode = @"
using Express.Net;
using System.Threading.Tasks;

namespace ProjectName.Controllers
{
    [Route("""")]
#line 1 ""Filename.en""
    public class HelloWorldService : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
#line 3
        public IResult __get0()
        {
#line 5
            return Ok(""Hello World"");
        }

        [HttpGet(""{name}"")]
        [ProducesResponseType(typeof(string), 200)]
#line 8
        public IResult __getname1([FromRoute] string name)
        {
#line 10
            var message = $""Hello {name}"";
#line 11
            return Ok(message);
        }
    }
}".Trim();

            var syntaxTree = SyntaxTree.Parse(text, "Filename.en");

            Assert.Empty(syntaxTree.Diagnostics);

            var transformedSyntaxTree = syntaxTree.Transform("ProjectName", true, out _);

            Assert.Empty(syntaxTree.Diagnostics);
            Assert.Empty(transformedSyntaxTree.GetDiagnostics());

            var generatedCode = transformedSyntaxTree.ToString();
            Assert.Equal(csharpCode, generatedCode);
        }

        [Fact]
        public void TestCSBlockDebugInfomation()
        {
            var text = @"
service HelloWorldService;

csharp
{
    private readonly string _text;

    public HelloWorldService()
    {

        _text = ""Hello World"";

    }
}

get Ok<string> ()
{
    return Ok(_text);
}".Trim();

            var csharpCode = @"
using Express.Net;
using System.Threading.Tasks;

namespace ProjectName.Controllers
{
    [Route("""")]
#line 1 ""Filename.en""
    public class HelloWorldService : ControllerBase
    {
#line 5
        private readonly string _text;
#line 7
        public HelloWorldService()
#line 8
        {
#line 10
            _text = ""Hello World"";
#line 12
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
#line 15
        public IResult __get0()
        {
#line 17
            return Ok(_text);
        }
    }
}".Trim();

            var syntaxTree = SyntaxTree.Parse(text, "Filename.en");

            Assert.Empty(syntaxTree.Diagnostics);

            var transformedSyntaxTree = syntaxTree.Transform("ProjectName", true, out _);

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
get Ok<TodoItem[]> (query int limit = 10, query int skip = 0)
{
    return Ok(todoItems.Skip(skip).Take(limit));
}

/*
    @description: Returns a todo item by its ID.
    #itemId: ID of the todo item.
*/
get ""{itemId}"" Ok<TodoItem> | NotFound<string> (route Guid itemId)
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
post Ok (body TodoItem newItem)
{
    todoItems.Add(newItem);
    
    return Ok();
}

/*
    @description: Updates a todo item.
    #itemId: ID of the todo item.
*/
put ""{itemId}"" Ok<TodoItem> | NotFound<string>  (
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
delete ""{itemId}"" NoContent | NotFound<string> (route Guid itemId)
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

            var csharpCode = @"
using System;
using System.Linq;
using System.Collections.Generic;
using Express.Net;
using System.Threading.Tasks;

namespace ProjectName.Controllers
{
    [Route(""api/v1/todo"")]
#line 1 ""Filename.en""
    public class TodoService : ControllerBase
    {
#line 10
        public record TodoItem(Guid Id, string Description);
#line 12
        private static readonly IList<TodoItem> todoItems = new List<TodoItem>()
#line 13
        {
#line 14
        new(Guid.NewGuid(), ""Test Item 1""), 
#line 15
        new(Guid.NewGuid(), ""Test Item 2""), 
#line 16
        new(Guid.NewGuid(), ""Test Item 3""), 
#line 17
        new(Guid.NewGuid(), ""Test Item 4""), 
#line 18
        new(Guid.NewGuid(), ""Test Item 5""), 
#line 19
        };
#line 21
        private static int FindIndexById(Guid itemId)
#line 22
        {
#line 23
            var index = -1;
#line 25
            for (var i = 0; i < todoItems.Count; i++)
#line 26
            {
#line 27
                if (todoItems[i].Id == itemId)
#line 28
                {
#line 29
                    index = i;
#line 30
                    break;
#line 31
                }
#line 32
            }

#line 34
            return index;
#line 35
        }

        [HttpGet]
        [ProducesResponseType(typeof(TodoItem[]), 200)]
#line 43
        public IResult __get0([FromQuery] int limit = 10, [FromQuery] int skip = 0)
        {
#line 45
            return Ok(todoItems.Skip(skip).Take(limit));
        }

        [HttpGet(""{itemId}"")]
        [ProducesResponseType(typeof(TodoItem), 200)]
        [ProducesResponseType(typeof(string), 404)]
#line 52
        public IResult __getitemId1([FromRoute] Guid itemId)
        {
#line 54
            var index = FindIndexById(itemId);
#line 56
            if (index < 0)
#line 57
            {
#line 59
                return NotFound($""TODO Item with ID: {itemId} not found."");
#line 60
            }

#line 62
            return Ok(todoItems[index]);
        }

        [HttpPost]
        [ProducesResponseType(200)]
#line 68
        public IResult __post2([FromBody] TodoItem newItem)
        {
#line 70
            todoItems.Add(newItem);
#line 72
            return Ok();
        }

        [HttpPut(""{itemId}"")]
        [ProducesResponseType(typeof(TodoItem), 200)]
        [ProducesResponseType(typeof(string), 404)]
#line 79
        public IResult __putitemId3([FromRoute] Guid itemId, [FromBody] TodoItem updateItem)
        {
#line 83
            var index = FindIndexById(itemId);
#line 85
            if (index < 0)
#line 86
            {
#line 88
                return NotFound($""TODO Item with ID: {itemId} not found."");
#line 89
            }

#line 91
            todoItems[index] = updateItem with {Id = itemId};
#line 93
            return Ok();
        }

        [HttpDelete(""{itemId}"")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
#line 100
        public IResult __deleteitemId4([FromRoute] Guid itemId)
        {
#line 102
            var index = FindIndexById(itemId);
#line 104
            if (index < 0)
#line 105
            {
#line 107
                return NotFound($""TODO Item with ID: {itemId} not found."");
#line 108
            }

#line 110
            todoItems.RemoveAt(index);
#line 112
            return NoContent();
        }
    }
}".Trim();

            var syntaxTree = SyntaxTree.Parse(text, "Filename.en");

            Assert.Empty(syntaxTree.Diagnostics);

            var transformedSyntaxTree = syntaxTree.Transform("ProjectName", true, out _);

            Assert.Empty(syntaxTree.Diagnostics);
            Assert.Empty(transformedSyntaxTree.GetDiagnostics());

            var generatedCode = transformedSyntaxTree.ToString();
            Assert.Equal(csharpCode, generatedCode);
        }
    }

}
