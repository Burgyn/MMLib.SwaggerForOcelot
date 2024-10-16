using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MMLib.ServiceDiscovery.Consul.DependencyInjection;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddConsulAutoServiceDiscovery(this WebApplicationBuilder builder,
        string consulAddress)
    {
        builder.Configuration.ConfigureSettings(consulAddress);
        builder.Services.RegisterServices(consulAddress);

        return builder;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="configManager"></param>
    /// <param name="consulAddress"></param>
    /// <returns></returns>
    public static IConfigurationBuilder ConfigureSettings(this IConfigurationBuilder configManager, string consulAddress)
    {
        configManager.AddJsonFile("appsettings.dev.json", true);
        configManager.AddJsonFile("appsettings.prod.json", true);
        configManager.AddConsul(consulAddress);
        configManager.AddEnvironmentVariables();

        return configManager;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="configurationManager"></param>
    /// <param name="consulAddress"></param>
    /// <returns></returns>
    public static IConfigurationBuilder AddConsul(this IConfigurationBuilder configurationManager, string consulAddress)
    {
        var confManager = configurationManager.Build();
        var address = confManager.GetValue<string>("ConsulAddress");
        var client = new ConsulClient();
        client.Config.Address = new Uri(address ?? consulAddress);

        var projectName = Assembly.GetEntryAssembly()?.GetName().Name;
        configurationManager.AddConsulJsonStream(client, "appsettings.json");
        configurationManager.AddConsulJsonStream(client, $"appsettings.{projectName}.json");
        return configurationManager;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="configurationManager"></param>
    /// <param name="consulClient"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private static IConfigurationBuilder AddConsulJsonStream(this IConfigurationBuilder configurationManager,
        ConsulClient consulClient, string key)
    {
        var res = consulClient.KV.Get(key).Result;
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            return configurationManager;

        var responseBytes = res.Response.Value;
        var stream = new MemoryStream(responseBytes);
        configurationManager.AddJsonStream(stream);
        return configurationManager;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <param name="consulAddress"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterServices(this IServiceCollection services, string consulAddress)
    {
        services
            .RegisterConsulClient(consulAddress)
            .AddHealthChecks();

        services.AddSingleton<ISwaggerService, SwaggerService>();
        services.AddSingleton<IConsulConnectionService, ConsulConnectionService>();
        services.AddSingleton<IConsulNameCorrectorService, ConsulNameCorrectorService>();

        return services;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <param name="consulAddress"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterConsulClient(this IServiceCollection services, string consulAddress)
    {
        services.AddSingleton<IConsulClient>(f => CreateConsuleClient(f, consulAddress));
        return services;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="consulAddress"></param>
    /// <returns></returns>
    private static ConsulClient CreateConsuleClient(IServiceProvider serviceProvider, string consulAddress)
    {
        var configuration = serviceProvider.GetService<IConfiguration>();
        var addr = configuration?.GetValue<string>("ConsulAddress");

        return new ConsulClient(c => c.Address = new Uri(addr ?? consulAddress));
    }
}
