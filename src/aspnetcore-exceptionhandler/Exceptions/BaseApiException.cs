using System;

namespace Frogvall.AspNetCore.ExceptionHandling.Exceptions
{
    public abstract class BaseApiException : Exception
    {
        public object DeveloperContext { get; set; }
        public BaseApiException(string message, object developerContext) : base(message)
        {
            DeveloperContext = developerContext;
        }

        public BaseApiException(string message, object developerContext, Exception innerException) : base(message, innerException)
        {
            DeveloperContext = developerContext;
        }

        public BaseApiException(string message) : base(message)
        {
            DeveloperContext = null;
        }

        public BaseApiException(string message, Exception innerException) : base(message, innerException)
        {
            DeveloperContext = null;
        }
    }
}