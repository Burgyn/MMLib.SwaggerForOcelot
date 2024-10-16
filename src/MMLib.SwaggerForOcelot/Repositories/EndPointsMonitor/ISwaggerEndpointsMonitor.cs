using MMLib.SwaggerForOcelot.Configuration;
using System;
using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Repositories;

/// <summary>
///
/// </summary>
public interface ISwaggerEndpointsMonitor
{
    /// <summary>
    ///
    /// </summary>
    event EventHandler<List<SwaggerEndPointOptions>> OptionsChanged;
}
