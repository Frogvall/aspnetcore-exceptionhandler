using System;
using Frogvall.AspNetCore.ExceptionHandling.Exceptions;

namespace Frogvall.AspNetCore.ExceptionHandling.Test.TestResources
{
    public class TestException3 : BaseApiException
    {
        public TestEnum ErrorCode { get; set; }
        public TestException3(TestEnum errorCode, string message, object developerContext) : base(message, developerContext)
        {
            ErrorCode = errorCode;
        }

        public TestException3(TestEnum errorCode, string message, object developerContext, Exception innerException) : base(message, developerContext, innerException)
        {
            ErrorCode = errorCode;
        }

        public TestException3(TestEnum errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public TestException3(TestEnum errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}