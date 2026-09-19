using System.Net;
using System.Runtime.Serialization;

namespace HuyHieuDang.Core.Common.Exceptions
{
    [Serializable]
    public sealed class InternalServerException : HttpCustomException
    {
        public InternalServerException(string? message, Exception? innerException)
            : base(message, innerException)
        {
            StatusCode = HttpStatusCode.InternalServerError;
        }

        public InternalServerException(string? message)
            : base(message)
        {
            StatusCode = HttpStatusCode.InternalServerError;
        }

        private InternalServerException()
        {
        }

        private InternalServerException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}