// TODO: StandardDeviation method along with .Average could be parallelized. See how much faster our implementation than Linq. How much faster is Linq parallel version versus serial.
// TODO: This function can be accelerated substantially, possibly using SIMD/SSE, since instead of Math.Pow we could use multiplication, after casting to double in SSE.
//       The flow from variety of integer data types would be to cast to double, then square by multiplying, then to sum up and divide by the number of elements to compute the average.
//       yielding a much faster standard deviation computation, which uses SIMD and multi-core. Use parallel .Sum that uses integer computation, which should be much faster.
// TODO: It should be pretty simple to implement the base-case as SSE/SIMD of (v - avg) * (v - avg) and Sum them, and then use Divide-And-Conquer algorithm in HPCsharp to
//       combine the partial sums with perfect precision, possibly using Kahan sum for doubles, or sum to Decimal and extended long first.
// TODO: For floating-point addition offer Kahan summation for a more accurate result.
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
        public static double StandardDeviationFasterPar(this int[] values)
        {
            long sum = HPCsharp.ParallelAlgorithms.Sum.SumToLongSsePar(values);
            double avg = (double)sum / values.Length;
            return Math.Sqrt(values.AsParallel().Average(v => (v - avg) * (v - avg)));
        }
        /// <summary>
        /// Summation of float[] array, using a double accumulator for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumForDeviationToDoubleSse(this int[] arrayToSum, double average, int startIndex, int length)
        {
            return arrayToSum.SumForDeviationSseDoubleInner(average, startIndex, startIndex + length - 1);
        }

        private static double SumForDeviationSseDoubleInner(this int[] arrayToSum, double average, int l, int r)
        {
            var averageVector  = new Vector<double>(average);
            var sumVectorLower = new Vector<double>();
            var sumVectorUpper = new Vector<double>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<int>.Count) * Vector<int>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVector = new Vector<int>(arrayToSum, i);
                Vector.Widen(inVector, out Vector<long> longLower, out Vector<long> longUpper);
                var subVectorLower = Vector.ConvertToDouble(longLower) - averageVector;
                var subVectorUpper = Vector.ConvertToDouble(longUpper) - averageVector;
                sumVectorLower += subVectorLower * subVectorLower;
                sumVectorUpper += subVectorUpper * subVectorUpper;
            }
            double overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<double>.Count; i++)
                overallSum += sumVectorLower[i];
            return overallSum;
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
        /// <summary>
        /// Summation of float[] array, using a double accumulator for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumForDeviationToDoubleSse(this long[] arrayToSum, double average, int startIndex, int length)
        {
            return arrayToSum.SumForDeviationSseDoubleInner(average, startIndex, startIndex + length - 1);
        }

        private static double SumForDeviationSseDoubleInner(this long[] arrayToSum, double average, int l, int r)
        {
            var averageVector = new Vector<double>(average);
            var sumVector     = new Vector<double>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<long>.Count) * Vector<long>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<long>.Count)
            {
                var inVector = new Vector<long>(arrayToSum, i);
                var subVector = Vector.ConvertToDouble(inVector) - averageVector;
                sumVector += subVector * subVector;
            }
            double overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<double>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }
        /// <summary>
        /// Standard deviation of an array of integers.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationPar(this float[] values)
        {
            double avg = values.AsParallel().Average();
            return Math.Sqrt(values.AsParallel().Average(v => Math.Pow(v - avg, 2)));
        }
        /// <summary>
        /// Summation of float[] array, using a double accumulator for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumForDeviationToDoubleSse(this float[] arrayToSum, double average, int startIndex, int length)
        {
            return arrayToSum.SumForDeviationSseDoubleInner(average, startIndex, startIndex + length - 1);
        }

        private static double SumForDeviationSseDoubleInner(this float[] arrayToSum, double average, int l, int r)
        {
            var averageVector  = new Vector<double>(average);
            var sumVectorLower = new Vector<double>();
            var sumVectorUpper = new Vector<double>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<float>.Count) * Vector<float>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<float>.Count)
            {
                var inVector = new Vector<float>(arrayToSum, i);
                Vector.Widen(inVector, out Vector<double> doubleLower, out Vector<double> doubleUpper);
                var subVectorLower = doubleLower - averageVector;
                var subVectorUpper = doubleUpper - averageVector;
                sumVectorLower += subVectorLower * subVectorLower;
                sumVectorUpper += subVectorUpper * subVectorUpper;
            }
            double overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<double>.Count; i++)
                overallSum += sumVectorLower[i];
            return overallSum;
        }
        /// <summary>
        /// Standard deviation of an array of integers.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationPar(this double[] values)
        {
            double avg = values.AsParallel().Average();
            return Math.Sqrt(values.AsParallel().Average(v => Math.Pow(v - avg, 2)));
        }
        /// <summary>
        /// Summation of float[] array, using a double accumulator for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumForDeviationToDoubleSse(this double[] arrayToSum, double average, int startIndex, int length)
        {
            return arrayToSum.SumForDeviationSseDoubleInner(average, startIndex, startIndex + length - 1);
        }

        private static double SumForDeviationSseDoubleInner(this double[] arrayToSum, double average, int l, int r)
        {
            var averageVector = new Vector<double>(average);
            var sumVector     = new Vector<double>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<double>.Count) * Vector<double>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<double>.Count)
            {
                var inVector = new Vector<double>(arrayToSum, i);
                var subVector = inVector - averageVector;
                sumVector += subVector * subVector;
            }
            double overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<double>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }
    }
}
