using Swashbuckle.AspNetCore.SwaggerUI;

namespace MMLib.SwaggerForOcelot.Configuration
{
    public class SwaggerForOCelotUIOptions: SwaggerUIOptions
    {
        public string EndPointBasePath { get; set; } = "/swagger/v1";
    }
}
