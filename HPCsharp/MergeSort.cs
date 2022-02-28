// TODO: Implement support for Comparison functions as well as Comparer (eek - double the fun), as Standard C# libraries support both methods.
// TODO: See if implementing stable IEnumerable sorting is faster than LINQ sorting, since that's the only stable one.
// TODO: Compare performance of sorting arrays and lists of classes with .Sort and LINQ to see if the advantages are bigger or smaller
// TODO: Expose all of the thresholds for users to be able to conrol. These can just be a List of thresholds that match up with the list of algorithms - i.e. a pair of algorithm and threshold.
//       Thresholds could be allowed to be negative or zero disable the associated algorithm.
// TODO: Test whether Merge is also stable, so that we don't have to resort to DivideAndConquerMerge for stability and give up performance in the process.
// TODO: Wonder if getting rid off the comparer function would speed up Merge Sort substantially, because C# is calling this function per element with all of the
//       overhead of the function call. Yes, having a comparison function provides the flexibility of handling any data type and comparing any field within that
//       data type, as well as ascending/decending selection by the user. However, we could setup special cases for sorting arrays of common data types much faster
//       by eliminating the comparison function, or detecting when it's null and seeing if the resulting hard-coded merge implementation would be much faster.
// TODO: Create a hybrid of in-place MSD Radix Sort and in-place Merge Sort to see if the combined algorithm is faster than .Sort and MSD Radix Sort running
//       on a single core. Study different thresholds.
// TODO: Combine LSD Radix Sort with Priority Queue, where LSD Radix Sort is doing L2 cache size chunks.
// TODO: For parallel in-place Merge Sort where recursion levels are expensive, to minimize the number of recursions and maximize parallelism, if the array size
//       is large enough, to where the amount of work is bigger than the threshold set, set the threshold internally to array/numberOfCores to maximize
//       parallelism and minimize the number of recursion levels within the Merge portion of the algorithm. Figure out the optimal thing to do, by
//       measuring the threshold versus array size and the number of memory channels and number of cores.
// TODO: The above idea of minimizing recursions is great for creating a parallel Array.Sort(), which is in-place but lacks .AsParallel(), which this method
//       would provide. It is also generic and in-place, which is enormously useful (and already implemented). This definitely needs to be tested and optimized
//       on 14-core and 32-core CPUs.
// TODO: Fix inconsistent parallel threshold settings
// TODO: Fix List sorting that is currently hidden because it's not truly in-place. Either make it truly in-place or call it not-in-place
// TODO: See if the experimental hidden algorithm is worthwhile
// TODO: Use Selection Sort instead of Insertion Sort for faster bottom of the recursion tree.
// TODO: Use Heap Sort or Array.Sort for faster bottom of the recursion tree, especially for in-place versions.

using System;
using System.Collections.Generic;
using System.Xml.Schema;
using HPCsharp.ParallelAlgorithms;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        /// <summary>
        /// Arrays or Lists smaller than this value will use Insertion Sort
        /// </summary>
        public static Int32 SortMergeInsertionThreshold { get; set; } = 16;

        static internal void SortMergeInner<T>(this T[] src, int l, int r, T[] dst, bool srcToDst = true, IComparer<T> comparer = null)
        {
            if (r == l)
            {    // termination/base case of sorting a single element
                if (srcToDst) dst[l] = src[l];    // copy the single element from src to dst
                return;
            }
            else if ((r - l) < SortMergeInsertionThreshold)
            {
                HPCsharp.Algorithm.InsertionSort<T>(src, l, r - l + 1, comparer);  // want to do dstToSrc, can just do it in-place, just sort the src, no need to copy
                if (srcToDst)
                    for (int i = l; i <= r; i++) dst[i] = src[i];	// copy from src to dst, when the result needs to be in dst
                return;
            }

            int m = (r + l) / 2;
            int length1 = m - l + 1;
            int length2 = r - (m + 1) + 1;

            SortMergeInner(src, l,     m, dst, !srcToDst, comparer);		// reverse direction of srcToDst for the next level of recursion
            SortMergeInner(src, m + 1, r, dst, !srcToDst, comparer);

            if (srcToDst) Merge(src, l, length1, m + 1, length2, dst, l, comparer);
            else          Merge(dst, l, length1, m + 1, length2, src, l, comparer);
        }

        /// <summary>
        /// Take a segment of the src array, sort it using the Merge Sort algorithm, and then return just the sorted range
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="source">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns a sorted array of length specified</returns>
        static public T[] SortMerge<T>(this T[] source, int startIndex, int length, IComparer<T> comparer = null)
        {
            T[] srcTrimmed = new T[length];
            T[] dst = new T[length];

            Array.Copy(source, startIndex, srcTrimmed, 0, length);

            srcTrimmed.SortMergeInner<T>(0, length - 1, dst, true, comparer);

            return dst;
        }

        /// <summary>
        /// Take the source array, sort it using the Merge Sort algorithm, and return a sorted array of full length.
        /// Not in-place algorithm.
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="source">source array</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns a sorted array of full length</returns>
        static public T[] SortMerge<T>(this T[] source, IComparer<T> comparer = null)
        {
            T[] dst = new T[source.Length];

            source.SortMergeInner<T>(0, source.Length - 1, dst, true, comparer);

            return dst;
        }

        static internal void SortMergeFourWayInner<T>(this T[] src, int l, int r, T[] dst, bool srcToDst = true, IComparer<T> comparer = null)
        {
            if (r == l)
            {    // termination/base case of sorting a single element
                if (srcToDst) dst[l] = src[l];    // copy the single element from src to dst
                return;
            }
            else if ((r - l) < SortMergeInsertionThreshold)
            {
                HPCsharp.Algorithm.InsertionSort<T>(src, l, r - l + 1, comparer);  // want to do dstToSrc, can just do it in-place, just sort the src, no need to copy
                if (srcToDst)
                    for (int i = l; i <= r; i++) dst[i] = src[i];	// copy from src to dst, when the result needs to be in dst
                return;
            }

            int m1 = (l  + r)  / 2;
            int m0 = (l  + m1) / 2;
            int m2 = (m1 + r)  / 2;
            int length0 = m0 - l + 1;
            int length1 = m1 - (m0 + 1) + 1;
            int length2 = m2 - (m1 + 1) + 1;
            int length3 = r  - (m2 + 1) + 1;
            //Console.WriteLine("SortMergeFourWay: Length = {0} l = {1} r = {2} m0 = {3} m1 = {4} m2 = {5} length0 = {6} length1 = {7} length2 = {8} length3 = {9}", src.Length, l, r, m0, m1, m2, length0, length1, length2, length3);

            SortMergeFourWayInner(src, l,      m0, dst, !srcToDst, comparer);		// reverse direction of srcToDst for the next level of recursion
            SortMergeFourWayInner(src, m0 + 1, m1, dst, !srcToDst, comparer);
            SortMergeFourWayInner(src, m1 + 1, m2, dst, !srcToDst, comparer);
            SortMergeFourWayInner(src, m2 + 1, r,  dst, !srcToDst, comparer);

            if (srcToDst) MergeFourWay2(src, l, length0, m0 + 1, length1, m1 + 1, length2, m2 + 1, length3, dst, l, comparer);
            else          MergeFourWay2(dst, l, length0, m0 + 1, length1, m1 + 1, length2, m2 + 1, length3, src, l, comparer);
        }
        /// <summary>
        /// Take the source array, sort it using the Merge Sort algorithm, and return a sorted array of full length.
        /// Not in-place algorithm. Uses a 4-way split and 4-way merge.
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="source">source array</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns a sorted array of full length</returns>
        static public T[] SortMergeFourWay<T>(this T[] source, IComparer<T> comparer = null)
        {
            T[] dst = new T[source.Length];

            source.SortMergeFourWayInner<T>(0, source.Length - 1, dst, true, comparer);

            return dst;
        }

        private static void SortMergeInPlaceHybridInner<T>(T[] arr, int startIndex, int endIndex, IComparer<T> comparer = null, int threshold = 16 * 1024)
        {
            //Console.WriteLine("merge sort: start = {0}, length = {1}", startIndex, length);
            int length = endIndex - startIndex + 1;
            if (length <= 1) return;
            if (length <= threshold)
            {
                Array.Sort(arr, startIndex, length, comparer);  // using InsertionSort here is much slower, since recursion has to go down to 32 elements
                return;
            }
            int midIndex = (endIndex + startIndex) / 2;
            SortMergeInPlaceHybridInner(arr, startIndex,   midIndex, comparer, threshold);    // recursive call left  half
            SortMergeInPlaceHybridInner(arr, midIndex + 1, endIndex, comparer, threshold);    // recursive call right half
            MergeInPlaceDivideAndConquer(arr, startIndex, midIndex, endIndex, comparer);      // merge the results
        }

        private static void SortMergeInPlaceAdaptiveInner<T>(this T[] arr, int startIndex, int endIndex, IComparer<T> comparer = null, int threshold = 16 * 1024)
        {
            if (endIndex <= startIndex) return;
            int length = endIndex - startIndex + 1;
            if (length <= 1) return;
            if (length <= threshold)
            {
                Array.Sort(arr, startIndex, length, comparer);  // using InsertionSort here is much slower, since recursion has to go down to 32 elements
                return;
            }
            int midIndex = (endIndex + startIndex) / 2;
            SortMergeInPlaceAdaptiveInner(arr, startIndex,   midIndex, comparer);                   // recursive call left  half
            SortMergeInPlaceAdaptiveInner(arr, midIndex + 1, endIndex, comparer);                   // recursive call right half
            MergeInPlaceAdaptiveDivideAndConquer(arr, startIndex, midIndex, endIndex, comparer);    // merge the results
        }

        static public void SortMergeInPlaceUsingAdaptiveMerge<T>(this T[] arr, IComparer<T> comparer = null)
        {
            arr.SortMergeInPlaceAdaptiveInner<T>(0, arr.Length - 1, comparer);
        }

        /// <summary>
        /// Take a segment of the source array, and sort it in place using the Merge Sort algorithm
        /// This algorithm uses a not in-place verion when there is enough memory available, allocating an array of the same size as the input array.
        /// When there is not enough memory, a purely in-place merge sort is used, which is slower.
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="arr">source array</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        static public void SortMergeInPlaceAdaptive<T>(this T[] arr, IComparer<T> comparer = null, int thresholdInPlacePure = 16 * 1024)
        {
            try
            {
                T[] dst = new T[arr.Length];
                SortMergeInner(arr, 0, arr.Length - 1, dst, false, comparer);
            }
            catch (System.OutOfMemoryException)
            {
                SortMergeInPlaceHybridInner(arr, 0, arr.Length - 1, comparer, thresholdInPlacePure);
            }
        }

        /// <summary>
        /// Take a segment of the source array, and sort it in place using the Merge Sort algorithm
        /// This algorithm uses a not in-place verion when there is enough memory available, allocating an array of the same size as the input array.
        /// When there is not enough memory, a purely in-place merge sort is used, which is slower.
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="array">source array</param>
        /// <param name="startIndex">index within the array where sorting starts, inclusive</param>
        /// <param name="length">number of elements to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        static public void SortMergeInPlaceAdaptive<T>(this T[] array, int startIndex, int length, IComparer<T> comparer = null, int thresholdInPlacePure = 16 * 1024)
        {
            try
            {
                T[] dst = new T[array.Length];
                SortMergeInner(array, startIndex, startIndex + length - 1, dst, false, comparer);
            } catch (System.OutOfMemoryException) {
                SortMergeInPlaceHybridInner(array, startIndex, startIndex + length - 1, comparer, thresholdInPlacePure);
            }
        }

        /// <summary>
        /// Take the source array, and sort all of it in-place using the in-place version of Merge Sort algorithm
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="array">source and result array</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        static public void SortMergeInPlace<T>(this T[] array, IComparer<T> comparer = null, int threshold = 16 * 1024)
        {
            SortMergeInPlaceHybridInner(array, 0, array.Length - 1, comparer, threshold);
        }

        /// <summary>
        /// Take the source array, and sort all of it in-place using the in-place version of Merge Sort algorithm
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="array">source and result array</param>
        /// <param name="startIndex">index within the array where sorting starts, inclusive</param>
        /// <param name="length">number of elements to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        static public void SortMergeInPlace<T>(this T[] array, int startIndex, int length, IComparer<T> comparer = null, int threshold = 16 * 1024)
        {
            SortMergeInPlaceHybridInner(array, startIndex, startIndex + length - 1, comparer, threshold);
        }

        /// <summary>
        /// Merge Sort. Takes a range of the src List, sorts it, and then returns just the sorted range
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns an array of length specified</returns>
        static public List<T> SortMerge<T>(this List<T> src, int startIndex, int length, IComparer<T> comparer = null)
        {
            T[] srcTrimmed = src.ToArrayPar(startIndex, length);
            T[] dst = new T[srcTrimmed.Length];

            srcTrimmed.SortMergeInner<T>(0, length - 1, dst, true, comparer);

            return new List<T>(dst);
        }

        /// <summary>
        /// Merge Sort
        /// Allocates the resulting array and returns it.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="comparer">method to compare List elements</param>
        public static List<T> SortMerge<T>(this List<T> src, IComparer<T> comparer = null)
        {
#if true
            T[] srcCopy = src.ToArrayPar();
            SortMerge(srcCopy, comparer);
            List<T> dst = new List<T>(srcCopy);
            return dst;
#else
            if (dst == null || dst.Count != src.Count)
                dst = new List<T>(src);
            SortMergeParallel<T>(src, 0, src.Count - 1, dst, true, comparer);
#endif
        }

        /// <summary>
        /// Merge Sort. Takes a range of the src List, sorts it, and then returns just the sorted range
        /// </summary>
        /// <typeparam name="T">List of type T</typeparam>
        /// <param name="list">source/destination List</param>
        /// <param name="startIndex">index within the src List where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two List elements of type T</param>
        /// <returns>returns an array of length specified</returns>
        static private void SortMergeInPlace<T>(ref List<T> list, int startIndex, int length, IComparer<T> comparer = null)
        {
            //T[] srcCopy = list.ToArrayPar();
            //srcCopy.SortMergeInPlace(startIndex, length, dst, comparer);
            //list = new List<T>(srcCopy);
        }

        /// <summary>
        /// In-place Merge Sort
        /// Uses a not-in-place parallel merge sort implementation, allocating the same size array as the input array, releasing it when sorting has completed.
        /// </summary>
        /// <typeparam name="T">data type of each List element</typeparam>
        /// <param name="list">source/destination List</param>
        /// <param name="comparer">method to compare List elements</param>
        private static void SortMergeInPlace<T>(ref List<T> list, IComparer<T> comparer = null)
        {
#if true
            T[] srcCopy = list.ToArrayPar();
            SortMergeInPlace(srcCopy, comparer);
            list = new List<T>(srcCopy);
#else
            //Stopwatch stopwatch = new Stopwatch();
            //long frequency = Stopwatch.Frequency;
            //long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            //stopwatch.Restart();
            List<T> dst = new List<T>(src);     // 0.039 seconds for 16M element List
            //stopwatch.Stop();
            //double timeNewList = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            //Console.WriteLine("New List from another list {0:0.000} sec", timeNewList);

            SortMergeParallel<T>(src, 0, src.Count - 1, dst, false);
#endif
        }

        private static void MergeSortInPlace<T>(T[] arr, IComparer<T> comparer = null)
        {
            MergeSortInPlace<T>(arr, 0, arr.Length, comparer);
        }

        // Shows how simple a sequential algorithm is
        // Listing One
        private static void MergeSortInPlace<T>(T[] arr, int startIndex, int length, IComparer<T> comparer = null)
        {
            if (length <= 1) return;
            int endIndex = startIndex + length;
            int midIndex = ((endIndex + startIndex) / 2);
            MergeSortInPlace(arr, startIndex,   midIndex, comparer);                         // recursive call left  half
            MergeSortInPlace(arr, midIndex + 1, endIndex, comparer);                         // recursive call right half
            MergeInPlaceDivideAndConquer(arr, startIndex, midIndex, endIndex, comparer);     // merge the results
        }

        private static void MergeSortInPlaceHybrid2<T>(T[] arr, IComparer<T> comparer = null, int buffLength = 1024, int threshold = 32)
        {
            T[] buff = new T[buffLength];
            MergeSortInPlaceHybridInner2<T>(arr, 0, arr.Length - 1, buff, comparer, threshold);
        }

        private static void MergeSortInPlaceHybrid2<T>(T[] arr, int startIndex, int length, IComparer<T> comparer = null, int buffLength = 1024, int threshold = 32)
        {
            T[] buff = new T[buffLength];
            MergeSortInPlaceHybridInner2<T>(arr, startIndex, length - 1, buff, comparer, threshold);
        }
        // start and end indexes are inclusive
        private static void MergeSortInPlaceHybridInner2<T>(T[] arr, int startIndex, int endIndex, T[] buff, IComparer<T> comparer = null, int threshold = 32)
        {
            //Console.WriteLine("merge sort: start = {0}, length = {1}", startIndex, length);
            int length = endIndex - startIndex + 1;
            if (length <= 1) return;
            if (length <= threshold)
            {
                Algorithm.InsertionSort(arr, startIndex, length, comparer);
                return;
            }
            int midIndex = (endIndex + startIndex) / 2;
            MergeSortInPlaceHybridInner2(arr, startIndex,   midIndex, buff, comparer, threshold);       // recursive call left  half
            MergeSortInPlaceHybridInner2(arr, midIndex + 1, endIndex, buff, comparer, threshold);       // recursive call right half
            MergeInPlaceDivideAndConquerHybrid(arr, startIndex, midIndex, endIndex, buff, comparer);    // merge the results
        }
    }
}
