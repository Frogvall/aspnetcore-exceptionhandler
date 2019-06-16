using System.Net;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;
using Frogvall.AspNetCore.ExceptionHandling.Test.MappingProfiles.TestResources;

namespace Frogvall.AspNetCore.ExceptionHandling.Test.MappingProfiles
{
    public class EmptyExceptionMapperProfile : ExceptionMappingProfile<TestEnum>
    {
    }

    public class TwoHundredExceptionMapperProfile : ExceptionMappingProfile<TestEnum>
    {
        public TwoHundredExceptionMapperProfile()
        {
            AddMapping<TestException>(HttpStatusCode.OK, TestEnum.MyFirstValue);
        }
    }

    public class OriginalExceptionMapperProfile : ExceptionMappingProfile<TestEnum>
    {
        public OriginalExceptionMapperProfile()
        {
            AddMapping<TestException>(HttpStatusCode.BadRequest, TestEnum.MyFirstValue);
        }
    }

    public class DuplicateExceptionMapperProfile : ExceptionMappingProfile<TestEnum>
    {
        public DuplicateExceptionMapperProfile()
        {
            AddMapping<TestException>(HttpStatusCode.InternalServerError, TestEnum.MyFirstValue);
        }
    }

    public class InternallyDuplicatedExceptionMapperProfile : ExceptionMappingProfile<TestEnum>
    {
        public InternallyDuplicatedExceptionMapperProfile()
        {
            AddMapping<TestException>(HttpStatusCode.BadRequest, TestEnum.MyFirstValue);
            AddMapping<TestException>(HttpStatusCode.InternalServerError, TestEnum.MyFirstValue);
        }
    }
}
