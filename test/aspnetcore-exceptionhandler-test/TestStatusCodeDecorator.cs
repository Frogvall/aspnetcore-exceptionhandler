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
        private HttpClient _client;

        public TestStatusCodeDecorator()
        {
            // Run for every test case
            SetupServer();
        }

        private void SetupServer()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddExceptionMapper(GetType()).AddMvc();
                })
                .Configure(app =>
                {
                    app.UseMiddleware<TestExceptionSwallowerMiddleware>();
                    app.UseExceptionStatusCodeDecorator();
                    app.UseMvc();
                });

            var server = new TestServer(builder);
            _client = server.CreateClient();
        }

        [Fact]
        public async Task PostTest_ValidDto_ReturnsOk()
        {
            //Arrange
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 1}}", Encoding.UTF8, "text/json");

            // Act
            var response = await _client.PostAsync("/api/Test", content);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task PostTest_NegativeIntDto_ReturnsInternalServerError()
        {
            //Arrange
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": -1}}", Encoding.UTF8, "text/json");

            //Act
            var response = await _client.PostAsync("/api/Test", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task PostTest_DtoIntSetToFour_ReturnsConflict()
        {
            //Arrange
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 4}}", Encoding.UTF8, "text/json");

            // Act
            var response = await _client.PostAsync("/api/Test", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task PostTest_DtoIntSetToThree_ReturnsError()
        {
            //Arrange
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 3}}", Encoding.UTF8, "text/json");

            // Act
            var response = await _client.PostAsync("/api/Test", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task PostTest_DtoIntSetToTwo_ReturnsFault()
        {
            //Arrange
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 2}}", Encoding.UTF8, "text/json");

            // Act
            var response = await _client.PostAsync("/api/Test", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }
    }
}