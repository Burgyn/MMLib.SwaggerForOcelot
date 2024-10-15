using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace MMLib.ServiceDiscovery.Consul.DependencyInjection;

/// <summary>
///
/// </summary>
public static class ConfigureExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseSwaggerForOcelotUI(this WebApplication builder)
    {
        builder.ConfigureConsulConnection();
        builder.MapHealthChecks("/api/health");

        return builder;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder ConfigureConsulConnection(this IApplicationBuilder builder)
    {
        var consul = builder.ApplicationServices.GetService<IConsulConnectionService>();
        consul?.Start();

        return builder;
    }
}
