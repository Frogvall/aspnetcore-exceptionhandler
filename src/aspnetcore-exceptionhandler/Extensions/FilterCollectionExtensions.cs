using System;
using System.Net;
using Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling;
using Frogvall.AspNetCore.ExceptionHandling.Filters;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    public static class FilterCollectionExtensions
    {
        public static void AddApiExceptionFilter(this FilterCollection filters, params Action<Exception>[] exceptionListeners)
        {
            filters.Add(new ApiExceptionFilter(exceptionListeners));
        }

        public static void AddApiExceptionFilter(this FilterCollection filters, Func<ApiError, HttpStatusCode, Object> customErrorObjectFunction, params Action<Exception>[] exceptionListeners)
        {
            filters.Add(new ApiExceptionFilter(customErrorObjectFunction, exceptionListeners));
        }
    }
}
