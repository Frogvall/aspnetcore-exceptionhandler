using System;
using System.Net;

namespace Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling
{
    internal sealed record CustomApiErrorResolver
    {
        private readonly Func<ApiError, HttpStatusCode, object> _customApiErrorFunction;

        internal CustomApiErrorResolver(Func<ApiError, HttpStatusCode, object> customApiErrorFunction)
        {
            _customApiErrorFunction = customApiErrorFunction;
        }

        internal object Resolve(ApiError error, HttpStatusCode statusCode) 
        {
            return _customApiErrorFunction(error, statusCode);
        }
    }
}