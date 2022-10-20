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
using Frogvall.AspNetCore.ExceptionHandling.Test.Helpers;
using Microsoft.AspNetCore.Builder;
using Xunit;
using Xunit.Abstractions;

namespace Frogvall.AspNetCore.ExceptionHandling.Test
{
    public class TestAttributes
    {
        private readonly ITestOutputHelper _output;
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private const string ValidationError = "Frogvall.AspNetCore.ExceptionHandling.ModelValidationError"; 

        public TestAttributes(ITestOutputHelper output)
        {
            _output = output;
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
            return ServerHelper.SetupServerWithMvc(options =>
                {
                    options.EnableEndpointRouting = false;
                    options.Filters.Add(new ValidateModelFilter { ErrorCode = 1337 });
                },
                app =>
                {
                    app.UseApiExceptionHandler();
                    app.UseExceptionStatusCodeDecorator();
                    app.UseMvc();
                },
                _output);
        }

        private HttpClient SetupServerWithControllers()
        {
            return ServerHelper.SetupServerWithControllers(options =>
                {
                    options.Filters.Add(new ValidateModelFilter { ErrorCode = 1337 });
                },
                app =>
                {
                    app.UseApiExceptionHandler();
                    app.UseExceptionStatusCodeDecorator();
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                },
                _output);
        }

        [Theory(Skip = "Used to manually verify caching of SkipModelValidation")]
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_TestCache_ManualVerify(ServerType serverType)
        {            
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 0}}", Encoding.UTF8, "text/json");

            // Act
            await client.PostAsync("/api/Test/NoValidation", content);
            await client.PostAsync("/api/Test/NoValidation", content);

            await client.PostAsync("/api/Test", content);
            await client.PostAsync("/api/Test", content);

            await client.PostAsync("/api/Test/NoValidation", content);
            await client.PostAsync("/api/Test", content);

        }


        [Theory]
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_NoValidation_ReturnsOk(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 0}}", Encoding.UTF8, "text/json");
            var content2 = new StringContent($@"{{""NullableObject"": ""string""}}", Encoding.UTF8, "text/json");

            // Act
            var response = await client.PostAsync("/api/Test/NoValidation", content);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_DefaultIntDto_ReturnsBadRequest(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 0}}", Encoding.UTF8, "text/json");
            const string expectedError = "The NonNullableObject field requires a non-default value.";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonSerializer.Deserialize<ApiError>(await response.Content.ReadAsStringAsync(), _serializerOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be(1337);
            error.Error.Should().Be(ValidationError);
            ((JsonElement)error.Context).GetProperty("NonNullableObject").EnumerateArray().FirstOrDefault().ToString().Should().Be(expectedError);
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_NoIntDto_ReturnsBadRequest(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string""}}", Encoding.UTF8, "text/json");
            const string expectedError = "The NonNullableObject field requires a non-default value.";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonSerializer.Deserialize<ApiError>(await response.Content.ReadAsStringAsync(), _serializerOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be(1337);
            error.Error.Should().Be(ValidationError);
            ((JsonElement)error.Context).GetProperty("NonNullableObject").EnumerateArray().FirstOrDefault().ToString().Should().Be(expectedError);
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_NoStringDto_ReturnsBadRequest(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NonNullableObject"": 1}}", Encoding.UTF8, "text/json");
            const string expectedError = "The NullableObject field is required.";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonSerializer.Deserialize<ApiError>(await response.Content.ReadAsStringAsync(), _serializerOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be(1337);
            error.Error.Should().Be(ValidationError);
            ((JsonElement)error.Context).GetProperty("NullableObject").EnumerateArray().FirstOrDefault().ToString().Should().Be(expectedError);
            error.Service.Should().Be(expectedServiceName);
        }
    }
}