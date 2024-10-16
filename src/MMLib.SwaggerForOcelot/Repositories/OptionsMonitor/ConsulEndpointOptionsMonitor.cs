using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.ServiceDiscovery.ConsulServiceDiscoveries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace MMLib.SwaggerForOcelot.Repositories;

/// <summary>
///
/// </summary>
public class ConsulEndpointOptionsMonitor : IConsulEndpointOptionsMonitor
{
    /// <summary>
    ///
    /// </summary>
    private readonly IConsulServiceDiscovery _service;

    /// <summary>
    ///
    /// </summary>
    private readonly Timer _timer;

    /// <summary>
    ///
    /// </summary>
    public event EventHandler<List<SwaggerEndPointOptions>> OptionsChanged;

    /// <summary>
    ///
    /// </summary>
    public List<SwaggerEndPointOptions> CurrentValue { get; private set; } = new();

    /// <summary>
    ///
    /// </summary>
    public ConsulEndpointOptionsMonitor(IConsulServiceDiscovery service)
    {
        _service = service;
        _timer = new Timer(300);
        _timer.Elapsed += (s, e) => TimerElapsed();
        _timer.Start();
    }

    /// <summary>
    ///
    /// </summary>
    private void TimerElapsed()
    {
        try
        {
            TryGetConsulOptions();
        }
        catch (Exception ex)
        {
            _timer.Start();
        }
    }

    /// <summary>
    ///
    /// </summary>
    private void TryGetConsulOptions()
    {
        _timer.Stop();

        var services = _service
            .GetServicesAsync()
            .GetAwaiter()
            .GetResult();

        if (!IsOptionsChanged(services))
        {
            _timer.Start();
            return;
        }

        CurrentValue = services;
        OptionsChanged?.Invoke(this, CurrentValue);
        _timer.Start();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private bool IsOptionsChanged(List<SwaggerEndPointOptions> services)
    {
        var newValues = services.ToList();
        var oldValues = CurrentValue.ToList();
        if (newValues.Count != oldValues.Count)
            return true;

        if (oldValues.Any(a => newValues.All(c => c.Key != a.Key)))
            return true;

        return oldValues.Any(a => IsConfigChanged(a, newValues));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="newValues"></param>
    /// <returns></returns>
    private bool IsConfigChanged(SwaggerEndPointOptions endpoint, List<SwaggerEndPointOptions> newValues)
    {
        var newEndpoint = newValues.FirstOrDefault(f => f.Key == endpoint.Key);
        if (newEndpoint is null)
            return true;

        return endpoint.Config.Any(a => newEndpoint.Config.All(c => c.Name != a.Name || c.Version != a.Version));
    }
}
