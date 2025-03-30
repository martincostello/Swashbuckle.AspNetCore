using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting;

public sealed class JsonBooleanValidator : IJsonValidator
{
    public bool CanValidate(IOpenApiSchema schema) => schema.Type == JsonSchemaTypes.Boolean;

    public bool Validate(
        IOpenApiSchema schema,
        OpenApiDocument openApiDocument,
        JToken instance,
        out IEnumerable<string> errorMessages)
    {
        if (instance.Type != JTokenType.Boolean)
        {
            errorMessages = [$"Path: {instance.Path}. Instance is not of type 'boolean'"];
            return false;
        }

        errorMessages = [];
        return true;
    }
}
