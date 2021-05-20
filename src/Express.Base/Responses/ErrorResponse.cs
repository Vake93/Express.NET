namespace Express.Net.System.Responses
{
    public abstract class ErrorResponse : BaseResponse
    {
        public ErrorResponse(string error)
        {
            Error = error;
        }

        public string Error { get; init; }
    }
}
