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
        /// Generates docs for gateway it self with options.
        /// </summary>
        /// <param name="options">Gateway itself docs generation options.</param>
        public void GenerateDocsDocsForGatewayItSelf(Action<OcelotGatewayItSelfSwaggerGenOptions> options = null)
        {
            GenerateDocsForGatewayItSelf = true;

            OcelotGatewayItSelfSwaggerGenOptions = new OcelotGatewayItSelfSwaggerGenOptions();
            options?.Invoke(OcelotGatewayItSelfSwaggerGenOptions);
        }

        /// <summary>
        /// Adds a mapping between Ocelot's AuthenticationProviderKey and Swagger's securityScheme
        /// If a route has a match, security definition will be added to the endpoint with the provided AllowedScopes from the config.
        /// </summary>
        /// <param name="authenticationProviderKey"></param>
        /// <param name="securityScheme"></param>
        public void AddAuthenticationProviderKeyMapping(string authenticationProviderKey, string securityScheme)
        {
            AuthenticationProviderKeyMap.Add(authenticationProviderKey, securityScheme);
        }

        /// <summary>
        /// Register aggregate docs generator post process.
        /// </summary>
        public Action<SwaggerAggregateRoute, IEnumerable<RouteDocs>, OpenApiPathItem, OpenApiDocument> AggregateDocsGeneratorPostProcess { get; set; }
             = AggregateRouteDocumentationGenerator.DefaultPostProcess;

        internal static OcelotSwaggerGenOptions Default { get; } = new OcelotSwaggerGenOptions();

        internal const string AggregatesKey = "aggregates";

        internal const string GatewayKey = "gateway";

        internal OcelotGatewayItSelfSwaggerGenOptions OcelotGatewayItSelfSwaggerGenOptions { get; private set; }

        internal Dictionary<string, string> AuthenticationProviderKeyMap { get; } = new();
    }
}
