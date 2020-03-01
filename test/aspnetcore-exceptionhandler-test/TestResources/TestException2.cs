using System;
using Frogvall.AspNetCore.ExceptionHandling.Exceptions;

namespace Frogvall.AspNetCore.ExceptionHandling.Test.TestResources
{
    public class TestException2 : BaseApiException
    {
        public TestException2(string message) : base(message)
        {
        }

        public TestException2(string message, object context) : base(message, context)
        {
        }

        public TestException2(string message, Exception innerException) : base(message, innerException)
        {
        }

        public TestException2(string message, object context, Exception innerException) : base(message, context, innerException)
        {
        }

        public TestException2(string message, object context, object developerContext) : base(message, context, developerContext)
        {
        }

        public TestException2(string message, object context, object developerContext, Exception innerException) : base(message, context, developerContext, innerException)
        {
        }
    }
}