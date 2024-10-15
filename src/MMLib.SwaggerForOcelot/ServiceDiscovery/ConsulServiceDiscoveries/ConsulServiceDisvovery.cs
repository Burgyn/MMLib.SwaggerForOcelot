using Consul;
using Microsoft.Extensions.DependencyInjection;
using MMLib.SwaggerForOcelot.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MMLib.SwaggerForOcelot.ServiceDiscovery.ConsulServiceDiscoveries;

/// <summary>
///
/// </summary>
public class ConsulServiceDisvovery : IConsulServiceDiscovery
{
    /// <summary>
    ///
    /// </summary>
    private readonly IConsulClient _consulClient;

    /// <summary>
    ///
    /// </summary>
    /// <param name="serviceProvider"></param>
    public ConsulServiceDisvovery(IServiceProvider serviceProvider)
    {
        _consulClient = serviceProvider.GetRequiredService<IConsulClient>();
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public Task<List<SwaggerEndPointOptions>> GetServicesAsync()
    {
        return GetConsulServices();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<SwaggerEndPointOptions> GetByKeyAsync(string key)
    {
        var services = await GetConsulServices();
        return services.FirstOrDefault(f => f.Key == key);
    }

    /// <summary>
    ///
    /// </summary>
    private async Task<List<SwaggerEndPointOptions>> GetConsulServices()
    {
        var services = await _consulClient.Agent.Services();

        var endpoints = services.Response
            .Select(service => new SwaggerEndPointOptions
            {
                Key = service.Key,
                TransformByOcelotConfig = false,
                Config = service.Value.Meta
                    .Where(w => w.Key.StartsWith("swagger"))
                    .Select(swagger => new SwaggerEndPointConfig
                    {
                        Name = $"{service.Value.Service} API",
                        Version = swagger.Value,
                        Service = new SwaggerService
                        {
                            Name = service.Value.Service, Path = $"swagger/{swagger.Value}/swagger.json"
                        }
                    }).ToList()
            }).ToList();

        return endpoints;
    }
}
