using Frogvall.AspNetCore.ExceptionHandling.Middleware;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseExceptionStatusCodeDecorator(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionStatusCodeDecoratorMiddleware>();
        }
    }
}
