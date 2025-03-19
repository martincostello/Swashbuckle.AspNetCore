#if !NET10_0_OR_GREATER
using Microsoft.OpenApi.Models;
#else
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;
#endif

namespace Swashbuckle.AspNetCore;

internal static class OpenApiSchemaExtensions
{
#if NET10_0_OR_GREATER
    public static string TryResolveReference(this IOpenApiSchema schema)
    {
        if (schema is OpenApiSchemaReference reference)
        {
            return reference.Id;
        }

        return null;
    }
#else
    public static string TryResolveReference(this OpenApiSchema schema)
        => schema?.Reference?.Id;
#endif
}
