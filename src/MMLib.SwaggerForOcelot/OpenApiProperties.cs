namespace MMLib.SwaggerForOcelot
{
    /// <summary>
    /// Swagger properties constants.
    /// </summary>
    public static class OpenApiProperties
    {
        /// <summary>
        /// The servers property name.
        /// </summary>
        public const string Servers = "servers";

        /// <summary>
        /// The uri property name.  Property is a child of <seealso cref="Servers"/>.
        /// </summary>
        public const string Url = "url";

        /// <summary>
        /// The paths property name.
        /// </summary>
        public const string Paths = "paths";

        /// <summary>
        /// The components property name.
        /// </summary>
        public const string Components = "components";

        /// <summary>
        /// The schemas property name.  Property is a child of <seealso cref="Components"/>.
        /// </summary>
        public const string Schemas = "schemas";

        /// <summary>
        /// The tags property name.
        /// </summary>
        public const string Tags = "tags";

        /// <summary>
        /// The tag's name property name.  Property is a child of <seealso cref="Tags"/>.
        /// </summary>
        public const string TagName = "name";
    }
}
