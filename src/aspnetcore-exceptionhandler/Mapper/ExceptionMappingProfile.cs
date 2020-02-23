using System;
using System.Collections.Generic;
using System.Net;
using Frogvall.AspNetCore.ExceptionHandling.Exceptions;

namespace Frogvall.AspNetCore.ExceptionHandling.Mapper
{
    public abstract class ExceptionMappingProfile<TErrorCode> : IExceptionMappingProfile where TErrorCode : struct, Enum
    {
        internal readonly Dictionary<Type, ExceptionMapper.ExceptionDescription> ExceptionMap = new Dictionary<Type, ExceptionMapper.ExceptionDescription>();

        protected void AddMapping<TException>(HttpStatusCode exceptionHandlerReturnCode, TErrorCode errorCode) where TException : BaseApiException
        {
            AddMapping<TException>(exceptionHandlerReturnCode, ex => (int)(object)errorCode, ex => typeof(TErrorCode).FullName);
        }

        protected void AddMapping<TException>(HttpStatusCode exceptionHandlerReturnCode, Func<TException, TErrorCode> errorCode) where TException : BaseApiException
        {
            AddMapping<TException>(exceptionHandlerReturnCode, ex => (int) (object) errorCode.Invoke(ex), ex => errorCode.Invoke(ex).GetType().FullName);
        }
        
        private void AddMapping<TException>(HttpStatusCode exceptionHandlerReturnCode, Func<TException, int> errorCode, Func<TException, string> error) where TException : BaseApiException
        {
            var typeOfTException = typeof(TException);
            if (ExceptionMap.ContainsKey(typeOfTException))
                throw new InvalidOperationException($"Duplicate entry. Exception already added to map: {typeOfTException.FullName}");
            if ((int)exceptionHandlerReturnCode < 400 || (int)exceptionHandlerReturnCode > 599)
                throw new ArgumentException($"Invalid http status code: {(int)exceptionHandlerReturnCode} {exceptionHandlerReturnCode.ToString()}. Only 4xx and 5xx status codes are allowed.");
            ExceptionMap.Add(typeOfTException, new ExceptionMapper.ExceptionDescription<TException> { ErrorCode = errorCode, Error = error, ExceptionHandlerReturnCode = exceptionHandlerReturnCode });
        }
    }
}