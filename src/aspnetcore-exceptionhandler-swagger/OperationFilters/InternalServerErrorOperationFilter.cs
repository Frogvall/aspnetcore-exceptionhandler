using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Frogvall.AspNetCore.ExceptionHandling.OperationFilters
{
    public class InternalServerErrorOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            operation.Responses.Add("500", new Response { Description = "Internal server error" });
        }
    }
}
