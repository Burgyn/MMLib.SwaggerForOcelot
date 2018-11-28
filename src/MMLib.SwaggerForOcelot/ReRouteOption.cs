namespace MMLib.SwaggerForOcelot
{
    internal class ReRouteOption
    {
        public SwaggerEndPointOption SwaggerEndPoint { get; set; }
    }

    internal class SwaggerEndPointOption
    {
        public string Name { get; set; }

        public string Url { get; set; }
    }
}
