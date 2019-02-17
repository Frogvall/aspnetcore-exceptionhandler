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
            var options = new ExceptionMapperOptions { ServiceName = Assembly.GetEntryAssembly().GetName().Name };
            return AddExceptionMapperClasses(services, AppDomain.CurrentDomain.GetAssemblies(), options);
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, ExceptionMapperOptions options)
        {
            if (options == null)
                options = new ExceptionMapperOptions();
            if (options.ServiceName == null)
                options.ServiceName = Assembly.GetEntryAssembly().GetName().Name;
            return AddExceptionMapperClasses(services, AppDomain.CurrentDomain.GetAssemblies(), options);
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, params Assembly[] assemblies)
        {
            var options = new ExceptionMapperOptions { ServiceName = Assembly.GetEntryAssembly().GetName().Name };
            return AddExceptionMapperClasses(services, assemblies, options);
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, ExceptionMapperOptions options, params Assembly[] assemblies)
        {
            if (options == null)
                options = new ExceptionMapperOptions();
            if (options.ServiceName == null)
                options.ServiceName = Assembly.GetEntryAssembly().GetName().Name;
            return AddExceptionMapperClasses(services, assemblies, options);
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, params Type[] profileAssemblyMarkerTypes)
        {
            var options = new ExceptionMapperOptions { ServiceName = Assembly.GetEntryAssembly().GetName().Name };
            return AddExceptionMapperClasses(services, profileAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly), options);
        }

        public static IServiceCollection AddExceptionMapper(this IServiceCollection services, ExceptionMapperOptions options, params Type[] profileAssemblyMarkerTypes)
        {
            if (options == null)
                options = new ExceptionMapperOptions();
            if (options.ServiceName == null)
                options.ServiceName = Assembly.GetEntryAssembly().GetName().Name;
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
            return services.AddSingleton<IExceptionMapper>(sp => new ExceptionMapper(profiles, options));
        }
    }
}