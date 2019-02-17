using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Frogvall.AspNetCore.ExceptionHandling.OperationFilters
{
    public class ValidateModelOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            operation.Responses.Add("400", new Response { Description = "Bad request" });
        }
    }
}
