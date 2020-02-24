using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Frogvall.AspNetCore.ExceptionHandling.Filters;
using Frogvall.AspNetCore.ExceptionHandling.Test.Helpers;
using Microsoft.AspNetCore.Builder;
using Xunit;
using Xunit.Abstractions;

namespace Frogvall.AspNetCore.ExceptionHandling.Test
{
    public class TestHttpResponseMessageExtensions
    {
        private const string ValidationError = "Frogvall.AspNetCore.ExceptionHandling.ModelValidationError"; 

        private readonly ITestOutputHelper _output;

        public TestHttpResponseMessageExtensions(ITestOutputHelper output)
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
            return ServerHelper.SetupServerWithMvc(
                options =>
                {
                    options.EnableEndpointRouting = false;
                    options.Filters.Add(new ValidateModelFilter {ErrorCode = 1337});
                    options.Filters.Add(new ApiExceptionFilter());
                },
                app => { app.UseMvc(); },
                _output
                );
        }

        private HttpClient SetupServerWithControllers()
        {
            return ServerHelper.SetupServerWithControllers(
                options =>
                {
                    options.EnableEndpointRouting = false;
                    options.Filters.Add(new ValidateModelFilter {ErrorCode = 1337});
                    options.Filters.Add(new ApiExceptionFilter());
                },
                app => 
                { 
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });    
                },
                _output
                );
        }

        [Theory]
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_ParseAsync_ReturnsValidApiError(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string""}}", Encoding.UTF8, "text/json");
            const string expectedError = "The NonNullableObject field requires a non-default value.";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = await response.ParseApiErrorAsync();

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
        public async Task PostTest_ParseAsyncNoApiError_ReturnsNullApiError(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 5}}",
                Encoding.UTF8, "text/json");

            // Act
            var response = await client.PostAsync("/api/NoExceptionNo20x", content);
            var error = await response.ParseApiErrorAsync();

            // Assert
            error.Should().BeNull();
        }

        [Theory]
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_ParseAsyncOnSuccessfulResponse_ReturnsNullApiError(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 1}}",
                Encoding.UTF8, "text/json");

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = await response.ParseApiErrorAsync();

            // Assert
            error.Should().BeNull();
        }

        [Theory]
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_ParseSync_ReturnsValidApiError(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string""}}", Encoding.UTF8, "text/json");
            const string expectedError = "The NonNullableObject field requires a non-default value.";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var success = response.TryParseApiError(out var error);

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
        public async Task PostTest_ParseSyncNoApiError_ReturnsNullApiError(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 5}}",
                Encoding.UTF8, "text/json");

            // Act
            var response = await client.PostAsync("/api/NoExceptionNo20x", content);
            var success = response.TryParseApiError(out var error);

            // Assert
            success.Should().Be(false);
            error.Should().BeNull();
        }

        [Theory]
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_ParseSyncOnSuccessfulResponse_ReturnsNullApiError(ServerType serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 1}}",
                Encoding.UTF8, "text/json");

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var success = response.TryParseApiError(out var error);

            // Assert
            success.Should().Be(false);
            error.Should().BeNull();
        }
    }
}