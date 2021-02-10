using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LordsMobile.Core
{
    /// <summary>
    /// How many migration scrolls do you need?.
    /// </summary>
    public static class Migration
    {
        private static class Rx
        {
            public static readonly Regex Pair = new Regex(@"\s*(?<Kingdom>\d+);\s*(?<Scrolls>\d+)");
        }

        private static Interval<int> Point(int v) =>
            new(
                new IntervalValue<int>(v, IntervalValueType.Inclusive),
                new IntervalValue<int>(v, IntervalValueType.Inclusive));

        private static Interval<int> Range(int a, int b) =>
            new(
                new IntervalValue<int>(a, IntervalValueType.Inclusive),
                new IntervalValue<int>(b, IntervalValueType.Inclusive));

        private static IDictionary<Interval<int>, int> Intervals { get; } = new Dictionary<Interval<int>, int>
            {
                { Point(1), 90 },
                { Point(2), 65 },
                { Point(3), 50 },
                { Point(4), 35 },
                { Point(5), 30 },
                { Point(6), 28 },
                { Point(7), 26 },
                { Point(8), 24 },
                { Point(9), 22 },
                { Point(10), 20 },
                { Range(11, 13), 18 },
                { Range(14, 16), 16 },
                { Range(17, 18), 14 },
                { Range(19, 20), 13 },
                { Range(21, 22), 12 },
                { Range(23, 24), 11 },
                { Range(25, 26), 10 },
                { Range(27, 28), 9 },
                { Range(29, 30), 8 },
                { Range(31, 32), 7 },
                { Range(33, 35), 6 },
                { Range(36, 38), 5 },
                { Range(39, 41), 4 },
                { Range(42, 45), 3 },
                { Range(46, 50), 2 },
                { Range(51, int.MaxValue), 1 }
            };

        /// <summary>
        /// How many migration scrolls do you need?.
        /// </summary>
        /// <param name="rank">The player's rank.</param>
        /// <returns>Number of scrolls.</returns>
        public static int Compute(int rank)
        {
            foreach (var (interval, n) in Intervals)
            {
                if (interval.ContainsValue(rank))
                {
                    return n;
                }
            }

            throw new ArgumentException($"Invalid value {rank}", nameof(rank));
        }

        /// <summary>
        /// Load the migration list.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>IDictionary{int, int}.</returns>
        public static IDictionary<int, int> Load(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            var dict = new SortedDictionary<int, int>();

            do
            {
                if (!File.Exists(fileName))
                {
                    break;
                }

                using (var sr = new StreamReader(fileName, Encoding.UTF8))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        var m = Rx.Pair.Match(line);
                        if (m.Success)
                        {
                            dict[int.Parse(m.Groups["Kingdom"].Value)] = int.Parse(m.Groups["Scrolls"].Value);
                        }
                    }
                }
            }
            while (false);

            return dict;
        }

        /// <summary>
        /// Save the migration list.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="dict">The migrations dictionary.</param>
        public static void Save(string fileName, IDictionary<int, int> dict)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            if (dict == null)
                throw new ArgumentNullException(nameof(dict));

            using (var sw = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                var sd = new SortedDictionary<int, int>(dict);
                foreach (var (kingdom, scrolls) in sd)
                {
                    sw.Write(kingdom);
                    sw.Write("; ");
                    sw.Write(scrolls);
                    sw.WriteLine();
                }
            }
        }
    }
}
