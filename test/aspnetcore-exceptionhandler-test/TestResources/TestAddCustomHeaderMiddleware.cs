using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Frogvall.AspNetCore.ExceptionHandling.Test.TestResources
{
    class TestAddCustomHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public static string TestHeader => "x-test-header";

        public TestAddCustomHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var testHeaderValue = context.Request.Headers[TestHeader];
            context.Response.Headers.TryAdd(TestHeader, testHeaderValue);
            await _next(context);
        }
    }
}
