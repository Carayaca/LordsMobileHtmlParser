using System;
using System.Globalization;

using Humanizer;

namespace LordsMobile.Core
{
    /// <summary>
    /// The Utils class.
    /// </summary>
    internal static class Utils
    {
        private static CultureInfo PointCulture { get; } = new("en")
                                                               {
                                                                   NumberFormat =
                                                                       {
                                                                           NumberDecimalSeparator = ",",
                                                                           NumberGroupSeparator = "."
                                                                       }
                                                               };

        /// <summary>
        /// Converts text to long.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Number.</returns>
        public static long ToLong(this string text)
        {
            return (long) Convert.ToDecimal(text, PointCulture);
        }

        /// <summary>
        /// Converts long to string metric.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Metric.</returns>
        public static string ToMetric(this long value)
        {
            return ((double) value).ToMetric(true, true, 1);
        }
    }
}
