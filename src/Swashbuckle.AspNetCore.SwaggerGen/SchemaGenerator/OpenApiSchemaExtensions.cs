﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;
using AnnotationsDataType = System.ComponentModel.DataAnnotations.DataType;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public static class OpenApiSchemaExtensions
{
    private static readonly Dictionary<AnnotationsDataType, string> DataFormatMappings = new()
    {
        [AnnotationsDataType.DateTime] = "date-time",
        [AnnotationsDataType.Date] = "date",
        [AnnotationsDataType.Time] = "time",
        [AnnotationsDataType.Duration] = "duration",
        [AnnotationsDataType.PhoneNumber] = "tel",
        [AnnotationsDataType.Currency] = "currency",
        [AnnotationsDataType.Text] = "string",
        [AnnotationsDataType.Html] = "html",
        [AnnotationsDataType.MultilineText] = "multiline",
        [AnnotationsDataType.EmailAddress] = "email",
        [AnnotationsDataType.Password] = "password",
        [AnnotationsDataType.Url] = "uri",
        [AnnotationsDataType.ImageUrl] = "uri",
        [AnnotationsDataType.CreditCard] = "credit-card",
        [AnnotationsDataType.PostalCode] = "postal-code",
        [AnnotationsDataType.Upload] = "binary",
    };

    public static void ApplyValidationAttributes(this IOpenApiSchema schema, IEnumerable<object> customAttributes)
    {
        if (schema is not OpenApiSchema concrete)
        {
            return;
        }

        foreach (var attribute in customAttributes)
        {
            if (attribute is DataTypeAttribute dataTypeAttribute)
            {
                ApplyDataTypeAttribute(concrete, dataTypeAttribute);
            }
            else if (attribute is MinLengthAttribute minLengthAttribute)
            {
                ApplyMinLengthAttribute(concrete, minLengthAttribute);
            }
            else if (attribute is MaxLengthAttribute maxLengthAttribute)
            {
                ApplyMaxLengthAttribute(concrete, maxLengthAttribute);
            }
#if NET
            else if (attribute is LengthAttribute lengthAttribute)
            {
                ApplyLengthAttribute(concrete, lengthAttribute);
            }
            else if (attribute is Base64StringAttribute base64Attribute)
            {
                ApplyBase64Attribute(concrete);
            }
#endif
            else if (attribute is RangeAttribute rangeAttribute)
            {
                ApplyRangeAttribute(concrete, rangeAttribute);
            }
            else if (attribute is RegularExpressionAttribute regularExpressionAttribute)
            {
                ApplyRegularExpressionAttribute(concrete, regularExpressionAttribute);
            }
            else if (attribute is StringLengthAttribute stringLengthAttribute)
            {
                ApplyStringLengthAttribute(concrete, stringLengthAttribute);
            }
            else if (attribute is ReadOnlyAttribute readOnlyAttribute)
            {
                ApplyReadOnlyAttribute(concrete, readOnlyAttribute);
            }
            else if (attribute is DescriptionAttribute descriptionAttribute)
            {
                ApplyDescriptionAttribute(concrete, descriptionAttribute);
            }
        }
    }

    public static void ApplyRouteConstraints(this IOpenApiSchema schema, ApiParameterRouteInfo routeInfo)
    {
        if (schema is not OpenApiSchema concrete)
        {
            return;
        }

        foreach (var constraint in routeInfo.Constraints)
        {
            if (constraint is MinRouteConstraint minRouteConstraint)
            {
                ApplyMinRouteConstraint(concrete, minRouteConstraint);
            }
            else if (constraint is MaxRouteConstraint maxRouteConstraint)
            {
                ApplyMaxRouteConstraint(concrete, maxRouteConstraint);
            }
            else if (constraint is MinLengthRouteConstraint minLengthRouteConstraint)
            {
                ApplyMinLengthRouteConstraint(concrete, minLengthRouteConstraint);
            }
            else if (constraint is MaxLengthRouteConstraint maxLengthRouteConstraint)
            {
                ApplyMaxLengthRouteConstraint(concrete, maxLengthRouteConstraint);
            }
            else if (constraint is RangeRouteConstraint rangeRouteConstraint)
            {
                ApplyRangeRouteConstraint(concrete, rangeRouteConstraint);
            }
            else if (constraint is RegexRouteConstraint regexRouteConstraint)
            {
                ApplyRegexRouteConstraint(concrete, regexRouteConstraint);
            }
            else if (constraint is LengthRouteConstraint lengthRouteConstraint)
            {
                ApplyLengthRouteConstraint(concrete, lengthRouteConstraint);
            }
            else if (constraint is FloatRouteConstraint or DecimalRouteConstraint)
            {
                concrete.Type = JsonSchemaTypes.Number;
            }
            else if (constraint is LongRouteConstraint or IntRouteConstraint)
            {
                concrete.Type = JsonSchemaTypes.Integer;
            }
            else if (constraint is GuidRouteConstraint or StringRouteConstraint)
            {
                concrete.Type = JsonSchemaTypes.String;
            }
            else if (constraint is BoolRouteConstraint)
            {
                concrete.Type = JsonSchemaTypes.Boolean;
            }
        }
    }

    internal static JsonSchemaType? ResolveType(this IOpenApiSchema schema, SchemaRepository schemaRepository)
    {
        if (schema is OpenApiSchemaReference reference &&
            schemaRepository.Schemas.TryGetValue(reference.Reference.Id, out var definitionSchema))
        {
            return definitionSchema.ResolveType(schemaRepository);
        }

        foreach (var subSchema in schema.AllOf)
        {
            var type = subSchema.ResolveType(schemaRepository);
            if (type != null)
            {
                return type;
            }
        }

        return schema.Type;
    }

    private static void ApplyDataTypeAttribute(OpenApiSchema schema, DataTypeAttribute dataTypeAttribute)
    {
        if (DataFormatMappings.TryGetValue(dataTypeAttribute.DataType, out string format))
        {
            schema.Format = format;
        }
    }

    private static void ApplyMinLengthAttribute(OpenApiSchema schema, MinLengthAttribute minLengthAttribute)
    {
        if (schema.Type is { } type && type.HasFlag(JsonSchemaTypes.Array))
        {
            schema.MinItems = minLengthAttribute.Length;
        }
        else
        {
            schema.MinLength = minLengthAttribute.Length;
        }
    }

    private static void ApplyMinLengthRouteConstraint(OpenApiSchema schema, MinLengthRouteConstraint minLengthRouteConstraint)
    {
        if (schema.Type is { } type && type.HasFlag(JsonSchemaTypes.Array))
        {
            schema.MinItems = minLengthRouteConstraint.MinLength;
        }
        else
        {
            schema.MinLength = minLengthRouteConstraint.MinLength;
        }
    }

    private static void ApplyMaxLengthAttribute(OpenApiSchema schema, MaxLengthAttribute maxLengthAttribute)
    {
        if (schema.Type is { } type && type.HasFlag(JsonSchemaTypes.Array))
        {
            schema.MaxItems = maxLengthAttribute.Length;
        }
        else
        {
            schema.MaxLength = maxLengthAttribute.Length;
        }
    }

    private static void ApplyMaxLengthRouteConstraint(OpenApiSchema schema, MaxLengthRouteConstraint maxLengthRouteConstraint)
    {
        if (schema.Type is { } type && type.HasFlag(JsonSchemaTypes.Array))
        {
            schema.MaxItems = maxLengthRouteConstraint.MaxLength;
        }
        else
        {
            schema.MaxLength = maxLengthRouteConstraint.MaxLength;
        }
    }

#if NET

    private static void ApplyLengthAttribute(OpenApiSchema schema, LengthAttribute lengthAttribute)
    {
        if (schema.Type is { } type && type.HasFlag(JsonSchemaTypes.Array))
        {
            schema.MinItems = lengthAttribute.MinimumLength;
            schema.MaxItems = lengthAttribute.MaximumLength;
        }
        else
        {
            schema.MinLength = lengthAttribute.MinimumLength;
            schema.MaxLength = lengthAttribute.MaximumLength;
        }
    }

    private static void ApplyBase64Attribute(OpenApiSchema schema)
    {
        schema.Format = "byte";
    }

#endif

    private static void ApplyRangeAttribute(OpenApiSchema schema, RangeAttribute rangeAttribute)
    {
#if NET

        if (rangeAttribute.MinimumIsExclusive)
        {
            schema.ExclusiveMinimum = true;
        }

        if (rangeAttribute.MaximumIsExclusive)
        {
            schema.ExclusiveMaximum = true;
        }

#endif

        schema.Maximum = decimal.TryParse(rangeAttribute.Maximum.ToString(), out decimal maximum)
            ? maximum
            : schema.Maximum;

        schema.Minimum = decimal.TryParse(rangeAttribute.Minimum.ToString(), out decimal minimum)
            ? minimum
            : schema.Minimum;
    }

    private static void ApplyRangeRouteConstraint(OpenApiSchema schema, RangeRouteConstraint rangeRouteConstraint)
    {
        schema.Maximum = rangeRouteConstraint.Max;
        schema.Minimum = rangeRouteConstraint.Min;
    }

    private static void ApplyMinRouteConstraint(OpenApiSchema schema, MinRouteConstraint minRouteConstraint)
        => schema.Minimum = minRouteConstraint.Min;

    private static void ApplyMaxRouteConstraint(OpenApiSchema schema, MaxRouteConstraint maxRouteConstraint)
        => schema.Maximum = maxRouteConstraint.Max;

    private static void ApplyRegularExpressionAttribute(OpenApiSchema schema, RegularExpressionAttribute regularExpressionAttribute)
    {
        schema.Pattern = regularExpressionAttribute.Pattern;
    }

    private static void ApplyRegexRouteConstraint(OpenApiSchema schema, RegexRouteConstraint regexRouteConstraint)
        => schema.Pattern = regexRouteConstraint.Constraint.ToString();

    private static void ApplyStringLengthAttribute(OpenApiSchema schema, StringLengthAttribute stringLengthAttribute)
    {
        schema.MinLength = stringLengthAttribute.MinimumLength;
        schema.MaxLength = stringLengthAttribute.MaximumLength;
    }

    private static void ApplyReadOnlyAttribute(OpenApiSchema schema, ReadOnlyAttribute readOnlyAttribute)
    {
        schema.ReadOnly = readOnlyAttribute.IsReadOnly;
    }

    private static void ApplyDescriptionAttribute(OpenApiSchema schema, DescriptionAttribute descriptionAttribute)
    {
        schema.Description ??= descriptionAttribute.Description;
    }

    private static void ApplyLengthRouteConstraint(OpenApiSchema schema, LengthRouteConstraint lengthRouteConstraint)
    {
        schema.MinLength = lengthRouteConstraint.MinLength;
        schema.MaxLength = lengthRouteConstraint.MaxLength;
    }
}
