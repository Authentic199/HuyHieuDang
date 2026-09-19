using HuyHieuDang.Core.Common.Exceptions;
using System.Net;
using System.Runtime.Serialization;

namespace HuyHieuDang.Infrastructure.Exceptions.HttpExceptions
{
    [Serializable]
    public sealed class ForbiddenException : HttpCustomException
    {
        public ForbiddenException()
        {
            StatusCode = HttpStatusCode.Forbidden;
        }

        public ForbiddenException(string? message, Exception? innerException)
            : base(message, innerException)
        {
            StatusCode = HttpStatusCode.Forbidden;
        }

        public ForbiddenException(string? message)
            : base(message)
        {
            StatusCode = HttpStatusCode.Forbidden;
        }

        private ForbiddenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}