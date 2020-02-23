using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Frogvall.AspNetCore.ExceptionHandling.Test.Helpers;
using Frogvall.AspNetCore.ExceptionHandling.Test.TestResources;
using Microsoft.AspNetCore.Builder;
using Xunit;
using Xunit.Abstractions;

namespace Frogvall.AspNetCore.ExceptionHandling.Test
{
    public class TestStatusCodeDecorator
    {
        private readonly ITestOutputHelper _output;

        public TestStatusCodeDecorator(ITestOutputHelper output)
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
                options => options.EnableEndpointRouting = false,
                app =>
                {
                    app.UseMiddleware<TestExceptionSwallowerMiddleware>();
                    app.UseExceptionStatusCodeDecorator();
                    app.UseMvc();
                },
                _output
            );
        }

        private HttpClient SetupServerWithControllers()
        {
            return ServerHelper.SetupServerWithControllers(
                options => options.EnableEndpointRouting = false,
                app =>
                {
                    app.UseMiddleware<TestExceptionSwallowerMiddleware>();
                    app.UseExceptionStatusCodeDecorator();
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
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": -1}}", Encoding.UTF8, "text/json");

            //Act
            var response = await client.PostAsync("/api/Test", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Theory]
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_DtoIntSetToThree_ReturnsError(ServerType serverType)
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
        [InlineData(ServerType.Mvc)]
        [InlineData(ServerType.Controllers)]
        public async Task PostTest_DtoIntSetToTwo_ReturnsFault(ServerType serverType)
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