using Kros.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MMLib.SwaggerForOcelot.Configuration;
using Ocelot.Configuration.Builder;
using Ocelot.Multiplexer;
using Ocelot.Responses;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;

namespace MMLib.SwaggerForOcelot.Aggregates
{
    internal class AggregateRouteDocumentationGenerator : IAggregateRouteDocumentationGenerator
    {
        private readonly IOptions<List<RouteOptions>> _routes;
        private readonly IRoutesDocumentationProvider _routesDocumentationProvider;
        private readonly IDefinedAggregatorProvider _definedAggregatorProvider;
        private readonly Action<SwaggerAggregateRoute, IEnumerable<RouteDocs>, OpenApiPathItem, OpenApiDocument> _postProcess;
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly SchemaRepository _schemaRepository = new SchemaRepository();

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRouteDocumentationGenerator"/> class.
        /// </summary>
        /// <param name="routes">Routes.</param>
        /// <param name="routesDocumentationProvider">Routes documentation provider.</param>
        /// <param name="definedAggregatorProvider">Aggregator provider.</param>
        public AggregateRouteDocumentationGenerator(
            IOptions<List<RouteOptions>> routes,
            IRoutesDocumentationProvider routesDocumentationProvider,
            IDefinedAggregatorProvider definedAggregatorProvider,
            Action<SwaggerAggregateRoute, IEnumerable<RouteDocs>, OpenApiPathItem, OpenApiDocument> postProcess,
            ISchemaGenerator schemaGenerator)
        {
            _routes = routes;
            _routesDocumentationProvider = routesDocumentationProvider;
            _definedAggregatorProvider = definedAggregatorProvider;
            _postProcess = postProcess;
            _schemaGenerator = schemaGenerator;
        }

        public OpenApiPathItem GenerateDocs(SwaggerAggregateRoute aggregateRoute, OpenApiDocument openApiDocument)
        {
            IEnumerable<RouteDocs> routes = _routesDocumentationProvider.GetRouteDocs(aggregateRoute.RouteKeys, _routes.Value);
            OpenApiPathItem docs = GenerateDocs(aggregateRoute, routes, openApiDocument);

            _postProcess(aggregateRoute, routes, docs, openApiDocument);

            return docs;
        }

        public static Action<SwaggerAggregateRoute, IEnumerable<RouteDocs>, OpenApiPathItem, OpenApiDocument> DefaultPostProcess
        { get; } = (a, r, d, o) => { };

        private OpenApiPathItem GenerateDocs(
            SwaggerAggregateRoute aggregateRoute,
            IEnumerable<RouteDocs> routes,
            OpenApiDocument openApiDocument)
        {
            Response response = CreateResponseSchema(routes, aggregateRoute, openApiDocument);
            Dictionary<OperationType, OpenApiOperation> operations = CreateOperations(aggregateRoute, routes, response);

            return new OpenApiPathItem()
            {
                Operations = operations
            };
        }

        private static Dictionary<OperationType, OpenApiOperation> CreateOperations(
            SwaggerAggregateRoute aggregateRoute,
            IEnumerable<RouteDocs> routes,
            Response response)
            => new Dictionary<OperationType, OpenApiOperation>()
            {
                {
                    OperationType.Get,
                    CreateOperation(aggregateRoute, routes, response)
                }
            };

        private Response CreateResponseSchema(
            IEnumerable<RouteDocs> routes,
            SwaggerAggregateRoute aggregateRoute,
            OpenApiDocument openApiDocument)
        {
            AggregateResponseAttribute attribute = GetAggregatorAttribute(aggregateRoute);
            if (attribute != null)
            {
                OpenApiSchema reference = _schemaGenerator.GenerateSchema(attribute.ResponseType, _schemaRepository);
                var response = new Response()
                {
                    Description = attribute.Description,
                    MediaType = attribute.MediaType,
                    StatusCode = attribute.StatusCode
                };
                foreach (KeyValuePair<string, OpenApiSchema> item in _schemaRepository.Schemas)
                {
                    openApiDocument.Components.Schemas.Add(item.Key, item.Value);
                }

                if (reference.Reference != null)
                {
                    response.Schema = _schemaRepository.Schemas[reference.Reference.Id];
                }
                else
                {
                    response.Schema = reference;
                }

                return response;
            }

            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new SortedSet<string>(),
                AdditionalPropertiesAllowed = false
            };

            foreach (RouteDocs docs in routes)
            {
                OpenApiResponse response = docs.GetResponse();
                if (response.Content.ContainsKey(MediaTypeNames.Application.Json))
                {
                    OpenApiMediaType content = response.Content[MediaTypeNames.Application.Json];
                    schema.Properties.Add(docs.Key, content.Schema);
                }
            }

            return new Response() { Schema = schema };
        }

        private AggregateResponseAttribute GetAggregatorAttribute(SwaggerAggregateRoute aggregateRoute)
        {
            if (!aggregateRoute.Aggregator.IsNullOrEmpty())
            {
                Response<IDefinedAggregator> aggregator = _definedAggregatorProvider
                    .Get(new RouteBuilder().WithAggregator(aggregateRoute.Aggregator).Build());
                if (!aggregator.IsError)
                {
                    return aggregator.Data.GetType().GetCustomAttribute<AggregateResponseAttribute>();
                }
            }
            return null;
        }

        private static OpenApiOperation CreateOperation(
            SwaggerAggregateRoute aggregateRoute,
            IEnumerable<RouteDocs> routesDocs,
            Response response) => new OpenApiOperation()
            {
                Tags = GetTags(routesDocs),
                Summary = GetSummary(routesDocs),
                Description = GetDescription(aggregateRoute, routesDocs),
                Responses = OpenApiHelper.Responses(response),
                Parameters = GetParameters(routesDocs)
            };

        private static List<OpenApiParameter> GetParameters(IEnumerable<RouteDocs> routesDocs)
        {
            var parameters = new Dictionary<string, OpenApiParameter>(StringComparer.OrdinalIgnoreCase);
            static string GetDescription(RouteDocs docs, OpenApiParameter parameter, string prefix = null)
                => parameter.Description.IsNullOrWhiteSpace()
                    ? string.Empty
                    : $"{prefix}<strong>{docs.Key}:</strong><br />{parameter.Description}";

            foreach (RouteDocs docs in routesDocs)
            {
                foreach (OpenApiParameter parameter in docs.Parameters)
                {
                    if (!parameters.TryGetValue(parameter.Name, out OpenApiParameter newParam))
                    {
                        newParam = parameter;
                        newParam.Description = GetDescription(docs, parameter);

                        parameters.Add(newParam.Name, newParam);
                    }
                    else
                    {
                        newParam.Description += GetDescription(docs, parameter, "<br /><br />");
                    }
                }
            }

            return parameters.Values.ToList();
        }

        private static string GetSummary(IEnumerable<RouteDocs> routes)
            => $"Aggregation of routes: {RoutesToString(routes, ", ")}";

        private static string GetDescription(SwaggerAggregateRoute aggregateRoute, IEnumerable<RouteDocs> routeDocs)
            => aggregateRoute.Description ?? GetDescription(routeDocs);

        private static string GetDescription(IEnumerable<RouteDocs> routeDocs)
        {
            var sb = new StringBuilder();
            bool canAddDescription = false;

            foreach (RouteDocs docs in routeDocs)
            {
                string summary = docs.GetSummary();
                if (!summary.IsNullOrWhiteSpace())
                {
                    sb.AppendFormat("<br /><br /><strong>{0}:</strong><br />{1}", docs.Key, summary);
                    canAddDescription = true;
                }
            }

            if (canAddDescription)
            {
                sb.Insert(0, "Description from downstream services.");
            }

            return sb.ToString();
        }

        private static List<OpenApiTag> GetTags(IEnumerable<RouteDocs> route)
            => new List<OpenApiTag>() {
                new OpenApiTag() {
                    Name = RoutesToString(route)
                }
            };

        private static string RoutesToString(IEnumerable<RouteDocs> route, string separator = "-")
            => string.Join(separator, route.OrderBy(p => p.SwaggerKey).Select(r => r.SwaggerKey).Distinct());

        private class OpenApiHelper
        {
            public static Dictionary<string, OpenApiMediaType> MediaType(Response response) =>
                new Dictionary<string, OpenApiMediaType>
                {
                    [response.MediaType] = new OpenApiMediaType() { Schema = response.Schema }
                };

            public static OpenApiResponses Responses(Response response) =>
                new OpenApiResponses
                {
                    { response.StatusCode.ToString(),
                        new OpenApiResponse {
                            Description = response.Description,
                            Content = MediaType(response)
                        }
                    }
                };
        }

        private class Response
        {
            public int StatusCode { get; set; } = StatusCodes.Status200OK;

            public string MediaType { get; set; } = MediaTypeNames.Application.Json;

            public string Description { get; set; } = "Success";

            public OpenApiSchema Schema { get; set; }
        }
    }
}
