// TODO: Implement a cache-aligned divide-and-conquer split. This is useful and fundamental when writing to cache lines, otherwise false sharing causes performance, and cache line boundary
//       divide-and-conquer is needed to improve consistency of performance - i.e. reduce veriability in performance. However, for algorithms such as .Sum() which only read from memory, this is not needed.
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
        /// <param name="l">left/starting index of the first element to be processed (inclusive)</param>
        /// <param name="r">right/last index of the last element to be processed (inclusive)</param>
        /// <param name="baseCase">function for the recursion base case (leaf node)</param>
        /// <param name="reduce">function for combining the two recursive results</param>
        /// <param name="threshold">if array is larger than this value, then parallel processing will be used, otherwise serial processing will be used by invoking the baseCase function</param>
        /// <returns>result value</returns>
        private static T DivideAndConquer<T>(this T[] arrayToProcess, int l, int r, Func<T[], int, int, T> baseCase, Func<T, T, T> reduce, int threshold = 16 * 1024)
        {
            T resultLeft = default(T);

            if (l > r)
                return resultLeft;
            if ((r - l + 1) <= threshold)
                return baseCase(arrayToProcess, l, r);

            int m = (r + l) / 2;

            T resultRight = default(T);

            resultLeft  = DivideAndConquer(arrayToProcess, l,     m, baseCase, reduce, threshold);
            resultRight = DivideAndConquer(arrayToProcess, m + 1, r, baseCase, reduce, threshold);

            return reduce(resultLeft, resultRight);
        }

        /// <summary>
        /// Parallel Divide and Conquer generic pattern.
        /// </summary>
        /// <param name="arrayToProcess">An input array to be processed</param>
        /// <param name="l">left/starting index of the first element to be processed (inclusive)</param>
        /// <param name="r">right/last index of the last element to be processed (inclusive)</param>
        /// <param name="baseCase">function for the recursion base case (leaf node)</param>
        /// <param name="reduce">function for combining the two recursive results</param>
        /// <param name="thresholdPar">if array is larger than this value, then parallel processing will be used, otherwise serial processing will be used by invoking the baseCase function</param>
        /// <param name="parallelism">amount of parallelism to be used - i.e. number of computational cores. When set to zero, all available cores will be utilized</param>
        /// <returns>result value</returns>
        private static T DivideAndConquerPar<T>(this T[] arrayToProcess, int l, int r, Func<T[], int, int, T> baseCase, Func<T, T, T> reduce, int thresholdPar = 16 * 1024, int parallelism = 0)
        {
            T resultLeft = default(T);

            if (l > r)
                return resultLeft;
            if ((r - l + 1) <= thresholdPar)
                return baseCase(arrayToProcess, l, r);

            int m = (r + l) / 2;

            T resultRight = default(T);

            if (parallelism == 0)
            {
                Parallel.Invoke(
                    () => { resultLeft  = DivideAndConquerPar(arrayToProcess, l,     m, baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerPar(arrayToProcess, m + 1, r, baseCase, reduce, thresholdPar); }
                );
            }
            else
            {
                var options = new ParallelOptions { MaxDegreeOfParallelism = parallelism };
                Parallel.Invoke( options,
                    () => { resultLeft  = DivideAndConquerPar(arrayToProcess, l,     m, baseCase, reduce, thresholdPar); },
                    () => { resultRight = DivideAndConquerPar(arrayToProcess, m + 1, r, baseCase, reduce, thresholdPar); }
                );
            }

            return reduce(resultLeft, resultRight);
        }
    }
}
