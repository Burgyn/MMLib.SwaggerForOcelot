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
            swagger.Remove("host");
            swagger.Remove("schemes");
            if (swagger["paths"] != null)
            {
                for (int i = 0; i < swagger["paths"].Count(); i++)
                {
                    string down = ((JProperty)swagger["paths"].ElementAt(i)).Name;

                    RenameToken(swagger["paths"].ElementAt(i), ReplaceFirst(down, route.DownstreamPath, route.UpstreamPath));
                }
            }
            return swagger.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        private string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static void RenameToken(JToken token, string newName)
        {
            JProperty property;
            property = (JProperty)token;
            var newProperty = new JProperty(newName, property.Value);
            property.Replace(newProperty);
        }
    }
}