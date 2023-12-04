namespace System
{
    /// <summary>
    /// String extensions.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Removes the slash from end.
        /// </summary>
        /// <param name="value">The value.</param>
        public static string RemoveSlashFromEnd(this string value)
            => value.TrimEnd().TrimEnd('/');

        /// <summary>
        /// Add slash to end.
        /// </summary>
        public static string WithSlashEnding(this string value)
        {
            value = value.TrimEnd();
            if (!value.EndsWith('/'))
            {
                value += "/";
            }
            return value;
        }
    }
}
