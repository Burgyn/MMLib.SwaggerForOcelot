using Kros.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MMLib.SwaggerForOcelot.Repositories
{
    /// <summary>
    /// Repository for obtaining downstream swager docs.
    /// </summary>
    public class DownstreamSwaggerDocsRepository : IDownstreamSwaggerDocsRepository
    {
        private readonly IOptions<SwaggerForOcelotUIOptions> _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISwaggerServiceDiscoveryProvider _serviceDiscoveryProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownstreamSwaggerDocsRepository"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        /// <param name="serviceDiscoveryProvider">The service discovery provider.</param>
        public DownstreamSwaggerDocsRepository(
            IOptions<SwaggerForOcelotUIOptions> options,
            IHttpClientFactory httpClientFactory,
            ISwaggerServiceDiscoveryProvider serviceDiscoveryProvider)
        {
            _options = options;
            _httpClientFactory = httpClientFactory;
            _serviceDiscoveryProvider = serviceDiscoveryProvider;
        }

        /// <inheritdoc />
        public async Task<string> GetSwaggerJsonAsync(
            RouteOptions route,
            SwaggerEndPointOptions endPoint,
            string docsVersion = null)
        {
            string url = await GetUrlAsync(route, endPoint, docsVersion);
            var clientName = (route?.DangerousAcceptAnyServerCertificateValidator ?? false) ? ServiceCollectionExtensions.IgnoreSslCertificate : string.Empty;
            HttpClient httpClient = _httpClientFactory.CreateClient(clientName);

            SetHttpVersion(httpClient, route);
            AddHeaders(httpClient);

            return await httpClient.GetStringAsync(url);
        }

        private void AddHeaders(HttpClient httpClient)
        {
            if (_options.Value.DownstreamSwaggerHeaders is null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> kvp in _options.Value.DownstreamSwaggerHeaders)
            {
                httpClient.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
            }
        }

        private void SetHttpVersion(HttpClient httpClient, RouteOptions route)
        {
            string downstreamHttpVersion = route?.DownstreamHttpVersion;
            if (!downstreamHttpVersion.IsNullOrEmpty())
            {
                int[] version = downstreamHttpVersion!.Split('.').Select(int.Parse).ToArray();
                httpClient.DefaultRequestVersion = new Version(version[0], version[1]);
                // HTTP/2 over insecure http requires non-default version policy.
                if (route?.DownstreamScheme == "http" && version[0] == 2)
                {
                    httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                }
            }
        }

        private async Task<string> GetUrlAsync(
            RouteOptions route,
            SwaggerEndPointOptions endPoint,
            string docsVersion)
        {
            SwaggerEndPointConfig config =
                string.IsNullOrEmpty(docsVersion)
                ? endPoint.Config.FirstOrDefault()
                : endPoint.Config.FirstOrDefault(x => x.Version == docsVersion);

            return (await _serviceDiscoveryProvider
                .GetSwaggerUriAsync(config, route))
                .AbsoluteUri;
        }
    }
}
