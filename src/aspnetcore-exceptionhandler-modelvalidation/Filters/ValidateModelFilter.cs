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
        public int ErrorCode { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            if (controllerActionDescriptor != null
                && (controllerActionDescriptor.ControllerTypeInfo != null && controllerActionDescriptor.ControllerTypeInfo.IsDefined(typeof(SkipModelValidationFilterAttribute), false)
                || controllerActionDescriptor.MethodInfo != null && controllerActionDescriptor.MethodInfo.IsDefined(typeof(SkipModelValidationFilterAttribute), false)))
                return;

            if (!context.ModelState.IsValid)
            {
                var mapper = context.HttpContext.RequestServices.GetService<IExceptionMapper>();
                context.Result = new BadRequestObjectResult(new ApiError(ErrorCode, context.ModelState, context.HttpContext.TraceIdentifier, mapper.Options.ServiceName));
            }
        }
    }
}