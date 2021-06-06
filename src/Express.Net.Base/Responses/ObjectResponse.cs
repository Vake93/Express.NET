namespace Express.Net.System.Responses
{
    public class ObjectResponse<T> : SuccessResponse
    {
        public ObjectResponse(T data)
        {
            Data = data;
        }

        public T Data { get; init; }
    }
}
