using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Express.Net
{
    public class BaseResponse : IResult
    {
        public object? Value { get; }

        public BaseResponse(object? value)
        {
            Value = value;
        }

        public Task ExecuteAsync(HttpContext httpContext)
        {
            return httpContext.Response.WriteAsJsonAsync(Value);
        }
    }
}
