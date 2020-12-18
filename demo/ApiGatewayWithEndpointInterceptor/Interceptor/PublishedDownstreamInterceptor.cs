using ApiGatewayWithEndpointSecurity.Repository;
using Microsoft.AspNetCore.Http;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.Middleware;

namespace ApiGatewayWithEndpointSecurity.Interceptor
{
    public class PublishedDownstreamInterceptor : ISwaggerDownstreamInterceptor
    {
        private readonly ISwaggerEndpointConfigurationRepository _endpointConfigurationRepository;

        public PublishedDownstreamInterceptor(ISwaggerEndpointConfigurationRepository endpointConfigurationRepository)
        {
            _endpointConfigurationRepository = endpointConfigurationRepository;
        }

        public bool DoDownstreamSwaggerEndpoint(HttpContext httpContext, string version, SwaggerEndPointOptions endPoint)
        {
            var myEndpointConfiguration = this._endpointConfigurationRepository.GetSwaggerEndpoint(endPoint, version);

            if (!myEndpointConfiguration.IsPublished)
            {
                httpContext.Response.StatusCode = 404;
                httpContext.Response.WriteAsync("This enpoint is under development, please come back later.");
            }

            return myEndpointConfiguration.IsPublished;
        }
    }
}
