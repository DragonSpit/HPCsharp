using System;
using System.Collections.Generic;

namespace HPCsharp
{
    public static partial class Algorithm
    {
        const int IntrospectiveSelectionThreshold = 4;

        // l and r are inclusive
        private static Tuple<int, int> QuickSelectRandomGenericNonRecursive_loc2<T>(T[] arr, int l, int r, int k, Random rand, IComparer<T> comparer = null)
        {
            int worstCaseNumberOfPartitions = IntrospectiveSelectionThreshold * Int32.Log2(r - l + 1);
            int numberOfPartitions = 0;
            while (r > l && numberOfPartitions < worstCaseNumberOfPartitions)
            {
                int i = Partition(arr, l, r, rand, comparer);
                if (i >= k) r = i - 1;
                if (i <= k) l = i + 1;
                numberOfPartitions++;
            }
            //Console.WriteLine("Selection: numberOfPartitions = {0}  worstCaseNumberOfPartitions = {1}", numberOfPartitions, worstCaseNumberOfPartitions);
            return Tuple.Create(l, r);
        }

        /// <summary>
        /// Generic Selection of k-th element algorithm. Takes an unsorted array of any type filled with distinct values and a comparer and returns the k-th element.
        /// Runs in O(n) time in the worst case.
        /// Introspective style implementation which switches to Median of Medians selection when the recursion depth exceeds c * log(n).
        /// The input array is modified during the selection process.
        /// </summary>
        /// <typeparam name="T">Array of type T</typeparam>
        /// <param name="arr">source array</param>
        /// <param name="l">left index of the subarray, inclusive</param>
        /// <param name="r">right index of the subarray, inclusive</param>
        /// <param name="k">index of the desired element to be selected</param>
        /// <param name="randSeed">Seed for the random number generator. Negative value makes random not-repeatable.</param>
        /// <param name="comparer">comparer used to compare two array elements of type T. Null uses the default comparer.</param>
        /// <returns>returns the k-th element of the array</returns>
        public static T Select<T>(this T[] arr, int l, int r, int k, int randSeed = -1, IComparer<T> comparer = null)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr));
            if (l < 0 || r >= arr.Length || l > r)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < l || k > r)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between l and r");
            Random rand = (randSeed < 0) ? new Random() : new Random(randSeed);
            Tuple<int, int> after = QuickSelectRandomGenericNonRecursive_loc2(arr, l, r, k, rand, comparer);
            if (after.Item1 < after.Item2)
            {
                //Console.WriteLine("SelectionMoM: l = {0}  r = {1}", after.Item1, after.Item2);
                SelectMomInplace(arr, after.Item1, after.Item2 - after.Item1 + 1, k, comparer);
            }
            return arr[k];
        }

        /// <summary>
        /// Generic Selection of k-th element algorithm. Takes an unsorted array of any type filled with distinct values and a comparer and returns the k-th element.
        /// Runs in O(n) time in the worst case.
        /// Introspective style implementation which switches to Median of Medians selection when the recursion depth exceeds c * log(n).
        /// The input array is modified during the selection process.
        /// </summary>
        /// <typeparam name="T">Array of type T</typeparam>
        /// <param name="arr">source array</param>
        /// <param name="k">index of the desired element to be selected</param>
        /// <param name="randSeed">Seed for the random number generator. Negative value makes random not-repeatable.</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns the k-th element of the array</returns>
        public static T Select<T>(this T[] arr, int k, int randSeed = -1, IComparer<T> comparer = null)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr));
            if (k < 0 || k >= arr.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between 0 and arr.Length-1");
            Random rand = (randSeed < 0) ? new Random() : new Random(randSeed);
            Tuple<int, int> after = QuickSelectRandomGenericNonRecursive_loc2(arr, 0, arr.Length - 1, k, rand, comparer);
            if (after.Item1 < after.Item2)
            {
                //Console.WriteLine("SelectionMoM: l = {0}  r = {1}", after.Item1, after.Item2);
                SelectMomInplace(arr, after.Item1, after.Item2 - after.Item1 + 1, k, comparer);
            }
            return arr[k];
        }
    }
}
