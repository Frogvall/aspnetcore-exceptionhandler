using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Frogvall.AspNetCore.ExceptionHandling.Exceptions;

namespace Frogvall.AspNetCore.ExceptionHandling.Mapper
{
    internal class ExceptionMapper : IExceptionMapper
    {
        private readonly Dictionary<Type, ExceptionDescription> _map;
        public ExceptionMapperOptions Options { get; }

        internal ExceptionMapper(IEnumerable<TypeInfo> profiles, ExceptionMapperOptions options)
        {
            _map = new Dictionary<Type, ExceptionDescription>();
            foreach (var profile in profiles.Select(t => t.AsType()))
            {
                if (!(Activator.CreateInstance(profile) is IExceptionMappingProfile profileInstance)) continue;
                var exceptionMap = (Dictionary<Type, ExceptionDescription>)profile.GetField(
                    "ExceptionMap", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField)?.GetValue(profileInstance);
                if (exceptionMap == null) continue;
                var intersect = _map.Keys.Intersect(exceptionMap.Keys).ToList();
                if (intersect.Any()) throw new InvalidOperationException($"Duplicate entry. Exceptions already added to map: {string.Join(",", intersect)}");
                _map = _map.Union(exceptionMap).ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            Options = options;
        }

        internal abstract class ExceptionDescription
        {
            public HttpStatusCode ExceptionHandlerReturnCode { get; set; }
        }

        internal class ExceptionDescription<TException> : ExceptionDescription where TException : BaseApiException
        {
            public Func<TException, int> ErrorCode { get; set; }
        }

        public int GetErrorCode(BaseApiException exception)
        {
            try
            {
                var exceptionType = exception.GetType();
                if (!_map.ContainsKey(exceptionType))
                    throw new ArgumentException(
                        $"Exception {exceptionType.FullName} has not been mapped accordingly. Should be treated as an unexpected exception");
                var exceptionDescriptionType = typeof(ExceptionDescription<>).MakeGenericType(exceptionType);
                var instance = Convert.ChangeType(_map[exceptionType], exceptionDescriptionType);
                var property = exceptionDescriptionType.GetProperty("ErrorCode");
                var method = property?.PropertyType.GetMethod("Invoke");
                var errorCode = method?.Invoke(property.GetValue(instance), new object[] { exception });
                if (errorCode == null)
                    throw new ArgumentException(
                        $"Exception {exceptionType.FullName} has not been mapped accordingly. Should be treated as an unexpected exception");
                return (int) errorCode;
            }
            catch
            {
                throw new ArgumentException(
                    $"Exception {exception.GetType().FullName} has not been mapped accordingly. Should be treated as an unexpected exception");
            }
        }

        public HttpStatusCode GetExceptionHandlerReturnCode(BaseApiException exception)
        {
            var exceptionType = exception.GetType();
            if (!exceptionType.IsSubclassOf(typeof(BaseApiException)))
                throw new ArgumentException("exception needs to be a subclass of BaseApiException");
            if (!_map.ContainsKey(exceptionType))
                throw new ArgumentException($"Exception {exceptionType.FullName} has not been mapped. Should be treated as an unexpected exception");
            return _map[exceptionType].ExceptionHandlerReturnCode;
        }
    }
}