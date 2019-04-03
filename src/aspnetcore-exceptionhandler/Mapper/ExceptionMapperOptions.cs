using System.Reflection;

namespace Frogvall.AspNetCore.ExceptionHandling.Mapper
{
    public class ExceptionMapperOptions
    {
        public string ServiceName { get; set; } = Assembly.GetEntryAssembly().GetName().Name;
        public bool RespondWithDeveloperContext { get; set; } = true;
    }
}