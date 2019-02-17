using System.Net;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;

namespace Frogvall.AspNetCore.ExceptionHandling.Test.TestResources
{
    public class TestExceptionMappingProfile2 : ExceptionMappingProfile<TestEnum>
    {
        public TestExceptionMappingProfile2()
        {
            AddMapping<TestException2>(HttpStatusCode.InternalServerError, TestEnum.MySecondValue);
            AddMapping<TestException3>(HttpStatusCode.BadRequest, ex => ex.ErrorCode);
        }       
    }
}