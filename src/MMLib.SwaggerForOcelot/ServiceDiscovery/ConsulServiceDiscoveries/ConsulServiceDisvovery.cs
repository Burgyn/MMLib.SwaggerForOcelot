using Consul;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.Configuration;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MMLib.SwaggerForOcelot.ServiceDiscovery.ConsulServiceDiscoveries;

public class ConsulServiceDisvovery : IConsulServiceDiscovery
{
    private readonly IConsulClient _consulClient;

    public ConsulServiceDisvovery(IServiceProvider serviceProvider)
    {
        _consulClient = serviceProvider.GetRequiredService<IConsulClient>();
    }

    public async Task<List<SwaggerEndPointOptions>> GetServicesAsync()
    {
        var services = await _consulClient.Agent.Services();
        var serviceNames = new List<SwaggerEndPointOptions>();

        serviceNames = services.Response.Select(service => new SwaggerEndPointOptions
        {
            Key = service.Key,
            TransformByOcelotConfig = false,
            Config = new List<SwaggerEndPointConfig>
            {
                new()
                {
                    Name = $"{service.Value.Service} API",
                    Version = "v1", //Need to make general logic
                    Service = new SwaggerService
                    {
                        Name = service.Value.Service,
                        Path = $"/swagger/v1/swagger.json" //set version
                    }
                }
            }
        }).ToList();


        return serviceNames;
    }
}
