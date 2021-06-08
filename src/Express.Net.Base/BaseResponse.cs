using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Express.Net
{
    public class BaseResponse : IResult
    {
        public BaseResponse(object? value = null, int statusCode = 200)
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
}
