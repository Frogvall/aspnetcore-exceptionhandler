using System;
using Frogvall.AspNetCore.ExceptionHandling.Exceptions;

namespace Frogvall.AspNetCore.ExceptionHandling.Test.TestResources
{
    public class TestException : BaseApiException
    {
        public TestException(string message) : base(message)
        {
        }

        public TestException(string message, object context) : base(message, context)
        {
        }

        public TestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public TestException(string message, object context, Exception innerException) : base(message, context, innerException)
        {
        }

        public TestException(string message, object context, object developerContext) : base(message, context, developerContext)
        {
        }

        public TestException(string message, object context, object developerContext, Exception innerException) : base(message, context, developerContext, innerException)
        {
        }
    }
}