namespace Express.Net.System.Responses
{
    public class RedirectResponse : BaseResponse
    {
        public RedirectResponse(string url, bool permanent)
        {
            Url = url;
            Permanent = permanent;
        }

        public string Url { get; init; }

        public bool Permanent { get; init; }
    }
}
