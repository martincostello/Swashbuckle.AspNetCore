using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;

namespace Swashbuckle.AspNetCore.ApiTesting;

internal static class OpenApiDocumentExtensions
{
    internal static bool TryFindOperationById(
        this OpenApiDocument openApiDocument,
        string operationId,
        out string pathTemplate,
        out OperationType operationType)
    {
        foreach (var pathEntry in openApiDocument.Paths ?? [])
        {
            var pathItem = pathEntry.Value;

            foreach (var operationEntry in pathItem.Operations)
            {
                if (operationEntry.Value.OperationId == operationId)
                {
                    pathTemplate = pathEntry.Key;
                    operationType = operationEntry.Key;
                    return true;
                }
            }
        }

        pathTemplate = null;
        operationType = default;
        return false;
    }

    internal static OpenApiOperation GetOperationByPathAndType(
        this OpenApiDocument openApiDocument,
        string pathTemplate,
        OperationType operationType,
        out IOpenApiPathItem pathSpec)
    {
        if (openApiDocument.Paths.TryGetValue(pathTemplate, out pathSpec) &&
            pathSpec.Operations.TryGetValue(operationType, out var type))
        {
            return type;
        }

        throw new InvalidOperationException($"Operation with path '{pathTemplate}' and type '{operationType}' not found");
    }
}
