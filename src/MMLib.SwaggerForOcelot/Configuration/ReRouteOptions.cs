using Kros.Extensions;
using System;
using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Ocelot ReRoute configuration.
    /// </summary>
    public class ReRouteOptions
    {
        private const string CatchAllPlaceHolder = "{everything}";

        /// <summary>
        /// Swagger key. This key is used for generating swagger documentation for downstream services.
        /// The same key have to be in <see cref="SwaggerEndPointOptions"/> collection.
        /// </summary>
        public string SwaggerKey { get; set; }

        /// <summary>
        /// Gets or sets the downstream path template.
        /// </summary>
        public string DownstreamPathTemplate { get; set; }

        /// <summary>
        /// Gets or sets the upstream path template.
        /// </summary>
        public string UpstreamPathTemplate { get; set; }

        /// <summary>
        /// Gets or sets the upstream HTTP method.
        /// </summary>
        public IEnumerable<string> UpstreamHttpMethod { get; set; }

        /// <summary>
        /// Gets or sets the virtual directory, where is host service.
        /// </summary>
        /// <remarks>Default value is <see langword="null"/>.</remarks>
        public string VirtualDirectory { get; set; }

        /// <summary>
        /// Gets the downstream path.
        /// </summary>
        public string DownstreamPath
        {
            get
            {
                var ret = Replace(DownstreamPathTemplate);
                if (!VirtualDirectory.IsNullOrWhiteSpace()
                    && ret.StartsWith(VirtualDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    ret = ret.Substring(VirtualDirectory.Length);
                }

                return ret;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can catch all.
        /// </summary>
        public bool CanCatchAll
            => DownstreamPathTemplate.EndsWith(CatchAllPlaceHolder, StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        /// Gets the upstream path.
        /// </summary>
        public string UpstreamPath => Replace(UpstreamPathTemplate);

        private string Replace(string value) => value.Replace(CatchAllPlaceHolder, "");
    }
}
