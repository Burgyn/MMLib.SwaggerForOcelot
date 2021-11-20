using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Configuration
{
    /// <summary>
    /// Ocelot AuthenticationOptions configuration.
    /// </summary>
    public class AuthenticationOptions
    {
        /// <summary>
        /// Ocelot AuthenticationProviderKey configuration.
        /// Authentication provider key.
        /// </summary>
        public string AuthenticationProviderKey { get; set; }

        /// <summary>
        /// Ocelot AllowedScopes configartion.
        /// Ocelot will get all scope claims and make sure that the user has all of them.
        /// </summary>
        public List<string> AllowedScopes { get; set; }
    }
}
