using MMLib.SwaggerForOcelot.Configuration;

namespace ApiGatewayWithEndpointSecurity.Repository
{
    public interface ISwaggerEndpointConfigurationRepository
    {
        ManageSwaggerEndpointData GetSwaggerEndpoint(SwaggerEndPointOptions endPoint, string version);
    }
}
