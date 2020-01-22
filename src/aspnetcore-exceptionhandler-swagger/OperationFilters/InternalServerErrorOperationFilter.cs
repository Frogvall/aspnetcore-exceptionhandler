using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Frogvall.AspNetCore.ExceptionHandling.OperationFilters
{
    public class InternalServerErrorOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Responses.Add("500", new OpenApiResponse { Description = "Internal server error" });
        }
    }
}
