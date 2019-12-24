// TODO: Implement a cache-aligned divide-and-conquer split. This is useful and fundamental when writing to cache lines, otherwise false sharing causes performance, and cache line boundary
//       divide-and-conquer is needed to improve consistency of performance - i.e. reduce veriability in performance. However, for algorithms such as .Sum() which only read from memory, this is not needed.
// TODO: Change the overall interface to be not (left, right), but (startIndex, length) instead, to be consistent with the rest of HPCsharp library
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
        /// <param name="left">left/starting index of the first element to be processed (inclusive)</param>
        /// <param name="right">right/last index of the last element to be processed (inclusive)</param>
        /// <param name="baseCase">function for the recursion base case (leaf node)</param>
        /// <param name="reduce">function for combining the two recursive results</param>
        /// <param name="threshold">if array is larger than this value, then divide-and-conquer will be applied, otherwise the basecase function will be invoked</param>
        /// <returns>result value</returns>
        private static T DivideAndConquer<T>(this T[] arrayToProcess, int left, int right, Func<T[], int, int, T> baseCase, Func<T, T, T> reduce, int threshold = 16 * 1024)
        {
            T resultLeft = default(T);

            if (left > right)
                return resultLeft;
            if ((right - left + 1) <= threshold)
                return baseCase(arrayToProcess, left, right - left + 1);    // Not (left, rigtht), but (start, length) instead for the baseCase function

            int mid = (right + left) / 2;

            T resultRight = default(T);

            resultLeft  = DivideAndConquer(arrayToProcess, left,    mid,   baseCase, reduce, threshold);
            resultRight = DivideAndConquer(arrayToProcess, mid + 1, right, baseCase, reduce, threshold);

            return reduce(resultLeft, resultRight);
        }
        /// <summary>
        /// Serial Divide and Conquer generic pattern.
        /// </summary>
        /// <param name="arrayToProcess">An input array to be processed</param>
        /// <param name="left">left/starting index of the first element to be processed (inclusive)</param>
        /// <param name="right">right/last index of the last element to be processed (inclusive)</param>
        /// <param name="baseCase">function for the recursion base case (leaf node)</param>
        /// <param name="reduce">function for combining the two recursive results</param>
        /// <param name="threshold">if array is larger than this value, then divide-and-conquer will be applied, otherwise the basecase function will be invoked</param>
        /// <returns>result value</returns>
        private static T2 DivideAndConquerTwoTypes<T, T2>(this T[] arrayToProcess, int left, int right, Func<T[], int, int, T2> baseCase, Func<T2, T2, T2> reduce,
                                                          int threshold = 16 * 1024)
        {
            T2 resultLeft = (T2)Convert.ChangeType(default(T), typeof(T2));

            if (left > right)
                return resultLeft;
            if ((right - left + 1) <= threshold)
                return baseCase(arrayToProcess, left, right - left + 1);    // Not (left, rigtht), but (start, length) instead for the baseCase function

            int mid = (right + left) / 2;

            T2 resultRight = (T2)Convert.ChangeType(default(T), typeof(T2));

            resultLeft  = DivideAndConquerTwoTypes(arrayToProcess, left,    mid,   baseCase, reduce, threshold);
            resultRight = DivideAndConquerTwoTypes(arrayToProcess, mid + 1, right, baseCase, reduce, threshold);

            return reduce(resultLeft, resultRight);
        }
        /// <summary>
        /// Parallel Divide and Conquer generic pattern.
        /// </summary>
        /// <param name="arrayToProcess">An input array to be processed</param>
        /// <param name="left">left/starting index of the first element to be processed (inclusive)</param>
        /// <param name="right">right/last index of the last element to be processed (inclusive)</param>
        /// <param name="baseCase">function for the recursion base case (leaf node)</param>
        /// <param name="reduce">function for combining the two recursive results</param>
        /// <param name="thresholdPar">if array is larger than this value, then parallel processing will be used, otherwise serial processing will be used by invoking the baseCase function</param>
        /// <param name="degreeOfParallelism">amount of parallelism to be used - i.e. number of computational cores. When set to zero or negative, all available cores will be utilized.
        /// When set to 1, then a single core will be used. When set to > 1, then that many cores will be used.</param>
        /// <returns>result value</returns>
        public static T DivideAndConquerPar<T>(this T[] arrayToProcess, int left, int right, Func<T[], int, int, T> baseCase, Func<T, T, T> reduce, int thresholdPar = 16 * 1024,
                                               int degreeOfParallelism = 0)
        {
            T resultLeft = default(T);

            if (left > right)
                return resultLeft;
            if ((right - left + 1) <= thresholdPar)
                return baseCase(arrayToProcess, left, right - left + 1);        // Not (left, rigtht), but (start, length) instead for the baseCase function

            int mid = (right + left) / 2;

            T resultRight = default(T);

            if (degreeOfParallelism == 1)
            {
                resultLeft  = DivideAndConquerPar(arrayToProcess, left,    mid,   baseCase, reduce, thresholdPar);
                resultRight = DivideAndConquerPar(arrayToProcess, mid + 1, right, baseCase, reduce, thresholdPar);
            }
            else if (degreeOfParallelism > 1)
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
                Parallel.Invoke( options,
                    () => { resultLeft  = DivideAndConquerPar(arrayToProcess, left,    mid,   baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerPar(arrayToProcess, mid + 1, right, baseCase, reduce, thresholdPar); }
                );
            }
            else
            {
                Parallel.Invoke(
                    () => { resultLeft  = DivideAndConquerPar(arrayToProcess, left,    mid,   baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerPar(arrayToProcess, mid + 1, right, baseCase, reduce, thresholdPar); }
                );
            }

            return reduce(resultLeft, resultRight);
        }
        /// <summary>
        /// Parallel Divide and Conquer generic pattern.
        /// </summary>
        /// <param name="arrayToProcess">An input array to be processed</param>
        /// <param name="start">starting index of the first element to be processed (inclusive)</param>
        /// <param name="length">number of elements to be processed</param>
        /// <param name="baseCase">function for the recursion base case (leaf node)</param>
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
        /// <summary>
        /// Parallel Divide and Conquer generic pattern.
        /// </summary>
        /// <param name="arrayToProcess">An input array to be processed</param>
        /// <param name="left">left/starting index of the first element to be processed (inclusive)</param>
        /// <param name="right">right/last index of the last element to be processed (inclusive)</param>
        /// <param name="baseCase">function for the recursion base case (leaf node)</param>
        /// <param name="reduce">function for combining the two recursive results</param>
        /// <param name="thresholdPar">if array is larger than this value, then parallel processing will be used, otherwise serial processing will be used by invoking the baseCase function</param>
        /// <param name="degreeOfParallelism">amount of parallelism to be used - i.e. number of computational cores. When set to zero or negative, all available cores will be utilized.
        /// When set to 1, then a single core will be used. When set to > 1, then that many cores will be used.</param>
        /// <returns>result value</returns>
        internal static T2 DivideAndConquerTwoTypesParLR<T, T2>(this T[] arrayToProcess, int left, int right, Func<T[], int, int, T2> baseCase, Func<T2, T2, T2> reduce,
                                                               int thresholdPar = 16 * 1024, int degreeOfParallelism = 0)
        {
            T2 resultLeft = (T2)Convert.ChangeType(default(T), typeof(T2));

            //Console.WriteLine("left = {0}   right = {1}", left, right);

            if (left > right)
                return resultLeft;
            if ((right - left + 1) <= thresholdPar)
                return baseCase(arrayToProcess, left, right - left + 1);    // Not (left, rigtht), but (start, length) instead for the baseCase function

            int mid = (right + left) / 2;

            T2 resultRight = (T2)Convert.ChangeType(default(T), typeof(T2));

            if (degreeOfParallelism == 1)
            {
                resultLeft  = DivideAndConquerTwoTypesParLR(arrayToProcess, left,    mid,   baseCase, reduce, thresholdPar);
                resultRight = DivideAndConquerTwoTypesParLR(arrayToProcess, mid + 1, right, baseCase, reduce, thresholdPar);
            }
            else if (degreeOfParallelism > 1)
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
                Parallel.Invoke(options,
                    () => { resultLeft  = DivideAndConquerTwoTypesParLR(arrayToProcess, left,    mid,   baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerTwoTypesParLR(arrayToProcess, mid + 1, right, baseCase, reduce, thresholdPar); }
                );
            }
            else
            {
                Parallel.Invoke(
                    () => { resultLeft  = DivideAndConquerTwoTypesParLR(arrayToProcess, left,    mid,   baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerTwoTypesParLR(arrayToProcess, mid + 1, right, baseCase, reduce, thresholdPar); }
                );
            }

            return reduce(resultLeft, resultRight);
        }
    }
}
