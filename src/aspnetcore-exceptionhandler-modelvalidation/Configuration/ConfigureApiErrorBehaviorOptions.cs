using Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using static System.Net.Mime.MediaTypeNames;

namespace Frogvall.AspNetCore.ExceptionHandling.Configuration
{
    public class ConfigureApiErrorBehaviorOptions : IConfigureNamedOptions<ApiBehaviorOptions>
    {
        private readonly IExceptionMapper _mapper;

        public ConfigureApiErrorBehaviorOptions(IExceptionMapper mapper)
        {
             _mapper = mapper;
        }

        public void Configure(ApiBehaviorOptions options)
        {
             options.InvalidModelStateResponseFactory = context =>
                    new BadRequestObjectResult(new ApiError(_mapper.Options.GetModelValidationErrorCode(), 
                                                            "Frogvall.AspNetCore.ExceptionHandling.ModelValidationError", 
                                                            context.ModelState, 
                                                            context.HttpContext.TraceIdentifier, 
                                                            _mapper.Options.ServiceName))
                    {
                        ContentTypes =
                        {
                            Application.Json
                        }
                    };
        }

        public void Configure(string name, ApiBehaviorOptions options)
        {
            Configure(options);
        }
    }
}