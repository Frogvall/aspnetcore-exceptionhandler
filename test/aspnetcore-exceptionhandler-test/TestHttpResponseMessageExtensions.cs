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
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Frogvall.AspNetCore.ExceptionHandling.Test
{
    public class TestHttpResponseMessageExtensions
    {
        public enum JsonParser
        {
            NewtonsoftJson,
            SystemTextJson
        } 

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
        [InlineData(ServerType.Mvc, JsonParser.SystemTextJson)]
        [InlineData(ServerType.Controllers, JsonParser.SystemTextJson)]
        [InlineData(ServerType.Mvc, JsonParser.NewtonsoftJson)]
        [InlineData(ServerType.Controllers, JsonParser.NewtonsoftJson)]
        public async Task PostTest_ParseAsync_ReturnsValidApiError(ServerType serverType, JsonParser jsonParser)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string""}}", Encoding.UTF8, "text/json");
            const string expectedError = "The NonNullableObject field requires a non-default value.";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = new ApiError();
            switch (jsonParser)
            {
                case JsonParser.SystemTextJson:
                    error = await response.ParseApiErrorAsync();
                    break;
                case  JsonParser.NewtonsoftJson:
                    error = await response.ParseApiErrorUsingNewtonsoftJsonAsync();
                    break;
            }

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be(1337);
            error.Error.Should().Be(ValidationError);
            switch (jsonParser)
            {
                case JsonParser.SystemTextJson:
                    ((JsonElement)error.Context).GetProperty("NonNullableObject").EnumerateArray().FirstOrDefault().ToString().Should().Be(expectedError);
                    break;
                case  JsonParser.NewtonsoftJson:
                    ((JObject)error.Context)["NonNullableObject"].ToObject<string[]>().FirstOrDefault().Should().Be(expectedError);
                    break;
            }
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData(ServerType.Mvc, JsonParser.SystemTextJson)]
        [InlineData(ServerType.Controllers, JsonParser.SystemTextJson)]
        [InlineData(ServerType.Mvc, JsonParser.NewtonsoftJson)]
        [InlineData(ServerType.Controllers, JsonParser.NewtonsoftJson)]
        public async Task PostTest_ParseAsyncNoApiError_ReturnsNullApiError(ServerType serverType, JsonParser jsonParser)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 5}}",
                Encoding.UTF8, "text/json");

            // Act
            var response = await client.PostAsync("/api/NoExceptionNo20x", content);
            var error = new ApiError();
            switch (jsonParser)
            {
                case JsonParser.SystemTextJson:
                    error = await response.ParseApiErrorAsync();
                    break;
                case  JsonParser.NewtonsoftJson:
                    error = await response.ParseApiErrorUsingNewtonsoftJsonAsync();
                    break;
            }

            // Assert
            error.Should().BeNull();
        }

        [Theory]
        [InlineData(ServerType.Mvc, JsonParser.SystemTextJson)]
        [InlineData(ServerType.Controllers, JsonParser.SystemTextJson)]
        [InlineData(ServerType.Mvc, JsonParser.NewtonsoftJson)]
        [InlineData(ServerType.Controllers, JsonParser.NewtonsoftJson)]
        public async Task PostTest_ParseAsyncOnSuccessfulResponse_ReturnsNullApiError(ServerType serverType, JsonParser jsonParser)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 1}}",
                Encoding.UTF8, "text/json");

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = new ApiError();
            switch (jsonParser)
            {
                case JsonParser.SystemTextJson:
                    error = await response.ParseApiErrorAsync();
                    break;
                case  JsonParser.NewtonsoftJson:
                    error = await response.ParseApiErrorUsingNewtonsoftJsonAsync();
                    break;
            }

            // Assert
            error.Should().BeNull();
        }

        [Theory]
        [InlineData(ServerType.Mvc, JsonParser.SystemTextJson)]
        [InlineData(ServerType.Controllers, JsonParser.SystemTextJson)]
        [InlineData(ServerType.Mvc, JsonParser.NewtonsoftJson)]
        [InlineData(ServerType.Controllers, JsonParser.NewtonsoftJson)]
        public async Task PostTest_ParseSync_ReturnsValidApiError(ServerType serverType, JsonParser jsonParser)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string""}}", Encoding.UTF8, "text/json");
            const string expectedError = "The NonNullableObject field requires a non-default value.";
            var expectedServiceName = Assembly.GetEntryAssembly().GetName().Name;

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = new ApiError();
            var success = false;
            switch (jsonParser)
            {
                case JsonParser.SystemTextJson:
                    success = response.TryParseApiError(out error);
                    break;
                case  JsonParser.NewtonsoftJson:
                    success = response.TryParseApiErrorUsingNewtonsoftJson(out error);
                    break;
            }

            // Assert
            success.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.ErrorCode.Should().Be(1337);
            error.Error.Should().Be(ValidationError);
            switch (jsonParser)
            {
                case JsonParser.SystemTextJson:
                    ((JsonElement)error.Context).GetProperty("NonNullableObject").EnumerateArray().FirstOrDefault().ToString().Should().Be(expectedError);
                    break;
                case  JsonParser.NewtonsoftJson:
                    ((JObject)error.Context)["NonNullableObject"].ToObject<string[]>().FirstOrDefault().Should().Be(expectedError);
                    break;
            }
            error.Service.Should().Be(expectedServiceName);
        }

        [Theory]
        [InlineData(ServerType.Mvc, JsonParser.SystemTextJson)]
        [InlineData(ServerType.Controllers, JsonParser.SystemTextJson)]
        [InlineData(ServerType.Mvc, JsonParser.NewtonsoftJson)]
        [InlineData(ServerType.Controllers, JsonParser.NewtonsoftJson)]
        public async Task PostTest_ParseSyncNoApiError_ReturnsNullApiError(ServerType serverType, JsonParser jsonParser)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 5}}",
                Encoding.UTF8, "text/json");

            // Act
            var response = await client.PostAsync("/api/NoExceptionNo20x", content);
            var error = new ApiError();
            var success = false;
            switch (jsonParser)
            {
                case JsonParser.SystemTextJson:
                    success = response.TryParseApiError(out error);
                    break;
                case  JsonParser.NewtonsoftJson:
                    success = response.TryParseApiErrorUsingNewtonsoftJson(out error);
                    break;
            }

            // Assert
            success.Should().BeFalse();
            error.Should().BeNull();
        }

        [Theory]
        [InlineData(ServerType.Mvc, JsonParser.SystemTextJson)]
        [InlineData(ServerType.Controllers, JsonParser.SystemTextJson)]
        [InlineData(ServerType.Mvc, JsonParser.NewtonsoftJson)]
        [InlineData(ServerType.Controllers, JsonParser.NewtonsoftJson)]
        public async Task PostTest_ParseSyncOnSuccessfulResponse_ReturnsNullApiError(ServerType serverType, JsonParser jsonParser)
        {
            //Arrange
            var client = SetupServer(serverType);
            var content = new StringContent($@"{{""NullableObject"": ""string"", ""NonNullableObject"": 1}}",
                Encoding.UTF8, "text/json");

            // Act
            var response = await client.PostAsync("/api/Test", content);
            var error = new ApiError();
            var success = false;
            switch (jsonParser)
            {
                case JsonParser.SystemTextJson:
                    success = response.TryParseApiError(out error);
                    break;
                case  JsonParser.NewtonsoftJson:
                    success = response.TryParseApiErrorUsingNewtonsoftJson(out error);
                    break;
            }

            // Assert
            success.Should().BeFalse();
            error.Should().BeNull();
        }
    }
}