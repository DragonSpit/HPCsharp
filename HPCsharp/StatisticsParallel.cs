// TODO: StandardDeviation method along with .Average could be parallelized. See how much faster our implementation than Linq. How much faster is Linq parallel version versus serial.
// TODO: This function can be accelerated substantially, possibly using SIMD/SSE, since instead of Math.Pow we could use multiplication, after casting to double in SSE.
//       The flow from variety of integer data types would be to cast to double, then square by multiplying, then to sum up and divide by the number of elements to compute the average.
//       yielding a much faster standard deviation computation, which uses SIMD and multi-core. Use parallel .Sum that uses integer computation, which should be much faster.
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HPCsharp.ParallelAlgorithms
{
    static public partial class Statistics
    {
        /// <summary>
        /// Standard deviation of an array of integers.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationPar(this int[] values)
        {
            double avg = values.AsParallel().Average();
            return Math.Sqrt(values.AsParallel().Average(v => Math.Pow(v - avg, 2)));
        }
        /// <summary>
        /// Standard deviation of an array of integers.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationPar(this long[] values)
        {
            double avg = values.AsParallel().Average();
            return Math.Sqrt(values.AsParallel().Average(v => Math.Pow(v - avg, 2)));
        }
    }
}
