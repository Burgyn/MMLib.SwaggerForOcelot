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

        return services.Response
            .Select(s => ConvertToOption(s.Key, s.Value))
            .ToList();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <param name="service"></param>
    /// <returns></returns>
    private SwaggerEndPointOptions ConvertToOption(string key, AgentService service)
    {
        var option = new SwaggerEndPointOptions();
        option.Key = key;
        option.TransformByOcelotConfig = false;
        option.Config = service.Meta
            .Where(w => w.Key.StartsWith("swagger"))
            .Select(swagger => ConvertToConfig(swagger, service))
            .ToList();

        if (option.Config.Count == 0)
            option.Config.Add(DefaultConfig(service));

        return option;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="swagger"></param>
    /// <param name="service"></param>
    /// <returns></returns>
    private SwaggerEndPointConfig ConvertToConfig(KeyValuePair<string, string> swagger, AgentService service)
    {
        var config = new SwaggerEndPointConfig();
        config.Name = $"{service.Service} API";
        config.Version = swagger.Value;
        config.Service = new SwaggerService { Name = service.Service, Path = $"swagger/{swagger.Value}/swagger.json" };

        return config;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    private SwaggerEndPointConfig DefaultConfig(AgentService service)
    {
        var config = new SwaggerEndPointConfig();
        config.Name = $"{service.Service} API";
        config.Version = "v1";
        config.Service = new SwaggerService { Name = service.Service, Path = $"swagger/{config.Version}/swagger.json" };

        return config;
    }
}
