// TODO: Since standard deviation computes input array average, it may be nice to return that average, since it is a useful value. Or, it needs to take an optional nullable average value (maybe).
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HPCsharp.Algorithms
{
    static public partial class Statistics
    {
        /// <summary>
        /// Standard deviation of an array of integers.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviation(this int[] values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }
        /// <summary>
        /// Standard deviation of an array of integers.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviation(this long[] values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }
    }

    static public partial class StatisticsExperimental
    {
        // Computes a similar value to Standard Deviation, but simpler/faster to compute, since no squaring per element and Square Root. Uses absolute value instead, which has a discontinuity and can't be integrated
        // Which means that it could be computed entirely using integer computation. Plus, no squarint means the resulting function is linear and doesn't warp the input data
        public static double StandardDeviation_NonStandardMethod(this int[] values)
        {
            double avg = values.Average();
            return values.Average(v => Math.Abs(v - avg));
        }
    }
}
