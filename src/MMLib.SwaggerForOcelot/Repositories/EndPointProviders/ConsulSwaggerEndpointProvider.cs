using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.ServiceDiscovery.ConsulServiceDiscoveries;
using System.Collections.Generic;
using System.Linq;

namespace MMLib.SwaggerForOcelot.Repositories;

/// <summary>
///
/// </summary>
public class ConsulSwaggerEndpointProvider : ISwaggerEndPointProvider
{
    /// <summary>
    ///
    /// </summary>
    private IConsulServiceDiscovery _service;

    /// <summary>
    ///
    /// </summary>
    private readonly IOptionsMonitor<List<SwaggerEndPointOptions>> _swaggerEndPointsOptions;

    /// <summary>
    ///
    /// </summary>
    /// <param name="service"></param>
    public ConsulSwaggerEndpointProvider(IConsulServiceDiscovery service,
        IOptionsMonitor<List<SwaggerEndPointOptions>> swaggerEndPointsOptions)
    {
        _service = service;
        _swaggerEndPointsOptions = swaggerEndPointsOptions;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<SwaggerEndPointOptions> GetAll()
    {
        var endpoints = _service.GetServicesAsync().GetAwaiter().GetResult();
        if (endpoints.Count == 0)
            endpoints = _swaggerEndPointsOptions.CurrentValue;

        return endpoints;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public SwaggerEndPointOptions GetByKey(string key)
    {
        var endpoint = _service.GetByKeyAsync(key).GetAwaiter().GetResult();
        if (endpoint is null)
            endpoint = _swaggerEndPointsOptions.CurrentValue
                .FirstOrDefault(f => f.Key == key);

        return endpoint;
    }
}
