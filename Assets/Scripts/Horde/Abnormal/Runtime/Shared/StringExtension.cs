using System.Threading;

namespace Horde.Abnormal.Shared
{
    public static class StringExtension
    {
        /// <summary>
        /// Converts the input string to title case, where the first letter of each word is capitalized.
        /// </summary>
        /// <param name="str">The input string to be converted to title case.</param>
        /// <returns>A new string where the first letter of each word is capitalized.</returns>
        public static string ToTitleCase(this string str) =>
            Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(str);
    }
}