using MMLib.SwaggerForOcelot.Configuration;
using System;
using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Repositories;

/// <summary>
///
/// </summary>
public interface IConsulEndpointOptionsMonitor
{
    /// <summary>
    ///
    /// </summary>
    event EventHandler<List<SwaggerEndPointOptions>> OptionsChanged;

    /// <summary>
    ///
    /// </summary>
    List<SwaggerEndPointOptions> CurrentValue { get; }
}
