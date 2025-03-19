using Microsoft.OpenApi.Models;
#if NET10_0_OR_GREATER
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.Readers;
#endif
using Microsoft.OpenApi.Writers;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public abstract class ApiTestRunnerBase : IDisposable
    {
#if NET10_0_OR_GREATER
        static ApiTestRunnerBase()
        {
            // TODO Make an assembly fixture
            OpenApiReaderRegistry.RegisterReader(OpenApiConstants.Yaml, new OpenApiYamlReader());
        }
#endif

        private readonly ApiTestRunnerOptions _options;
        private readonly RequestValidator _requestValidator;
        private readonly ResponseValidator _responseValidator;

        protected ApiTestRunnerBase()
        {
            _options = new ApiTestRunnerOptions();
            _requestValidator = new RequestValidator(_options.ContentValidators);
            _responseValidator = new ResponseValidator(_options.ContentValidators);
        }

        public void Configure(Action<ApiTestRunnerOptions> setupAction)
        {
            setupAction(_options);
        }

        public void ConfigureOperation(
            string documentName,
            string pathTemplate,
            OperationType operationType,
            OpenApiOperation operation)
        {
            var openApiDocument = _options.GetOpenApiDocument(documentName);

            openApiDocument.Paths ??= [];

            if (!openApiDocument.Paths.TryGetValue(pathTemplate, out var pathItem))
            {
                pathItem = new OpenApiPathItem();
                openApiDocument.Paths.Add(pathTemplate, pathItem);
            }

            // TODO Validate this is correct
            if (pathItem is OpenApiPathItem item)
            {
                item.AddOperation(operationType, operation);
            }
        }

        public async Task TestAsync(
            string documentName,
            string operationId,
            string expectedStatusCode,
            HttpRequestMessage request,
            HttpClient httpClient)
        {
            var openApiDocument = _options.GetOpenApiDocument(documentName);
            if (!openApiDocument.TryFindOperationById(operationId, out string pathTemplate, out OperationType operationType))
                throw new InvalidOperationException($"Operation with id '{operationId}' not found in OpenAPI document '{documentName}'");

#if NET8_0_OR_GREATER
            if (expectedStatusCode.StartsWith('2'))
#else
            if (expectedStatusCode.StartsWith("2"))
#endif
            {
                _requestValidator.Validate(request, openApiDocument, pathTemplate, operationType);
            }

            var response = await httpClient.SendAsync(request);

            _responseValidator.Validate(response, openApiDocument, pathTemplate, operationType, expectedStatusCode);
        }

        public void Dispose()
        {
            if (!_options.GenerateOpenApiFiles) return;

            if (_options.FileOutputRoot == null)
            {
                throw new Exception("GenerateOpenApiFiles set but FileOutputRoot is null");
            }

            foreach (var entry in _options.OpenApiDocs)
            {
                var outputDir = Path.Combine(_options.FileOutputRoot, entry.Key);
                Directory.CreateDirectory(outputDir);

                using var streamWriter = new StreamWriter(Path.Combine(outputDir, "openapi.json"));
                var openApiJsonWriter = new OpenApiJsonWriter(streamWriter);
                entry.Value.SerializeAsV3(openApiJsonWriter);
            }

            GC.SuppressFinalize(this);
        }
    }
}
