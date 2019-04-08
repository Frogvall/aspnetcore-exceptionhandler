using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Frogvall.AspNetCore.ExceptionHandling.Mapper;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExceptionMapper(this IServiceCollection services)
        {
            return AddExceptionMapperClasses(services, AppDomain.CurrentDomain.GetAssemblies(), null);
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, ExceptionMapperOptions options)
        {
            return AddExceptionMapperClasses(services, AppDomain.CurrentDomain.GetAssemblies(), options);
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, params Assembly[] assemblies)
        {
            return AddExceptionMapperClasses(services, assemblies, null);
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, ExceptionMapperOptions options, params Assembly[] assemblies)
        {
            return AddExceptionMapperClasses(services, assemblies, options);
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, params Type[] profileAssemblyMarkerTypes)
        {
            return AddExceptionMapperClasses(services, profileAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly), null);
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, ExceptionMapperOptions options, params Type[] profileAssemblyMarkerTypes)
        {
            return AddExceptionMapperClasses(services, profileAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly), options);
        }

        private static IServiceCollection AddExceptionMapperClasses(IServiceCollection services, IEnumerable<Assembly> assembliesToScan, ExceptionMapperOptions options)
        {
            // Just return if we've already added ExceptionMapper to avoid double-registration
            if (services.Any(sd => sd.ServiceType == typeof(IExceptionMapper)))
                return services;
            var allTypes = assembliesToScan
                .SelectMany(a => a.DefinedTypes)
                .ToArray();
            var profileTypeInfo = typeof(IExceptionMappingProfile).GetTypeInfo();
            var profiles = allTypes
                .Where(t => profileTypeInfo.IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();
            return services.AddSingleton<IExceptionMapper>(sp => new ExceptionMapper(profiles, options ?? new ExceptionMapperOptions()));
        }
    }
}