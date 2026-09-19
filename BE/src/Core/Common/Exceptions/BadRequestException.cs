using System.Net;
using System.Runtime.Serialization;

namespace HuyHieuDang.Core.Common.Exceptions
{
    [Serializable]
    public sealed class BadRequestException : HttpCustomException
    {
        public BadRequestException(string? message, Exception? innerException)
            : base(message, innerException)
        {
            StatusCode = HttpStatusCode.BadRequest;
        }

        public BadRequestException(string? message)
            : base(message)
        {
            StatusCode = HttpStatusCode.BadRequest;
        }

        private BadRequestException()
        {
        }

        private BadRequestException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}