using Microsoft.OpenApi.Models;

namespace MMLib.ServiceDiscovery.Consul;

/// <summary>
///
/// </summary>
public interface ISwaggerService
{
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    List<string> GetSwaggerInfo();

}
