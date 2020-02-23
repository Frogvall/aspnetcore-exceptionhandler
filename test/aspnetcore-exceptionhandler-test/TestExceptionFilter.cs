using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling;
using Frogvall.AspNetCore.ExceptionHandling.Filters;
using Frogvall.AspNetCore.ExceptionHandling.Test.Helpers;
using Frogvall.AspNetCore.ExceptionHandling.Test.TestResources;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Frogvall.AspNetCore.ExceptionHandling.Test
{
    public class TestExceptionFilter
    {
        private readonly ITestOutputHelper _output;
        private Exception _exceptionSetByExceptionListener;

        public TestExceptionFilter(ITestOutputHelper output)
        {
            _output = output;
            // Run for every test case
            _exceptionSetByExceptionListener = null;
        }

        private HttpClient SetupServer(ServerType serverType)
        {
            switch (serverType) {
                case ServerType.Mvc:
                    return SetupServerWithMvc();
                case ServerType.Controllers:
                    return SetupServerWithControllers();
                default:
                    throw new NotImplementedException();;
            }
        }

        private HttpClient SetupServerWithMvc()
        {
            return ServerHelper.SetupServerWithMvc(
                options =>
                {
                    options.EnableEndpointRouting = false;
                    options.Filters.Add(new ValidateModelFilter {ErrorCode = 1337});
                    options.Filters.Add(new ApiExceptionFilter(ex => _exceptionSetByExceptionListener = ex));
                },
                app =>
                {
                    app.UseMiddleware<TestAddCustomHeaderMiddleware>();
                    app.UseMvc();
                },
                _output);
        }

        private HttpClient SetupServerWithControllers()
        {
            return ServerHelper.SetupServerWithMvc(options =>
                {
                    options.EnableEndpointRouting = false;
                    options.Filters.Add(new ValidateModelFilter { ErrorCode = 1337 });
                    options.Filters.Add(new ApiExceptionFilter(ex => _exceptionSetByExceptionListener = ex));
                },
                app =>
                {
                    app.UseMiddleware<TestAddCustomHeaderMiddleware>();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });

                },
                _output);
        }

        [Theory]
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_ValidDto_ReturnsOk(ServerType serverType)
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
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_NegativeIntDto_ReturnsInternalServerError(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var expectedHeaderValue = "test-value";
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": -1}}", Encoding.UTF8, "text/json");
            content.Headers.Add(TestAddCustomHeaderMiddleware.TestHeader, new[]{expectedHeaderValue});
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            response.Headers.TryGetValues(TestAddCustomHeaderMiddleware.TestHeader, out var actualValues);
            actualValues.FirstOrDefault().Should().Be(expectedHeaderValue);
            error.ErrorCode.Should().Be(-1);
            error.DeveloperContext.Should().BeNull();
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_ValidDto_PostTest_DtoIntSetToOne_ExceptionListenerNotSet(ServerType serverType)
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
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_DtoIntSetToFour_ExceptionListenerSetsException(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 4}}", Encoding.UTF8, "text/json");

            // Act
            await client.PostAsync("/api/Test", content);

            // Assert
            _exceptionSetByExceptionListener.Should().BeOfType<TestException3>();
        }

        [Theory]
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_DtoIntSetToFour_ReturnsError(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var expectedErrorCode = TestEnum.MyThirdValue;
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 4}}", Encoding.UTF8, "text/json");
            const string expectedContext = "Test1";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

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
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_DtoIntSetToThree_ReturnsError(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var expectedErrorCode = TestEnum.MyFirstValue;
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 3}}", Encoding.UTF8, "text/json");
            const string expectedContext = "Test1";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

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
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_DtoIntSetToTwo_ReturnsFault(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var expectedErrorCode = TestEnum.MySecondValue;
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 2}}", Encoding.UTF8, "text/json");
            const string expectedContext = "Test2";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

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
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task GetCancellationTest_Always_ReturnsFault(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);

            // Act
            var response = await client.GetAsync("/api/Test/Cancellation");
            var error = JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            error.ErrorCode.Should().Be(-1);
            error.Service.Should().Be(Assembly.GetEntryAssembly().GetName().Name);
            error.DeveloperContext.Should().BeNull();
        }
    }
}