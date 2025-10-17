// TODO: Consider implementing shuffle and randomized pivot selection to avoid worst-case scenarios even further.
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
        /// The input array is modified during the process.
        /// </summary>
        /// <param name="arr">source array</param>
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
        /// The input array is modified during the process.
        /// </summary>
        /// <param name="arr">source array</param>
        /// <param name="l">left index of the subarray, inclusive</param>
        /// <param name="r">right index of the subarray, inclusive</param>
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
        /// The input array is modified during the process.
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
        /// The input array is modified during the process.
        /// </summary>
        /// <param name="arr">source array</param>
        /// <param name="l">left index of the subarray, inclusive</param>
        /// <param name="r">right index of the subarray, inclusive</param>
        /// <param name="rand">random number generator.</param>
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

        private static void QuickSelectRandom_loc(int[] arr, int l, int r, int k, Random rand)
        {
            if (r <= l) return;
            int i = Partition(arr, l, r, rand);
            if (i > k) QuickSelectRandom_loc(arr, l, i - 1, k, rand);
            if (i < k) QuickSelectRandom_loc(arr, i + 1, r, k, rand);
        }

        /// <summary>
        /// QuickSelect algorithm. Takes an unsorted array of integers and returns the k-th element. Runs in O(n) time on average.
        /// Minimizes the chance of worst-case O(n^2) behavior by randomly choosing pivot element.
        /// The input array is modified during the process.
        /// </summary>
        /// <param name="arr">source array</param>
        /// <param name="l">left index of the subarray, inclusive</param>
        /// <param name="r">right index of the subarray, inclusive</param>
        /// <param name="k">index of the desired element</param>
        /// <param name="randSeed">Seed for the random number generator. Negative value makes random not-repeatable.</param>
        /// <returns>returns the k-th element of the array</returns>
        public static int QuickSelectRandom(this int[] arr, int l, int r, int k, int randSeed = -1)
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
        /// QuickSelect algorithm. Takes an unsorted array of integers and returns the k-th element. Runs in O(n) time on average.
        /// Minimizes the chance of worst-case O(n^2) behavior by randomly choosing pivot element.
        /// The input array is modified during the process.
        /// </summary>
        /// <param name="arr">source array</param>
        /// <param name="k">index of the desired element</param>
        /// <param name="randSeed">Seed for the random number generator. Negative value makes random not-repeatable.</param>
        /// <returns>returns the k-th element of the array</returns>
        public static int QuickSelectRandom(this int[] arr, int k, int randSeed = -1)
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
        /// Generic QuickSelect algorithm. Takes an unsorted array of any type with a comparer and returns the k-th element. Runs in O(n) time on average.
        /// Minimizes the chance of worst-case O(n^2) behavior by randomly choosing pivot element.
        /// The input array is modified during the process.
        /// </summary>
        /// <typeparam name="T">Array of type T</typeparam>
        /// <param name="arr">source array</param>
        /// <param name="l">left index of the subarray, inclusive</param>
        /// <param name="r">right index of the subarray, inclusive</param>
        /// <param name="k">index of the desired element</param>
        /// <param name="randSeed">Seed for the random number generator. Negative value makes random not-repeatable.</param>
        /// <param name="comparer">comparer used to compare two array elements of type T. Null uses teh default comparer.</param>
        /// <returns>returns the k-th element of the array</returns>
        public static T QuickSelectRandomGeneric<T>(this T[] arr, int l, int r, int k, int randSeed = -1, IComparer<T> comparer = null)
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
        /// Generic QuickSelect algorithm. Takes an unsorted array of any type with a comparer and returns the k-th element. Runs in O(n) time on average.
        /// Minimizes the chance of worst-case O(n^2) behavior by randomly choosing pivot element.
        /// The input array is modified during the process.
        /// </summary>
        /// <typeparam name="T">Array of type T</typeparam>
        /// <param name="arr">source array</param>
        /// <param name="k">index of the desired element</param>
        /// <param name="randSeed">Seed for the random number generator. Negative value makes random not-repeatable.</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns the k-th element of the array</returns>
        public static T QuickSelectRandomGeneric<T>(this T[] arr, int k, int randSeed = -1, IComparer<T> comparer = null)
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
        /// Generic QuickSelect algorithm. Takes an unsorted array of any type with a comparer and returns the k-th element. Runs in O(n) time on average.
        /// Minimizes the chance of worst-case O(n^2) behavior by randomly shuffling the array before using it.
        /// The input array is modified during the process.
        /// </summary>
        /// <typeparam name="T">Array of type T</typeparam>
        /// <param name="arr">source array</param>
        /// <param name="l">left index of the subarray, inclusive</param>
        /// <param name="r">right index of the subarray, inclusive</param>
        /// <param name="k">index of the desired element</param>
        /// <param name="randSeed">Seed for the random number generator. Negative value makes random not-repeatable.</param>
        /// <param name="comparer">comparer used to compare two array elements of type T. Null uses teh default comparer.</param>
        /// <returns>returns the k-th element of the array</returns>
        public static T QuickSelectShuffleGeneric<T>(this T[] arr, int l, int r, int k, int randSeed = -1, IComparer<T> comparer = null)
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
        /// Generic QuickSelect algorithm. Takes an unsorted array of any type with a comparer and returns the k-th element. Runs in O(n) time on average.
        /// Minimizes the chance of worst-case O(n^2) behavior by randomly shuffling the array before using it.
        /// The input array is modified during the process.
        /// </summary>
        /// <typeparam name="T">Array of type T</typeparam>
        /// <param name="arr">source array</param>
        /// <param name="k">index of the desired element</param>
        /// <param name="randSeed">Seed for the random number generator. Negative value makes random not-repeatable.</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns the k-th element of the array</returns>
        public static T QuickSelectShuffleGeneric<T>(this T[] arr, int k, int randSeed = -1, IComparer<T> comparer = null)
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


