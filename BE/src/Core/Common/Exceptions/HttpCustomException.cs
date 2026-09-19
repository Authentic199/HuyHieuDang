using System.Net;
using System.Runtime.Serialization;

namespace HuyHieuDang.Core.Common.Exceptions
{
    [Serializable]
    public class HttpCustomException : CustomException
    {
        public HttpCustomException()
        {
        }

        public HttpCustomException(string? message)
            : base(message)
        {
            Value = new { message };
        }

        public HttpCustomException(string? message, Exception? innerException)
            : base(message, innerException)
        {
            Value = new { message };
        }

        public HttpStatusCode StatusCode { get; set; }

        public object? Value { get; set; }

        protected HttpCustomException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}