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

        private Lazy<HashSet<string>> _httpMethods;

        public ReRouteOptions()
        {
            _httpMethods = new Lazy<HashSet<string>>(()
                => new HashSet<string>(UpstreamHttpMethod, StringComparer.OrdinalIgnoreCase));
            UpstreamHttpMethod =
                new List<string>(){ "get", "post", "put", "delete", "options", "patch", "head", "connect", "trace" };
        }

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
        /// Determines whether [contains HTTP method] [the specified method type].
        /// </summary>
        /// <param name="methodType">Type of the method.</param>
        /// <returns>
        ///   <c>true</c> if [contains HTTP method] [the specified method type]; otherwise, <c>false</c>.
        /// </returns>
        internal bool ContainsHttpMethod(string methodType)
            => _httpMethods.Value.Contains(methodType);

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

                return ret.RemoveSlashFromEnd();
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
        public string UpstreamPath => Replace(UpstreamPathTemplate).RemoveSlashFromEnd();

        private string Replace(string value) => value.Replace(CatchAllPlaceHolder, "");
    }
}
