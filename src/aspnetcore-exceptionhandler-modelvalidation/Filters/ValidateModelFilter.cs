using System.Collections.Generic;
using Frogvall.AspNetCore.ExceptionHandling.Attributes;
using Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Frogvall.AspNetCore.ExceptionHandling.Filters
{
    public sealed class ValidateModelFilter : ActionFilterAttribute
    {
        private readonly Dictionary<string, bool> _actionSkipValidationCache = new Dictionary<string, bool>();

        public int ErrorCode { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var actionId = context.ActionDescriptor?.Id;
            if (actionId != null && _actionSkipValidationCache.ContainsKey(actionId))
            {
                if (_actionSkipValidationCache[actionId]) return;
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
            var mapper = context.HttpContext.RequestServices.GetService<IExceptionMapper>();
            context.Result = new BadRequestObjectResult(new ApiError(ErrorCode, context.ModelState, context.HttpContext.TraceIdentifier, mapper.Options.ServiceName));
        }
    }
}