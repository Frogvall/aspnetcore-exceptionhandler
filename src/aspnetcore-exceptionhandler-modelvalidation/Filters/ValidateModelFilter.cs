using System.Collections.Concurrent;
using System.Net;
using Frogvall.AspNetCore.ExceptionHandling.Attributes;
using Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Frogvall.AspNetCore.ExceptionHandling.Filters
{
    public sealed class ValidateModelFilter : ActionFilterAttribute
    {
        private readonly ConcurrentDictionary<string, bool> _actionSkipValidationCache = new ConcurrentDictionary<string, bool>();

        public int ErrorCode { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var actionId = context.ActionDescriptor?.Id;
            if (actionId != null && _actionSkipValidationCache.TryGetValue(actionId, out var skipValidationValidation))
            {
                if (skipValidationValidation) return;
            }
            else
            {
                var controllerActionDescriptor = (ControllerActionDescriptor) context.ActionDescriptor;
                if
                (
                    controllerActionDescriptor != null
                    &&
                    (
                        controllerActionDescriptor.ControllerTypeInfo != null && controllerActionDescriptor.ControllerTypeInfo.IsDefined(typeof(SkipModelValidationFilterAttribute), false)
                        ||
                        controllerActionDescriptor.MethodInfo != null && controllerActionDescriptor.MethodInfo.IsDefined(typeof(SkipModelValidationFilterAttribute), false)
                    )
                )
                {
                    _actionSkipValidationCache[actionId] = true;
                    return;
                }
                _actionSkipValidationCache[actionId] = false;
            }

            if (context.ModelState.IsValid) return;

            var logger = context.HttpContext.RequestServices.GetService<ILogger<ValidateModelFilter>>();
            logger.LogInformation("A request have been rejected due to invalid parameters: {invalidParams}. Will return with {statusCodeInt} {statusCodeString}.", 
                JsonConvert.SerializeObject(new SerializableError(context.ModelState)), (int)HttpStatusCode.BadRequest, HttpStatusCode.BadRequest.ToString());
            var mapper = context.HttpContext.RequestServices.GetService<IExceptionMapper>();
            context.Result = new BadRequestObjectResult(new ApiError(ErrorCode, context.ModelState, context.HttpContext.TraceIdentifier, mapper.Options.ServiceName));
        }
    }
}