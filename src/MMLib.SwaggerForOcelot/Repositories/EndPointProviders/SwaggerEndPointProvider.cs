using Kros.Utils;
using Microsoft.Extensions.Options;
using MMLib.SwaggerForOcelot.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMLib.SwaggerForOcelot.Repositories
{
    /// <summary>
    /// Provider for obtaining <see cref="SwaggerEndPointOptions"/>.
    /// </summary>
    public class SwaggerEndPointProvider : ISwaggerEndPointProvider
    {
        private readonly Lazy<Dictionary<string, SwaggerEndPointOptions>> _swaggerEndPoints;
        private readonly IOptionsMonitor<List<SwaggerEndPointOptions>> _swaggerEndPointsOptions;
        private readonly OcelotSwaggerGenOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerEndPointProvider"/> class.
        /// </summary>
        /// <param name="swaggerEndPoints">The swagger end points.</param>
        public SwaggerEndPointProvider(
            IOptionsMonitor<List<SwaggerEndPointOptions>> swaggerEndPoints,
            OcelotSwaggerGenOptions options)
        {
            _swaggerEndPointsOptions = Check.NotNull(swaggerEndPoints, nameof(swaggerEndPoints));

            _swaggerEndPoints = new Lazy<Dictionary<string, SwaggerEndPointOptions>>(Init);
            _options = options;
        }

        /// <inheritdoc/>
        public IReadOnlyList<SwaggerEndPointOptions> GetAll()
            => _swaggerEndPoints.Value.Values.ToList();

        /// <inheritdoc/>
        public SwaggerEndPointOptions GetByKey(string key)
            => _swaggerEndPoints.Value[$"/{key}"];

        private Dictionary<string, SwaggerEndPointOptions> Init()
        {
            var ret = _swaggerEndPointsOptions.CurrentValue.ToDictionary(p => $"/{p.KeyToPath}", p => p);

            if (_options.GenerateDocsForAggregates)
            {
                AddEndpoint(ret, OcelotSwaggerGenOptions.AggregatesKey, "Aggregates");
            }

            if (_options.GenerateDocsForGatewayItSelf)
            {
                AddEndpoint(ret, OcelotSwaggerGenOptions.GatewayKey, _options.GatewayDocsTitle);
            }

            return ret;
        }

        private static void AddEndpoint(Dictionary<string, SwaggerEndPointOptions> ret, string key, string description)
            => ret.Add($"/{key}",
                new SwaggerEndPointOptions()
                {
                    Key = key,
                    TransformByOcelotConfig = false,
                    Config = new List<SwaggerEndPointConfig>()
                    {
                        new SwaggerEndPointConfig() { Name = description, Version = key, Url = "" }
                    }
                });
    }
}
