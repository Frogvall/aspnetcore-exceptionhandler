using System.Net;
using Frogvall.AspNetCore.ExceptionHandling.Exceptions;

namespace Frogvall.AspNetCore.ExceptionHandling.Mapper
{
    public interface IExceptionMapper
    {
        ExceptionMapperOptions Options { get; }
        int GetErrorCode(BaseApiException exception);
        HttpStatusCode GetExceptionHandlerReturnCode(BaseApiException exception);
    }
}