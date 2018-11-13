using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations
{
    public class AnnotationsOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.MethodInfo == null) return;

            // Action descriptions should take precedence over controller descriptions and
            // the most derived controller descrption should take precedence over the least,
            // so do not overwrite the description if there is a value already there.
            var controllerAttributes = GetOrderedControllerAttributes(context);
            var actionAttributes = context.MethodInfo.GetCustomAttributes(true);
            var controllerAndActionAttributes = controllerAttributes.Union(actionAttributes);

            ApplySwaggerOperationAttribute(operation, actionAttributes);
            ApplySwaggerOperationFilterAttributes(operation, context, controllerAndActionAttributes);
            ApplySwaggerResponseAttributes(operation, controllerAndActionAttributes);
        }

        private static void ApplySwaggerOperationAttribute(
            Operation operation,
            IEnumerable<object> actionAttributes)
        {
            var swaggerOperationAttribute = actionAttributes
                .OfType<SwaggerOperationAttribute>()
                .FirstOrDefault();

            if (swaggerOperationAttribute == null) return;

            if (swaggerOperationAttribute.Summary != null)
                operation.Summary = swaggerOperationAttribute.Summary;

            if (swaggerOperationAttribute.Description != null)
                operation.Description = swaggerOperationAttribute.Description;

            if (swaggerOperationAttribute.OperationId != null)
                operation.OperationId = swaggerOperationAttribute.OperationId;

            if (swaggerOperationAttribute.Tags != null)
                operation.Tags = swaggerOperationAttribute.Tags;

            if (swaggerOperationAttribute.Consumes != null)
                operation.Consumes = swaggerOperationAttribute.Consumes;

            if (swaggerOperationAttribute.Produces != null)
                operation.Produces = swaggerOperationAttribute.Produces;

            if (swaggerOperationAttribute.Schemes != null)
                operation.Schemes = swaggerOperationAttribute.Schemes;
        }

        public static void ApplySwaggerOperationFilterAttributes(
            Operation operation,
            OperationFilterContext context,
            IEnumerable<object> actionAndControllerAttributes)
        {
            var swaggerOperationFilterAttributes = actionAndControllerAttributes
                .OfType<SwaggerOperationFilterAttribute>();

            foreach (var swaggerOperationFilterAttribute in swaggerOperationFilterAttributes)
            {
                var filter = (IOperationFilter)Activator.CreateInstance(swaggerOperationFilterAttribute.FilterType);
                filter.Apply(operation, context);
            }
        }

        private void ApplySwaggerResponseAttributes(
            Operation operation,
            IEnumerable<object> actionAndControllerAttributes)
        {
            var swaggerResponseAttributes = actionAndControllerAttributes
                .OfType<SwaggerResponseAttribute>();

            foreach (var swaggerResponseAttribute in swaggerResponseAttributes)
            {
                var statusCode = swaggerResponseAttribute.StatusCode.ToString();

                if (operation.Responses == null)
                {
                    operation.Responses = new Dictionary<string, Response>();
                }

                if (!operation.Responses.TryGetValue(statusCode, out Response response))
                {
                    response = new Response();
                }

                if (swaggerResponseAttribute.Description != null)
                    response.Description = swaggerResponseAttribute.Description;

                operation.Responses[statusCode] = response;
            }
        }

        private ICollection<object> GetOrderedControllerAttributes(OperationFilterContext context)
        {
            var allAttributes = context.MethodInfo.DeclaringType
                .GetTypeInfo()
                .GetCustomAttributes(true)
                .ToArray();

            // If there are multiple response attributes, sort them so that the "last one wins"
            int responseAttributeCount = allAttributes.OfType<SwaggerResponseAttribute>().Count();

            if (responseAttributeCount > 1)
            {
                var otherAttributes = allAttributes.Where(x => !(x is SwaggerResponseAttribute));
                var responseAttributes = GetSortedResponseAttributes(context.MethodInfo.DeclaringType);

                allAttributes = otherAttributes.Concat(responseAttributes).ToArray();
            }

            return allAttributes;
        }

        private List<SwaggerResponseAttribute> GetSortedResponseAttributes(Type actionDeclaringType)
        {
            var result = new List<SwaggerResponseAttribute>();

            // Walk the class hierarchy of the declaring class for the action to find the response attributes
            Type type = actionDeclaringType;

            while (type != null)
            {
                result.AddRange(type.GetCustomAttributes<SwaggerResponseAttribute>(inherit: false));
                type = type.BaseType;
            }

            result.Reverse(); // We want the most derived type's attributes to be last so that they "win"

            return result;
        }
    }
}