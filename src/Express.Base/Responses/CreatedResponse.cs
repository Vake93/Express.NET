namespace Express.Net.System.Responses
{
    public class CreatedResponse : SuccessResponse
    {
        public CreatedResponse(string id)
        {
            Id = id;
        }

        public string Id { get; init; }
    }
}
