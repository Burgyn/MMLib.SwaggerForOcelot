
namespace System
{
    /// <summary>
    ///String extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Removes the slash from end.
        /// </summary>
        /// <param name="value">The value.</param>
        public static string RemoveSlashFromEnd(this string value)
            => value.TrimEnd().EndsWith("/")
            ? value.TrimEnd().TrimEnd('/')
            : value;
    }
}
