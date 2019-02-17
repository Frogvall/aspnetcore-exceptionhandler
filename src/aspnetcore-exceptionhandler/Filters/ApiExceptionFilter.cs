using System.IO;
using System.Threading.Tasks;
using Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Frogvall.AspNetCore.ExceptionHandling.Filters
{
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        private readonly IExceptionMapper _mapper;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<ApiExceptionFilter> _logger;
        private readonly JsonSerializer _serializer;

        public ApiExceptionFilter(IExceptionMapper mapper, IHostingEnvironment env,
            ILogger<ApiExceptionFilter> logger)
        {
            _mapper = mapper;
            _env = env;
            _logger = logger;
            _serializer = new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            var ex = context.Exception;
            if (ex == null) return;

            var error = ApiErrorFactory.Build(context.HttpContext, ex, _mapper, _logger, _env.IsDevelopment());

            using (var writer = new StreamWriter(context.HttpContext.Response.Body))
            {
                _serializer.Serialize(writer, error);
                await writer.FlushAsync().ConfigureAwait(false);
            }

            context.ExceptionHandled = true;
        }
    }
}