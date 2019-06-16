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
            return AddExceptionMapperClasses(services, null, AppDomain.CurrentDomain.GetAssemblies());
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, ExceptionMapperOptions options)
        {
            return AddExceptionMapperClasses(services, options, AppDomain.CurrentDomain.GetAssemblies());
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, params Assembly[] assemblies)
        {
            return AddExceptionMapperClasses(services, null, assemblies);
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, ExceptionMapperOptions options, params Assembly[] assemblies)
        {
            return AddExceptionMapperClasses(services, options, assemblies);
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, params Type[] profileAssemblyMarkerTypes)
        {
            return AddExceptionMapperClasses(services, null, profileAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly));
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, ExceptionMapperOptions options, params Type[] profileAssemblyMarkerTypes)
        {
            return AddExceptionMapperClasses(services, options, profileAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly));
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, params TypeInfo[] profileTypes)
        {
            return AddExceptionMapperClasses(services, null, profileTypes);
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, ExceptionMapperOptions options, params TypeInfo[] profileTypes)
        {
            return AddExceptionMapperClasses(services, options, profileTypes);
        }

        private static IServiceCollection AddExceptionMapperClasses(IServiceCollection services, ExceptionMapperOptions options, IEnumerable<Assembly> assembliesToScan)
        {
            // Just return if we've already added ExceptionMapper to avoid double-registration
            if (services.Any(sd => sd.ServiceType == typeof(IExceptionMapper)))
                return services;
            var allTypes = assembliesToScan
                .SelectMany(a => a.DefinedTypes)
                .ToArray();
            return AddExceptionMapperClasses(services, options, allTypes);
        }

        private static IServiceCollection AddExceptionMapperClasses(IServiceCollection services, ExceptionMapperOptions options, TypeInfo[] typeInfos)
        {
            // Just return if we've already added ExceptionMapper to avoid double-registration
            if (services.Any(sd => sd.ServiceType == typeof(IExceptionMapper)))
                return services;
            var profileTypeInfo = typeof(IExceptionMappingProfile).GetTypeInfo();
            var profiles = typeInfos
                .Where(t => profileTypeInfo.IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();
            return services.AddSingleton<IExceptionMapper>(sp => new ExceptionMapper(profiles, options ?? new ExceptionMapperOptions()));
        }
    }
}