using MMLib.SwaggerForOcelot.Configuration;
using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Repositories
{
    /// <summary>
    /// Interface which describe provider for obtaining <see cref="SwaggerEndPointOptions"/>
    /// </summary>
    public interface ISwaggerEndPointProvider
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        IReadOnlyList<SwaggerEndPointOptions> GetAll();

        /// <summary>
        /// Gets the by key.
        /// </summary>
        /// <param name="key">The key.</param>
        SwaggerEndPointOptions GetByKey(string key);
    }
}
