#nullable enable
using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.Configuration;
using System;
using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Repositories;

/// <summary>
///
/// </summary>
public class SwaggerEndpointsMonitor : ISwaggerEndpointsMonitor
{
    /// <summary>
    ///
    /// </summary>
    private readonly IOptionsMonitor<List<SwaggerEndPointOptions>> _optionsMonitor;

    /// <summary>
    ///
    /// </summary>
    public event EventHandler<List<SwaggerEndPointOptions>> OptionsChanged;

    /// <summary>
    ///
    /// </summary>
    /// <param name="optionsMonitor"></param>
    public SwaggerEndpointsMonitor(IOptionsMonitor<List<SwaggerEndPointOptions>> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
        _optionsMonitor.OnChange(ConfigChanged);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="configOptions"></param>
    /// <returns></returns>
    private void ConfigChanged(List<SwaggerEndPointOptions> configOptions)
    {
        CallOptionsChanged(_optionsMonitor.CurrentValue);
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
