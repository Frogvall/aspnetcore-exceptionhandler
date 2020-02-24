using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling
{
    public sealed class ApiError
    {
        public const string ModelBindingErrorMessage = "Invalid parameters.";

        public ApiError()
        {
            
        }

        public ApiError(string serviceName)
        {
            Service = serviceName;
        }

        public ApiError(int errorCode, string error, object context, object developerContext, string message, string serviceName)
        {
            ErrorCode = errorCode;
            Error = error;
            Service = serviceName;
            Message = message;
            Context = context;
            DeveloperContext = developerContext;
        }

        /// <summary>
        /// Creates a new <see cref="ApiError"/> from the result of a model binding attempt.
        /// The model binding errors (if any) are placed in the <see cref="DeveloperContext"/> property.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="modelState"></param>
        /// <param name="correlationId"></param>
        /// <param name="serviceName"></param>
        public ApiError(int errorCode, string error, ModelStateDictionary modelState, string correlationId, string serviceName)
        {
            Service = serviceName;
            Message = ModelBindingErrorMessage;
            ErrorCode = errorCode;
            Error = error;
            Context = new SerializableError(modelState);
            CorrelationId = correlationId;
        }
        public string Service { get; set; }

        public string CorrelationId { get; set; }

        public string Message { get; set; }

        public string DetailedMessage { get; set; }

        public int ErrorCode { get; set; }

        public string Error { get; set; }

        public object Context { get; set; }

        public object DeveloperContext { get; set; }
    }
}
