using System.Text.RegularExpressions;
using Horde.Abnormal.Shared;
using UnityEngine.InputSystem;

namespace Horde.Abnormal.Input
{
    public static class InputBindingExtension
    {
        /// <summary>
        /// Converts the path of an <see cref="InputBinding">Input Binding</see> to a more readable format for display purposes.
        /// </summary>
        /// <param name="inputBinding">The input binding whose path is to be formatted.</param>
        /// <returns>A string representing the formatted path, suitable for display.</returns>
        public static string PathDisplay(this InputBinding inputBinding)
        {
            var path = inputBinding.path;

            if (!path.Contains("/"))
                return path.Trim().ToTitleCase();

            var split = path.Split('/')[1].Trim();
            var tmp = Regex.Replace(split, "([^A-Z ])([A-Z])", "$1 $2");
            
            return Regex.Replace(tmp, "([A-Z]+)([A-Z][^A-Z$])", "$1 $2").Trim().ToTitleCase();
        }
    }
}