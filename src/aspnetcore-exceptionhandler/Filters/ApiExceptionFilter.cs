using System;
using System.Threading.Tasks;
using Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net;

namespace Frogvall.AspNetCore.ExceptionHandling.Filters
{
    public class ApiExceptionFilter : TypeFilterAttribute
    {
        public ApiExceptionFilter(params Action<Exception>[] exceptionListeners) : base(typeof(ApiExceptionFilterImpl))
        {
            // ReSharper disable once CoVariantArrayConversion
            Arguments = new object[] { exceptionListeners };
        }
            
        public ApiExceptionFilter(Func<ApiError, HttpStatusCode, Object> customErrorObjectFunction, params Action<Exception>[] exceptionListeners) : base(typeof(ApiExceptionFilterWithResolverImpl))
        {
            // ReSharper disable once CoVariantArrayConversion
            Arguments = new object[] { new CustomApiErrorResolver(customErrorObjectFunction), exceptionListeners };
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
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
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

        private class ApiExceptionFilterWithResolverImpl : ExceptionFilterAttribute
        {
            private readonly IExceptionMapper _mapper;
            private readonly IHostEnvironment _env;
            private readonly ILogger<ApiExceptionFilter> _logger;
            private readonly CustomApiErrorResolver _resolver;
            private readonly Action<Exception>[] _exceptionListeners;
            private readonly JsonSerializerOptions _serializerOptions;

            public ApiExceptionFilterWithResolverImpl(IExceptionMapper mapper, IHostEnvironment env,
                ILogger<ApiExceptionFilter> logger, CustomApiErrorResolver resolver, 
                Action<Exception>[] exceptionListeners)
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

            public override async Task OnExceptionAsync(ExceptionContext context)
            {
                var ex = context.Exception;
                if (ex == null) return;

                var error = ApiErrorFactory.Build(context.HttpContext, ex, _mapper, _logger, _env.IsDevelopment(), _exceptionListeners); 
                var customErrorObject = _resolver.Resolve(error, (HttpStatusCode)context.HttpContext.Response.StatusCode);
                await JsonSerializer.SerializeAsync(context.HttpContext.Response.Body, customErrorObject, _serializerOptions);
                context.ExceptionHandled = true;
            }
        }
    }
}