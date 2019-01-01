using MMLib.SwaggerForOcelot.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMLib.SwaggerForOcelot.Transformation
{
    /// <summary>
    /// Class which implement transformation downstream service swagger json into upstream format
    /// </summary>
    /// <seealso cref="MMLib.SwaggerForOcelot.Transformation.ISwaggerJsonTransformer" />
    public class SwaggerJsonTransformer : ISwaggerJsonTransformer
    {

        /// <inheritdoc/>
        public string Transform(string swaggerJson, IEnumerable<ReRouteOptions> reRoutes)
        {
            var route = reRoutes.First();
            JObject swagger = JObject.Parse(swaggerJson);
            var paths = swagger[SwaggerProperties.Paths];

            RemoveHost(swagger);

            if (paths != null)
            {
                for (int i = 0; i < paths.Count(); i++)
                {
                    var property = paths.ElementAt(i) as JProperty;
                    string downstreamPath = property.Name;

                    RenameToken(property, ReplaceFirst(downstreamPath, route.DownstreamPath, route.UpstreamPath));
                }
            }

            return swagger.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        private static void RemoveHost(JObject swagger)
        {
            swagger.Remove(SwaggerProperties.Host);
            swagger.Remove(SwaggerProperties.Schemes);
        }

        private string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return $"{text.Substring(0, pos)}{replace}{text.Substring(pos + search.Length)}";
        }

        private static void RenameToken(JProperty property, string newName)
        {
            var newProperty = new JProperty(newName, property.Value);
            property.Replace(newProperty);
        }
    }
}