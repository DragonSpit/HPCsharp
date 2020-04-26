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

        /// <summary>
        /// Mean absolute deviation of an array of integers.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>result as a double</returns>
        public static double MeanAbsoluteDeviation(this int[] values)
        {
            long sum = Sum.SumToLong(values);
            double avg = (double)sum / values.Length;
            return values.Average(v => Math.Abs(v - avg));
        }
    }
}
