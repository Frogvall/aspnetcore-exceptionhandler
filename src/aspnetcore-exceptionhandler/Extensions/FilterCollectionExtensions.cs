using System;
using Frogvall.AspNetCore.ExceptionHandling.Filters;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    public static class FilterCollectionExtensions
    {
        public static void AddApiExceptionFilter(this FilterCollection filters, params Action<Exception>[] exceptionListeners)
        {
            filters.Add(new ApiExceptionFilter(exceptionListeners));
        }
    }
}
