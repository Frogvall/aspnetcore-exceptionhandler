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
using Frogvall.AspNetCore.ExceptionHandling.Test.TestResources;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Frogvall.AspNetCore.ExceptionHandling.Test
{
    public class TestAttributes
    {
        private readonly ITestOutputHelper _output;

        public TestAttributes(ITestOutputHelper output)
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
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    var descriptor =
                        new ServiceDescriptor(
                            typeof(ILogger<ValidateModelFilter>),
                            TestLogger.Create<ValidateModelFilter>(_output));
                    services.Replace(descriptor);
                    services.AddExceptionMapper(GetType());
                    services.AddMvc(options =>
                    {
                        options.EnableEndpointRouting = false;
                        options.Filters.Add(new ValidateModelFilter { ErrorCode = 1337 });
                    });
                })
                .Configure(app =>
                {
                    app.UseApiExceptionHandler();
                    app.UseExceptionStatusCodeDecorator();
                    app.UseMvc();
                });

            var server = new TestServer(builder);
            return server.CreateClient();
        }

        [Theory(Skip = "Used to manually verify caching of SkipModelValidation")]
        [InlineData("mvc")]
        public async Task PostTest_TestCache_ManualVerify(string serverType)
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
        [InlineData("mvc")]
        public async Task PostTest_NoValidation_ReturnsOk(string serverType)
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
        [InlineData("mvc")]
        public async Task PostTest_DefaultIntDto_ReturnsBadRequest(string serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 0}}", Encoding.UTF8, "text/json");
            const string expectedError = "The NonNullableObject field requires a non-default value.";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be(1337);
            ((JObject)error.DeveloperContext)["NonNullableObject"].ToObject<string[]>().FirstOrDefault().Should().Be(expectedError);
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_NoIntDto_ReturnsBadRequest(string serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string""}}", Encoding.UTF8, "text/json");
            const string expectedError = "The NonNullableObject field requires a non-default value.";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be(1337);
            ((JObject)error.DeveloperContext)["NonNullableObject"].ToObject<string[]>().FirstOrDefault().Should().Be(expectedError);
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_NoStringDto_ReturnsBadRequest(string serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NonNullableObject"": 1}}", Encoding.UTF8, "text/json");
            const string expectedError = "The NullableObject field is required.";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = JsonConvert.DeserializeObject<ApiError>(await response.Content.ReadAsStringAsync());

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be(1337);
            ((JObject)error.DeveloperContext)["NullableObject"].ToObject<string[]>().FirstOrDefault().Should().Be(expectedError);
            error.Service.Should().Be(expectedServiceName);
        }
    }
}