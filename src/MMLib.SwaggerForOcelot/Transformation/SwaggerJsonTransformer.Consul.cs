using MMLib.SwaggerForOcelot.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMLib.SwaggerForOcelot.Transformation;

/// <summary>
///
/// </summary>
public partial class SwaggerJsonTransformer
{
    /// <summary>
    /// Modifies the paths in a given Swagger JSON by adding a specified service name as a prefix to each path.
    /// If the "paths" section is missing or null, the method returns the original Swagger JSON without modifications.
    /// </summary>
    /// <param name="swaggerJson">The original Swagger JSON as a string.</param>
    /// <param name="serviceName">The service name to be prefixed to each path in the Swagger JSON.</param>
    /// <param name="version"></param>
    /// <returns>
    /// A modified Swagger JSON string where each path in the "paths" section is prefixed with the provided service name.
    /// If the "paths" section does not exist or is null, the original Swagger JSON is returned.
    /// </returns>
    public string AddServiceNamePrefixToPaths(string swaggerJson, SwaggerEndPointOptions endPoint, string version)
    {
        var config = string.IsNullOrEmpty(version)
            ? endPoint.Config.FirstOrDefault()
            : endPoint.Config.FirstOrDefault(x => x.Version == version);

        var serviceName = config?.Service?.Name;
        if (string.IsNullOrEmpty(serviceName))
            return swaggerJson;

        var swaggerObj = JObject.Parse(swaggerJson);
        if (!swaggerObj.TryGetValue(OpenApiProperties.Paths, out var swaggerPaths))
            return swaggerJson;

        if (swaggerPaths is not JObject pathsObj)
            return swaggerJson;

        var properties = pathsObj.Properties().ToList();
        properties.ForEach(f => SetToPathServiceName(f, pathsObj, serviceName));

        return swaggerObj.ToString();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="jProperty"></param>
    /// <param name="pathsObj"></param>
    /// <param name="serviceName"></param>
    private void SetToPathServiceName(JProperty jProperty, JObject pathsObj, string serviceName)
    {
        jProperty.Remove();

        var path = $"/{serviceName}{jProperty.Name}";
        pathsObj.Add(path, jProperty.Value);
    }
}
