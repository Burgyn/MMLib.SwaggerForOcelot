using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MMLib.SwaggerForOcelot.ServiceDiscovery.ConsulServiceDiscoveries;

/// <summary>
///
/// </summary>
public interface IConsulServiceDiscovery
{
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    Task<List<SwaggerEndPointOptions>> GetServicesAsync();

    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<SwaggerEndPointOptions> GetByKeyAsync(string key);
}
