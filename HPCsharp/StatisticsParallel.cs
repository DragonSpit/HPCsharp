// TODO: StandardDeviation method along with .Average could be parallelized. See how much faster our implementation than Linq. How much faster is Linq parallel version versus serial.
// TODO: This function can be accelerated substantially, possibly using SIMD/SSE, since instead of Math.Pow we could use multiplication, after casting to double in SSE.
//       The flow from variety of integer data types would be to cast to double, then square by multiplying, then to sum up and divide by the number of elements to compute the average.
//       yielding a much faster standard deviation computation, which uses SIMD and multi-core. Use parallel .Sum that uses integer computation, which should be much faster.
// TODO: It should be pretty simple to implement the base-case as SSE/SIMD of (v - avg) * (v - avg) and Sum them, and then use Divide-And-Conquer algorithm in HPCsharp to
//       combine the partial sums with perfect precision, possibly using Kahan sum for doubles, or sum to Decimal and extended long first.
// TODO: For floating-point addition offer Kahan summation for a more accurate result.
// TODO: Figure out how to pass in an arbitrary number and types of arguments to the divide-and-conquer generic funtion, possibly as the last params object[] argument
//       with = null as default.
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
        public static double StandardDeviationTestPar(this int[] values)
        {
            double avg = values.AsParallel().Average();
            return Math.Sqrt(values.AsParallel().Average(v => (v - avg) * (v - avg)) / (values.Length - 1));
        }
        /// <summary>
        /// Standard deviation of an array of integers.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationPar(this int[] values)
        {
            long sum = HPCsharp.ParallelAlgorithms.Sum.SumToLongSsePar(values);     // multi-core, SSE, no exceptions and full accuracy
            double avg = (double)sum / values.Length;
            return Math.Sqrt(values.SumForStdDeviationPar(avg) / (values.Length - 1 ));
        }
        private static double SumForStdDeviationPar(this int[] arrayToSum, double average, int thresholdParallel = 16 * 1024)
        {
            return DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, average, SumForDeviationToDoubleSse, (x, y) => x + y, thresholdParallel);
        }

        private static double SumForStdDeviationPar(this int[] arrayToSum, int startIndex, int length, double average, int thresholdParallel = 16 * 1024)
        {
            return DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, average, SumForDeviationToDoubleSse, (x, y) => x + y, thresholdParallel);
        }

        private static double SumForDeviationToDoubleSse(this int[] arrayToSum, int startIndex, int length, double average)
        {
            return arrayToSum.SumForDeviationSseDoubleInner(startIndex, startIndex + length - 1, average);
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
        public static T2 DivideAndConquerTwoTypesPar<T, T2>(this T[] arrayToProcess, int start, int length, double average, Func<T[], int, int, double, T2> baseCase,
                                                            Func<T2, T2, T2> reduce, int thresholdPar = 16 * 1024, int degreeOfParallelism = 0)
        {
            return DivideAndConquerTwoTypesParLR(arrayToProcess, start, start + length - 1, average, baseCase, reduce, thresholdPar, degreeOfParallelism);
        }
        public static T2 DivideAndConquerTwoTypesPar2<T, T2>(this T[] arrayToProcess, int start, int length, double average, Func<T[], int, int, double, T2> baseCase,
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
                resultLeft = DivideAndConquerTwoTypesParLR(arrayToProcess, left,     mid,   average, baseCase, reduce, thresholdPar);
                resultRight = DivideAndConquerTwoTypesParLR(arrayToProcess, mid + 1, right, average, baseCase, reduce, thresholdPar);
            }
            else if (degreeOfParallelism > 1)
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
                Parallel.Invoke(options,
                    () => { resultLeft = DivideAndConquerTwoTypesParLR(arrayToProcess, left,     mid,   average, baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerTwoTypesParLR(arrayToProcess, mid + 1, right, average, baseCase, reduce, thresholdPar); }
                );
            }
            else
            {
                Parallel.Invoke(
                    () => { resultLeft = DivideAndConquerTwoTypesParLR(arrayToProcess, left,     mid,   average, baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerTwoTypesParLR(arrayToProcess, mid + 1, right, average, baseCase, reduce, thresholdPar); }
                );
            }

            return reduce(resultLeft, resultRight);
        }

        public static T DivideAndConquerPar<T>(this T[] arrayToProcess, int start, int length, double average, Func<T[], int, int, double, T> baseCase, Func<T, T, T> reduce,
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
        /// <summary>
        /// Standard deviation of an array of integers.
        /// </summary>
        /// <param name="values">An array to sum up</param>
        /// <returns>standard deviation as a double</returns>
        public static double StandardDeviationPar(this long[] values)
        {
            double avg = values.AsParallel().Average();
            return Math.Sqrt(values.AsParallel().Average(v => (v - avg) * (v - avg)) / (values.Length - 1 ));
        }
        /// <summary>
        /// Summation of float[] array, using a double accumulator for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        private static double SumForDeviationToDoubleSse(this long[] arrayToSum, double average, int startIndex, int length)
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
                overallSum += (arrayToSum[i] - average) * (arrayToSum[i] - average);
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
            return Math.Sqrt(values.AsParallel().Average(v => (v - avg) * (v - avg)) / (values.Length - 1));
        }
        /// <summary>
        /// Summation of float[] array, using a double accumulator for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        private static double SumForDeviationToDoubleSse(this float[] arrayToSum, double average, int startIndex, int length)
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
                overallSum += (arrayToSum[i] - average) * (arrayToSum[i] - average);
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
            return Math.Sqrt(values.AsParallel().Average(v => (v - avg) * (v - avg)) / (values.Length - 1));
        }
        /// <summary>
        /// Summation of float[] array, using a double accumulator for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        private static double SumForStdDeviationToDoubleSse(this double[] arrayToSum, double average, int startIndex, int length)
        {
            return arrayToSum.SumForStdDeviationSseDoubleInner(average, startIndex, startIndex + length - 1);
        }

        private static double SumForStdDeviationSseDoubleInner(this double[] arrayToSum, double average, int l, int r)
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
                overallSum += (arrayToSum[i] - average) * (arrayToSum[i] - average);
            for (i = 0; i < Vector<double>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }
        /// <summary>
        /// Summation of float[] array, using a double accumulator for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumForMeanDeviationToDoubleSse(this double[] arrayToSum, double average, int startIndex, int length)
        {
            return arrayToSum.SumForMeanDeviationSseDoubleInner(average, startIndex, startIndex + length - 1);
        }

        private static double SumForMeanDeviationSseDoubleInner(this double[] arrayToSum, double average, int l, int r)
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
        /// <returns>mean absolute deviation as a double</returns>
        public static double MeanAbsoluteDeviationPar(this int[] values)
        {
            double avg = (double)values.SumToLongSsePar() / values.Length;
            return values.AsParallel().Average(v => Math.Abs(v - avg)) / values.Length;
        }

        /// <summary>
        /// Mean absolute deviation of an array of longs.
        /// </summary>
        /// <param name="values">An array of doubles as input data</param>
        /// <returns>mean absolute deviation as a double</returns>
        public static double MeanAbsoluteDeviationPar(this long[] values)
        {
            double avg = (double)values.SumToDecimalFasterPar() / values.Length;
            return values.AsParallel().Average(v => Math.Abs(v - avg)) / values.Length;
        }
        /// <summary>
        /// Mean absolute deviation of an array of floats.
        /// </summary>
        /// <param name="values">An array of doubles as input data</param>
        /// <returns>mean absolute deviation as a double</returns>
        public static double MeanAbsoluteDeviationPar(this float[] values)
        {
            double avg = (double)values.SumPar() / values.Length;
            return values.AsParallel().Average(v => Math.Abs(v - avg)) / values.Length;
        }

        /// <summary>
        /// Mean absolute deviation of an array of doubles.
        /// </summary>
        /// <param name="values">An array of doubles as input data</param>
        /// <returns>mean absolute deviation as a double</returns>
        public static double MeanAbsoluteDeviationPar(this double[] values)
        {
            double avg = values.SumPar() / values.Length;
            return values.AsParallel().Average(v => Math.Abs(v - avg)) / values.Length;
        }
    }
}
