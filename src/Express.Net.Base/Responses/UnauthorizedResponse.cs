namespace Express.Net.System.Responses
{
    public class UnauthorizedResponse : ErrorResponse
    {
        public UnauthorizedResponse(string error)
            : base(error)
        {
        }
    }
}
