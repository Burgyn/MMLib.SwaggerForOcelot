using Kros.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Ocelot ReRoute configuration.
    /// </summary>
    public class ReRouteOptions
    {
        private const string CatchAllPlaceHolder = "{everything}";
        private readonly string[] _defaultMethodsTypes =
            new string[] { "get", "post", "put", "delete", "options", "patch", "head", "connect", "trace" };

        private readonly Lazy<HashSet<string>> _httpMethods;

        /// <summary>
        /// Ctor.
        /// </summary>
        public ReRouteOptions()
        {
            _httpMethods = new Lazy<HashSet<string>>(()
                => new HashSet<string>(
                    UpstreamHttpMethod?.Count() > 0 ? UpstreamHttpMethod : _defaultMethodsTypes,
                    StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReRouteOptions"/> class.
        /// </summary>
        /// <param name="swaggerKey">The swagger key.</param>
        /// <param name="upstreamPathTemplate">The upstream path template.</param>
        /// <param name="downstreamPathTemplate">The downstream path template.</param>
        /// <param name="virtualDirectory">The virtual directory.</param>
        /// <param name="upstreamMethods">The upstream methods.</param>
        public ReRouteOptions(
            string swaggerKey,
            string upstreamPathTemplate,
            string downstreamPathTemplate,
            string virtualDirectory,
            IEnumerable<string> upstreamMethods)
            : this()
        {
            SwaggerKey = swaggerKey;
            UpstreamPathTemplate = upstreamPathTemplate;
            DownstreamPathTemplate = downstreamPathTemplate;
            VirtualDirectory = virtualDirectory;
            UpstreamHttpMethod = upstreamMethods;
        }

        /// <summary>
        /// Swagger key. This key is used for generating swagger documentation for downstream services.
        /// The same key have to be in <see cref="SwaggerEndPointOptions"/> collection.
        /// </summary>
        public string SwaggerKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the service namespace.
        /// </summary>
        public string ServiceNamespace { get; set; }

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
        public string DownstreamPath => DownstreamPathWithVirtualDirectory.RemoveSlashFromEnd();

        internal string DownstreamPathWithShash => DownstreamPathWithVirtualDirectory.WithShashEnding();

        private readonly string _downstreamPathWithVirtualDirectory = null;

        private string DownstreamPathWithVirtualDirectory
        {
            get
            {
                if (!_downstreamPathWithVirtualDirectory.IsNullOrEmpty())
                {
                    return _downstreamPathWithVirtualDirectory;
                }

                string ret = Replace(DownstreamPathTemplate);
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
        public string UpstreamPath => Replace(UpstreamPathTemplate).RemoveSlashFromEnd();

        private string Replace(string value) => value.Replace(CatchAllPlaceHolder, "");

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
            => $"{UpstreamPathTemplate} => {DownstreamPathTemplate} | ({UpstreamPath} => {DownstreamPath})";
    }
}
