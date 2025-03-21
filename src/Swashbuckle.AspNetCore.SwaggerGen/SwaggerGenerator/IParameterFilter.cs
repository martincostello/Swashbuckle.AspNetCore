﻿using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models.Interfaces;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface IParameterFilter
    {
        void Apply(IOpenApiParameter parameter, ParameterFilterContext context);
    }

    public interface IParameterAsyncFilter
    {
        Task ApplyAsync(IOpenApiParameter parameter, ParameterFilterContext context, CancellationToken cancellationToken);
    }

    public class ParameterFilterContext
    {
        public ParameterFilterContext(
            ApiParameterDescription apiParameterDescription,
            ISchemaGenerator schemaGenerator,
            SchemaRepository schemaRepository,
            PropertyInfo propertyInfo = null,
            ParameterInfo parameterInfo = null)
        {
            ApiParameterDescription = apiParameterDescription;
            SchemaGenerator = schemaGenerator;
            SchemaRepository = schemaRepository;
            PropertyInfo = propertyInfo;
            ParameterInfo = parameterInfo;
        }

        public ApiParameterDescription ApiParameterDescription { get; }

        public ISchemaGenerator SchemaGenerator { get; }

        public SchemaRepository SchemaRepository { get; }

        public PropertyInfo PropertyInfo { get; }

        public ParameterInfo ParameterInfo { get; }

        public string DocumentName => SchemaRepository.DocumentName;
    }
}
