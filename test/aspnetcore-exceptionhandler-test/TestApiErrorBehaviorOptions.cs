using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Frogvall.AspNetCore.ExceptionHandling.Configuration;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Frogvall.AspNetCore.ExceptionHandling.Test
{
    public class TestApiErrorBehaviorOptions
    {
        private const string ValidationError = "Frogvall.AspNetCore.ExceptionHandling.ModelValidationError"; 
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _client;

        public TestApiErrorBehaviorOptions()
        {
            var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddControllers(mvcOptions =>
                    {   
                        mvcOptions.Filters.AddApiExceptionFilter();
                    });

                    services.ConfigureOptions<ConfigureApiErrorBehaviorOptions>();
                    services.AddExceptionMapper(new ExceptionMapperOptions().SetModelValidationErrorCode(911), typeof(TestApiErrorBehaviorOptions));
                });
                builder.Configure(app => 
                {
                    app.UseApiExceptionHandler();
                    app.UseRouting();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });
            });

            _client = application.CreateClient();
        }

        [Fact]
        public async Task PostTest_DefaultIntDto_ReturnsBadRequest()
        {
            //Arrange
            // var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 0}}", Encoding.UTF8, "text/json");
            const string expectedError = "The NonNullableObject field requires a non-default value.";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await _client.PostAsync("/api/TestApi", content);
            var content1 = await response.Content.ReadAsStringAsync();
            var error = await response.ParseApiErrorAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be(911);
            error.Error.Should().Be(ValidationError);
            ((JsonElement)error.Context).GetProperty("NonNullableObject").EnumerateArray().FirstOrDefault().ToString().Should().Be(expectedError);
            error.Service.Should().Be(expectedServiceName);
        }
    }
}