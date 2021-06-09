namespace Express.Net
{
    public class ControllerBase
    {
        protected virtual IResult Ok()
        {
            return new Response();
        }

        protected virtual IResult Ok(object? value)
        {
            return new Response(value);
        }

        protected virtual IResult Created()
        {
            return new Response(statusCode: 201);
        }

        protected virtual IResult Created(object? value)
        {
            return new Response(value, 201);
        }

        protected virtual IResult NotFound()
        {
            return new Response(statusCode: 404);
        }

        protected virtual IResult NotFound(object? value)
        {
            return new Response(value, 404);
        }

        protected virtual IResult BadRequest()
        {
            return new Response(statusCode: 401);
        }

        protected virtual IResult BadRequest(object? value)
        {
            return new Response(value, 401);
        }

        protected virtual IResult Redirect(string url, bool permanent = false)
        {
            return new Redirect(url, permanent);
        }

        protected virtual IResult NoContent()
        {
            return new Response(statusCode: 204);
        }

        protected virtual IResult StatusCode(int statusCode)
        {
            return new Response(statusCode: statusCode);
        }

        protected virtual IResult StatusCode(object? value, int statusCode)
        {
            return new Response(value, statusCode);
        }
    }
}
