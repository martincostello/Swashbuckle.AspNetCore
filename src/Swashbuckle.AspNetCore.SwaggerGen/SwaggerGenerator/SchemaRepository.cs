using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class SchemaRepository(string documentName = null)
{
    private readonly Dictionary<Type, string> _reservedIds = [];

    public string DocumentName { get; } = documentName;

    public Dictionary<string, IOpenApiSchema> Schemas { get; private set; } = [];

    public void RegisterType(Type type, string schemaId)
    {
        if (!_reservedIds.TryAdd(type, schemaId))
        {
            var conflictingType = _reservedIds.First(entry => entry.Value == schemaId).Key;

            throw new InvalidOperationException(
                $"Cannot use schema Id \"${schemaId}\" for type \"${type}\". " +
                $"The same schema Id is already used for type \"${conflictingType}\"");
        }
    }

    public bool TryLookupByType(Type type, out IOpenApiSchema referenceSchema)
    {
        if (_reservedIds.TryGetValue(type, out string schemaId))
        {
            referenceSchema = new OpenApiSchemaReference(schemaId);
            return true;
        }

        referenceSchema = null;
        return false;
    }

    public IOpenApiSchema AddDefinition(string schemaId, IOpenApiSchema schema)
    {
        Schemas.Add(schemaId, schema);

        return new OpenApiSchemaReference(schemaId);
    }
}
