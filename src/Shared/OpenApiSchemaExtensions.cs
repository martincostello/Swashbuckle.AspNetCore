using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;

namespace Swashbuckle.AspNetCore;

internal static class OpenApiSchemaExtensions
{
    public static string TryResolveReference(this IOpenApiSchema schema)
    {
        if (schema is OpenApiSchemaReference reference)
        {
            IOpenApiSchema target = reference.Target;
            ////var definitiveTarget = reference.RecursiveTarget;
            return target.Id;
        }

        return null;
    }

    public static OpenApiSchema GetSchema(this IOpenApiSchema value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value is OpenApiSchema schema)
        {
            return schema;
        }
        else if (value is OpenApiSchemaReference reference)
        {
            return reference.Target.GetSchema();
            //return reference.RecursiveTarget;
        }
        else
        {
            throw new NotSupportedException($"Unable to obtain a {nameof(OpenApiSchema)} from a value with type {value.GetType()}.");
        }
    }

    public static void SetNullable(this IOpenApiSchema value, bool nullable)
    {
        var schema = value.GetSchema();
        schema.SetNullable(nullable);
    }

    public static void SetNullable(this OpenApiSchema value, bool nullable)
    {
        if (nullable)
        {
            value.Type |= JsonSchemaType.Null;
        }
        else
        {
            value.Type &= ~JsonSchemaType.Null;
        }
    }
}
