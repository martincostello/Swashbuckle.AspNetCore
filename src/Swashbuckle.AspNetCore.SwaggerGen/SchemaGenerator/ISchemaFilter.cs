using System.Reflection;
using Microsoft.OpenApi.Models.Interfaces;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface ISchemaFilter
{
    void Apply(IOpenApiSchema schema, SchemaFilterContext context);
}

public class SchemaFilterContext(
    Type type,
    ISchemaGenerator schemaGenerator,
    SchemaRepository schemaRepository,
    MemberInfo memberInfo = null,
    ParameterInfo parameterInfo = null)
{
    public Type Type { get; } = type;

    public ISchemaGenerator SchemaGenerator { get; } = schemaGenerator;

    public SchemaRepository SchemaRepository { get; } = schemaRepository;

    public MemberInfo MemberInfo { get; } = memberInfo;

    public ParameterInfo ParameterInfo { get; } = parameterInfo;

    public string DocumentName => SchemaRepository.DocumentName;
}
