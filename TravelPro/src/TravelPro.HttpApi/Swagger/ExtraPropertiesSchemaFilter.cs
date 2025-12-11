using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Volo.Abp.ObjectExtending;
using System.Linq;

namespace TravelPro.HttpApi.Swagger
{
    public class ExtraPropertiesSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var extensionManager = ObjectExtensionManager.Instance;
            var properties = extensionManager.GetProperties(context.Type);

            if (properties == null || !properties.Any())
            {
                return;
            }

            foreach (var prop in properties)
            {
                schema.Properties[prop.Name] = new OpenApiSchema
                {
                    Type = "string",
                    Nullable = true
                };
            }
        }
    }
}
