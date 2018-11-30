namespace MMLib.SwaggerForOcelot
{
    internal class ReRouteOption
    {
        public string SwaggerKey { get; set; }

        public string DownstreamPathTemplate { get; set; }

        public string UpstreamPathTemplate { get; set; }

        public string DownstreamPath => Replace(DownstreamPathTemplate);

        public string UpstreamPath => Replace(UpstreamPathTemplate);

        private string Replace(string value) => value.Replace("{everything}", "");
    }

    internal class SwaggerEndPointOption
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }
    }
}
