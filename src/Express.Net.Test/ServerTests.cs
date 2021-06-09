using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Express.Net.Test
{
    public record TodoItem(Guid Id, string Description);

    [Route("api/v1/todo")]
    public class TodoService : ControllerBase
    {
        private static readonly IList<TodoItem> todoItems = new List<TodoItem>()
        {
            new(Guid.NewGuid(), "Test Item 1"),
            new(Guid.NewGuid(), "Test Item 2"),
            new(Guid.NewGuid(), "Test Item 3"),
            new(Guid.NewGuid(), "Test Item 4"),
            new(Guid.NewGuid(), "Test Item 5"),
        };

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TodoItem>), 200)]
        public IResult Get0([FromQuery] int limit = 10, [FromQuery] int skip = 0)
        {
            return Ok(todoItems.Skip(skip).Take(limit));
        }

        [HttpGet("{itemId}")]
        [ProducesResponseType(typeof(TodoItem), 200)]
        [ProducesResponseType(404)]
        public IResult Get1([FromRoute]Guid itemId)
        {
            var item = todoItems.Where(i => i.Id == itemId).FirstOrDefault();

            if (item is TodoItem)
                return Ok(item);

            return NotFound();
        }

        [HttpPost]
        [ProducesResponseType(typeof(TodoItem), 201)]
        public IResult Post0([FromBody] TodoItem item)
        {
            if (item.Id == Guid.Empty)
            {
                item = item with { Id = Guid.NewGuid() };
            }

            todoItems.Add(item);

            return Created(item);
        }

        [HttpDelete("{itemId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IResult Delete0([FromRoute] Guid itemId)
        {
            var item = todoItems.Where(i => i.Id == itemId).FirstOrDefault();

            if (item is TodoItem)
            {
                todoItems.Remove(item);
                return NoContent();
            }

            return NotFound();
        }
    }

    public class ServerTests
    {
        [Fact]
        public async Task SwaggerTest()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();

                    webHost.ConfigureServices(services =>
                    {
                        services.AddRouting();
                        services.AddExpressSwagger("Todo Service", "V1");
                    });

                    webHost.Configure(app =>
                    {
                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapExpressController();
                            endpoints.MapExpressSwagger();
                        });
                    });
                });

            var host = await hostBuilder.StartAsync();

            var client = host.GetTestClient();

            var openApiJson = await client.GetStringAsync("swagger/documents/swagger.json");

            Assert.NotEmpty(openApiJson);
        }

        [Fact]
        public async Task TodoServiceTests()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();

                    webHost.ConfigureServices(services =>
                    {
                        services.AddRouting();
                    });

                    webHost.Configure(app =>
                    {
                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapExpressController();
                        });
                    });
                });

            var host = await hostBuilder.StartAsync();

            var client = host.GetTestClient();

            var response = await client.GetFromJsonAsync<TodoItem[]>("api/v1/todo");

            Assert.NotNull(response);
            Assert.Equal(5, response.Length);

            response = await client.GetFromJsonAsync<TodoItem[]>("api/v1/todo?skip=1");

            Assert.NotNull(response);
            Assert.Equal(4, response.Length);

            response = await client.GetFromJsonAsync<TodoItem[]>("api/v1/todo?limit=3");

            Assert.NotNull(response);
            Assert.Equal(3, response.Length);

            response = await client.GetFromJsonAsync<TodoItem[]>("api/v1/todo?skip=3&limit=3");

            Assert.NotNull(response);
            Assert.Equal(2, response.Length);

            var newItem = new TodoItem(Guid.NewGuid(), "Test Item 6");

            var createdResponse = await client.PostAsJsonAsync("api/v1/todo", newItem);

            var todoItem = await createdResponse.Content.ReadFromJsonAsync<TodoItem>();

            Assert.Equal(newItem, todoItem);

            response = await client.GetFromJsonAsync<TodoItem[]>("api/v1/todo");

            Assert.NotNull(response);
            Assert.Equal(6, response.Length);

            var TodoItemresponse = await client.GetAsync($"api/v1/todo/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, TodoItemresponse.StatusCode);
        }
    }
}