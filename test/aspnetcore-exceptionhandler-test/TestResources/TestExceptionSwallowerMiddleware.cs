using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Frogvall.AspNetCore.ExceptionHandling.Test.TestResources
{
    class TestExceptionSwallowerMiddleware
    {
        private readonly RequestDelegate _next;

        public TestExceptionSwallowerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch
            {
                //swallow all exceptions
            }
        }
    }
}
