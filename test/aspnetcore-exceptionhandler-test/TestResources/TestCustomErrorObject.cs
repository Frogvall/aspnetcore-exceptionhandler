using System.Net;
using Frogvall.AspNetCore.ExceptionHandling.ExceptionHandling;

namespace Frogvall.AspNetCore.ExceptionHandling.Test.TestResources
{
    public sealed record TestCustomErrorObject
    {
        public TestCustomErrorObject(){}

        public TestCustomErrorObject(ApiError error, HttpStatusCode statusCode)
        {
            Detail = error.DetailedMessage;
            Status = $"{(int)statusCode}";
            Title = error.Message;
            TraceId = error.CorrelationId;
            Type = error.Error;
        }

        public string Detail { get; set; }

        public string Status { get; set; }

        public string Title { get; set; }

        public string TraceId { get; set; }

        public string Type { get; set; }
    }
}