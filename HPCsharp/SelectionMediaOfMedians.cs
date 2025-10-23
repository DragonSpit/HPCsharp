// Explanation and Reasoning:

using System;
using System.Collections;
using System.Collections.Generic;

namespace HPCsharp
{
    public static partial class Algorithm
    {
        /// <summary>
        /// Median of Medians computation.
        /// </summary>
        /// <param name="arr">array that is to be selected from in place</param>
        /// <param name="arr_working">and additional array which is used as a scratchpad for median-of-medians computation</param>
        /// <param name="start">starting index of the subarray</param>
        /// <param name="length">length of the subarray</param>
        /// <param name="comparer">optional method to compare array elements</param>
        /// <returns>returns the index of an approximate median of the subarray</returns>
        public static int MedianOfMedians<T>(T[] arr, T[] arr_working, int start, int length, IComparer<T> comparer = null, int chunkSize = 5)
        {
            if (arr == null || arr_working == null)
                throw new ArgumentNullException(nameof(arr));
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "start is invalid");
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "length is invalid");
            if (chunkSize < 5 || chunkSize > 31)
                throw new ArgumentOutOfRangeException(nameof(chunkSize), "5 <= chunkSize <= 31");

            Array.Copy(arr, start, arr_working, start, length);

            int halfChunkSize = chunkSize / 2;
            int length_working = length;
            //Console.WriteLine("MedianOfMedians: starting with chunkSize = {0}", chunkSize);
            while (length_working > 1)
            {
                //Console.WriteLine("MedianOfMedians: length_working = {0}", length_working);
                int numFullFiveTuples = length_working / chunkSize;
                for (int i = 0; i < numFullFiveTuples; i++)
                {
                    //Array.Sort(arr_working, start + i * chunkSize, chunkSize, comparer);
                    HPCsharp.Algorithm.InsertionSort<T>(arr_working, start + i * chunkSize, chunkSize, comparer);
                    arr_working[start + i] = arr_working[start + i * chunkSize + halfChunkSize]; // Move the median to the front
                }
                int remainingElements = length_working % chunkSize;
                if (remainingElements > 0)
                {
                    //Array.Sort(arr_working, start + numFullFiveTuples * chunkSize, remainingElements, comparer);
                    HPCsharp.Algorithm.InsertionSort<T>(arr_working, start + numFullFiveTuples * chunkSize, remainingElements, comparer);
                    arr_working[start + numFullFiveTuples] = arr_working[start + numFullFiveTuples * chunkSize + remainingElements / 2]; // Move the median to the front
                }
                length_working = numFullFiveTuples + (remainingElements > 0 ? 1 : 0);
            }
            //Console.WriteLine("MedianOfMedians: last length_working = {0}", length_working);
            int j = 0;
            var equalityComparer = comparer ?? Comparer<T>.Default;
            for (; j < length; j++)
                if (equalityComparer.Compare(arr[start + j], arr_working[start]) == 0) break;
            if (j == length)
                throw new Exception("MedianOfMedians: Cannot find the median of medians in the original array, which should never happen");
            //Console.WriteLine("MedianOfMedians: median of medians index = {0}", j);
            return start + j;
        }
        /// <summary>
        /// Partition for QuickSelect or QuickSort which chooses the last element within arr[l..r] as the pivot.
        /// For the presorted array case, it will cause the worst-case O(n^2) performance for QuickSort or QuickSelect.
        /// The input array is modified during the process.
        /// </summary>
        /// <param name="arr">source array</param>
        /// <param name="l">left index of the subarray, inclusive</param>
        /// <param name="r">right index of the subarray, inclusive</param>
        /// <param name="comparer">optional method to compare array elements</param>
        /// <returns>returns the index of where the pivot element ended up at</returns>
        public static int PartitionMoM<T>(this T[] arr, int l, int r, T[] copy_arr, IComparer<T> comparer = null, int chunkSize = 5)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            int i = l - 1;   // start index of smaller elements
            int j = r;       // start index of larger  elements
            int median_index = MedianOfMedians<T>(arr, copy_arr, l, r - l + 1, comparer, chunkSize);
            (arr[r], arr[median_index]) = (arr[median_index], arr[r]); // Move pivot to the last element location
            T v = arr[r];    // pivot is the last element

            while (true)
            {
                while (equalityComparer.Compare(arr[++i], v) < 0) ; // find first item which is >= v starting from left - i.e. item which doesn't belong on the left of the pivot
                while (equalityComparer.Compare(arr[--j], v) > 0) if (j == l) break; // find first item which is <= v starting from right
                if (i >= j) break;                      // if pointers cross then done
                (arr[i], arr[j]) = (arr[j], arr[i]);    // swap
            }
            (arr[i], arr[r]) = (arr[r], arr[i]);        // swap arr[i+1] and arr[r] (or pivot)
            return i;
        }
        private static void SelectMoMGenericNonRecursive_loc<T>(T[] arr, int l, int r, int k, T[] copy_arr, IComparer<T> comparer = null, int chunkSize = 5)
        {
            while (r > l)
            {
                //Console.WriteLine("SelectMoMGenericNonRecursive_loc: l = {0}, r = {1}, k = {2}", l, r, k);
                int i = PartitionMoM(arr, l, r, copy_arr, comparer, chunkSize);
                //Console.WriteLine("SelectMoMGenericNonRecursive_loc: PartitionMoM returned i = {0}", i);
                if (i >= k) r = i - 1;
                if (i <= k) l = i + 1;
            }
        }
        /// <summary>
        /// Selection using Median of Medians when choosing the pivot partition element.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be selected from in place</param>
        /// <param name="start">starting index of the subarray</param>
        /// <param name="length">length of the subarray</param>
        /// <param name="k">index of the desired element to be selected</param>
        /// <param name="comparer">optional method to compare array elements</param>
        /// <returns>returns the k-th element of the array</returns>
        public static T SelectMoM<T>(T[] arrayToBeSelected, Int32 start, Int32 length, Int32 k, IComparer<T> comparer = null, int chunkSize = 5)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), "start is invalid");
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "length is invalid");
            if (k < start || k > (start + arrayToBeSelected.Length))
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length - 1)");

            T[] tmp_arr = new T[arrayToBeSelected.Length];
            SelectMoMGenericNonRecursive_loc(arrayToBeSelected, start, start + length - 1, k, tmp_arr, comparer, chunkSize);
            return arrayToBeSelected[k];
        }
        /// <summary>
        /// Selection using Median of Medians when choosing the pivot partition element.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be sorted in place</param>
        /// <param name="k">index of the desired element to be selected</param>
        /// <param name="comparer">optional method to compare array elements</param>
        /// <returns>returns the k-th element of the array</returns>
        public static T SelectMoM<T>(T[] arrayToBeSelected, Int32 k, IComparer<T> comparer = null, int chunkSize = 5)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (arrayToBeSelected.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < 0 || k > arrayToBeSelected.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");

            T[] copy_arr = new T[arrayToBeSelected.Length];
            SelectMoMGenericNonRecursive_loc(arrayToBeSelected, 0, arrayToBeSelected.Length - 1, k, copy_arr, comparer, chunkSize);
            return arrayToBeSelected[k];
        }
    }
}
