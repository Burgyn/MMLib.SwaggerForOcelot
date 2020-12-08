using Kros.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MMLib.SwaggerForOcelot.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMLib.SwaggerForOcelot.Aggregates
{
    internal class AggregateRouteDocumentationGenerator : IAggregateRouteDocumentationGenerator
    {
        private readonly IOptions<List<RouteOptions>> _routes;
        private readonly IRoutesDocumentationProvider _routesDocumentationProvider;
        private readonly Func<SwaggerAggregateRoute, IEnumerable<RouteDocs>, OpenApiPathItem> _docsGenerator;
        private readonly Action<SwaggerAggregateRoute, IEnumerable<RouteDocs>, OpenApiPathItem> _postProcess;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateRouteDocumentationGenerator"/> class.
        /// </summary>
        /// <param name="routes">Routes.</param>
        /// <param name="routesDocumentationProvider">Routes documentation provider.</param>
        /// <param name="docsGenerator">Docs generator.</param>
        public AggregateRouteDocumentationGenerator(
            IOptions<List<RouteOptions>> routes,
            IRoutesDocumentationProvider routesDocumentationProvider,
            Func<SwaggerAggregateRoute, IEnumerable<RouteDocs>, OpenApiPathItem> docsGenerator,
            Action<SwaggerAggregateRoute, IEnumerable<RouteDocs>, OpenApiPathItem> postProcess)
        {
            _routes = routes;
            _routesDocumentationProvider = routesDocumentationProvider;
            _docsGenerator = docsGenerator;
            _postProcess = postProcess;
        }

        public OpenApiPathItem GenerateDocs(SwaggerAggregateRoute aggregateRoute)
        {
            IEnumerable<RouteDocs> routes = _routesDocumentationProvider.GetRouteDocs(aggregateRoute.RouteKeys, _routes.Value);
            OpenApiPathItem docs = _docsGenerator(aggregateRoute, routes);

            if (docs == null)
            {
                docs = GenerateDocs(aggregateRoute, routes);
            }

            _postProcess(aggregateRoute, routes, docs);

            return docs;
        }

        private static OpenApiPathItem GenerateDocs(SwaggerAggregateRoute aggregateRoute, IEnumerable<RouteDocs> routes)
        {
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>(),
                Required = new SortedSet<string>(),
                AdditionalPropertiesAllowed = false
            };

            foreach (string key in aggregateRoute.RouteKeys)
            {
                schema.Properties.Add(key, new OpenApiSchema() { Type = "string" });
            }

            var operations = new Dictionary<OperationType, OpenApiOperation>()
            {
                {
                    OperationType.Get,
                    CreateOperation(aggregateRoute, routes, schema)
                }
            };

            return new OpenApiPathItem()
            {
                Operations = operations
            };
        }

        public static Func<SwaggerAggregateRoute, IEnumerable<RouteDocs>, OpenApiPathItem> DefaultGenerator { get; }
            = GenerateDocs;

        public static Action<SwaggerAggregateRoute, IEnumerable<RouteDocs>, OpenApiPathItem> DefaultPostProcess { get; }
            = (a, r, d) => { };

        private static OpenApiOperation CreateOperation(
            SwaggerAggregateRoute aggregateRoute,
            IEnumerable<RouteDocs> routes,
            OpenApiSchema schema) => new OpenApiOperation()
        {
            Tags = GetTags(routes),
            Summary = GetSummary(routes),
            Description = GetDescription(aggregateRoute, routes),
            Responses = OpenApiHelper.Responses(schema),
            Parameters = new List<OpenApiParameter>(){
                                new OpenApiParameter()
                                {
                                    Name = "everything",
                                    Description = "fsdfsd dsf dsfsd dsfsd",
                                    In = ParameterLocation.Path
                                }
                            }
        };

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
            public static Dictionary<string, OpenApiMediaType> MediaType(OpenApiSchema responseScheme) =>
                new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType() { Schema = responseScheme }
                };

            public static OpenApiResponses Responses(OpenApiSchema responseScheme) =>
                new OpenApiResponses
                {
                    { "200",
                        new OpenApiResponse {
                            Description = "Success",
                            Content = MediaType(responseScheme)
                        }
                    }
                };
        }
    }
}
