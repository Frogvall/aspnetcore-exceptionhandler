using System;
using System.Linq;
using System.Reflection;
using Frogvall.AspNetCore.ExceptionHandling.Test.MappingProfiles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Frogvall.AspNetCore.ExceptionHandling.Test
{
    public class TestExceptionMapperProfile
    {
        private void SetupServer(params Type[] profileTypes)
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services => { services.AddExceptionMapper(profileTypes.Select(pt => pt.GetTypeInfo()).ToArray()); })
                .Configure(app => { app.UseApiExceptionHandler(); });

            new TestServer(builder);
        }

        [Fact]
        public void CreateExceptionMapperTest_EmptyProfile_NoExceptions()
        {
            //Act
            SetupServer(typeof(EmptyExceptionMapperProfile));
        }

        [Fact]
        public void CreateExceptionMapperTest_HttpStatusCodeOutOfRangeProfile_ArgumentException()
        {
            //Arrange
            const string expected = "Invalid http status code: 200 OK. Only 4xx and 5xx status codes are allowed.";

            //Act
            var ex = Assert.Throws<TargetInvocationException>(() => SetupServer(typeof(TwoHundredExceptionMapperProfile)));

            //Assert
            Assert.IsType<ArgumentException>(ex.InnerException);
            Assert.Equal(expected, ex.InnerException.Message);
        }

        [Fact]
        public void CreateExceptionMapperTest_DuplicateProfiles_ArgumentException()
        {
            //Arrange
            const string expected = "Duplicate entry. Exceptions already added to map: Frogvall.AspNetCore.ExceptionHandling.Test.MappingProfiles.TestResources.TestException";

            //Act
            var ex = Assert.Throws<InvalidOperationException>(() => SetupServer(typeof(OriginalExceptionMapperProfile), typeof(DuplicateExceptionMapperProfile)));

            //Assert
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void CreateExceptionMapperTest_DuplicateMappingWithinProfile_ArgumentException()
        {
            //Arrange
            const string expected = "Duplicate entry. Exception already added to map: Frogvall.AspNetCore.ExceptionHandling.Test.MappingProfiles.TestResources.TestException";

            //Act
            var ex = Assert.Throws<TargetInvocationException>(() => SetupServer(typeof(InternallyDuplicatedExceptionMapperProfile)));

            //Assert
            //Assert
            Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Equal(expected, ex.InnerException.Message);
        }
    }
}