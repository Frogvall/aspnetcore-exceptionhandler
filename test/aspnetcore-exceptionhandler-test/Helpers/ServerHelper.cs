using System;
using System.Net.Http;
using Frogvall.AspNetCore.ExceptionHandling.Filters;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;
using Frogvall.AspNetCore.ExceptionHandling.Test.TestResources;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Frogvall.AspNetCore.ExceptionHandling.Test.Helpers
{
    public class ServerHelper
    {
        public static HttpClient SetupServerWithMvc(Action<MvcOptions> mvcOptions, Action<IApplicationBuilder> appBuilder, ITestOutputHelper output, ExceptionMapperOptions exceptionMapperOptions = null)
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    var descriptor =
                        new ServiceDescriptor(
                            typeof(ILogger<ValidateModelFilter>),
                            TestLogger.Create<ValidateModelFilter>(output));
                    services.Replace(descriptor);
                    services.AddExceptionMapper(exceptionMapperOptions, typeof(ServerHelper));
                    services.AddMvc(mvcOptions);
                })
                .Configure(appBuilder);

            var server = new TestServer(builder);
            return server.CreateClient();
        }

        public static HttpClient SetupServerWithControllers(Action<MvcOptions> mvcOptions, Action<IApplicationBuilder> appBuilder, ITestOutputHelper output, ExceptionMapperOptions exceptionMapperOptions = null)
        {
            var builder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.ConfigureServices(services =>
                        {
                            var descriptor =
                                new ServiceDescriptor(
                                    typeof(ILogger<ValidateModelFilter>),
                                    TestLogger.Create<ValidateModelFilter>(output));
                            services.Replace(descriptor);
                            services.AddExceptionMapper(exceptionMapperOptions, typeof(ServerHelper));
                            services.AddControllers(mvcOptions)
                                .AddNewtonsoftJson()
                                .AddApplicationPart(typeof(ServerHelper).Assembly);
                        })
                        .Configure(appBuilder);
                });
            var host = builder.Start();
            return host.GetTestClient();
        }
    }
}