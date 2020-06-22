using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LordsMobile.Core
{
    /// <summary>
    /// <a href="http://stackoverflow.com/questions/5343006/is-there-a-c-sharp-type-for-representing-an-integer-range"/>.
    /// </summary>
    public static class Range
    {
        private static readonly char[] Separators = { ',' };

        private static List<int> Explode(int from, int to)
        {
            return Enumerable.Range(from, (to - from) + 1)
                .ToList();
        }

        /// <summary>
        /// Interprets the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>List{int}.</returns>
        public static List<int> Interpret(string input)
        {
            var result = new List<int>();
            var values = input.Split(Separators);

            const string RangePattern = @"(?<range>(?<from>\d+)-(?<to>\d+))";
            var regex = new Regex(RangePattern);

            foreach (var value in values)
            {
                var match = regex.Match(value);
                if (match.Success)
                {
                    var from = Parse(match.Groups["from"].Value);
                    var to = Parse(match.Groups["to"].Value);
                    result.AddRange(Explode(from, to));
                }
                else
                {
                    result.Add(Parse(value));
                }
            }

            return result;
        }

        /// <summary>
        /// Split this out to allow custom throw etc.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>An integer value.</returns>
        private static int Parse(string value)
        {
            var ok = int.TryParse(value, out var output);
            return !ok ? throw new FormatException($"Failed to parse '{value}' as an integer") : output;
        }
    }
}
