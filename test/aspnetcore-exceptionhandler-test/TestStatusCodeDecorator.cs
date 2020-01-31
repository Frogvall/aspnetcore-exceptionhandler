using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Frogvall.AspNetCore.ExceptionHandling.Test.TestResources;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Frogvall.AspNetCore.ExceptionHandling.Test
{
    public class TestStatusCodeDecorator
    {

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
                    services.AddExceptionMapper(GetType()).AddMvc(options => options.EnableEndpointRouting = false);
                })
                .Configure(app =>
                {
                    app.UseMiddleware<TestExceptionSwallowerMiddleware>();
                    app.UseExceptionStatusCodeDecorator();
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
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": -1}}", Encoding.UTF8, "text/json");

            //Act
            var response = await client.PostAsync("/api/Test", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_DtoIntSetToFour_ReturnsConflict(string serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 4}}", Encoding.UTF8, "text/json");

            // Act
            var response = await client.PostAsync("/api/Test", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_DtoIntSetToThree_ReturnsError(string serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 3}}", Encoding.UTF8, "text/json");

            // Act
            var response = await client.PostAsync("/api/Test", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("mvc")]
        public async Task PostTest_DtoIntSetToTwo_ReturnsFault(string serverType)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 2}}", Encoding.UTF8, "text/json");

            // Act
            var response = await client.PostAsync("/api/Test", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }
    }
}