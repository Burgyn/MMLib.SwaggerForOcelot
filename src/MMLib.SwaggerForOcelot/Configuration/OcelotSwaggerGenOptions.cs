using Microsoft.OpenApi.Models;
using MMLib.SwaggerForOcelot.Aggregates;
using System;
using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Options for generating docs of ApiGateway.
    /// </summary>
    public class OcelotSwaggerGenOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether [generate docs for aggregates].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [generate docs for aggregates]; otherwise, <c>false</c>.
        /// </value>
        public bool GenerateDocsForAggregates { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether [generate docs for gateway it self].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [generate docs for gateway it self]; otherwise, <c>false</c>.
        /// </value>
        public bool GenerateDocsForGatewayItSelf { get; set; } = false;

        /// <summary>
        /// Get or sets values indicating file paths of XML comment of gateway it self.
        /// </summary>
        public string[] FilePathsForXmlCommentsOfGatewayItSelf { get; set; }

        /// <summary>
        /// Register aggregate docs generator post process.
        /// </summary>
        public Action<SwaggerAggregateRoute, IEnumerable<RouteDocs>, OpenApiPathItem, OpenApiDocument> AggregateDocsGeneratorPostProcess { get; set; }
             = AggregateRouteDocumentationGenerator.DefaultPostProcess;

        internal static OcelotSwaggerGenOptions Default { get; } = new OcelotSwaggerGenOptions();

        internal const string AggregatesKey = "aggregates";

        internal const string GatewayKey = "gateway";
    }
}
