using HuyHieuDang.Core.Common.Exceptions;
using System.Net;
using System.Runtime.Serialization;

namespace HuyHieuDang.Infrastructure.Exceptions.HttpExceptions
{
    [Serializable]
    public class UnAuthorizedException : HttpCustomException
    {
        public UnAuthorizedException()
        {
            StatusCode = HttpStatusCode.Unauthorized;
        }

        public UnAuthorizedException(string? message, Exception? innerException)
            : base(message, innerException)
        {
            StatusCode = HttpStatusCode.Unauthorized;
        }

        public UnAuthorizedException(string? message)
            : base(message)
        {
            StatusCode = HttpStatusCode.Unauthorized;
        }

        protected UnAuthorizedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}