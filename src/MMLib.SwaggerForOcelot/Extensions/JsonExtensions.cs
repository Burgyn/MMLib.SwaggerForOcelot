using Newtonsoft.Json.Linq;
using System;

namespace MMLib.SwaggerForOcelot.Extensions;

/// <summary>
///
/// </summary>
public static class JsonExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="swaggerJson"></param>
    /// <returns></returns>
    public static bool TryParse(this string swaggerJson, out JObject jObj)
    {
        try
        {
            jObj = JObject.Parse(swaggerJson);
            return true;
        }
        catch (Exception ex)
        {
            jObj = null;
            return false;
        }
    }
}
