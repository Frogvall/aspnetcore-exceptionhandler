using System;
using System.IO;
using System.Threading.Tasks;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling
{
    internal class ApiExceptionHandler
    {
        private readonly IExceptionMapper _mapper;
        private readonly IHostingEnvironment _env;
        private readonly ILogger<ApiExceptionHandler> _logger;
        private readonly Action<Exception>[] _exceptionListeners;
        private readonly JsonSerializer _serializer;

        internal ApiExceptionHandler(IExceptionMapper mapper, IHostingEnvironment env,
            ILogger<ApiExceptionHandler> logger, Action<Exception>[] exceptionListeners)
        {
            _mapper = mapper;
            _env = env;
            _logger = logger;
            _exceptionListeners = exceptionListeners;
            _serializer = new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        internal async Task ExceptionHandler(HttpContext context)
        {
            var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            if (ex == null) return;

            var error = ApiErrorFactory.Build(context, ex, _mapper, _logger, _env.IsDevelopment(), _exceptionListeners);

            using (var writer = new StreamWriter(context.Response.Body))
            {
                _serializer.Serialize(writer, error);
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}
