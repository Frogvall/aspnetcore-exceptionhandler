using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Frogvall.AspNetCore.ExceptionHandling.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Frogvall.AspNetCore.ExceptionHandling.Test
{
    public class TestHttpResponseMessageExtensions
    {
        private HttpClient _client;

        public TestHttpResponseMessageExtensions()
        {
            // Run for every test case
            SetupServer();
        }

        private void SetupServer()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddExceptionMapper(GetType());
                    services.AddMvc(options =>
                    {
                        options.Filters.Add(new ValidateModelFilter {ErrorCode = 1337});
                        options.Filters.Add(new ApiExceptionFilter());
                    });
                })
                .Configure(app => { app.UseMvc(); });

            var server = new TestServer(builder);
            _client = server.CreateClient();
        }

        [Fact]
        public async Task PostTest_ParseAsync_ReturnsValidApiError()
        {
            //Arrange
            var content = new StringContent($@"{{""NullableObject"": ""string""}}", Encoding.UTF8, "text/json");
            const string expectedError = "The NonNullableObject field requires a non-default value.";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await _client.PostAsync("/api/Test", content);
            var error = await response.ParseApiErrorAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be(1337);
            ((JObject) error.DeveloperContext)["NonNullableObject"].ToObject<string[]>().FirstOrDefault().Should()
                .Be(expectedError);
            error.Service.Should().Be(expectedServiceName);
        }

        [Fact]
        public async Task PostTest_ParseAsyncNoApiError_ReturnsNullApiError()
        {
            //Arrange
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 5}}",
                Encoding.UTF8, "text/json");

            // Act
            var response = await _client.PostAsync("/api/NoExceptionNo20x", content);
            var error = await response.ParseApiErrorAsync();

            // Assert
            error.Should().BeNull();
        }

        [Fact]
        public async Task PostTest_ParseAsyncOnSuccessfulResponse_ReturnsNullApiError()
        {
            //Arrange
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 1}}",
                Encoding.UTF8, "text/json");

            // Act
            var response = await _client.PostAsync("/api/Test", content);
            var error = await response.ParseApiErrorAsync();

            // Assert
            error.Should().BeNull();
        }

        [Fact]
        public async Task PostTest_ParseSync_ReturnsValidApiError()
        {
            //Arrange
            var content = new StringContent($@"{{""NullableObject"": ""string""}}", Encoding.UTF8, "text/json");
            const string expectedError = "The NonNullableObject field requires a non-default value.";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await _client.PostAsync("/api/Test", content);
            var success = response.TryParseApiError(out var error);

            // Assert
            success.Should().Be(true);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be(1337);
            ((JObject) error.DeveloperContext)["NonNullableObject"].ToObject<string[]>().FirstOrDefault().Should()
                .Be(expectedError);
            error.Service.Should().Be(expectedServiceName);
        }

        [Fact]
        public async Task PostTest_ParseSyncNoApiError_ReturnsNullApiError()
        {
            //Arrange
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 5}}",
                Encoding.UTF8, "text/json");

            // Act
            var response = await _client.PostAsync("/api/NoExceptionNo20x", content);
            var success = response.TryParseApiError(out var error);

            // Assert
            success.Should().Be(false);
            error.Should().BeNull();
        }

        [Fact]
        public async Task PostTest_ParseSyncOnSuccessfulResponse_ReturnsNullApiError()
        {
            //Arrange
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 1}}",
                Encoding.UTF8, "text/json");

            // Act
            var response = await _client.PostAsync("/api/Test", content);
            var success = response.TryParseApiError(out var error);

            // Assert
            success.Should().Be(false);
            error.Should().BeNull();
        }
    }
}