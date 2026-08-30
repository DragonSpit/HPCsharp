using System;
using System.Collections.Generic;

namespace HPCsharp
{
    public static partial class Algorithm
    {
        private static void QuickSelectRandom_loc(int[] arr, int l, int r, int k, Random rand)
        {
            if (r <= l) return;
            int i = Partition(arr, l, r, rand);
            if (i > k) QuickSelectRandom_loc(arr, l, i - 1, k, rand);
            if (i < k) QuickSelectRandom_loc(arr, i + 1, r, k, rand);
        }

        /// <summary>
        /// QuickSelect algorithm. Takes an unsorted array of distinct integers and returns the k-th element. Runs in O(n) time on average.
        /// Minimizes the chance of worst-case O(n^2) behavior by randomly choosing pivot element.
        /// The input array is modified during the process.
        /// </summary>
        /// <param name="arr">source array</param>
        /// <param name="l">left index of the subarray, inclusive</param>
        /// <param name="r">right index of the subarray, inclusive</param>
        /// <param name="k">index of the desired element</param>
        /// <param name="randSeed">Seed for the random number generator. Negative value makes random not-repeatable.</param>
        /// <returns>returns the k-th element of the array</returns>
        private static int QuickSelectRandom(this int[] arr, int l, int r, int k, int randSeed = -1)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr));
            if (l < 0 || r >= arr.Length || l > r)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < l || k > r)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between l and r");
            Random rand = (randSeed < 0) ? new Random() : new Random(randSeed);
            QuickSelectRandom_loc(arr, l, r, k, rand);
            return arr[k];
        }

        /// <summary>
        /// QuickSelect algorithm. Takes an unsorted array of distinct integers and returns the k-th element. Runs in O(n) time on average.
        /// Minimizes the chance of worst-case O(n^2) behavior by randomly choosing pivot element.
        /// The input array is modified during the selection process.
        /// </summary>
        /// <param name="arr">source array</param>
        /// <param name="k">index of the desired element</param>
        /// <param name="randSeed">Seed for the random number generator. Negative value makes random not-repeatable.</param>
        /// <returns>returns the k-th element of the array</returns>
        private static int QuickSelectRandom(this int[] arr, int k, int randSeed = -1)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr));
            if (k < 0 || k >= arr.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between 0 and arr.Length-1");
            Random rand = (randSeed < 0) ? new Random() : new Random(randSeed);
            QuickSelectRandom_loc(arr, 0, arr.Length - 1, k, rand);
            return arr[k];
        }

        private static void QuickSelectRandomGeneric_loc<T>(T[] arr, int l, int r, int k, Random rand, IComparer<T> comparer = null)
        {
            if (r <= l) return;
            int i = Partition(arr, l, r, rand, comparer);
            if (i > k) QuickSelectRandomGeneric_loc(arr, l, i - 1, k, rand, comparer);
            if (i < k) QuickSelectRandomGeneric_loc(arr, i + 1, r, k, rand, comparer);
        }

        private static void QuickSelectRandomGenericNonRecursive_loc<T>(T[] arr, int l, int r, int k, Random rand, IComparer<T> comparer = null)
        {
            while (r > l)
            {
                int i = Partition(arr, l, r, rand, comparer);
                if (i >= k) r = i - 1;
                if (i <= k) l = i + 1;
            }
        }

        private static void QuickSelectGenericNonRecursive_loc<T>(T[] arr, int l, int r, int k, IComparer<T> comparer = null)
        {
            while (r > l)
            {
                int i = Partition(arr, l, r, comparer);
                if (i >= k) r = i - 1;
                if (i <= k) l = i + 1;
            }
        }

        /// <summary>
        /// Generic QuickSelect algorithm. Takes an unsorted array of any type filled with distinct values and a comparer and returns the k-th element.
        /// Runs in O(n) time on average.
        /// Minimizes the chance of worst-case O(n^2) behavior by randomly choosing pivot element.
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
        private static T QuickSelectRandomGeneric<T>(this T[] arr, int l, int r, int k, int randSeed = -1, IComparer<T> comparer = null)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr));
            if (l < 0 || r >= arr.Length || l > r)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < l || k > r)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between l and r");
            Random rand = (randSeed < 0) ? new Random() : new Random(randSeed);
            QuickSelectRandomGenericNonRecursive_loc(arr, l, r, k, rand, comparer);
            return arr[k];
        }

        /// <summary>
        /// Generic QuickSelect algorithm. Takes an unsorted array of any type filled with distinct values and a comparer and returns the k-th element.
        /// Runs in O(n) time on average.
        /// Minimizes the chance of worst-case O(n^2) behavior by randomly choosing pivot element.
        /// The input array is modified during the selection process.
        /// </summary>
        /// <typeparam name="T">Array of type T</typeparam>
        /// <param name="arr">source array</param>
        /// <param name="k">index of the desired element to be selected</param>
        /// <param name="randSeed">Seed for the random number generator. Negative value makes random not-repeatable.</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns the k-th element of the array</returns>
        private static T QuickSelectRandomGeneric<T>(this T[] arr, int k, int randSeed = -1, IComparer<T> comparer = null)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr));
            if (k < 0 || k >= arr.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between 0 and arr.Length-1");
            Random rand = (randSeed < 0) ? new Random() : new Random(randSeed);
            QuickSelectRandomGenericNonRecursive_loc(arr, 0, arr.Length - 1, k, rand, comparer);
            return arr[k];
        }
        /// <summary>
        /// Generic QuickSelect algorithm. Takes an unsorted array of any type filled with distinct values and a comparer and returns the k-th element.
        /// Runs in O(n) time on average.
        /// Minimizes the chance of worst-case O(n^2) behavior by randomly shuffling the array before using it.
        /// The input array is modified during the selection process.
        /// </summary>
        /// <typeparam name="T">Array of type T</typeparam>
        /// <param name="arr">source array</param>
        /// <param name="l">left index of the subarray, inclusive</param>
        /// <param name="r">right index of the subarray, inclusive</param>
        /// <param name="k">index of the desired element to be selected</param>
        /// <param name="randSeed">Seed for the random number generator. Negative value makes random not-repeatable.</param>
        /// <param name="comparer">comparer used to compare two array elements of type T. Null uses teh default comparer.</param>
        /// <returns>returns the k-th element of the array</returns>
        private static T QuickSelectShuffleGeneric<T>(this T[] arr, int l, int r, int k, int randSeed = -1, IComparer<T> comparer = null)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr));
            if (l < 0 || r >= arr.Length || l > r)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < l || k > r)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between l and r");
            // Shuffle the array using Fisher-Yates algorithm
            Random rand = (randSeed < 0) ? new Random() : new Random(randSeed);
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            QuickSelectGenericNonRecursive_loc(arr, l, r, k, comparer);
            return arr[k];
        }

        /// <summary>
        /// Generic QuickSelect algorithm. Takes an unsorted array of any type filled with distinct values and a comparer and returns the k-th element.
        /// Runs in O(n) time on average.
        /// Minimizes the chance of worst-case O(n^2) behavior by randomly shuffling the array before using it.
        /// The input array is modified during the process.
        /// </summary>
        /// <typeparam name="T">Array of type T</typeparam>
        /// <param name="arr">source array</param>
        /// <param name="k">index of the desired element to be selected</param>
        /// <param name="randSeed">Seed for the random number generator. Negative value makes random not-repeatable.</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns the k-th element of the array</returns>
        private static T QuickSelectShuffleGeneric<T>(this T[] arr, int k, int randSeed = -1, IComparer<T> comparer = null)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr));
            if (k < 0 || k >= arr.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between 0 and arr.Length-1");
            // Shuffle the array using Fisher-Yates algorithm
            Random rand = (randSeed < 0) ? new Random() : new Random(randSeed);
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            QuickSelectGenericNonRecursive_loc(arr, 0, arr.Length - 1, k, comparer);
            return arr[k];
        }
    }
}
