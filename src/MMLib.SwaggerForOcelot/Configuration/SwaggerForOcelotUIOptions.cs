﻿using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Configuration for Swagger UI.
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIOptions" />
    public class SwaggerForOcelotUIOptions : SwaggerUIOptions
    {
        private string _pathToSwaggerUI = "/swagger/docs";

        /// <summary>
        /// The relative path to gateway swagger generator.
        /// </summary>
        public string PathToSwaggerGenerator
        {
            get => !string.IsNullOrWhiteSpace(EndPointBasePath) ? EndPointBasePath : _pathToSwaggerUI;
            set => _pathToSwaggerUI = value;
        }

        /// <summary>
        /// The base path to ocelot gateway swagger UI.
        /// </summary>
        [Obsolete("Use the 'PathToSwaggerUI' property.")]
        public string EndPointBasePath { get; set; }

        /// <summary>
        /// The base path to downstream service api swagger generator endpoint.
        /// Final path is:
        /// <see cref="EndPointBasePath"/> + <see cref="SwaggerEndPointConfig.Version"/> + <see cref="SwaggerEndPointOptions.Key"/>
        /// </summary>
        public string DownstreamSwaggerEndPointBasePath { get; set; } = "/swagger/docs";
        
        /// <summary>
        /// Headers to include when requesting a downstream swagger endpoint.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> DownstreamSwaggerHeaders { get; set; }

        /// <summary>
        /// Alter swagger/openApi json after it has been transformed
        /// </summary>
        public Func<HttpContext, string, string> ReConfigureUpstreamSwaggerJson { get; set; }

        /// <summary>
        /// Alter swagger/openApi json after it has been transformed
        /// </summary>
        public Func<HttpContext, string, Task<string>> ReConfigureUpstreamSwaggerJsonAsync { get; set; }
    }
}
