using System;

namespace Express.Net
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ProducesResponseTypeAttribute : Attribute
    {
        public ProducesResponseTypeAttribute(int statusCode)
        {
            StatusCode = statusCode;
        }

        public ProducesResponseTypeAttribute(Type type, int statusCode)
        {
            Type = type;
            StatusCode = statusCode;
        }

        public int StatusCode { get; set; }

        public Type? Type { get; set; }
    }
}
