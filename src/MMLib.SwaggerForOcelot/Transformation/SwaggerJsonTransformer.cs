using Kros.IO;
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
        public string Transform(string swaggerJson, IEnumerable<ReRouteOptions> reRoutes, string hostOverride)
        {
            JObject swagger = JObject.Parse(swaggerJson);
            var paths = swagger[SwaggerProperties.Paths];
            var basePath = swagger.ContainsKey(SwaggerProperties.BasePath)
                ? swagger.GetValue(SwaggerProperties.BasePath).ToString()
                : "";
            basePath = basePath.TrimEnd('/');

            RemoveHost(swagger);
            if (hostOverride != "")
            {
                AddHost(swagger, hostOverride);
            }

            if (paths != null)
            {
                RemovePaths(reRoutes, paths, basePath);

                RemoveItems<JProperty>(
                    swagger[SwaggerProperties.Definitions],
                    paths,
                    (i) => $"$..[?(@*.$ref == '#/definitions/{i.Name}')]",
                    (i) => $"$..[?(@*.*.items.$ref == '#/definitions/{i.Name}')]");
                if (swagger["tags"] != null)
                {
                    RemoveItems<JObject>(
                        swagger[SwaggerProperties.Tags],
                        paths,
                        (i) => $"$..tags[?(@ == '{i[SwaggerProperties.TagName]}')]");
                }
            }

            return swagger.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        private void RemovePaths(IEnumerable<ReRouteOptions> reRoutes, JToken paths, string basePath)
        {
            var forRemove = new List<JProperty>();

            for (int i = 0; i < paths.Count(); i++)
            {
                var path = paths.ElementAt(i) as JProperty;
                string downstreamPath = path.Name.RemoveSlashFromEnd();
                var reRoute = FindReRoute(reRoutes, downstreamPath, basePath);

                if (reRoute != null && RemoveMethods(path, reRoute))
                {
                    RenameToken(path, ReplaceFirstInUrl(downstreamPath, reRoute.DownstreamPath, reRoute.UpstreamPath, basePath));
                }
                else
                {
                    forRemove.Add(path);
                }
            }

            foreach (var p in forRemove)
            {
                p.Remove();
            }
        }

        private bool RemoveMethods(JProperty path, ReRouteOptions reRoute)
        {
            var forRemove = new List<JProperty>();
            var method = path.First.First as JProperty;

            while (method != null)
            {
                if (!reRoute.ContainsHttpMethod(method.Name))
                {
                    forRemove.Add(method);
                }
                method = method.Next as JProperty;
            }

            foreach (var m in forRemove)
            {
                m.Remove();
            }

            return path.First.Any();
        }

        private static void RemoveItems<T>(JToken token, JToken paths, params Func<T, string>[] searchPaths)
            where T : class
        {
            var forRemove = token
                .Cast<T>()
                .Where(i => searchPaths.Select(p
                    => paths.SelectTokens(p(i)).Any()).All(p => !p))
                .ToList();

            if (typeof(T) == typeof(JProperty))
            {
                CheckSubreferences(token, searchPaths, forRemove);
            }

            foreach (var item in forRemove)
            {
                if (item is JObject o)
                {
                    o.Remove();
                }
                else if (item is JProperty t)
                {
                    t.Remove();
                }
            }
        }

        private static void CheckSubreferences<T>(IEnumerable<JToken> token, Func<T, string>[] searchPaths, List<T> forRemove)
            where T : class
        {
            var notForRemove = token.Cast<T>().Where(t => !forRemove.Contains(t)).Cast<JProperty>().ToList();
            var subReference = forRemove
                    .Cast<JProperty>()
                    .Where(i
                    => searchPaths
                        .Select(p => notForRemove.Any(t => t.SelectTokens(p(i as T)).Any())).Any(p => p))
                    .ToDictionary(p => p.Name, p => p);

            forRemove.RemoveAll(p => subReference.ContainsKey((p as JProperty).Name));

            if (subReference.Count > 0)
            {
                CheckSubreferences(subReference.Values, searchPaths, forRemove);
            }
        }

        private static ReRouteOptions FindReRoute(IEnumerable<ReRouteOptions> reRoutes, string downstreamPath, string basePath)
        {
            var downstreamPathWithBasePath = PathHelper.BuildPath(basePath, downstreamPath);
            return reRoutes.FirstOrDefault(p
                => p.CanCatchAll
                    ? downstreamPathWithBasePath.StartsWith(p.DownstreamPath, StringComparison.CurrentCultureIgnoreCase)
                    : p.DownstreamPath.Equals(downstreamPathWithBasePath, StringComparison.CurrentCultureIgnoreCase));
        }

        private static void AddHost(JObject swagger, string swaggerHost)
        {
            swagger.Add(SwaggerProperties.Host, swaggerHost);
        }

        private static void RemoveHost(JObject swagger)
        {
            swagger.Remove(SwaggerProperties.Host);
            swagger.Remove(SwaggerProperties.Schemes);
        }

        private string ReplaceFirstInUrl(string text, string search, string replace, string basePath)
        {
            //remove basePath from search and replace
            if (basePath.Length > 0)
            {
                search = search.Replace(basePath, "");
                replace = replace.Replace(basePath, "");
            }

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