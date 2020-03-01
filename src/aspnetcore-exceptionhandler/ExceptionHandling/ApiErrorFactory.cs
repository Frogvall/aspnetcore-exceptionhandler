using System;
using System.Net;
using System.Text.RegularExpressions;
using AsyncFriendlyStackTrace;
using Frogvall.AspNetCore.ExceptionHandling.Exceptions;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling
{
    public static class ApiErrorFactory
    {
        internal static ApiError Build<TCategoryName>(HttpContext context, Exception ex, IExceptionMapper mapper,
            ILogger<TCategoryName> logger, bool isDevelopment, Action<Exception>[] exceptionListeners)
        {
            //Execute custom exception handlers first.
            foreach (var customExceptionListener in exceptionListeners)
            {
                try 
                {
                    customExceptionListener.Invoke(ex);
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Custom exception listener {exceptionListener} threw an exception.", customExceptionListener.GetType().ToString());
                }
            }

            context.Response.ContentType = "application/json";

            HttpStatusCode statusCode;
            (int errorCode, string error) errorObject;
            object developerContext = null;
            object exceptionContext = new {};

            switch (ex)
            {
                case BaseApiException baseApiException:
                    try
                    {
                        if (mapper.Options.RespondWithDeveloperContext) developerContext = baseApiException.DeveloperContext;
                        exceptionContext = baseApiException.Context;
                        errorObject = mapper.GetError(baseApiException);
                        statusCode = mapper.GetExceptionHandlerReturnCode(baseApiException);
                        context.Response.StatusCode = (int)statusCode;
                        logger.LogInformation(ex,
                            "Mapped BaseApiException of type {exceptionType} caught by ApiExceptionHandler. Will return with {statusCodeInt} {statusCodeString}. Unexpected: {unexpected}",
                            ex.GetType(), (int)statusCode, statusCode.ToString(), false);
                    }
                    catch (ArgumentException)
                    {
                        goto default;
                    }

                    break;
                case OperationCanceledException _:
                    errorObject = (-1, "Frogvall.AspNetCore.ExceptionHandling.OperationCanceled");
                    statusCode = HttpStatusCode.InternalServerError;
                    context.Response.StatusCode = (int)statusCode;
                    logger.LogWarning(ex,
                        "OperationCanceledException exception caught by ApiExceptionHandler. Will return with {statusCodeInt} {statusCodeString}. Unexpected: {unexpected}",
                        (int)statusCode, statusCode.ToString(), true);
                    break;
                default:
                    errorObject = (-1, "Frogvall.AspNetCore.ExceptionHandling.InternalServerError");
                    statusCode = HttpStatusCode.InternalServerError;
                    context.Response.StatusCode = (int)statusCode;
                    logger.LogError(ex,
                        "Unhandled exception of type {exceptionType} caught by ApiExceptionHandler. Will return with {statusCodeInt} {statusCodeString}. Unexpected: {unexpected}",
                        ex.GetType(), (int)statusCode, statusCode.ToString(), true);
                    break;
            }

            var error = new ApiError(mapper.Options.ServiceName)
            {
                CorrelationId = context.TraceIdentifier,
                Context = exceptionContext,
                DeveloperContext = developerContext,
                ErrorCode = errorObject.errorCode,
                Error = errorObject.error
            };

            if (isDevelopment)
            {
                error.Message = ex.Message;
                error.DetailedMessage = ex.ToAsyncString();
            }
            else
            {
                error.Message = Regex.Replace(statusCode.ToString(), "[a-z][A-Z]",
                    m => m.Value[0] + " " + char.ToLower(m.Value[1]));
                error.DetailedMessage = ex.Message;
            }

            return error;
        }
    }
}