using System;
using Frogvall.AspNetCore.ExceptionHandling.Exceptions;

namespace Frogvall.AspNetCore.ExceptionHandling.Test.TestResources
{
    public class TestException2 : BaseApiException
    {
        public TestException2(string message, object developerContext) : base(message, developerContext)
        {
        }

        public TestException2(string message, object developerContext, Exception innerException) : base(message, developerContext, innerException)
        {
        }

        public TestException2(string message) : base(message)
        {
        }

        public TestException2(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}