using System.Net;
using Frogvall.AspNetCore.ExceptionHandling.Exceptions;

namespace Frogvall.AspNetCore.ExceptionHandling.Mapper
{
    public interface IExceptionMapper
    {
        ExceptionMapperOptions Options { get; }
        (int errorCode, string error) GetError(BaseApiException exception);
        HttpStatusCode GetExceptionHandlerReturnCode(BaseApiException exception);
    }
}