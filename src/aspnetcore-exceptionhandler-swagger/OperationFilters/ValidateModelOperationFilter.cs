using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Frogvall.AspNetCore.ExceptionHandling.OperationFilters
{
    public class ValidateModelOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Responses.Add("400", new OpenApiResponse { Description = "Bad request" });
        }
    }
}
