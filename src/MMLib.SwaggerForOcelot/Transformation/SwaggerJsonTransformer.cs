using MMLib.SwaggerForOcelot.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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
            JObject swagger = JObject.Parse(swaggerJson);
            var paths = swagger[SwaggerProperties.Paths];

            RemoveHost(swagger);
            var forRemove = new List<JProperty>();

            if (paths != null)
            {
                for (int i = 0; i < paths.Count(); i++)
                {
                    var path = paths.ElementAt(i) as JProperty;
                    string downstreamPath = path.Name;
                    var reRoute = FindReRoute(reRoutes, downstreamPath);

                    if (reRoute != null)
                    {
                        RenameToken(path, ReplaceFirst(downstreamPath, reRoute.DownstreamPath, reRoute.UpstreamPath));
                    }
                    else
                    {
                        forRemove.Add(path);
                    }
                }

                foreach (var path in forRemove)
                {
                    path.Remove();
                }

                var definitions = swagger["definitions"];
                var definitionsForRemove = new List<JToken>();

                foreach (var definition in definitions.Cast<JProperty>())
                {
                    var path = $"$..parameters[?(@schema.$ref == '#/definitions/{definition.Name}')].schema";

                    var schema = paths.SelectTokens(path);
                    if (!schema.Any())
                    {
                        definitionsForRemove.Add(definition);
                    }
                }

                foreach (var definition in definitionsForRemove)
                {
                    definition.Remove();
                }

                var tags = swagger["tags"];
                var tagsForRemove = new List<JObject>();

                foreach (var tag in tags.Cast<JObject>())
                {
                    var path = $"$..tags[?(@ == '{tag["name"]}')]";

                    var tagInPat = paths.SelectTokens(path);
                    if (!tagInPat.Any())
                    {
                        tagsForRemove.Add(tag);
                    }
                }

                foreach (var tag in tagsForRemove)
                {
                    tag.Remove();
                }
            }

            return swagger.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        private static ReRouteOptions FindReRoute(IEnumerable<ReRouteOptions> reRoutes, string downstreamPath)
            => reRoutes.FirstOrDefault(p =>
                p.CanCatchAll
                ? downstreamPath.StartsWith(p.DownstreamPath, StringComparison.CurrentCultureIgnoreCase)
                : p.DownstreamPath.Equals(downstreamPath, StringComparison.CurrentCultureIgnoreCase));

        private static void RemoveHost(JObject swagger)
        {
            swagger.Remove(SwaggerProperties.Host);
            swagger.Remove(SwaggerProperties.Schemes);
        }

        private string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search, StringComparison.CurrentCultureIgnoreCase);
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