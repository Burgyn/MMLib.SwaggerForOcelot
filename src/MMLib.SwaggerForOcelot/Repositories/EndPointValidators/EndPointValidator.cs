using MMLib.SwaggerForOcelot.Configuration;
using System;
using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Repositories.EndPointValidators;

/// <summary>
///
/// </summary>
public class EndPointValidator : IEndPointValidator
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="endPoints"></param>
    public void Validate(IReadOnlyList<SwaggerEndPointOptions> endPoints)
    {
        if (endPoints is null || endPoints.Count == 0)
        {
            throw new InvalidOperationException(
                $"{SwaggerEndPointOptions.ConfigurationSectionName} configuration section is missing or empty.");
        }
    }
}
