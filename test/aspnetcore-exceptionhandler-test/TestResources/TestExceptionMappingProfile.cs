using System.Net;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;

namespace Frogvall.AspNetCore.ExceptionHandling.Test.TestResources
{
    public class TestExceptionMappingProfile : ExceptionMappingProfile<TestEnum>
    {
        public TestExceptionMappingProfile()
        {
            AddMapping<TestException>(HttpStatusCode.BadRequest, TestEnum.MyFirstValue);
        }
    }
}