using Ocelot.Configuration.File;
using System;
using System.Collections.Generic;
using System.Text;

namespace MMLib.SwaggerForOcelot.Configuration
{
    public class SwaggerFileReRoute : FileReRoute
    {
        public string SwaggerKey { get; set; }
    }
}
