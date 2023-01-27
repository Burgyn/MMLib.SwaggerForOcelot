using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Configuration for Swagger UI.
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIOptions" />
    public class SwaggerForOcelotUIOptions
    {
        /// <summary>
        /// The relative path to gateway swagger generator.
        /// </summary>
        public string PathToSwaggerGenerator { get; set; } = "/swagger/docs";

        /// <summary>
        /// The base path to downstream service api swagger generator endpoint.
        /// </summary>
        public string DownstreamSwaggerEndPointBasePath { get; set; } = "/swagger/docs";

        /// <summary>
        /// Headers to include when requesting a downstream swagger endpoint.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> DownstreamSwaggerHeaders { get; set; }

        /// <summary>
        /// Alter swagger/openApi json after it has been transformed.
        /// </summary>
        public Func<HttpContext, string, string> ReConfigureUpstreamSwaggerJson { get; set; }

        /// <summary>
        /// Alter swagger/openApi json after it has been transformed.
        /// </summary>
        public Func<HttpContext, string, Task<string>> ReConfigureUpstreamSwaggerJsonAsync { get; set; }

        /// <summary>
        /// Configure route of server of swagger in ocelot.
        /// </summary>
        public string ServerOcelot { get; set; }

        /// <summary>
        /// Replace the internal HttpClient with a custom named client.<br/><br/> When this is set, Some downstream route specific properties will be ignored
        /// such as <see cref="RouteOptions.DangerousAcceptAnyServerCertificateValidator"/>
        /// <br/><br/><see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0#named-clients"/>
        /// </summary>
        public string? HttpClientName { get; set; }
    }
}
