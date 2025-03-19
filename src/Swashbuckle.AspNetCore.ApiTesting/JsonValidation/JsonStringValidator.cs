using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting;

public sealed class JsonStringValidator : IJsonValidator
{
    public bool CanValidate(IOpenApiSchema schema) => schema.Type == JsonSchemaTypes.String;

    public bool Validate(
        IOpenApiSchema schema,
        OpenApiDocument openApiDocument,
        JToken instance,
        out IEnumerable<string> errorMessages)
    {
        if (instance.Type is not JTokenType.Date and not JTokenType.Guid and not JTokenType.String)
        {
            errorMessages = [$"Path: {instance.Path}. Instance is not of type 'string'"];
            return false;
        }

        var rawValue = instance.Value<string>();
        var errors = new List<string>();

        // maxLength
        if (schema.MaxLength.HasValue && (rawValue.Length > schema.MaxLength.Value))
        {
            errors.Add($"Path: {instance.Path}. String length is greater than maxLength");
        }

        // minLength
        if (schema.MinLength.HasValue && (rawValue.Length < schema.MinLength.Value))
        {
            errors.Add($"Path: {instance.Path}. String length is less than minLength");
        }

        // pattern
        if ((schema.Pattern != null) && !Regex.IsMatch(rawValue, schema.Pattern))
        {
            errors.Add($"Path: {instance.Path}. String does not match pattern");
        }

        errorMessages = errors;
        return !errorMessages.Any();
    }
}
