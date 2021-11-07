using System.Collections.Generic;

namespace MMLib.SwaggerForOcelot.Configuration
{
    public class AuthenticationOptions
    {
        public string AuthenticationProviderKey { get; set; }

        public List<string> AllowedScopes { get; set; }
    }
}
