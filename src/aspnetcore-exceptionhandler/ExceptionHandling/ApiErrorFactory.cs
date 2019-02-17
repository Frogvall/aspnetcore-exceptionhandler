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
            ILogger<TCategoryName> logger, bool isDevelopment)
        {
            context.Response.ContentType = "application/json";

            HttpStatusCode statusCode;
            int errorCode;
            object developerContext = null;

            switch (ex)
            {
                case BaseApiException _:
                    try
                    {
                        developerContext = (ex as BaseApiException)?.DeveloperContext;
                        errorCode = mapper.GetErrorCode(ex as BaseApiException);
                        statusCode = mapper.GetExceptionHandlerReturnCode(ex as BaseApiException);
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
                case ApiException _:
                    errorCode = -2;
                    statusCode = (ex as ApiException).StatusCode;
                    context.Response.StatusCode = (int)statusCode;
                    logger.LogInformation(ex,
                        "ApiException caught by ApiExceptionHandler with  {statusCodeInt} {statusCodeString}. Unexpected: {unexpected}", (int)statusCode, statusCode.ToString(), false);
                    break;
                default:
                    errorCode = -1;
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
                DeveloperContext = developerContext,
                ErrorCode = errorCode,
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