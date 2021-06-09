using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Express.Net
{
    public class Response : IResult
    {
        public Response(object? value = null, int statusCode = 200)
        {
            Value = value;
            StatusCode = statusCode;
        }

        public int StatusCode { get; }

        public object? Value { get; }

        public Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = StatusCode;
            return httpContext.Response.WriteAsJsonAsync(Value);
        }
    }

    public class Redirect : IResult
    {
        public Redirect(string url, bool permanent = false)
        {
            Url = url;
            Permanent = permanent;
        }

        public string Url { get; }

        public bool Permanent { get; }

        public Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.Redirect(Url, Permanent);

            return Task.CompletedTask;
        }
    }
}
