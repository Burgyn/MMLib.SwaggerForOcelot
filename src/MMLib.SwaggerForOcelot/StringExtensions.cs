namespace System
{
    /// <summary>
    /// String extensions
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Removes the slash from end.
        /// </summary>
        /// <param name="value">The value.</param>
        public static string RemoveSlashFromEnd(this string value)
            => value.TrimEnd().EndsWith("/")
            ? value.TrimEnd().TrimEnd('/')
            : value;

        /// <summary>
        /// Add slash to end.
        /// </summary>
        public static string WithShashEnding(this string value)
            => !value.TrimEnd().EndsWith("/")
            ? value + "/"
            : value;
    }
}
