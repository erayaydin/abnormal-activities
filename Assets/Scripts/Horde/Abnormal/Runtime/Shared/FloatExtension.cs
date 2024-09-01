using UnityEngine;

namespace Horde.Abnormal.Shared
{
    public static class FloatExtension
    {
        /// <summary>
        /// Normalizes the given angle within the range of -360 to 360 degrees.
        /// </summary>
        /// <param name="angle">The angle to be normalized.</param>
        /// <returns>The normalized angle within the range of -360 to 360 degrees.</returns>
        public static float Normalize(this float angle)
        {
            return angle % 360f;
        }
        
        /// <summary>
        /// Clamps the given angle within the specified range.
        /// </summary>
        /// <param name="angle">The angle to be clamped.</param>
        /// <param name="min">The minimum value for the clamped angle.</param>
        /// <param name="max">The maximum value for the clamped angle.</param>
        /// <returns>The clamped angle within the specified range.</returns>
        public static float Clamp(this float angle, float min, float max)
        {
            return Mathf.Clamp(angle, min, max);
        }
    }
}