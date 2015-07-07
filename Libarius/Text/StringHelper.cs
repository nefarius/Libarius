using System.Text;

namespace Libarius.Text
{
    /// <summary>
    ///     Provides utility methods for string interaction.
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        ///     Converts a string of foreign encoding to UTF-8.
        /// </summary>
        /// <param name="input">The source string.</param>
        /// <returns>The UTF-8 encoded string.</returns>
        public static string ToUtf8(this string input)
        {
            var utf8Bytes = Encoding.UTF8.GetBytes(input);
            return Encoding.UTF8.GetString(utf8Bytes);
        }
    }
}