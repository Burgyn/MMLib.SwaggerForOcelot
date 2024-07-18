using Kros.IO;
using Microsoft.Extensions.Caching.Memory;
using MMLib.SwaggerForOcelot.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MMLib.SwaggerForOcelot.Transformation
{
    /// <summary>
    /// Class which implement transformation downstream service swagger json into upstream format
    /// </summary>
    /// <seealso cref="ISwaggerJsonTransformer" />
    public class SwaggerJsonTransformer : ISwaggerJsonTransformer
    {
        private readonly OcelotSwaggerGenOptions _ocelotSwaggerGenOptions;
        private readonly IMemoryCache _memoryCache;

        public SwaggerJsonTransformer(OcelotSwaggerGenOptions ocelotSwaggerGenOptions, IMemoryCache memoryCache)
        {
            _ocelotSwaggerGenOptions = ocelotSwaggerGenOptions;
            _memoryCache = memoryCache;
        }

        /// <inheritdoc/>
        public string Transform(
            string swaggerJson,
            IEnumerable<RouteOptions> routes,
            string serverOverride,
            SwaggerEndPointOptions endPointOptions)
        {
            if (_ocelotSwaggerGenOptions.DownstreamDocsCacheExpire == TimeSpan.Zero)
            {
                return TransformSwaggerOrOpenApi(swaggerJson, routes, serverOverride, endPointOptions);
            }

            return _memoryCache.GetOrCreate(
                ComputeHash(swaggerJson),
                entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = _ocelotSwaggerGenOptions.DownstreamDocsCacheExpire;
                    return TransformSwaggerOrOpenApi(swaggerJson, routes, serverOverride, endPointOptions);
                });
        }

        private string TransformSwaggerOrOpenApi(
            string swaggerJson,
            IEnumerable<RouteOptions> routes,
            string serverOverride,
            SwaggerEndPointOptions endPointOptions)
        {
            var swagger = JObject.Parse(swaggerJson);

            if (swagger.ContainsKey("swagger"))
            {
                return TransformSwagger(swagger, routes, serverOverride, endPointOptions);
            }

            if (swagger.ContainsKey("openapi"))
            {
                return TransformOpenApi(swagger, routes, serverOverride, endPointOptions);
            }

            throw new InvalidOperationException("Unknown swagger/openapi version");
        }

        private string TransformSwagger(
            JObject swagger,
            IEnumerable<RouteOptions> routes,
            string hostOverride,
            SwaggerEndPointOptions endPointOptions)
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

            if (paths is not null)
            {
                RenameAndRemovePaths(routes, paths, basePath);

                if (endPointOptions.RemoveUnusedComponentsFromScheme)
                {
                    RemoveItems<JProperty>(
                        swagger[SwaggerProperties.Definitions],
                        paths,
                        i => $"$..[?(@*.$ref == '#/{SwaggerProperties.Definitions}/{i.Name}')]",
                        i => $"$..[?(@*.*.items.$ref == '#/{SwaggerProperties.Definitions}/{i.Name}')]",
                        i => $"$..[?(@*.*.allOf[?(@.$ref == '#/{SwaggerProperties.Definitions}/{i.Name}')])]",
                        i => $"$..allOf[?(@.$ref == '#/{SwaggerProperties.Definitions}/{i.Name}')]",
                        i => $"$..[?(@*.*.oneOf[?(@.$ref == '#/{SwaggerProperties.Definitions}/{i.Name}')])]");
                    if (swagger["tags"] is not null)
                    {
                        RemoveItems<JObject>(
                            swagger[SwaggerProperties.Tags],
                            paths,
                            i => $"$..tags[?(@ == '{i[SwaggerProperties.TagName]}')]");
                    }
                }
            }

            if (swagger.ContainsKey(SwaggerProperties.BasePath))
            {
                swagger[SwaggerProperties.BasePath] = "/";
            }

            return swagger.ToString(Formatting.Indented);
        }

        private string TransformOpenApi(
            JObject openApi,
            IEnumerable<RouteOptions> routes,
            string serverOverride,
            SwaggerEndPointOptions endPointOptions)
        {
            // NOTE: Only supporting one server for now.
            string downstreamBasePath = "";
            if (openApi.GetValue(OpenApiProperties.Servers)?.Any() == true && !endPointOptions.TakeServersFromDownstreamService)
            {
                string firstServerUrl = openApi.GetValue(OpenApiProperties.Servers).First.Value<string>(OpenApiProperties.Url);
                var downstreamUrl = new Uri(firstServerUrl, UriKind.RelativeOrAbsolute);
                downstreamBasePath =
                    (downstreamUrl.IsAbsoluteUri ? downstreamUrl.AbsolutePath : downstreamUrl.OriginalString)
                    .RemoveSlashFromEnd();
            }

            JToken paths = openApi[OpenApiProperties.Paths];
            if (paths is not null)
            {
                RenameAndRemovePaths(routes, paths, downstreamBasePath);

                JToken schemaToken = openApi[OpenApiProperties.Components][OpenApiProperties.Schemas];
                if (endPointOptions.RemoveUnusedComponentsFromScheme && schemaToken is not null)
                {
                    RemoveItems<JProperty>(schemaToken,
                        paths,
                        i => $"$..[?(@*.$ref == '#/{OpenApiProperties.Components}/{OpenApiProperties.Schemas}/{i.Name}')]",
                        i => $"$..[?(@*.*.items.$ref == '#/{OpenApiProperties.Components}/{OpenApiProperties.Schemas}/{i.Name}')]",
                        i => $"$..[?(@*.*.allOf[?(@.$ref == '#/{OpenApiProperties.Components}/{OpenApiProperties.Schemas}/{i.Name}')])]",
                        i => $"$..allOf[?(@.$ref == '#/{OpenApiProperties.Components}/{OpenApiProperties.Schemas}/{i.Name}')]",
                        i => $"$..oneOf[?(@.$ref == '#/{OpenApiProperties.Components}/{OpenApiProperties.Schemas}/{i.Name}')]",
                        i => $"$..anyOf[?(@.$ref == '#/{OpenApiProperties.Components}/{OpenApiProperties.Schemas}/{i.Name}')]",
                        i => $"$..[?(@*.*.oneOf[?(@.$ref == '#/{OpenApiProperties.Components}/{OpenApiProperties.Schemas}/{i.Name}')])]");
                }

                if (endPointOptions.RemoveUnusedComponentsFromScheme && openApi["tags"] is not null)
                {
                    RemoveItems<JObject>(
                        openApi[OpenApiProperties.Tags],
                        paths,
                        i => $"$..tags[?(@ == '{i[OpenApiProperties.TagName]}')]");
                }
            }

            TransformServerPaths(openApi, serverOverride, endPointOptions.TakeServersFromDownstreamService);

            return openApi.ToString(Formatting.Indented);
        }

        private void RenameAndRemovePaths(IEnumerable<RouteOptions> routes, JToken paths, string basePath)
        {
            var oldPaths = new List<JProperty>();
            var newPaths = new Dictionary<string, JProperty>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < paths.Count(); i++)
            {
                var oldPath = paths.ElementAt(i) as JProperty;
                oldPaths.Add(oldPath);
                string downstreamPath = oldPath.Name.RemoveSlashFromEnd();
                foreach (var tmpMethod in oldPath.First)
                {
                    var method = tmpMethod as JProperty;
                    List<RouteOptions> matchedRoutes = FindRoutes(routes, oldPath.Name.WithSlashEnding(), method.Name, basePath);
                    foreach (var route in matchedRoutes)
                    {
                        string newPath = ConvertDownstreamPathToUpstreamPath(
                            downstreamPath, route.DownstreamPath, route.UpstreamPath, basePath);
                        if (!newPaths.TryGetValue(newPath, out JProperty newPathJson))
                        {
                            newPathJson = new JProperty(newPath, new JObject());
                            newPaths.Add(newPath, newPathJson);
                        }
                        var newMethod = (method.DeepClone() as JProperty);
                        AddSecurityDefinition(newMethod, route);
                        ((JObject)newPathJson.Value).Add(newMethod);
                    }
                }
            }

            oldPaths.ForEach(oldPath => oldPath.Remove());
            newPaths.Select(item => item.Value)
                .OrderBy(newPath => newPath.Name)
                .ForEach(newPath => ((JObject)paths).Add(newPath));
        }

        private void AddSecurityDefinition(JProperty method, RouteOptions route)
        {
            var authProviderKey = route.AuthenticationOptions?.AuthenticationProviderKey;
            if (string.IsNullOrEmpty(authProviderKey))
            {
                return;
            }
            if (_ocelotSwaggerGenOptions.AuthenticationProviderKeyMap.TryGetValue(authProviderKey, out var securityScheme))
            {
                var securityProperty = new JProperty(OpenApiProperties.Security,
                    new JArray(
                        new JObject(
                            new JProperty(securityScheme,
                                new JArray(route.AuthenticationOptions?.AllowedScopes?.ToArray() ?? [])))));
                ((JObject)method.Value).Add(securityProperty);
            }
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

        private static List<RouteOptions> FindRoutes(
            IEnumerable<RouteOptions> routes,
            string downstreamPath,
            string method,
            string basePath)
        {
            static bool MatchPaths(RouteOptions route, string downstreamPath)
                => route.CanCatchAll
                    ? downstreamPath.StartsWith(route.DownstreamPathWithSlash, StringComparison.OrdinalIgnoreCase)
                    : route.DownstreamPathWithSlash.Equals(downstreamPath, StringComparison.OrdinalIgnoreCase);

            string downstreamPathWithBasePath = PathHelper.BuildPath(basePath, downstreamPath);
            var matchedRoutes = routes
                .Where(route => route.ContainsHttpMethod(method) && MatchPaths(route, downstreamPathWithBasePath))
                .ToList();

            RemoveRedundantRoutes(matchedRoutes);
            return matchedRoutes;
        }

        // Redundant routes are routes with the ALMOST same upstream path templates. For example these path templates
        // are redundant:
        //   - /api/projects/Projects
        //   - /api/projects/Projects/
        //   - /api/projects/Projects/{everything}
        //
        // `route.UpstreamPath` contains route without trailing slash and without catch-all placeholder, so all previous
        // routes have the same upstream path `/api/projects/Projects`. The logic is to keep just the shortestof the path
        // templates. If we would keep all routes, it will throw an exception during the generation of the swagger document
        // later because of the same paths.
        private static void RemoveRedundantRoutes(List<RouteOptions> routes)
        {
            IEnumerable<IGrouping<string, RouteOptions>> groups = routes
                .GroupBy(route => route.UpstreamPath, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1);
            foreach (var group in groups)
            {
                group.OrderBy(r => r.DownstreamPathTemplate.Length)
                    .Skip(1)
                    .ForEach(r => routes.Remove(r));
            }
        }

        private static void AddHost(JObject swagger, string swaggerHost)
        {
            swaggerHost = swaggerHost.Contains(Uri.SchemeDelimiter) ? new Uri(swaggerHost).Authority : swaggerHost;
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

            int pos = downstreamPath.IndexOf(downstreamPattern, StringComparison.OrdinalIgnoreCase);
            if (pos < 0)
            {
                return downstreamPath;
            }
            return $"{downstreamPath.Substring(0, pos)}{upstreamPattern}{downstreamPath.Substring(pos + downstreamPattern.Length)}";
        }

        private static void TransformServerPaths(JObject openApi, string serverOverride, bool takeServersFromDownstreamService)
        {
            if (!openApi.ContainsKey(OpenApiProperties.Servers) || takeServersFromDownstreamService)
            {
                return;
            }

            foreach (JToken server in openApi.GetValue(OpenApiProperties.Servers))
            {
                if (server[OpenApiProperties.Url] is not null)
                {
                    server[OpenApiProperties.Url] = serverOverride.RemoveSlashFromEnd();
                }
            }
        }

        private static string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
