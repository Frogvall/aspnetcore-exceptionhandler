using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Frogvall.AspNetCore.ExceptionHandling.Filters;
using Frogvall.AspNetCore.ExceptionHandling.Test.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Frogvall.AspNetCore.ExceptionHandling.Test
{
    public class TestHttpResponseMessageExtensions
    {
        private readonly ITestOutputHelper _output;

        public TestHttpResponseMessageExtensions(ITestOutputHelper output)
        {
            _output = output;
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

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_ParseAsync_ReturnsValidApiError(string serverType)
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
            ((JObject) error.DeveloperContext)["NonNullableObject"].ToObject<string[]>().FirstOrDefault().Should()
                .Be(expectedError);
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_ParseAsyncNoApiError_ReturnsNullApiError(string serverType)
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
        [InlineData("mvc")]
        public async Task PostTest_ParseAsyncOnSuccessfulResponse_ReturnsNullApiError(string serverType)
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
        [InlineData("mvc")]
        public async Task PostTest_ParseSync_ReturnsValidApiError(string serverType)
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
            success.Should().Be(true);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be(1337);
            ((JObject) error.DeveloperContext)["NonNullableObject"].ToObject<string[]>().FirstOrDefault().Should()
                .Be(expectedError);
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_ParseSyncNoApiError_ReturnsNullApiError(string serverType)
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
        [InlineData("mvc")]
        public async Task PostTest_ParseSyncOnSuccessfulResponse_ReturnsNullApiError(string serverType)
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