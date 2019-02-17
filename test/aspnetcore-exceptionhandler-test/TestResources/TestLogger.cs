using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Frogvall.AspNetCore.ExceptionHandling.Test.TestResources
{
    public static class TestLogger
    {
        public static ILogger<T> Create<T>(ITestOutputHelper output)
        {
            var logger = new XUnitLogger<T>(output);
            return logger;
        }

        class XUnitLogger<T> : ILogger<T>, IDisposable
        {
            private readonly Action<string> _output;

            public XUnitLogger(ITestOutputHelper output)
            {
                _output = output.WriteLine;
            }

            public void Dispose()
            {
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter) => _output(formatter(state, exception));

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state) => this;
        }
    }
}