using System;
using System.IO;
using System.Threading.Tasks;
using Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Frogvall.AspNetCore.ExceptionHandling.Filters
{
    public class ApiExceptionFilter : TypeFilterAttribute
    {
        public ApiExceptionFilter(params Action<Exception>[] exceptionListeners) : base(typeof(ApiExceptionFilterImpl))
        {
            // ReSharper disable once CoVariantArrayConversion
            Arguments = new object[] { exceptionListeners };
        }

        private class ApiExceptionFilterImpl : ExceptionFilterAttribute
        {
            private readonly IExceptionMapper _mapper;
            private readonly IHostEnvironment _env;
            private readonly ILogger<ApiExceptionFilter> _logger;
            private readonly Action<Exception>[] _exceptionListeners;
            private readonly JsonSerializerOptions _serializerOptions;

            public ApiExceptionFilterImpl(IExceptionMapper mapper, IHostEnvironment env,
                ILogger<ApiExceptionFilter> logger, Action<Exception>[] exceptionListeners)
            {
                _mapper = mapper;
                _env = env;
                _logger = logger;
                _exceptionListeners = exceptionListeners;
                _serializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    IgnoreNullValues = true
                };
            }

            public override async Task OnExceptionAsync(ExceptionContext context)
            {
                var ex = context.Exception;
                if (ex == null) return;

                var error = ApiErrorFactory.Build(context.HttpContext, ex, _mapper, _logger, _env.IsDevelopment(), _exceptionListeners);
                await JsonSerializer.SerializeAsync(context.HttpContext.Response.Body, error, _serializerOptions);

                context.ExceptionHandled = true;
            }
        }
    }
}