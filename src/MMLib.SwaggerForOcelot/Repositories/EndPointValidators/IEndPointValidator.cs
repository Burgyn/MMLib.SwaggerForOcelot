using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Repositories.EndPointValidators;

/// <summary>
///
/// </summary>
public interface IEndPointValidator
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="endPoints"></param>
    void Validate(IReadOnlyList<SwaggerEndPointOptions> endPoints);
}
