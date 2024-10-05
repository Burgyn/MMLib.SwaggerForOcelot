using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace MMLib.SwaggerForOcelot.Transformation;

public partial class SwaggerJsonTransformer
{
    public string AddServiceNamePrefixToPaths(string swaggerJson, SwaggerEndPointOptions endPoint, string version)
    {
        SwaggerEndPointConfig config =
            string.IsNullOrEmpty(version)
                ? endPoint.Config.FirstOrDefault()
                : endPoint.Config.FirstOrDefault(x => x.Version == version);

        var serviceName = config?.Service?.Name;
        if (string.IsNullOrEmpty(serviceName))
            return swaggerJson;

        var swaggerDoc = JsonSerializer.Deserialize<Dictionary<string, object>>(swaggerJson);

        if (swaggerDoc != null && swaggerDoc.ContainsKey("paths"))
        {
            var paths = JsonSerializer.Deserialize<Dictionary<string, object>>(swaggerDoc["paths"].ToString());
            var modifiedPaths = new Dictionary<string, object>();

            foreach (var path in paths)
            {
                var newPath = $"/{serviceName}{path.Key}";
                modifiedPaths[newPath] = path.Value;
            }

            swaggerDoc["paths"] = modifiedPaths;
            return JsonSerializer.Serialize(swaggerDoc, new JsonSerializerOptions { WriteIndented = true });
        }

        return swaggerJson;
    }
}
