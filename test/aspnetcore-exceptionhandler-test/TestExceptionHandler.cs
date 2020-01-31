using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling;
using Frogvall.AspNetCore.ExceptionHandling.Filters;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;
using Frogvall.AspNetCore.ExceptionHandling.Test.TestResources;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Frogvall.AspNetCore.ExceptionHandling.Test
{
    public class TestExceptionHandler
    {
        private Exception _exceptionSetByExceptionListener;

        private const string TestServiceName = "TestServiceName";

        public TestExceptionHandler()
        {
            // Run for every test case
            _exceptionSetByExceptionListener = null;
        }

        private HttpClient SetupServer(string serverType)
        {
            switch (serverType) {
                case "mvc":
                    return SetupServerWithMvc();
                default:
                    throw new NotImplementedException();;
            }
        }

        private HttpClient SetupServerWithMvc()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddExceptionMapper(new ExceptionMapperOptions
                    {
                        ServiceName = TestServiceName
                    }, GetType());
                    services.AddMvc(options =>
                    {
                        options.EnableEndpointRouting = false;
                        options.Filters.Add(new ValidateModelFilter { ErrorCode = 1337 });
                    });
                })
                .Configure(app =>
                {
                    app.UseApiExceptionHandler(ex => _exceptionSetByExceptionListener = ex);
                    app.UseMiddleware<TestAddCustomHeaderMiddleware>();
                    app.UseMvc();
                });

            var server = new TestServer(builder);
            return server.CreateClient();
        }

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_ValidDto_ReturnsOk(string serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 1}}", Encoding.UTF8, "text/json");

            // Act
            var response = await client.PostAsync("/api/Test", content);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_NegativeIntDto_ReturnsInternalServerError(string serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var notExpectedHeaderValue = "test-value";
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": -1}}", Encoding.UTF8, "text/json");
            content.Headers.Add(TestAddCustomHeaderMiddleware.TestHeader, new[] { notExpectedHeaderValue });
            var expectedServiceName = TestServiceName;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            response.Headers.TryGetValues(TestAddCustomHeaderMiddleware.TestHeader, out var actualValues);
            actualValues.Should().BeNull();
            error.ErrorCode.Should().Be(-1);
            error.DeveloperContext.Should().BeNull();
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_ValidDto_PostTest_DtoIntSetToFive_ExceptionListenerNotSet(string serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 1}}", Encoding.UTF8, "text/json");

            // Act
            await client.PostAsync("/api/Test", content);

            // Assert
            _exceptionSetByExceptionListener.Should().BeNull();
        }

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_DtoIntSetToFive_ExceptionListenerSetsException(string serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 5}}", Encoding.UTF8, "text/json");

            // Act
            await client.PostAsync("/api/Test", content);

            // Assert
            _exceptionSetByExceptionListener.Should().BeOfType<TestException3>();
        }

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_DtoIntSetToFive_ReturnsError(string serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var expectedErrorCode = TestEnum.MyThirdValue;
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 5}}", Encoding.UTF8, "text/json");
            const string expectedContext = "Test1";
            var expectedServiceName = TestServiceName;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be((int)expectedErrorCode);
            ((JObject)error.DeveloperContext).ToObject<TestDeveloperContext>().TestContext.Should().Be(expectedContext);
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_DtoIntSetToFour_ReturnsConflict(string serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 4}}", Encoding.UTF8, "text/json");
            var expectedServiceName = TestServiceName;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            error.ErrorCode.Should().Be(-2);
            error.DeveloperContext.Should().BeNull();
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_DtoIntSetToThree_ReturnsError(string serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var expectedErrorCode = TestEnum.MyFirstValue;
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 3}}", Encoding.UTF8, "text/json");
            const string expectedContext = "Test1";
            var expectedServiceName = TestServiceName;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be((int)expectedErrorCode);
            ((JObject) error.DeveloperContext).ToObject<TestDeveloperContext>().TestContext.Should().Be(expectedContext);
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_DtoIntSetToTwo_ReturnsFault(string serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var expectedErrorCode = TestEnum.MySecondValue;
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 2}}", Encoding.UTF8, "text/json");
            const string expectedContext = "Test2";
            var expectedServiceName = TestServiceName;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            error.ErrorCode.Should().Be((int)expectedErrorCode);
            ((JObject)error.DeveloperContext).ToObject<TestDeveloperContext>().TestContext.Should().Be(expectedContext);
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData("mvc")]
        public async Task GetCancellationTest_Always_ReturnsFault(string serverType)
        {
            //Arrange
            var client = SetupServer(serverType);

            // Act
            var response = await client.GetAsync("/api/Test/Cancellation");
            var error = JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            error.ErrorCode.Should().Be(-1);
            error.Service.Should().Be(TestServiceName);
            error.DeveloperContext.Should().BeNull();
        }
    }
}
