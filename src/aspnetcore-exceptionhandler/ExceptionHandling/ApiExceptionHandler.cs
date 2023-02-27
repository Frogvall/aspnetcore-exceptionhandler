using System;
using System.Threading.Tasks;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Net;

namespace Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling
{
    internal class ApiExceptionHandler
    {
        private readonly IExceptionMapper _mapper;
        private readonly IHostEnvironment _env;
        private readonly ILogger<ApiExceptionHandler> _logger;
        private readonly CustomApiErrorResolver _resolver;
        private readonly Action<Exception>[] _exceptionListeners;
        private readonly JsonSerializerOptions _serializerOptions;

        internal ApiExceptionHandler(IExceptionMapper mapper, IHostEnvironment env,
            ILogger<ApiExceptionHandler> logger, CustomApiErrorResolver resolver, Action<Exception>[] exceptionListeners)
        {
            _mapper = mapper;
            _env = env;
            _logger = logger;
            _resolver = resolver;
            _exceptionListeners = exceptionListeners;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        internal async Task ExceptionHandler(HttpContext context)
        {
            var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            if (ex == null) return;

            var error = ApiErrorFactory.Build(context, ex, _mapper, _logger, _env.IsDevelopment(), _exceptionListeners);
            
            if (_resolver == null)
            {
                await JsonSerializer.SerializeAsync(context.Response.Body, error, _serializerOptions);
            }
            else
            {   
                var customErrorObject = _resolver.Resolve(error, (HttpStatusCode)context.Response.StatusCode);
                await JsonSerializer.SerializeAsync(context.Response.Body, customErrorObject, _serializerOptions);
            }
        }
    }
}
