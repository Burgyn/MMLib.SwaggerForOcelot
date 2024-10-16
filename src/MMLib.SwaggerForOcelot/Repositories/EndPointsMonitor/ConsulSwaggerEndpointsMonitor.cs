#nullable enable
using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.Configuration;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMLib.SwaggerForOcelot.Repositories;

/// <summary>
///
/// </summary>
public class ConsulSwaggerEndpointsMonitor : ISwaggerEndpointsMonitor
{
    /// <summary>
    ///
    /// </summary>
    private readonly IOptionsMonitor<List<SwaggerEndPointOptions>> _optionsMonitor;

    /// <summary>
    ///
    /// </summary>
    private readonly IConsulEndpointOptionsMonitor _consulOptionsMonitor;

    /// <summary>
    ///
    /// </summary>
    public event EventHandler<List<SwaggerEndPointOptions>> OptionsChanged;

    /// <summary>
    ///
    /// </summary>
    /// <param name="optionsMonitor"></param>
    /// <param name="consulOptionsMonitor"></param>
    public ConsulSwaggerEndpointsMonitor(IOptionsMonitor<List<SwaggerEndPointOptions>> optionsMonitor,
        IConsulEndpointOptionsMonitor consulOptionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
        _consulOptionsMonitor = consulOptionsMonitor;
        _optionsMonitor.OnChange(ConfigChanged);
        _consulOptionsMonitor.OptionsChanged +=(s,e) => ConfigChanged(e);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="configOptions"></param>
    /// <returns></returns>
    private void ConfigChanged(List<SwaggerEndPointOptions> configOptions)
    {
        var options = ConcatOptions(_optionsMonitor.CurrentValue, _consulOptionsMonitor.CurrentValue);
        CallOptionsChanged(options);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="configOptions"></param>
    /// <param name="localOptions"></param>
    /// <returns></returns>
    protected virtual List<SwaggerEndPointOptions> ConcatOptions(List<SwaggerEndPointOptions> configOptions,
        List<SwaggerEndPointOptions> localOptions)
    {
        return configOptions
            .Concat(localOptions)
            .DistinctBy(s => s.Key)
            .ToList();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="options"></param>
    protected virtual void CallOptionsChanged(List<SwaggerEndPointOptions> options)
    {
        OptionsChanged?.Invoke(this, options);
    }
}
