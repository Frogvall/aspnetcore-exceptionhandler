using System;
using System.Net;

namespace Frogvall.AspNetCore.ExceptionHandling.Exceptions
{
    [Obsolete("You should migrate to using ExceptionMapper and BaseApiExceptions. ApiExceptions will be removed in the future.")]
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }
        public ApiException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public ApiException(HttpStatusCode statusCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
