namespace MMLib.SwaggerForOcelot.Configuration
{
    public class SwaggerEndPointOption
    {
        public const string ConfigurationSectionName = "SwaggerEndPoints";

        public string Key { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }
    }
}
