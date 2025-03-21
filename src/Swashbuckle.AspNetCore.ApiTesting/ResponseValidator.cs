using System.Collections.Specialized;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;

namespace Swashbuckle.AspNetCore.ApiTesting;

public sealed class ResponseValidator(IEnumerable<IContentValidator> contentValidators)
{
    private readonly IEnumerable<IContentValidator> _contentValidators = contentValidators;

    public void Validate(
        HttpResponseMessage response,
        OpenApiDocument openApiDocument,
        string pathTemplate,
        OperationType operationType,
        string expectedStatusCode)
    {
        var operationSpec = openApiDocument.GetOperationByPathAndType(pathTemplate, operationType, out _);
        if (!operationSpec.Responses.TryGetValue(expectedStatusCode, out var responseSpec))
        {
            throw new InvalidOperationException($"Response for status '{expectedStatusCode}' not found for operation '{operationSpec.OperationId}'");
        }

        var statusCode = (int)response.StatusCode;
        if (statusCode.ToString() != expectedStatusCode)
        {
            throw new ResponseDoesNotMatchSpecException($"Status code '{statusCode}' does not match expected value '{expectedStatusCode}'");
        }

        ValidateHeaders(responseSpec.Headers, openApiDocument, response.Headers.ToNameValueCollection());

        if (responseSpec.Content != null && responseSpec.Content.Keys.Count != 0)
        {
            ValidateContent(responseSpec.Content, openApiDocument, response.Content);
        }
    }

    private static void ValidateHeaders(
        IDictionary<string, IOpenApiHeader> headerSpecs,
        OpenApiDocument openApiDocument,
        NameValueCollection headerValues)
    {
        foreach (var entry in headerSpecs)
        {
            var value = headerValues[entry.Key];
            var headerSpec = entry.Value;

            if (headerSpec.Required && value == null)
            {
                throw new ResponseDoesNotMatchSpecException($"Required header '{entry.Key}' is not present");
            }

            if (value == null || headerSpec.Schema == null)
            {
                continue;
            }

            var schema = headerSpec.Schema;

            if (schema is OpenApiSchemaReference reference)
            {
                schema = new OpenApiSchemaReference(reference.Reference.Id, openApiDocument);
            }

            // TODO This isn't right
            if (schema is OpenApiSchema s && !s.TryParse(value, out object typedValue))
            {
                throw new ResponseDoesNotMatchSpecException($"Header '{entry.Key}' is not of type '{s.TypeIdentifier()}'");
            }
        }
    }

    private void ValidateContent(
        IDictionary<string, OpenApiMediaType> contentSpecs,
        OpenApiDocument openApiDocument,
        HttpContent content)
    {
        if (content == null || content?.Headers?.ContentLength == 0)
        {
            throw new RequestDoesNotMatchSpecException("Expected content is not present");
        }

        if (!contentSpecs.TryGetValue(content.Headers.ContentType.MediaType, out OpenApiMediaType mediaTypeSpec))
        {
            throw new ResponseDoesNotMatchSpecException($"Content media type '{content.Headers.ContentType.MediaType}' is not specified");
        }

        try
        {
            foreach (var contentValidator in _contentValidators)
            {
                if (contentValidator.CanValidate(content.Headers.ContentType.MediaType))
                {
                    contentValidator.Validate(mediaTypeSpec, openApiDocument, content);
                }
            }
        }
        catch (ContentDoesNotMatchSpecException contentException)
        {
            throw new ResponseDoesNotMatchSpecException($"Content does not match spec. {contentException.Message}");
        }
    }
}

public class ResponseDoesNotMatchSpecException(string message) : Exception(message);
