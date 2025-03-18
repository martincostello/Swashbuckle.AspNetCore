using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.ApiDescriptions
{
    /// <summary>
    /// This service will be looked up by name from the service collection when using
    /// the <c>dotnet-getdocument</c> tool from the Microsoft.Extensions.ApiDescription.Server package.
    /// </summary>
    internal interface IDocumentProvider
    {
        IEnumerable<string> GetDocumentNames();

        Task GenerateAsync(string documentName, TextWriter writer);
    }

    internal class DocumentProvider : IDocumentProvider
    {
        private readonly SwaggerGeneratorOptions _generatorOptions;
        private readonly SwaggerOptions _options;
        private readonly IAsyncSwaggerProvider _swaggerProvider;

        public DocumentProvider(
            IOptions<SwaggerGeneratorOptions> generatorOptions,
            IOptions<SwaggerOptions> options,
            IAsyncSwaggerProvider swaggerProvider)
        {
            _generatorOptions = generatorOptions.Value;
            _options = options.Value;
            _swaggerProvider = swaggerProvider;
        }

        public IEnumerable<string> GetDocumentNames()
        {
            return _generatorOptions.SwaggerDocs.Keys;
        }

        public async Task GenerateAsync(string documentName, TextWriter writer)
        {
            // Let UnknownSwaggerDocument or other exception bubble up to caller.
            var swagger = await _swaggerProvider.GetSwaggerAsync(documentName, host: null, basePath: null);
            var jsonWriter = new OpenApiJsonWriter(writer);

            if (_options.CustomDocumentSerializer != null)
            {
                _options.CustomDocumentSerializer.SerializeDocument(swagger, jsonWriter, _options.OpenApiVersion);
            }
            else
            {
                switch (_options.OpenApiVersion)
                {
                    case OpenApi.OpenApiSpecVersion.OpenApi2_0:
                        swagger.SerializeAsV2(jsonWriter);
                        break;

#if NET10_0_OR_GREATER
                    case OpenApi.OpenApiSpecVersion.OpenApi3_1:
                        swagger.SerializeAsV31(jsonWriter);
                        break;
#endif

                    default:
                    case OpenApi.OpenApiSpecVersion.OpenApi3_0:
                        swagger.SerializeAsV3(jsonWriter);
                        break;
                }
            }
        }
    }
}
