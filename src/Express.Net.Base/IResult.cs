using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Express.Net
{
    public interface IResult
    {
        Task ExecuteAsync(HttpContext httpContext);
    }
}
