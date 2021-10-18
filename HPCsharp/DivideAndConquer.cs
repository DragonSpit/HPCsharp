// TODO: Implement a cache-aligned divide-and-conquer split. This is useful and fundamental when writing to cache lines, otherwise false sharing causes performance, and cache line boundary
//       divide-and-conquer is needed to improve consistency of performance - i.e. reduce veriability in performance. However, for algorithms such as .Sum() which only read from memory, this is not needed.
// TODO: Either figure out how to control the degree of parallelism or remove it from the interface since currently it only works for all cores or one.
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class AlgorithmPatterns
    {
        /// <summary>
        /// Serial Divide and Conquer generic pattern.
        /// </summary>
        /// <param name="arrayToProcess">An input array to be processed</param>
        /// <param name="start">starting index of the first element to be processed (inclusive)</param>
        /// <param name="length">number of elements to be processed</param>
        /// <param name="baseCase">function for the recursion base case (leaf node). The two parameters are start and length</param>
        /// <param name="reduce">function for combining the two recursive results</param>
        /// <param name="threshold">if array is larger than this value, then divide-and-conquer will be applied, otherwise the baseCase function will be invoked</param>
        /// <returns>result value</returns>
        public static T DivideAndConquer<T>(this T[] arrayToProcess, int start, int length, Func<T[], int, int, T> baseCase, Func<T, T, T> reduce, int threshold = 16 * 1024)
        {
            return DivideAndConquerLR(arrayToProcess, start, start + length - 1, baseCase, reduce, threshold);
        }
        /// baseCase function is on (start, length) interface and not (left, right) because the pulic divide-and-conquer function has that interface
        internal static T DivideAndConquerLR<T>(this T[] arrayToProcess, int left, int right, Func<T[], int, int, T> baseCase, Func<T, T, T> reduce, int threshold = 16 * 1024)
        {
            T resultLeft = default;

            if (left > right)
                return resultLeft;
            if ((right - left + 1) <= threshold)
                return baseCase(arrayToProcess, left, right - left + 1);    // Not (left, rigtht), but (start, length) instead for the baseCase function

            int mid = (right + left) / 2;

            T resultRight = default;

            resultLeft  = DivideAndConquerLR(arrayToProcess, left,    mid,   baseCase, reduce, threshold);
            resultRight = DivideAndConquerLR(arrayToProcess, mid + 1, right, baseCase, reduce, threshold);

            return reduce(resultLeft, resultRight);
        }
        /// <summary>
        /// Serial Divide and Conquer generic pattern, using input type T and output type T2.
        /// </summary>
        /// <param name="arrayToProcess">An input array to be processed</param>
        /// <param name="start">starting index of the first element to be processed (inclusive)</param>
        /// <param name="length">number of elements to be processed</param>
        /// <param name="baseCase">function for the recursion base case (leaf node). The two parameters are start and length</param>
        /// <param name="reduce">function for combining the two recursive results</param>
        /// <param name="threshold">if array is larger than this value, then divide-and-conquer will be applied, otherwise the basecase function will be invoked</param>
        /// <returns>result value</returns>
        public static T2 DivideAndConquerTwoTypes<T, T2>(this T[] arrayToProcess, int start, int length, Func<T[], int, int, T2> baseCase, Func<T2, T2, T2> reduce,
                                                         int threshold = 16 * 1024)
        {
            return DivideAndConquerTwoTypesLR(arrayToProcess, start, start + length - 1, baseCase, reduce, threshold);
        }
        /// Note that the baseCase function is on (start, length) interface and not (left, right) because the pulic divide-and-conquer function has that interface
        internal static T2 DivideAndConquerTwoTypesLR<T, T2>(this T[] arrayToProcess, int left, int right, Func<T[], int, int, T2> baseCase, Func<T2, T2, T2> reduce,
                                                             int threshold = 16 * 1024)
        {
            T2 resultLeft = (T2)Convert.ChangeType(default(T), typeof(T2));

            if (left > right)
                return resultLeft;
            if ((right - left + 1) <= threshold)
                return baseCase(arrayToProcess, left, right - left + 1);    // Not (left, rigtht), but (start, length) instead for the baseCase function

            int mid = (right + left) / 2;

            T2 resultRight = (T2)Convert.ChangeType(default(T), typeof(T2));

            resultLeft  = DivideAndConquerTwoTypesLR(arrayToProcess, left,    mid,   baseCase, reduce, threshold);
            resultRight = DivideAndConquerTwoTypesLR(arrayToProcess, mid + 1, right, baseCase, reduce, threshold);

            return reduce(resultLeft, resultRight);
        }
        /// <summary>
        /// Parallel Divide and Conquer generic pattern.
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
        /// Note that the baseCase function is on (start, length) interface and not (left, right) because the pulic divide-and-conquer function has that interface too
        public static T DivideAndConquerPar<T>(this T[] arrayToProcess, int start, int length, Func<T[], int, int, T> baseCase, Func<T, T, T> reduce,
                                               int thresholdPar = 16 * 1024, int degreeOfParallelism = 0)
        {
            return DivideAndConquerParLR(arrayToProcess, start, start + length - 1, baseCase, reduce, thresholdPar, degreeOfParallelism);
        }
        /// Note that the baseCase function is on (start, length) interface and not (left, right) because the pulic divide-and-conquer function has that interface
        internal static T DivideAndConquerParLR<T>(this T[] arrayToProcess, int left, int right, Func<T[], int, int, T> baseCase, Func<T, T, T> reduce, int thresholdPar = 16 * 1024,
                                                   int degreeOfParallelism = 0)
        {
            T resultLeft = default;

            if (left > right)
                return resultLeft;
            if ((right - left + 1) <= thresholdPar)
                return baseCase(arrayToProcess, left, right - left + 1);        // Not (left, right), but (start, length) instead for the baseCase function

            int mid = (right + left) / 2;

            T resultRight = default;

            if (degreeOfParallelism == 1)
            {
                resultLeft  = DivideAndConquerParLR(arrayToProcess, left,    mid,   baseCase, reduce, thresholdPar);
                resultRight = DivideAndConquerParLR(arrayToProcess, mid + 1, right, baseCase, reduce, thresholdPar);
            }
            else if (degreeOfParallelism > 1)
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
                Parallel.Invoke( options,
                    () => { resultLeft  = DivideAndConquerParLR(arrayToProcess, left,    mid,   baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerParLR(arrayToProcess, mid + 1, right, baseCase, reduce, thresholdPar); }
                );
            }
            else
            {
                Parallel.Invoke(
                    () => { resultLeft  = DivideAndConquerParLR(arrayToProcess, left,    mid,   baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerParLR(arrayToProcess, mid + 1, right, baseCase, reduce, thresholdPar); }
                );
            }

            return reduce(resultLeft, resultRight);
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
        public static T2 DivideAndConquerTwoTypesPar<T, T2>(this T[] arrayToProcess, int start, int length, Func<T[], int, int, T2> baseCase,
                                                            Func<T2, T2, T2> reduce, int thresholdPar = 16 * 1024, int degreeOfParallelism = 0)
        {
            return DivideAndConquerTwoTypesParLR(arrayToProcess, start, start + length - 1, baseCase, reduce, thresholdPar, degreeOfParallelism);
        }
        public static T2 DivideAndConquerTwoTypesPar2<T, T2>(this T[] arrayToProcess, int start, int length, Func<T[], int, int, T2> baseCase,
                                                            Func<T2, T2, T2> reduce, int thresholdPar = 16 * 1024, int degreeOfParallelism = 0)
        {
            return DivideAndConquerTwoTypesParLR2(arrayToProcess, start, start + length - 1, baseCase, reduce, thresholdPar, degreeOfParallelism);
        }
        /// Note that the baseCase function is on (start, length) interface and not (left, right) because the pulic divide-and-conquer function has that interface
        internal static T2 DivideAndConquerTwoTypesParLR<T, T2>(this T[] arrayToProcess, int left, int right, Func<T[], int, int, T2> baseCase, Func<T2, T2, T2> reduce,
                                                               int thresholdPar = 16 * 1024, int degreeOfParallelism = 0)
        {
            T2 resultLeft = (T2)Convert.ChangeType(default(T), typeof(T2));

            if (left > right)
                return resultLeft;
            if ((right - left + 1) <= thresholdPar)
                return baseCase(arrayToProcess, left, right - left + 1);    // Not (left, rigtht), but (start, length) instead for the baseCase function

            int mid = (right + left) / 2;

            T2 resultRight = (T2)Convert.ChangeType(default(T), typeof(T2));

            if (degreeOfParallelism == 1)
            {
                resultLeft  = DivideAndConquerTwoTypesParLR(arrayToProcess, left,    mid,   baseCase, reduce, thresholdPar, degreeOfParallelism);
                resultRight = DivideAndConquerTwoTypesParLR(arrayToProcess, mid + 1, right, baseCase, reduce, thresholdPar, degreeOfParallelism);
            }
            else if (degreeOfParallelism > 1)
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
                Parallel.Invoke(options,
                    () => { resultLeft  = DivideAndConquerTwoTypesParLR(arrayToProcess, left,    mid,   baseCase, reduce, thresholdPar, degreeOfParallelism); },
                    () => { resultRight = DivideAndConquerTwoTypesParLR(arrayToProcess, mid + 1, right, baseCase, reduce, thresholdPar, degreeOfParallelism); }
                );
            }
            else
            {
                Parallel.Invoke(
                    () => { resultLeft  = DivideAndConquerTwoTypesParLR(arrayToProcess, left,    mid,   baseCase, reduce, thresholdPar, degreeOfParallelism); },
                    () => { resultRight = DivideAndConquerTwoTypesParLR(arrayToProcess, mid + 1, right, baseCase, reduce, thresholdPar, degreeOfParallelism); }
                );
            }

            return reduce(resultLeft, resultRight);
        }
        /// Note that the baseCase function is on (start, length) interface and not (left, right) because the pulic divide-and-conquer function has that interface
        internal static T2 DivideAndConquerTwoTypesParLR2<T, T2>(this T[] arrayToProcess, int left, int right, Func<T[], int, int, T2> baseCase, Func<T2, T2, T2> reduce,
                                                               int thresholdPar = 16 * 1024, int degreeOfParallelism = 0)
        {
            T2 resultLeft = (T2)Convert.ChangeType(default(T), typeof(T2));

            if (left > right)
                return resultLeft;
            if ((right - left + 1) <= thresholdPar)
                return baseCase(arrayToProcess, left, right - left + 1);    // Not (left, rigtht), but (start, length) instead for the baseCase function

            int mid = ((right + left) / 2 ) & 0x7ffffff0;       // set mid-point on cache boundary (16 elements of 4 bytes each = 64 bytes)

            T2 resultRight = (T2)Convert.ChangeType(default(T), typeof(T2));

            if (degreeOfParallelism == 1)
            {
                resultLeft = DivideAndConquerTwoTypesParLR(arrayToProcess, left, mid, baseCase, reduce, thresholdPar);
                resultRight = DivideAndConquerTwoTypesParLR(arrayToProcess, mid + 1, right, baseCase, reduce, thresholdPar);
            }
            else if (degreeOfParallelism > 1)
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
                Parallel.Invoke(options,
                    () => { resultLeft = DivideAndConquerTwoTypesParLR(arrayToProcess, left, mid, baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerTwoTypesParLR(arrayToProcess, mid + 1, right, baseCase, reduce, thresholdPar); }
                );
            }
            else
            {
                Parallel.Invoke(
                    () => { resultLeft = DivideAndConquerTwoTypesParLR(arrayToProcess, left, mid, baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerTwoTypesParLR(arrayToProcess, mid + 1, right, baseCase, reduce, thresholdPar); }
                );
            }

            return reduce(resultLeft, resultRight);
        }
    }
}
