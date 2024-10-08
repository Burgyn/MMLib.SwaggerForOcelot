using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MMLib.SwaggerForOcelot.ServiceDiscovery.ConsulServiceDiscoveries;

public interface IConsulServiceDiscovery
{
    Task<List<SwaggerEndPointOptions>> GetServicesAsync();
}
