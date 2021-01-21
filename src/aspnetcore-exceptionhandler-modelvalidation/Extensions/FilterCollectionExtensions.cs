using System;
using Frogvall.AspNetCore.ExceptionHandling.Filters;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    public static class FilterCollectionExtensions
    {
        public static void AddValidateModelFilter(this FilterCollection filters)
        {
            filters.Add(new ValidateModelFilter());
        }

        public static void AddValidateModelFilter(this FilterCollection filters, int errorCode)
        {
            filters.Add(new ValidateModelFilter { ErrorCode = errorCode });
        }
    }
}