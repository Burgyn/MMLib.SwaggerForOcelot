using Kros.IO;
using MMLib.SwaggerForOcelot.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MMLib.SwaggerForOcelot.Transformation
{
    /// <summary>
    /// Class which implement transformation downstream service swagger json into upstream format
    /// </summary>
    /// <seealso cref="MMLib.SwaggerForOcelot.Transformation.ISwaggerJsonTransformer" />
    public class SwaggerJsonTransformer : ISwaggerJsonTransformer
    {
        /// <inheritdoc/>
        public string Transform(string swaggerJson, IEnumerable<ReRouteOptions> reRoutes, string serverOverride)
        {
            var swagger = JObject.Parse(swaggerJson);

            if (swagger.ContainsKey("swagger"))
            {
                return TransformSwagger(swagger, reRoutes, serverOverride);
            }

            if (swagger.ContainsKey("openapi"))
            {
                return TransformOpenApi(swagger, reRoutes, serverOverride);
            }

            throw new InvalidOperationException("Unknown swagger/openapi version");
        }

        private string TransformSwagger(JObject swagger, IEnumerable<ReRouteOptions> reRoutes, string hostOverride)
        {
            JToken paths = swagger[SwaggerProperties.Paths];
            string basePath = swagger.ContainsKey(SwaggerProperties.BasePath)
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
                RenameAndRemovePaths(reRoutes, paths, basePath);

                RemoveItems<JProperty>(
                    swagger[SwaggerProperties.Definitions],
                    paths,
                    i => $"$..[?(@*.$ref == '#/{SwaggerProperties.Definitions}/{i.Name}')]",
                    i => $"$..[?(@*.*.items.$ref == '#/{SwaggerProperties.Definitions}/{i.Name}')]",
                    i => $"$..[?(@*.*.allOf[?(@.$ref == '#/{SwaggerProperties.Definitions}/{i.Name}')])]");
                if (swagger["tags"] != null)
                {
                    RemoveItems<JObject>(
                        swagger[SwaggerProperties.Tags],
                        paths,
                        i => $"$..tags[?(@ == '{i[SwaggerProperties.TagName]}')]");
                }
            }

            if (swagger.ContainsKey(SwaggerProperties.BasePath))
            {
                swagger[SwaggerProperties.BasePath] = "/";
            }

            return swagger.ToString(Formatting.Indented);
        }

        private string TransformOpenApi(JObject openApi, IEnumerable<ReRouteOptions> reRoutes, string serverOverride = "/")
        {
            // NOTE: Only supporting one server for now.
            string downstreamBasePath = "";
            if (openApi.ContainsKey(OpenApiProperties.Servers))
            {
                string firstServerUrl = openApi.GetValue(OpenApiProperties.Servers).First.Value<string>(OpenApiProperties.Url);
                var downstreamUrl = new Uri(firstServerUrl, UriKind.RelativeOrAbsolute);
                downstreamBasePath =
                    (downstreamUrl.IsAbsoluteUri ? downstreamUrl.AbsolutePath : downstreamUrl.OriginalString)
                    .RemoveSlashFromEnd();
            }

            JToken paths = openApi[OpenApiProperties.Paths];
            if (paths != null)
            {
                RenameAndRemovePaths(reRoutes, paths, downstreamBasePath);

                JToken schemaToken = openApi[OpenApiProperties.Components][OpenApiProperties.Schemas];
                if (schemaToken != null)
                {
                    RemoveItems<JProperty>(schemaToken,
                        paths,
                        i => $"$..[?(@*.$ref == '#/{OpenApiProperties.Components}/{OpenApiProperties.Schemas}/{i.Name}')]",
                        i => $"$..[?(@*.*.items.$ref == '#/{OpenApiProperties.Components}/{OpenApiProperties.Schemas}/{i.Name}')]",
                        i => $"$..[?(@*.*.allOf[?(@.$ref == '#/{OpenApiProperties.Components}/{OpenApiProperties.Schemas}/{i.Name}')])]");
                }

                if (openApi["tags"] != null)
                {
                    RemoveItems<JObject>(
                        openApi[OpenApiProperties.Tags],
                        paths,
                        i => $"$..tags[?(@ == '{i[OpenApiProperties.TagName]}')]");
                }
            }

            TransformServerPaths(openApi, serverOverride);

            return openApi.ToString(Formatting.Indented);
        }

        private void RenameAndRemovePaths(IEnumerable<ReRouteOptions> reRoutes, JToken paths, string basePath)
        {
            var forRemove = new List<JProperty>();

            for (int i = 0; i < paths.Count(); i++)
            {
                var path = paths.ElementAt(i) as JProperty;
                string downstreamPath = path.Name.RemoveSlashFromEnd();
                ReRouteOptions reRoute = FindReRoute(reRoutes, path.Name.WithShashEnding(), basePath);

                if (reRoute != null && RemoveMethods(path, reRoute))
                {
                    RenameToken(path, ConvertDownstreamPathToUpstreamPath(downstreamPath, reRoute.DownstreamPath, reRoute.UpstreamPath, basePath));
                }
                else
                {
                    forRemove.Add(path);
                }
            }

            foreach (JProperty p in forRemove)
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

            foreach (JProperty m in forRemove)
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
                .AsParallel()
                .Where(
                    i => searchPaths.Select(p => paths.SelectTokens(p(i)).Any())
                        .All(p => !p))
                .ToHashSet();

            if (typeof(T) == typeof(JProperty))
            {
                CheckSubreferences(token, searchPaths, forRemove);
            }

            foreach (T item in forRemove)
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

        private static void CheckSubreferences<T>(IEnumerable<JToken> token, Func<T, string>[] searchPaths, HashSet<T> forRemove)
            where T : class
        {
            var notForRemove = token.Cast<T>().Where(t => !forRemove.Contains(t)).Cast<JProperty>().ToList();
            var subReference = forRemove
                    .Cast<JProperty>()
                    .Where(i
                    => searchPaths
                        .Select(p => notForRemove.Any(t => t.SelectTokens(p(i as T)).Any())).Any(p => p))
                    .ToDictionary(p => p.Name, p => p);

            forRemove.RemoveWhere(p => subReference.ContainsKey((p as JProperty).Name));

            if (subReference.Count > 0)
            {
                CheckSubreferences(subReference.Values, searchPaths, forRemove);
            }
        }

        private static ReRouteOptions FindReRoute(IEnumerable<ReRouteOptions> reRoutes, string downstreamPath, string basePath)
        {
            string downstreamPathWithBasePath = PathHelper.BuildPath(basePath, downstreamPath);
            return reRoutes.FirstOrDefault(p
                => p.CanCatchAll
                    ? downstreamPathWithBasePath.StartsWith(p.DownstreamPathWithShash, StringComparison.CurrentCultureIgnoreCase)
                    : p.DownstreamPathWithShash.Equals(downstreamPathWithBasePath, StringComparison.CurrentCultureIgnoreCase));
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

        private static string ConvertDownstreamPathToUpstreamPath(string downstreamPath, string downstreamPattern, string upstreamPattern, string downstreamBasePath)
        {
            if (downstreamBasePath.Length > 0)
            {
                downstreamPath = PathHelper.BuildPath(downstreamBasePath, downstreamPath);
            }

            int pos = downstreamPath.IndexOf(downstreamPattern, StringComparison.CurrentCultureIgnoreCase);
            if (pos < 0)
            {
                return downstreamPath;
            }
            return $"{downstreamPath.Substring(0, pos)}{upstreamPattern}{downstreamPath.Substring(pos + downstreamPattern.Length)}";
        }

        private static void RenameToken(JProperty property, string newName)
        {
            var newProperty = new JProperty(newName, property.Value);
            property.Replace(newProperty);
        }

        private static void TransformServerPaths(JObject openApi, string serverOverride)
        {
            if (!openApi.ContainsKey(OpenApiProperties.Servers))
            {
                return;
            }

            foreach (JToken server in openApi.GetValue(OpenApiProperties.Servers))
            {
                if (server[OpenApiProperties.Url] != null)
                {
                    server[OpenApiProperties.Url] = serverOverride.RemoveSlashFromEnd();
                }
            }
        }
    }
}
