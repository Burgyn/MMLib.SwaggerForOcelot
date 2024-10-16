using Consul;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace MMLib.ServiceDiscovery.Consul;

public class ConsulConnectionService : IConsulConnectionService
{
    /// <summary>
    ///
    /// </summary>
    private IConsulClient _client;

    /// <summary>
    ///
    /// </summary>
    private IHostApplicationLifetime _applicationLifetime;

    /// <summary>
    ///
    /// </summary>
    private IServer _server;

    /// <summary>
    ///
    /// </summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    ///
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///
    /// </summary>
    private readonly ISwaggerService _swaggerService;

    /// <summary>
    ///
    /// </summary>
    /// <param name="client"></param>
    /// <param name="applicationLifetime"></param>
    /// <param name="server"></param>
    public ConsulConnectionService(
        IConsulClient client,
        IHostApplicationLifetime applicationLifetime,
        IServer server,
        IConfiguration configuration,
        IServiceProvider serviceProvider, ISwaggerService swaggerService)
    {
        _client = client;
        _applicationLifetime = applicationLifetime;
        _server = server;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _swaggerService = swaggerService;
    }

    /// <summary>
    ///
    /// </summary>
    public void Start()
    {
        _applicationLifetime.ApplicationStarted.Register(OnApplicationStarted);
    }

    /// <summary>
    ///
    /// </summary>
    private void OnApplicationStarted()
    {
        var regInfo = GetRegistrationInfo();
        _client.Agent.ServiceDeregister(regInfo.ID).Wait();
        _client.Agent.ServiceRegister(regInfo).Wait();

        Console.WriteLine("Service registered on consul: " + regInfo.Name);
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    private string? GetStartedAddress()
    {
        var addressFeature = _server.Features.Get<IServerAddressesFeature>();
        var address = addressFeature?.Addresses?.FirstOrDefault();
        if (address is null)
            return null;

        if (!address.Contains("[::]") && !address.Contains("0.0.0.0"))
            return address;

        var localIp = GetLocalIPAddress();
        address = address.Replace("[::]", localIp).Replace("0.0.0.0", localIp);

        return address;
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                return ip.ToString();
        }

        return "localhost";
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    private AgentServiceRegistration GetRegistrationInfo()
    {
        var serviceName = GetAssemblyName();
        var address = GetStartedAddress();
        var url = new Uri(address);

        var reg = new AgentServiceRegistration();
        reg.ID = serviceName;
        reg.Name = serviceName;
        reg.Port = url.Port;
        reg.Address = url.Host;
        reg.Check = new AgentServiceCheck
        {
            HTTP = $"{address}/api/health",
            Notes = "Checks /health on service",
            Timeout = TimeSpan.FromSeconds(5),
            Interval = TimeSpan.FromSeconds(2),
        };

        AddSwaggerVersionsToMeta(reg);

        var cfgPort = _configuration.GetValue<int>("ConsulSettingsPort");
        if (cfgPort != 0)
            reg.Port = cfgPort;

        var cfgHost = _configuration.GetValue<string>("ConsulSettingsAddress");
        if (!cfgHost.IsNullOrEmpty())
            reg.Address = cfgHost;

        var cfgName = _configuration.GetValue<string>("ConsulSettingsName");
        if (!cfgName.IsNullOrEmpty())
        {
            reg.Name = cfgName;
            reg.ID = cfgName;
        }

        return reg;
    }

    /// <summary>
    ///
    /// </summary>
    private string GetAssemblyName()
    {
        var assemblyName = Assembly.GetEntryAssembly()?.GetName()?.Name;
        var consulNameCorrectors = _serviceProvider.GetServices<IConsulNameCorrectorService>().ToList();
        consulNameCorrectors.ForEach(f => assemblyName = f.CorrectConsulName(assemblyName));

        return assemblyName;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="reg"></param>
    private void AddSwaggerVersionsToMeta(AgentServiceRegistration reg)
    {
        var swaggerDocs = _swaggerService.GetSwaggerInfo();

        reg.Meta = new Dictionary<string, string>();
        swaggerDocs.ForEach(version =>
        {
            reg.Meta.Add(
                $"swagger_{version.Replace(".", string.Empty)}",
                version);
            Console.WriteLine(version);
        });
    }

    /// <summary>
    /// swagger/{documentName}/swagger.{json|yaml}
    /// </summary>
    /// <param name="routeTemplate"></param>
    /// <param name="docVersion"></param>
    /// <returns></returns>
    private string GetSwaggerFileLink(string routeTemplate, string docVersion)
    {
        var urlBuilder = new StringBuilder(routeTemplate);
        urlBuilder.Replace("{documentName}", docVersion);
        urlBuilder.Replace("{json|yaml}", "json");

        return urlBuilder.ToString();
    }
}
