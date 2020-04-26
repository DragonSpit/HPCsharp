// TODO: StandardDeviation method along with .Average could be parallelized. See how much faster our implementation than Linq. How much faster is Linq parallel version versus serial.
// TODO: This function can be accelerated substantially, possibly using SIMD/SSE, since instead of Math.Pow we could use multiplication, after casting to double in SSE.
//       The flow from variety of integer data types would be to cast to double, then square by multiplying, then to sum up and divide by the number of elements to compute the average.
//       yielding a much faster standard deviation computation, which uses SIMD and multi-core. Use parallel .Sum that uses integer computation, which should be much faster.
// TODO: It should be pretty simple to implement the base-case as SSE/SIMD of (v - avg) * (v - avg) and Sum them, and then use Divide-And-Conquer algorithm in HPCsharp to
//       combine the partial sums with perfect precision, possibly using Kahan sum for doubles, or sum to Decimal and extended long first.
// TODO: For floating-point addition offer Kahan summation for a more accurate result.
// TODO: Figure out how to pass in an arbitrary number and types of arguments to the divide-and-conquer generic funtion, possibly as the last params object[] argument
//       with = null as default. One possibility is to pass it a tuple of settings, followed by params object[] which defaults to null.
// TODO: Need to implement the EvenFaster version of .Sum(long[]) which uses a 128-bit long accumulator to avoid over/under-flow and exceptions, to make StdDeviationSsePar even faster.
// TODO: Conside using Decimal for long and ulong operations for a more accurate but much slower option, that would not lose as much precision
// TODO: Really show off the capability and performance gain of StdDeviation and MeanAbsoluteDeviation of long and ulong, where the standard C# .Sum() will throw an exception, to fix this developers would need to cast
//       each element to a double or decimal to get around overflow exception, this will result in severe performance drop, which when compared to HPCsharp is most likely 100X slower if not more. Good idea for a blog! To walk them thru
// TODO: Implement Mean Absolute Deviation for float[] which uses doubles (ToDouble version).
// TODO: Implement other data types for Mean Absolute Deviation: int, uint, long, ulong
// TODO: Implement StdDeviation for uint[], byte[], sbyte[], short[], short[], and possibly Decimal[]
// TODO: To improve testing for Kahan summation based standard deviation and mean absolute deviation, create a unit test which is scalar and uses Kahan summation (already part of HPCsharp) and compare to that
//       "golden" implementation
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
        /// Standard deviation of an array of integers. Uses SSE data-parallel instruction within each core.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationSse(this int[] values)
        {
            long sum = Sum.SumToLongSse(values);     // SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return Math.Sqrt(values.SumForDeviationToDoubleSse(0, values.Length, avg) / (values.Length - 1));
        }
        /// <summary>
        /// Standard deviation of an array of integers. Uses multiple processor cores and SSE instruction within each core for multiple types of parallellism.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationSsePar(this int[] values)
        {
            long sum = Sum.SumToLongSsePar(values);     // multi-core, SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return Math.Sqrt(values.SumForStdDeviationSsePar(avg) / (values.Length - 1 ));
        }

        private static double SumForDeviationToDoubleSse(this int[] arrayToSum, int startIndex, int length, double average)
        {
            return arrayToSum.SumForDeviationSseDoubleInner(startIndex, startIndex + length - 1, average);
        }
        private static double SumForStdDeviationSsePar(this int[] arrayToSum, double average, int thresholdParallel = 16 * 1024)
        {
            return DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, average, SumForDeviationToDoubleSse, (x, y) => x + y, thresholdParallel);
        }

        private static double SumForDeviationSseDoubleInner(this int[] arrayToSum, int l, int r, double average)
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
                overallSum += (arrayToSum[i] - average) * (arrayToSum[i] - average);
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<double>.Count; i++)
                overallSum += sumVectorLower[i];
            return overallSum;
        }

        /// <summary>
        /// Standard deviation of an array of long integers. Uses SSE data-parallel instruction within each core.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationSse(this long[] values)
        {
            decimal sum = Sum.SumToDecimalSseFaster(values);     // multi-car, SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return Math.Sqrt(values.SumForDeviationToDoubleSse(0, values.Length, avg) / (values.Length - 1));
        }
        /// <summary>
        /// Standard deviation of an array of long integers. Uses multiple processor cores and SSE instruction within each core for multiple types of parallellism.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationSsePar(this long[] values)
        {
            decimal sum = HPCsharp.ParallelAlgorithms.Sum.SumToDecimalSseFasterPar(values);   // multi-core, SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return Math.Sqrt(values.SumForStdDeviationSsePar(avg) / (values.Length - 1));
        }

        private static double SumForDeviationToDoubleSse(this long[] arrayToSum, int startIndex, int length, double average)
        {
            return arrayToSum.SumForDeviationSseDoubleInner(startIndex, startIndex + length - 1, average);
        }
        private static double SumForStdDeviationSsePar(this long[] arrayToSum, double average, int thresholdParallel = 16 * 1024)
        {
            return DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, average, SumForDeviationToDoubleSse, (x, y) => x + y, thresholdParallel);
        }

        private static double SumForDeviationSseDoubleInner(this long[] arrayToSum, int l, int r, double average)
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
                overallSum += (arrayToSum[i] - average) * (arrayToSum[i] - average);
            for (i = 0; i < Vector<double>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }
        /// <summary>
        /// Standard deviation of an array of ulong integers. Uses SSE data-parallel instruction within each core.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationSse(this ulong[] values)
        {
            decimal sum  = Sum.SumToDecimalSseEvenFaster(values);     // multi-core, SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return Math.Sqrt(values.SumForDeviationToDoubleSse(0, values.Length, avg) / (values.Length - 1));
        }
        /// <summary>
        /// Standard deviation of an array of long integers. Uses multiple processor cores and SSE instruction within each core for multiple types of parallellism.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationSsePar(this ulong[] values)
        {
            decimal sum = Sum.SumToDecimalSseEvenFasterPar(values);   // multi-core, SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return Math.Sqrt(values.SumForStdDeviationSsePar(avg) / (values.Length - 1));
        }

        private static double SumForDeviationToDoubleSse(this ulong[] arrayToSum, int startIndex, int length, double average)
        {
            return arrayToSum.SumForDeviationSseDoubleInner(startIndex, startIndex + length - 1, average);
        }
        private static double SumForStdDeviationSsePar(this ulong[] arrayToSum, double average, int thresholdParallel = 16 * 1024)
        {
            return DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, average, SumForDeviationToDoubleSse, (x, y) => x + y, thresholdParallel);
        }

        private static double SumForDeviationSseDoubleInner(this ulong[] arrayToSum, int l, int r, double average)
        {
            var averageVector = new Vector<double>(average);
            var sumVector     = new Vector<double>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<ulong>.Count) * Vector<ulong>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<long>.Count)
            {
                var inVector = new Vector<ulong>(arrayToSum, i);
                var subVector = Vector.ConvertToDouble(inVector) - averageVector;
                sumVector += subVector * subVector;
            }
            double overallSum = 0;
            for (; i <= r; i++)
                overallSum += (arrayToSum[i] - average) * (arrayToSum[i] - average);
            for (i = 0; i < Vector<double>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        /// <summary>
        /// Standard deviation of an array of floats. Uses SSE data-parallel instruction within each core.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static float StandardDeviationSse(this float[] values)
        {
            float sum = Sum.SumSse(values);     // SSE, no exceptions and full accuracy
            float avg = sum / values.Length;
            return (float)Math.Sqrt(values.SumForDeviationSse(0, values.Length, avg) / (values.Length - 1));
        }
        /// <summary>
        /// Standard deviation of an array of floats. Uses multiple processor cores and SSE instruction within each core for multiple types of parallellism.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static float StandardDeviationSsePar(this float[] values)
        {
            float sum = Sum.SumSsePar(values);     // multi-core, SSE, no exceptions and full accuracy
            float avg = sum / values.Length;
            return (float)Math.Sqrt(values.SumForStdDeviationSsePar(avg) / (values.Length - 1));
        }

        private static float SumForDeviationSse(this float[] arrayToSum, int startIndex, int length, double average)
        {
            return arrayToSum.SumForDeviationSseInner(startIndex, startIndex + length - 1, (float)average);
        }
        private static float SumForStdDeviationSsePar(this float[] arrayToSum, float average, int thresholdParallel = 16 * 1024)
        {
            return DivideAndConquerTwoTypesPar<float, float>(arrayToSum, 0, arrayToSum.Length, average, SumForDeviationSse, (x, y) => x + y, thresholdParallel);
        }

        private static float SumForDeviationSseInner(this float[] arrayToSum, int l, int r, float average)
        {
            var averageVector = new Vector<float>(average);
            var sumVector     = new Vector<float>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<float>.Count) * Vector<float>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<float>.Count)
            {
                var inVector = new Vector<float>(arrayToSum, i);
                var subVector = inVector - averageVector;
                sumVector += subVector * subVector;
            }
            float overallSum = 0;
            for (; i <= r; i++)
                overallSum += (arrayToSum[i] - average) * (arrayToSum[i] - average);
            for (i = 0; i < Vector<float>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        /// <summary>
        /// Standard deviation of an array of floats, with all computations using double. Uses SSE data-parallel instruction within each core.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationToDoubleSse(this float[] values)
        {
            double sum = Sum.SumToDoubleSse(values);     // SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return Math.Sqrt(values.SumForDeviationToDoubleSse(0, values.Length, avg) / (values.Length - 1));
        }
        /// <summary>
        /// Standard deviation of an array of floats, with all computations using double. Uses multiple processor cores and SSE instruction within each core for multiple types of parallellism.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationToDoubleSsePar(this float[] values)
        {
            double sum = Sum.SumToDoubleSsePar(values);     // multi-core, SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return Math.Sqrt(values.SumForStdDeviationToDoubleSsePar(avg) / (values.Length - 1));
        }

        private static double SumForDeviationToDoubleSse(this float[] arrayToSum, int startIndex, int length, double average)
        {
            return arrayToSum.SumForDeviationSseDoubleInner(startIndex, startIndex + length - 1, average);
        }
        private static double SumForStdDeviationToDoubleSsePar(this float[] arrayToSum, double average, int thresholdParallel = 16 * 1024)
        {
            return DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, average, SumForDeviationToDoubleSse, (x, y) => x + y, thresholdParallel);
        }

        private static double SumForDeviationSseDoubleInner(this float[] arrayToSum, int l, int r, double average)
        {
            var averageVector = new Vector<double>(average);
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
                overallSum += (arrayToSum[i] - average) * (arrayToSum[i] - average);
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<double>.Count; i++)
                overallSum += sumVectorLower[i];
            return overallSum;
        }
        /// <summary>
        /// Standard deviation of an array of doubles. Uses SSE data-parallel instruction within each core.
        /// </summary>
        /// <param name="values">An array to compute on</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationSse(this double[] values)
        {
            double sum = Sum.SumSse(values);     // SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return Math.Sqrt(values.SumForDeviationSse(0, values.Length, avg) / (values.Length - 1));
        }
        /// <summary>
        /// Standard deviation of an array of doubles. Uses multiple processor cores and SSE instruction within each core for multiple types of parallellism.
        /// </summary>
        /// <param name="values">An array to compute on</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationSsePar(this double[] values)
        {
            double sum = Sum.SumSsePar(values);     // multi-core, SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return Math.Sqrt(values.SumForStdDeviationSsePar(avg) / (values.Length - 1));
        }

        private static double SumForDeviationSse(this double[] arrayToSum, int startIndex, int length, double average)
        {
            return arrayToSum.SumForDeviationSseInner(startIndex, startIndex + length - 1, average);
        }
        private static double SumForStdDeviationSsePar(this double[] arrayToSum, double average, int thresholdParallel = 16 * 1024)
        {
            return DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, average, SumForDeviationSse, (x, y) => x + y, thresholdParallel);
        }

        private static double SumForDeviationSseInner(this double[] arrayToSum, int l, int r, double average)
        {
            var averageVector = new Vector<double>(average);
            var sumVector = new Vector<double>();
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
                overallSum += (arrayToSum[i] - average) * (arrayToSum[i] - average);
            for (i = 0; i < Vector<double>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        /// <summary>
        /// Standard deviation of an array of doubles. Uses SSE data-parallel instruction within each core.
        /// </summary>
        /// <param name="values">An array to compute on</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationMostAccurateSse(this double[] values)
        {
            double sum = Sum.SumSseMostAccurate(values);     // SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return Math.Sqrt(values.SumForDeviationMostAccurateSse(0, values.Length, avg) / (values.Length - 1));
        }
        /// <summary>
        /// Standard deviation of an array of doubles. Uses multiple processor cores and SSE instruction within each core for multiple types of parallellism.
        /// </summary>
        /// <param name="values">An array to compute on</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationMostAccurateSsePar(this double[] values)
        {
            double sum = Sum.SumSseParMostAccurate(values);     // multi-core, SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return Math.Sqrt(values.SumForStdDeviationMostAccurateSsePar(avg) / (values.Length - 1));
        }

        private static double SumForDeviationMostAccurateSse(this double[] arrayToSum, int startIndex, int length, double average)
        {
            return arrayToSum.SumForDeviationMostAccurateSseInner(startIndex, startIndex + length - 1, average);
        }
        private static double SumForStdDeviationMostAccurateSsePar(this double[] arrayToSum, double average, int thresholdParallel = 16 * 1024)
        {
            return DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, average, SumForDeviationMostAccurateSse, (x, y) => x + y, thresholdParallel);
        }

        private static double SumForDeviationMostAccurateSseInner(this double[] arrayToSum, int l, int r, double average)
        {
            var averageVector = new Vector<double>(average);
            var sumVector     = new Vector<double>();
            var cVector       = new Vector<double>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<double>.Count) * Vector<double>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<double>.Count)
            {
                var inVector = new Vector<double>(arrayToSum, i);
                var subVector = inVector - averageVector;
                var subVectorSqrd = subVector * subVector;
                var tVector = sumVector + subVectorSqrd;
                Vector<long> gteMask = Vector.GreaterThanOrEqual(Vector.Abs(sumVector), Vector.Abs(subVectorSqrd));  // if true then 0xFFFFFFFFFFFFFFFFL else 0L at each element of the Vector<long> 
                cVector += Vector.ConditionalSelect(gteMask, sumVector, subVectorSqrd) - tVector + Vector.ConditionalSelect(gteMask, subVectorSqrd, sumVector);
                sumVector = tVector;
            }
            int iLast = i;
            // At this point we have sumVector and cVector, which have Vector<double>.Count number of sum's and c's
            // Reduce these Vector's to a single sum and a single c
            double sum = 0.0;
            double c   = 0.0;
            for (i = 0; i < Vector<double>.Count; i++)
            {
                double t = sum + sumVector[i];
                if (Math.Abs(sum) >= Math.Abs(sumVector[i]))
                    c += (sum - t) + sumVector[i];         // If sum is bigger, low-order digits of input[i] are lost.
                else
                    c += (sumVector[i] - t) + sum;         // Else low-order digits of sum are lost
                sum = t;
                c += cVector[i];
            }
            for (i = iLast; i <= r; i++)
            {
                double inValueSubSqrd = (arrayToSum[i] - average) * (arrayToSum[i] - average);
                double t = sum + inValueSubSqrd;
                if (Math.Abs(sum) >= Math.Abs(inValueSubSqrd))
                    c += (sum - t) + inValueSubSqrd;         // If sum is bigger, low-order digits of input[i] are lost.
                else
                    c += (inValueSubSqrd - t) + sum;         // Else low-order digits of sum are lost
                sum = t;
            }
            return sum + c;
        }

        /// <summary>
        /// Mean absolute deviation of an array of integers. Uses SSE data-parallel instruction within each core.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>result as a double</returns>
        public static double MeanAbsoluteDeviationSse(this int[] values)
        {
            long sum = Sum.SumToLongSse(values);     // SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return values.SumForMeanDeviationSse(0, values.Length, avg) / values.Length;
        }
        /// <summary>
        /// Mean absolute deviation of an array of integers. Uses multiple processor cores and SSE instruction within each core for multiple types of parallellism.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>result as a double</returns>
        public static double MeanAbsoluteDeviationSsePar(this int[] values)
        {
            long sum = Sum.SumToLongSsePar(values);     // multi-core, SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return values.SumForMeanDeviationSsePar(avg) / values.Length;
        }

        private static double SumForMeanDeviationSse(this int[] arrayToSum, int startIndex, int length, double average)
        {
            return arrayToSum.SumForMeanDeviationSseInner(startIndex, startIndex + length - 1, (float)average);
        }
        private static double SumForMeanDeviationSsePar(this int[] arrayToSum, double average, int thresholdParallel = 16 * 1024)
        {
            return DivideAndConquerTwoTypesPar<int, double>(arrayToSum, 0, arrayToSum.Length, average, SumForMeanDeviationSse, (x, y) => x + y, thresholdParallel);
        }

        private static double SumForMeanDeviationSseInner(this int[] arrayToSum, int l, int r, double average)
        {
            var averageVector = new Vector<double>(average);
            var sumVector     = new Vector<double>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<int>.Count) * Vector<int>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVector = new Vector<int>(arrayToSum, i);
                Vector.Widen(inVector, out var longLower, out var longUpper);
                var doubleLower = Vector.ConvertToDouble(longLower);
                var doubleUpper = Vector.ConvertToDouble(longUpper);
                sumVector += Vector.Abs(doubleLower - averageVector);
                sumVector += Vector.Abs(doubleUpper - averageVector);
            }
            double overallSum = 0;
            for (; i <= r; i++)
                overallSum += Math.Abs(arrayToSum[i] - average);
            for (i = 0; i < Vector<int>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }


        /// <summary>
        /// Mean absolute deviation of an array of floats. Uses SSE data-parallel instruction within each core.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>result as a float</returns>
        public static float MeanAbsoluteDeviationSse(this float[] values)
        {
            float sum = Sum.SumSse(values);     // SSE, no exceptions and full accuracy
            float avg = sum / values.Length;
            return values.SumForMeanDeviationSse(0, values.Length, avg) / values.Length;
        }
        /// <summary>
        /// Mean absolute deviation of an array of floats. Uses multiple processor cores and SSE instruction within each core for multiple types of parallellism.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>result as a float</returns>
        public static float MeanAbsoluteDeviationSsePar(this float[] values)
        {
            float sum = Sum.SumSsePar(values);     // multi-core, SSE, no exceptions and full accuracy
            float avg = sum / values.Length;
            return values.SumForMeanDeviationSsePar(avg) / values.Length;
        }

        private static float SumForMeanDeviationSse(this float[] arrayToSum, int startIndex, int length, double average)
        {
            return arrayToSum.SumForMeanDeviationSseInner(startIndex, startIndex + length - 1, (float)average);
        }
        private static float SumForMeanDeviationSsePar(this float[] arrayToSum, float average, int thresholdParallel = 16 * 1024)
        {
            return DivideAndConquerTwoTypesPar<float, float>(arrayToSum, 0, arrayToSum.Length, average, SumForMeanDeviationSse, (x, y) => x + y, thresholdParallel);
        }

        private static float SumForMeanDeviationSseInner(this float[] arrayToSum, int l, int r, float average)
        {
            var averageVector = new Vector<float>(average);
            var sumVector     = new Vector<float>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<float>.Count) * Vector<float>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<float>.Count)
            {
                var inVector = new Vector<float>(arrayToSum, i);
                sumVector += Vector.Abs(inVector - averageVector);
            }
            float overallSum = 0;
            for (; i <= r; i++)
                overallSum += (float)Math.Abs(arrayToSum[i] - average);
            for (i = 0; i < Vector<float>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        /// <summary>
        /// Mean absolute deviation of an array of floats, with all computations using double. Uses SSE data-parallel instruction within each core.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double MeanAbsoluteDeviationToDoubleSse(this float[] values)
        {
            double sum = Sum.SumToDoubleSse(values);
            double avg = sum / values.Length;
            return values.SumForMeanDeviationToDoubleSse(0, values.Length, avg) / values.Length;
        }
        /// <summary>
        /// Mean absolute deviation of an array of floats, with all computations using double. Uses multiple processor cores and SSE instruction within each core for multiple types of parallellism.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>result as a double</returns>
        public static double MeanAbsoluteDeviationToDoubleSsePar(this float[] values)
        {
            double sum = Sum.SumToDoubleSsePar(values);
            double avg = sum / values.Length;
            return values.SumForMeanDeviationToDoubleSsePar(avg) / values.Length;
        }

        private static double SumForMeanDeviationToDoubleSse(this float[] arrayToSum, int startIndex, int length, double average)
        {
            return arrayToSum.SumForMeanDeviationToDoubleSseInner(startIndex, startIndex + length - 1, average);
        }
        private static double SumForMeanDeviationToDoubleSsePar(this float[] arrayToSum, double average, int thresholdParallel = 16 * 1024)
        {
            return DivideAndConquerTwoTypesPar<float, double>(arrayToSum, 0, arrayToSum.Length, average, SumForMeanDeviationToDoubleSse, (x, y) => x + y, thresholdParallel);
        }

        private static double SumForMeanDeviationToDoubleSseInner(this float[] arrayToSum, int l, int r, double average)
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
                sumVectorLower += Vector.Abs(doubleLower - averageVector);
                sumVectorUpper += Vector.Abs(doubleLower - averageVector);
            }
            double overallSum = 0;
            for (; i <= r; i++)
                overallSum += Math.Abs(arrayToSum[i] - average);
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<float>.Count; i++)
                overallSum += sumVectorLower[i];
            return overallSum;
        }

        /// <summary>
        /// Mean absolute deviation of an array of doubles. Uses SSE data-parallel instruction within each core.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>result as a double</returns>
        public static double MeanAbsoluteDeviationSse(this double[] values)
        {
            double sum = Sum.SumSse(values);     // SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return values.SumForMeanDeviationSse(0, values.Length, avg) / values.Length;
        }
        /// <summary>
        /// Mean absolute deviation of an array of doubles. Uses multiple processor cores and SSE instruction within each core for multiple types of parallellism.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>result as a double</returns>
        public static double MeanAbsoluteDeviationSsePar(this double[] values)
        {
            double sum = Sum.SumSsePar(values);     // multi-core, SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return values.SumForMeanDeviationSsePar(avg) / values.Length;
        }

        private static double SumForMeanDeviationSse(this double[] arrayToSum, int startIndex, int length, double average)
        {
            return arrayToSum.SumForMeanDeviationSseInner(startIndex, startIndex + length - 1, average);
        }
        private static double SumForMeanDeviationSsePar(this double[] arrayToSum, double average, int thresholdParallel = 16 * 1024)
        {
            return DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, average, SumForMeanDeviationSse, (x, y) => x + y, thresholdParallel);
        }

        private static double SumForMeanDeviationSseInner(this double[] arrayToSum, int l, int r, double average)
        {
            var averageVector = new Vector<double>(average);
            var sumVector     = new Vector<double>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<double>.Count) * Vector<double>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<double>.Count)
            {
                var inVector = new Vector<double>(arrayToSum, i);
                sumVector += Vector.Abs(inVector - averageVector);
            }
            double overallSum = 0;
            for (; i <= r; i++)
                overallSum += Math.Abs(arrayToSum[i] - average);
            for (i = 0; i < Vector<double>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        /// <summary>
        /// Mean absolute deviation of an array of ints.
        /// </summary>
        /// <param name="values">An array of ints as input data</param>
        /// <returns>result as a double</returns>
        private static double MeanAbsoluteDeviationTestPar(this int[] values)
        {
            double avg = (double)values.Sum() / values.Length;
            return values.AsParallel().Sum(v => Math.Abs(v - avg)) / values.Length;
        }


        /// <summary>
        /// Parallel Divide and Conquer generic pattern, using input type T and output type T2.
        /// </summary>
        /// <param name="arrayToProcess">An input array to be processed</param>
        /// <param name="start">starting index of the first element to be processed (inclusive)</param>
        /// <param name="length">number of elements to be processed</param>
        /// <param name="baseCase">function for the recursion base case (leaf node). The two parameters are start and length</param>
        /// <param name="reduce">function for combining the two recursive results</param>
        /// <param name="thresholdPar">if array is larger than this value, then parallel processing will be used, otherwise serial processing will be used by invoking the baseCase function</param>
        /// <param name="degreeOfParallelism">amount of parallelism to be used - i.e. number of computational cores. When set to zero or negative, all available cores will be utilized.
        /// When set to 1, then a single core will be used. When set to > 1, then that many cores will be used.</param>
        /// <returns>result value</returns>
        private static T2 DivideAndConquerTwoTypesPar<T, T2>(this T[] arrayToProcess, int start, int length, double average, Func<T[], int, int, double, T2> baseCase,
                                                            Func<T2, T2, T2> reduce, int thresholdPar = 16 * 1024, int degreeOfParallelism = 0)
        {
            return DivideAndConquerTwoTypesParLR(arrayToProcess, start, start + length - 1, average, baseCase, reduce, thresholdPar, degreeOfParallelism);
        }
        private static T2 DivideAndConquerTwoTypesPar2<T, T2>(this T[] arrayToProcess, int start, int length, double average, Func<T[], int, int, double, T2> baseCase,
                                                            Func<T2, T2, T2> reduce, int thresholdPar = 16 * 1024, int degreeOfParallelism = 0)
        {
            return DivideAndConquerTwoTypesParLR(arrayToProcess, start, start + length - 1, average, baseCase, reduce, thresholdPar, degreeOfParallelism);
        }
        /// Note that the baseCase function is on (start, length) interface and not (left, right) because the pulic divide-and-conquer function has that interface
        internal static T2 DivideAndConquerTwoTypesParLR<T, T2>(this T[] arrayToProcess, int left, int right, double average, Func<T[], int, int, double, T2> baseCase, Func<T2, T2, T2> reduce,
                                                               int thresholdPar = 16 * 1024, int degreeOfParallelism = 0)
        {
            T2 resultLeft = (T2)Convert.ChangeType(default(T), typeof(T2));

            if (left > right)
                return resultLeft;
            if ((right - left + 1) <= thresholdPar)
                return baseCase(arrayToProcess, left, right - left + 1, average);    // Not (left, rigtht), but (start, length) instead for the baseCase function

            int mid = (right + left) / 2;

            T2 resultRight = (T2)Convert.ChangeType(default(T), typeof(T2));

            if (degreeOfParallelism == 1)
            {
                resultLeft  = DivideAndConquerTwoTypesParLR(arrayToProcess, left,     mid,  average, baseCase, reduce, thresholdPar);
                resultRight = DivideAndConquerTwoTypesParLR(arrayToProcess, mid + 1, right, average, baseCase, reduce, thresholdPar);
            }
            else if (degreeOfParallelism > 1)
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
                Parallel.Invoke(options,
                    () => { resultLeft  = DivideAndConquerTwoTypesParLR(arrayToProcess, left,     mid,  average, baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerTwoTypesParLR(arrayToProcess, mid + 1, right, average, baseCase, reduce, thresholdPar); }
                );
            }
            else
            {
                Parallel.Invoke(
                    () => { resultLeft  = DivideAndConquerTwoTypesParLR(arrayToProcess, left,     mid,  average, baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerTwoTypesParLR(arrayToProcess, mid + 1, right, average, baseCase, reduce, thresholdPar); }
                );
            }

            return reduce(resultLeft, resultRight);
        }

        private static T DivideAndConquerPar<T>(this T[] arrayToProcess, int start, int length, double average, Func<T[], int, int, double, T> baseCase, Func<T, T, T> reduce,
                                               int thresholdPar = 16 * 1024, int degreeOfParallelism = 0)
        {
            return DivideAndConquerParLR(arrayToProcess, start, start + length - 1, average, baseCase, reduce, thresholdPar, degreeOfParallelism);
        }
        /// Note that the baseCase function is on (start, length) interface and not (left, right) because the pulic divide-and-conquer function has that interface
        internal static T DivideAndConquerParLR<T>(this T[] arrayToProcess, int left, int right, double average, Func<T[], int, int, double, T> baseCase, Func<T, T, T> reduce, int thresholdPar = 16 * 1024,
                                                   int degreeOfParallelism = 0)
        {
            T resultLeft = default(T);

            if (left > right)
                return resultLeft;
            if ((right - left + 1) <= thresholdPar)
                return baseCase(arrayToProcess, left, right - left + 1, average);        // Not (left, rigtht), but (start, length) instead for the baseCase function

            int mid = (right + left) / 2;

            T resultRight = default(T);

            if (degreeOfParallelism == 1)
            {
                resultLeft  = DivideAndConquerParLR(arrayToProcess, left,    mid,   average, baseCase, reduce, thresholdPar);
                resultRight = DivideAndConquerParLR(arrayToProcess, mid + 1, right, average, baseCase, reduce, thresholdPar);
            }
            else if (degreeOfParallelism > 1)
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
                Parallel.Invoke(options,
                    () => { resultLeft  = DivideAndConquerParLR(arrayToProcess, left,    mid,   average, baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerParLR(arrayToProcess, mid + 1, right, average, baseCase, reduce, thresholdPar); }
                );
            }
            else
            {
                Parallel.Invoke(
                    () => { resultLeft = DivideAndConquerParLR(arrayToProcess, left,     mid,   average, baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerParLR(arrayToProcess, mid + 1, right, average, baseCase, reduce, thresholdPar); }
                );
            }

            return reduce(resultLeft, resultRight);
        }
    }
}
