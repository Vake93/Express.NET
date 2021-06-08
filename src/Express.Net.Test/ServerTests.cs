using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Express.Net.Test
{
    public record TodoItem(Guid Id, string Description);

    [Route("api/v1/todo")]
    public class TodoService
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
        public IResult __Get0([FromQuery] int limit = 10, [FromQuery] int skip = 0)
        {
            return new BaseResponse(todoItems.Skip(skip).Take(limit));
        }
    }

    public class ServerTests
    {
        [Fact]
        public async Task MapControllersTest()
        {
            var x = typeof(ServerTests).IsAssignableFrom(typeof(object));
            var y = typeof(object).IsAssignableFrom(typeof(object));

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
                            endpoints.MapHttpHandler<TodoService>();
                        });
                    });
                });

            var host = await hostBuilder.StartAsync();

            var client = host.GetTestClient();

            var response = await client.GetFromJsonAsync<TodoItem[]>("api/v1/todo");

            Assert.NotEmpty(response);
        }
    }
}