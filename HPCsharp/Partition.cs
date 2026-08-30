using System;
using System.Collections.Generic;

namespace HPCsharp
{
    public static partial class Algorithm
    {
        /// <summary>
        /// ported from Sedgewick "Algorithms in C++" p. 319
        /// Partition for QuickSelect or QuickSort which chooses the last element within arr[l..r] as the pivot.
        /// For the presorted array case, it will cause the worst-case O(n^2) performance for QuickSort or QuickSelect.
        /// The input array is modified during the partitioning process.
        /// </summary>
        /// <param name="arr">source array of integers</param>
        /// <param name="l">left index of the subarray, inclusive</param>
        /// <param name="r">right index of the subarray, inclusive</param>
        /// <returns>returns the index of where the pivot element ended up at</returns>
        public static int Partition(this int[] arr, int l, int r)
        {
            int i = l - 1;   // start index of smaller elements
            int j = r;       // start index of larger  elements
            int v = arr[r];  // pivot arbitrarily chosen as last element

            while (true)
            {
                while (arr[++i] < v) ;                  // find first item which is >= v starting from left - i.e. item which doesn't belong on the left of the pivot
                while (arr[--j] > v) if (j == l) break; // find first item which is <= v starting from right
                if (i >= j) break;                      // if pointers cross then done
                (arr[i], arr[j]) = (arr[j], arr[i]);    // swap
            }
            (arr[i], arr[r]) = (arr[r], arr[i]);        // swap arr[i+1] and arr[r] (pivot)
            return i;
        }

        /// <summary>
        /// Partition for QuickSelect or QuickSort which chooses the last element within arr[l..r] as the pivot.
        /// For the presorted array case, it will cause the worst-case O(n^2) performance for QuickSort or QuickSelect.
        /// The input array is modified during the partitioning process.
        /// </summary>
        /// <param name="arr">source array</param>
        /// <param name="l">left index of the subarray, inclusive</param>
        /// <param name="r">right index of the subarray, inclusive</param>
        /// <param name="comparer">optional method to compare array elements</param>
        /// <returns>returns the index of where the pivot element ended up at</returns>
        public static int Partition<T>(this T[] arr, int l, int r, IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            int i = l - 1;   // start index of smaller elements
            int j = r;       // start index of larger  elements
            T v = arr[r];    // pivot arbitrarily chosen as last element

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

        /// <summary>
        /// Partition for QuickSelect or QuickSort with randomly chosen pivot within arr[l..r]
        /// Minimizes the chance of worst-case O(n^2) behavior for QuickSort or QuickSelect by randomly choosing pivot element.
        /// The input array is modified during the partitioning process.
        /// </summary>
        /// <param name="arr">source array</param>
        /// <param name="l">left index of the subarray, inclusive</param>
        /// <param name="r">right index of the subarray, inclusive</param>
        /// <param name="rand">random number generator.</param>
        /// <returns>returns the index of where the pivot element ended up at</returns>
        public static int Partition(this int[] arr, int l, int r, Random rand)
        {
            int i = l - 1;                     // start index of smaller elements
            int j = r;                         // start index of larger  elements
            int r_index = rand.Next(l, r + 1); // Random index between l and r
            (arr[r], arr[r_index]) = (arr[r_index], arr[r]); // Move pivot to the last element location
            int v = arr[r];                    // pivot is the last element

            while (true)
            {
                while (arr[++i] < v) ;                  // find first item which is >= v starting from left - i.e. item which doesn't belong on the left of the pivot
                while (arr[--j] > v) if (j == l) break; // find first item which is <= v starting from right
                if (i >= j) break;                      // if pointers cross then done
                (arr[i], arr[j]) = (arr[j], arr[i]);    // swap
            }
            (arr[i], arr[r]) = (arr[r], arr[i]);        // swap arr[i+1] and arr[r] (or pivot)
            return i;
        }
        /// <summary>
        /// Partition for QuickSelect or QuickSort with randomly chosen pivot element within arr[l..r]
        /// Minimizes the chance of worst-case O(n^2) behavior for QuickSort or QuickSelect by randomly choosing pivot element.
        /// The input array is modified during the partitioning process.
        /// </summary>
        /// <param name="arr">source array</param>
        /// <param name="l">left index of the subarray, inclusive</param>
        /// <param name="r">right index of the subarray, inclusive</param>
        /// <param name="rand">random number generator.</param>
        /// <param name="comparer">optional method to compare array elements</param>
        /// <returns>returns the index of where the pivot element ended up at</returns>
        public static int Partition<T>(this T[] arr, int l, int r, Random rand, IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            int i = l - 1;   // start index of smaller elements
            int j = r;       // start index of larger  elements
            int r_index = rand.Next(l, r + 1); // Random index between l and r
            (arr[r], arr[r_index]) = (arr[r_index], arr[r]); // Move pivot to the last element location
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
    }
}