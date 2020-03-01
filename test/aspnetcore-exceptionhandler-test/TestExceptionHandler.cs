using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling;
using Frogvall.AspNetCore.ExceptionHandling.Filters;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;
using Frogvall.AspNetCore.ExceptionHandling.Test.Helpers;
using Frogvall.AspNetCore.ExceptionHandling.Test.TestResources;
using Microsoft.AspNetCore.Builder;
using Xunit;
using Xunit.Abstractions;

namespace Frogvall.AspNetCore.ExceptionHandling.Test
{
    public class TestExceptionHandler
    {
        private Exception _exceptionSetByExceptionListener;

        private const string TestServiceName = "TestServiceName";
        private const string ExpectedErrorMyFirstValue = "Frogvall.AspNetCore.ExceptionHandling.Test.TestResources.TestEnum.MyFirstValue";
        private const string ExpectedErrorMySecondValue = "Frogvall.AspNetCore.ExceptionHandling.Test.TestResources.TestEnum.MySecondValue";
        private const string ExpectedErrorMyThirdValue = "Frogvall.AspNetCore.ExceptionHandling.Test.TestResources.TestEnum.MyThirdValue";

        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly ITestOutputHelper _output;

        public TestExceptionHandler(ITestOutputHelper output)
        {
            _output = output;
            _exceptionSetByExceptionListener = null;
        }

        private HttpClient SetupServer(ServerType serverType, bool useExceptionHandlerFilter, string testServiceName)
        {
            switch (serverType) {
                case ServerType.Mvc:
                    return SetupServerWithMvc(useExceptionHandlerFilter, testServiceName);
                case ServerType.Controllers:
                    return SetupServerWithControllers(useExceptionHandlerFilter, testServiceName);
                default:
                    throw new NotImplementedException();;
            }
        }

        private HttpClient SetupServerWithMvc(bool useExceptionHandlerFilter, string testServiceName)
        {
            var options = new ExceptionMapperOptions
                {
                    RespondWithDeveloperContext = true
                };
            if (testServiceName != null) options.ServiceName = testServiceName; 
            return ServerHelper.SetupServerWithMvc(
                options =>
                {
                    options.EnableEndpointRouting = false;
                    options.Filters.Add(new ValidateModelFilter {ErrorCode = 1337});
                    if (useExceptionHandlerFilter) options.Filters.Add(new ApiExceptionFilter(ex => _exceptionSetByExceptionListener = ex, ex => throw new Exception("Should not crash the application.")));
                },
                app =>
                {
                    if (!useExceptionHandlerFilter) app.UseApiExceptionHandler(ex => _exceptionSetByExceptionListener = ex);
                    app.UseMiddleware<TestAddCustomHeaderMiddleware>();
                    app.UseMvc();
                },
                _output,
                options);
                
        }

        private HttpClient SetupServerWithControllers(bool useExceptionHandlerFilter, string testServiceName)
        {
             var options = new ExceptionMapperOptions
                {
                    RespondWithDeveloperContext = true
                };
            if (testServiceName != null) options.ServiceName = testServiceName; 
            return ServerHelper.SetupServerWithControllers(
                options =>
                {
                    options.EnableEndpointRouting = false;
                    options.Filters.Add(new ValidateModelFilter {ErrorCode = 1337});
                    if (useExceptionHandlerFilter) options.Filters.Add(new ApiExceptionFilter(ex => _exceptionSetByExceptionListener = ex, ex => throw new Exception("Should not crash the application.")));
                },
                app =>
                {
                    if (!useExceptionHandlerFilter) app.UseApiExceptionHandler(ex => _exceptionSetByExceptionListener = ex);
                    app.UseMiddleware<TestAddCustomHeaderMiddleware>();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                },
                _output,
                options);
        }

        [Theory]
        [InlineData(ServerType.Mvc, true, null)]
        [InlineData(ServerType.Controllers, true, null)]
        [InlineData(ServerType.Mvc, false, TestServiceName)]
        [InlineData(ServerType.Controllers, false, TestServiceName)]
        public async Task PostTest_ValidDto_ReturnsOk(ServerType serverType, bool useExceptionHandlerFilter, string testServiceName)
        {
            //Arrange
            var client = SetupServer(serverType, useExceptionHandlerFilter, testServiceName);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 1}}", Encoding.UTF8, "text/json");

            // Act
            var response = await client.PostAsync("/api/Test", content);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [InlineData(ServerType.Mvc, true, null)]
        [InlineData(ServerType.Controllers, true, null)]
        public async Task PostTest_NegativeIntDto_ReturnsInternalServerErrorWithHeader(ServerType serverType, bool useExceptionHandlerFilter, string testServiceName)
        {
            //Arrange
            var client = SetupServer(serverType, useExceptionHandlerFilter, testServiceName);
            var expectedHeaderValue = "test-value";
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": -1}}", Encoding.UTF8, "text/json");
            content.Headers.Add(TestAddCustomHeaderMiddleware.TestHeader, new[]{expectedHeaderValue});
            var expectedServiceName = testServiceName ?? Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonSerializer.Deserialize<ApiError>(await response.Content.ReadAsStringAsync(), _serializerOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            response.Headers.TryGetValues(TestAddCustomHeaderMiddleware.TestHeader, out var actualValues);
            actualValues.FirstOrDefault().Should().Be(expectedHeaderValue);
            error.ErrorCode.Should().Be(-1);
            error.Error.Should().Be("Frogvall.AspNetCore.ExceptionHandling.InternalServerError");
            error.DeveloperContext.Should().BeNull();
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData(ServerType.Mvc, false, TestServiceName)]
        [InlineData(ServerType.Controllers, false, TestServiceName)]
        public async Task PostTest_NegativeIntDto_ReturnsInternalServerErrorNoHeader(ServerType serverType, bool useExceptionHandlerFilter, string testServiceName)
        {
            //Arrange
            var client = SetupServer(serverType, useExceptionHandlerFilter, testServiceName);
            var notExpectedHeaderValue = "test-value";
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": -1}}", Encoding.UTF8, "text/json");
            content.Headers.Add(TestAddCustomHeaderMiddleware.TestHeader, new[] { notExpectedHeaderValue });
            var expectedServiceName = testServiceName  ?? Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonSerializer.Deserialize<ApiError>(await response.Content.ReadAsStringAsync(), _serializerOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            response.Headers.TryGetValues(TestAddCustomHeaderMiddleware.TestHeader, out var actualValues);
            actualValues.Should().BeNull();
            error.ErrorCode.Should().Be(-1);
            error.Error.Should().Be("Frogvall.AspNetCore.ExceptionHandling.InternalServerError");
            error.DeveloperContext.Should().BeNull();
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData(ServerType.Mvc, true, null)]
        [InlineData(ServerType.Controllers, true, null)]
        [InlineData(ServerType.Mvc, false, TestServiceName)]
        [InlineData(ServerType.Controllers, false, TestServiceName)]
        public async Task PostTest_ValidDto_PostTest_DtoIntSetToOne_ExceptionListenerNotSet(ServerType serverType, bool useExceptionHandlerFilter, string testServiceName)
        {
            //Arrange
            var client = SetupServer(serverType, useExceptionHandlerFilter, testServiceName);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 1}}", Encoding.UTF8, "text/json");

            // Act
            await client.PostAsync("/api/Test", content);

            // Assert
            _exceptionSetByExceptionListener.Should().BeNull();
        }

        [Theory]
        [InlineData(ServerType.Mvc, true, null)]
        [InlineData(ServerType.Controllers, true, null)]
        [InlineData(ServerType.Mvc, false, TestServiceName)]
        [InlineData(ServerType.Controllers, false, TestServiceName)]
        public async Task PostTest_DtoIntSetToFour_ExceptionListenerSetsException(ServerType serverType, bool useExceptionHandlerFilter, string testServiceName)
        {
            //Arrange
            var client = SetupServer(serverType, useExceptionHandlerFilter, testServiceName);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 4}}", Encoding.UTF8, "text/json");

            // Act
            await client.PostAsync("/api/Test", content);

            // Assert
            _exceptionSetByExceptionListener.Should().BeOfType<TestException3>();
        }

        [Theory]
        [InlineData(ServerType.Mvc, true, null)]
        [InlineData(ServerType.Controllers, true, null)]
        [InlineData(ServerType.Mvc, false, TestServiceName)]
        [InlineData(ServerType.Controllers, false, TestServiceName)]
        public async Task PostTest_DtoIntSetToFour_ReturnsError(ServerType serverType, bool useExceptionHandlerFilter, string testServiceName)
        {
            //Arrange
            var client = SetupServer(serverType, useExceptionHandlerFilter, testServiceName);
            var expectedErrorCode = TestEnum.MyThirdValue;
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 4}}", Encoding.UTF8, "text/json");
            const string expectedContext = "Test1";
            var expectedServiceName = testServiceName ?? Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonSerializer.Deserialize<ApiError>(await response.Content.ReadAsStringAsync(), _serializerOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be((int)expectedErrorCode);
            error.Error.Should().Be(ExpectedErrorMyThirdValue);
            JsonSerializer.Deserialize<TestContext>(((JsonElement)error.Context).GetRawText(), _serializerOptions).TestValue.Should().Be(expectedContext);
            JsonSerializer.Deserialize<TestDeveloperContext>(((JsonElement)error.DeveloperContext).GetRawText(), _serializerOptions).TestValue.Should().Be(expectedContext);
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData(ServerType.Mvc, true, null)]
        [InlineData(ServerType.Controllers, true, null)]
        [InlineData(ServerType.Mvc, false, TestServiceName)]
        [InlineData(ServerType.Controllers, false, TestServiceName)]
        public async Task PostTest_DtoIntSetToThree_ReturnsError(ServerType serverType, bool useExceptionHandlerFilter, string testServiceName)
        {
            //Arrange
            var client = SetupServer(serverType, useExceptionHandlerFilter, testServiceName);
            var expectedErrorCode = TestEnum.MyFirstValue;
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 3}}", Encoding.UTF8, "text/json");
            const string expectedContext = "Test1";
            var expectedServiceName = testServiceName ?? Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonSerializer.Deserialize<ApiError>(await response.Content.ReadAsStringAsync(), _serializerOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be((int)expectedErrorCode);
            error.Error.Should().Be(ExpectedErrorMyFirstValue);
            JsonSerializer.Deserialize<TestContext>(((JsonElement)error.Context).GetRawText(), _serializerOptions).TestValue.Should().Be(expectedContext);
            JsonSerializer.Deserialize<TestDeveloperContext>(((JsonElement)error.DeveloperContext).GetRawText(), _serializerOptions).TestValue.Should().Be(expectedContext);
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData(ServerType.Mvc, true, null)]
        [InlineData(ServerType.Controllers, true, null)]
        [InlineData(ServerType.Mvc, false, TestServiceName)]
        [InlineData(ServerType.Controllers, false, TestServiceName)]
        public async Task PostTest_DtoIntSetToTwo_ReturnsFault(ServerType serverType, bool useExceptionHandlerFilter, string testServiceName)
        {
            //Arrange
            var client = SetupServer(serverType, useExceptionHandlerFilter, testServiceName);
            var expectedErrorCode = TestEnum.MySecondValue;
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 2}}", Encoding.UTF8, "text/json");
            const string expectedContext = "Test2";
            var expectedServiceName = testServiceName ?? Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonSerializer.Deserialize<ApiError>(await response.Content.ReadAsStringAsync(), _serializerOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            error.ErrorCode.Should().Be((int)expectedErrorCode);
            error.Error.Should().Be(ExpectedErrorMySecondValue);
            JsonSerializer.Deserialize<TestContext>(((JsonElement)error.Context).GetRawText(), _serializerOptions).TestValue.Should().Be(expectedContext);
            JsonSerializer.Deserialize<TestDeveloperContext>(((JsonElement)error.DeveloperContext).GetRawText(), _serializerOptions).TestValue.Should().Be(expectedContext);
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData(ServerType.Mvc, true, null)]
        [InlineData(ServerType.Controllers, true, null)]
        [InlineData(ServerType.Mvc, false, TestServiceName)]
        [InlineData(ServerType.Controllers, false, TestServiceName)]
        public async Task GetCancellationTest_Always_ReturnsFault(ServerType serverType, bool useExceptionHandlerFilter, string testServiceName)
        {
            //Arrange
            var client = SetupServer(serverType, useExceptionHandlerFilter, testServiceName);
            var expectedServiceName = testServiceName ?? Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.GetAsync("/api/Test/Cancellation");
            var error = JsonSerializer.Deserialize<ApiError>(await response.Content.ReadAsStringAsync(), _serializerOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            error.ErrorCode.Should().Be(-1);
            error.Error.Should().Be("Frogvall.AspNetCore.ExceptionHandling.OperationCanceled");
            error.Service.Should().Be(expectedServiceName);
            error.DeveloperContext.Should().BeNull();
        }
    }
}