using System.Collections.Generic;
using MMLib.SwaggerForOcelot.Configuration;

namespace ApiGatewayWithEndpointSecurity.Repository
{
    public class DummySwaggerEndpointRepository : ISwaggerEndpointConfigurationRepository
    {
        private readonly Dictionary<string, ManageSwaggerEndpointData> _endpointDatas =
            new Dictionary<string, ManageSwaggerEndpointData>()
        {
            { "orders_v2", new ManageSwaggerEndpointData() { IsPublished = true } }
        };

        public ManageSwaggerEndpointData GetSwaggerEndpoint(SwaggerEndPointOptions endPoint, string version)
        {
            var lookupKey = $"{endPoint.Key}_{version}";
            var endpointData = new ManageSwaggerEndpointData();
            if (this._endpointDatas.ContainsKey(lookupKey))
            {
                endpointData = this._endpointDatas[lookupKey];
            }

            return endpointData;
        }
    }
}
