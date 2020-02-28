using System;
using System.Collections.Generic;
using System.Text;

namespace MMLib.SwaggerForOcelot.ServiceDiscovery
{
    public class SwaggerService
    {
        public string Name { get; set; }

        public string Path { get; set; } = "/swagger/v1/swagger.json";
    }
}
