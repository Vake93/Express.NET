namespace Express.Net.System.Responses
{
    public class ForbiddenResponse : ErrorResponse 
    {
        public ForbiddenResponse(string error)
            : base(error)
        {
        }
    }
}
