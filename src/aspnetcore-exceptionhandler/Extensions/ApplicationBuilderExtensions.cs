using Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseExceptionHandler(new ExceptionHandlerOptions
            {
                ExceptionHandler = new ApiExceptionHandler(
                    builder.ApplicationServices.GetRequiredService<IExceptionMapper>(),
                    builder.ApplicationServices.GetRequiredService<IHostingEnvironment>(),
                    builder.ApplicationServices.GetRequiredService<ILogger<ApiExceptionHandler>>())
                    .ExceptionHandler
            });
        }
    }
}
