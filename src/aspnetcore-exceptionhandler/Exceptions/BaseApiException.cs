using System;

namespace Frogvall.AspNetCore.ExceptionHandling.Exceptions
{
    public abstract class BaseApiException : Exception
    {
        public object Context { get; set; }
        public object DeveloperContext { get; set; }

        public BaseApiException(string message, object context) : base(message)
        {
            Context = context;
        }

        public BaseApiException(string message, object context, Exception innerException) : base(message, innerException)
        {
            Context = context;
        }

        public BaseApiException(string message, object context, object developerContext) : base(message)
        {
            Context = context;
            DeveloperContext = developerContext;
        }

        public BaseApiException(string message, object context, object developerContext, Exception innerException) : base(message, innerException)
        {
            Context = context;
            DeveloperContext = developerContext;
        }

        public BaseApiException(string message) : base(message)
        {
            Context = new {};
            DeveloperContext = null;
        }

        public BaseApiException(string message, Exception innerException) : base(message, innerException)
        {
            Context = new {};
            DeveloperContext = null;
        }
    }
}