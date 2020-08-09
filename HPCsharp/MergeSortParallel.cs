// TODO: Need a Merge Sort Stable that is not in-place.
// TODO: Figure out a way to specify both stable as a method like LINQ does .Stable and .Parallel
// TODO: Expose all of the thresholds for users to be able to conrol
// TODO: Tune Merge Sort Stable threshold to my current laptop (as a good starting point)
// TODO: Implement Parallel Merge Sort in a more generic way where there are not just thresholds, but also selection of serial algorithms to choose from
//       to optimize depending on how well the serial algorithms perform on a particular hardware platform.
// TODO: For readme, compare Radix Sort with Linq Sort and C# .Sort using several array sizes to show that the delta grows as size grows
// TODO: Show the difference between Stable Merge Sort and Linq Sorting to compare apples to apples
// TODO: While doing the Binary Search looking for the split for divide-and-conquer portion, since we know the index of each element as we are working on them before
//       moving them, could we keep the position in mind as part of the comparison to break the ties during the comparison? Would that help?
// TODO: Make sure to document the fact that not-in-place merge and merge sort change the input array in the process of sorting.
// TODO: Focus not only on the worst case of random value arrays, but also on the best case of nearly sorted arrays to compete with TimSort, as Merge Sort does well in this case.
// TODO: Implement HeapSort to be able to create more hybrix variations.
// TODO: Try removing Insertion Sort, since ArraySort already implements it, and to also verify that Insertion Sort (with a copy) is helping performance in the worst case and best case.
// TODO: Implement a hybrid of Parallel Merge Sort and Radix Sort as base case, with the threshold of being completely inside the cache, to allow better random access pattern (inside cache)
//       as that should help Radix Sort.
// TODO: Add the ability to limit the recursion depth to limit parallelism, as is done in http://dzmitryhuba.blogspot.com/2010/10/parallel-merge-sort.html this may help control parallelism better
//       than Microsoft does and possibly limit oversubscription.
// TODO: For sort of two arrays (one keys and one items), a potentially faster method would be to have keys to not only hold a key at each location, but also
//       an index, and then sort these keys/indexes pairs. Once the key/index array has been sorted, do a single pass of moving the items into their final locations.
// TODO: Once in-place parallel merge sort is working, implement an adaptive parallel in-place merge sort algorithm, which tries to allocate memory to use not-in-place algorithm
//       first catches an out of memory exception and performs in-place merge sort algorithm in that case - same as C++ STL implementation idea, but parallel for merge sort.
// TODO: Develop several in-place parallel hybrid sort algorithms: in-place parallel merge using Array.Sort as the base case, in-place parallel merge using in-place MSD Radix Sort as
//       the base case with the base size such that it fits within L2 cache of the CPU.
// TODO: Determine which of the three BlockSwap algorithms uses the least amount of memory bandwidth, as this may be the most important
//       factor for allowing parallel scalability. Test each of these algorithms to compare parallel scalability.
// TODO: Check if offering a destination array as an argument instead of a return array would provide a performance benefit when re-using the destination array,
//       for not-in-place version. This idea may provide 25% performance improvement, as seen from benchmarks when memory allocator re-uses the array.
// TODO: See if threshold for Insertion Sort can be removed, since .Sort() already uses it and has its own threshold internally for it.
// TODO: Eliminate copy operation in the versions of Parallel Merge sort where the startIndex and length are specified, to reduce the memory footprint.
// TODO: Improve efficiency and memory usage size of the Adaptive Merge Sort for a sub-array by allocating the destination of the size of the sub-array, and for
//       the source sub-array and parallel copying the source. Determine when it is advantageous versus creating the destination array that is as big as the source array,
//       for the not-in-place part of the algorithm. It would mean two smaller allocations, where both need to succeed to be able to proceed.
// TODO: Figure out why in-place Merge Sort (pure) uses such a large parallel threshold. This seems like a really large threshold, to where currently parallel version will never happen

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using HPCsharp.ParallelAlgorithms;

namespace HPCsharp
{
    /// <summary>
    /// Parallel Algorithms operating on variety of containers, providing trade-off between abstraction and performance
    /// </summary>
    static public partial class ParallelAlgorithm
    {
        /// <summary>
        /// Arrays or Lists smaller than this value will not be copied using a parallel copy
        /// </summary>
        public static Int32 SortMergeParallelInsertionThreshold { get; set; } = 16;

        /// <summary>
        /// Parallel Merge Sort that is not-in-place. Also, not stable, since Array.Sort is not stable, and is used as the recursion base-case.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="l">left  index of the source array, inclusive</param>
        /// <param name="r">right index of the source array, inclusive</param>
        /// <param name="dst">destination array</param>
        /// <param name="srcToDst">true => destination array will hold the sorted array; false => source array will hold the sorted array</param>
        /// <param name="comparer">method to compare array elements</param>
        /// <param name ="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        private static void SortMergeInnerPar<T>(this T[] src, Int32 l, Int32 r, T[] dst, bool srcToDst = true, IComparer<T> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            if (r == l)
            {    // termination/base case of sorting a single element
                if (srcToDst) dst[l] = src[l];    // copy the single element from src to dst
                return;
            }
            // TODO: This threshold may not be needed as C# sort already does it
            if ((r - l) <= SortMergeParallelInsertionThreshold)
            {
                HPCsharp.Algorithm.InsertionSort<T>(src, l, r - l + 1, comparer);  // want to do dstToSrc, can just do it in-place, just sort the src, no need to copy
                if (srcToDst)
                    for (int i = l; i <= r; i++) dst[i] = src[i];	// copy from src to dst, when the result needs to be in dst
                return;
            }
            else if ((r - l) <= parallelThreshold)
            {
                Array.Sort<T>(src, l, r - l + 1, comparer);     // not a stable sort
                if (srcToDst)
                    for (int i = l; i <= r; i++) dst[i] = src[i];	// copy from src to dst, when the result needs to be in dst
                return;
            }
            int m = ((r + l) / 2);
            Parallel.Invoke(
                () => { SortMergeInnerPar<T>(src, l,     m, dst, !srcToDst, comparer, parallelThreshold); },      // reverse direction of srcToDst for the next level of recursion
                () => { SortMergeInnerPar<T>(src, m + 1, r, dst, !srcToDst, comparer, parallelThreshold); }
            );
            // reverse direction of srcToDst for the next level of recursion
            if (srcToDst) MergeInnerPar<T>(src, l, m, m + 1, r, dst, l, comparer);
            else          MergeInnerPar<T>(dst, l, m, m + 1, r, src, l, comparer);
        }

        /// <summary>
        /// Parallel Merge Sort that is not-in-place
        /// </summary>
        /// <typeparam name="T1">data type of each key element</typeparam>
        /// <param name="srcKeys">source array</param>
        /// <param name="l">left  index of the source array, inclusive</param>
        /// <param name="r">right index of the source array, inclusive</param>
        /// <param name="dstKeys">destination array</param>
        /// <param name="srcToDst">true => destination array will hold the sorted array; false => source array will hold the sorted array</param>
        /// <param name="comparer">method to compare array elements</param>
        private static void SortMergeInnerPar<T1, T2>(this T1[] srcKeys, T2[] srcItems, Int32 l, Int32 r, T1[] dstKeys, T2[] dstItems, bool srcToDst = true, IComparer<T1> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            //Console.WriteLine("merge sort: #1 " + l + " " + r);
            if (r == l)
            {    // termination/base case of sorting a single element
                if (srcToDst)
                {
                    dstKeys[ l] = srcKeys[ l];    // copy the single element from src to dst
                    dstItems[l] = srcItems[l];
                }
                return;
            }
            // TODO: This threshold may not be needed as C# sort already does it
            if ((r - l) <= SortMergeParallelInsertionThreshold)
            {
                HPCsharp.Algorithm.InsertionSort<T1, T2>(srcKeys, srcItems, l, r - l + 1, comparer);  // want to do dstToSrc, can just do it in-place, just sort the src, no need to copy
                if (srcToDst)
                    for (int i = l; i <= r; i++)
                    {
                        dstKeys[ i] = srcKeys[ i];                // copy from src to dst, when the result needs to be in dst
                        dstItems[i] = srcItems[i];
                    }
                return;
            }
            else if ((r - l) <= parallelThreshold)
            {
                Array.Sort<T1, T2>(srcKeys, srcItems, l, r - l + 1, comparer);            // not a stable sort
                if (srcToDst)
                    for (int i = l; i <= r; i++)
                    {
                        dstKeys[ i] = srcKeys[ i];    // copy from src to dst, when the result needs to be in dst
                        dstItems[i] = srcItems[i];
                    }
                return;
            }
            int m = ((r + l) / 2);
            Parallel.Invoke(
                () => { SortMergeInnerPar<T1, T2>(srcKeys, srcItems, l,     m, dstKeys, dstItems, !srcToDst, comparer, parallelThreshold); },      // reverse direction of srcToDst for the next level of recursion
                () => { SortMergeInnerPar<T1, T2>(srcKeys, srcItems, m + 1, r, dstKeys, dstItems, !srcToDst, comparer, parallelThreshold); }
            );
            // reverse direction of srcToDst for the next level of recursion
            if (srcToDst) MergeInnerPar<T1, T2>(srcKeys, srcItems, l, m, m + 1, r, dstKeys, dstItems, l, comparer);
            else          MergeInnerPar<T1, T2>(dstKeys, dstItems, l, m, m + 1, r, srcKeys, srcItems, l, comparer);
        }

        /// <summary>
        /// Parallel Merge Sort that is not-in-place and stable
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="l">left  index of the source array, inclusive</param>
        /// <param name="r">right index of the source array, inclusive</param>
        /// <param name="dst">destination array</param>
        /// <param name="srcToDst">true => destination array will hold the sorted array; false => source array will hold the sorted array</param>
        /// <param name="comparer">method to compare array elements</param>
        private static void SortMergeStableInnerPar<T>(this T[] src, Int32 l, Int32 r, T[] dst, bool srcToDst = true, IComparer<T> comparer = null, Int32 parallelThreshold = 8 * 1024)
        {
            if (r == l)
            {    // termination/base case of sorting a single element
                if (srcToDst) dst[l] = src[l];    // copy the single element from src to dst
                return;
            }
            if ((r - l) <= SortMergeParallelInsertionThreshold)
            {
                HPCsharp.Algorithm.InsertionSort<T>(src, l, r - l + 1, comparer);  // want to do dstToSrc, can just do it in-place, just sort the src, no need to copy
                if (srcToDst)
                    for (int i = l; i <= r; i++) dst[i] = src[i];	// copy from src to dst, when the result needs to be in dst
                return;
            }
            else if ((r - l) <= parallelThreshold)
            {
                HPCsharp.Algorithm.SortMergeInner<T>(src, l, r, dst, srcToDst, comparer);
                return;
            }
            int m = ((r + l) / 2);
            Parallel.Invoke(
                () => { SortMergeStableInnerPar<T>(src, l,     m, dst, !srcToDst, comparer, parallelThreshold); },      // reverse direction of srcToDst for the next level of recursion
                () => { SortMergeStableInnerPar<T>(src, m + 1, r, dst, !srcToDst, comparer, parallelThreshold); }
            );
            // reverse direction of srcToDst for the next level of recursion
            if (srcToDst) HPCsharp.Algorithm.Merge<T>(src, l, m - l + 1, m + 1, r - m, dst, l, comparer);
            else          HPCsharp.Algorithm.Merge<T>(dst, l, m - l + 1, m + 1, r - m, src, l, comparer);
        }

        /// <summary>
        /// Parallel Merge Sort. Allocates the resulting sorted array and returns it.
        /// Modifies the original source array.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="comparer">method to compare array elements</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        /// <returns>returns a newly allocated sorted array</returns>
        public static T[] SortMergePar<T>(this T[] src, IComparer<T> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            var dst = new T[src.Length];
            if ((parallelThreshold * Environment.ProcessorCount) < src.Length)
                parallelThreshold = src.Length / Environment.ProcessorCount;

            src.SortMergeInnerPar<T>(0, src.Length - 1, dst, true, comparer, parallelThreshold);
            return dst;
        }
        /// <summary>
        /// Parallel Merge Sort. Takes a range of the src array, sorts it, and then returns just the sorted range
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        /// <returns>returns a sorted array of length specified</returns>
        static public T[] SortMergePar<T>(this T[] src, int startIndex, int length, IComparer<T> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            T[] srcTrimmed = new T[length];
            T[] dst        = new T[length];
            if ((parallelThreshold * Environment.ProcessorCount) < src.Length)
                parallelThreshold = src.Length / Environment.ProcessorCount;

            Array.Copy(src, startIndex, srcTrimmed, 0, length);

            srcTrimmed.SortMergeInnerPar<T>(0, length - 1, dst, true, comparer, parallelThreshold);
            return dst;
        }
        /// <summary>
        /// Parallel Stable Merge Sort. Allocates the resulting sorted array and returns it.
        /// Modifies the original source array.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="comparer">method to compare array elements</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        /// <returns>returns a sorted array of length specified</returns>
        public static T[] SortMergeStablePar<T>(this T[] src, IComparer<T> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            var dst = new T[src.Length];
            if ((parallelThreshold * Environment.ProcessorCount) < src.Length)
                parallelThreshold = src.Length / Environment.ProcessorCount;

            src.SortMergeStableInnerPar<T>(0, src.Length - 1, dst, true, comparer, parallelThreshold);
            return dst;
        }
        /// <summary>
        /// Parallel Stable Merge Sort. Takes a range of the src array, sorts it, and then returns just the sorted range
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        /// <returns>returns an array of length specified</returns>
        static public T[] SortMergeStablePar<T>(this T[] src, int startIndex, int length, IComparer<T> comparer = null, Int32 parallelThreshold = 8 * 1024)
        {
            T[] srcTrimmed = new T[length];
            T[] dst        = new T[length];
            if ((parallelThreshold * Environment.ProcessorCount) < src.Length)
                parallelThreshold = src.Length / Environment.ProcessorCount;

            Array.Copy(src, startIndex, srcTrimmed, 0, length);

            srcTrimmed.SortMergeStableInnerPar<T>(0, length - 1, dst, true, comparer, parallelThreshold);
            return dst;
        }
        /// <summary>
        /// Adaptive in-place Parallel Merge Sort.
        /// If memory is available, allocates a temporary array of the same size as the src array, and uses a faster not-in-place Merge Sort.
        /// Otherwise, uses a purely in-place but slower Parallel Merge Sort.
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        static public void SortMergeInPlacePar<T>(this T[] src, IComparer<T> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            try
            {
                T[] dst = new T[src.Length];
                if ((parallelThreshold * Environment.ProcessorCount) < src.Length)
                    parallelThreshold = src.Length / Environment.ProcessorCount;

                src.SortMergeInnerPar<T>(0, src.Length - 1, dst, false, comparer, parallelThreshold);
            }
            catch (System.OutOfMemoryException)
            {
                src.SortMergeInPlacePureInnerPar<T>(0, src.Length - 1, comparer, parallelThreshold);
            }
        }
        /// <summary>
        /// Adaptive in-place Parallel Merge Sort. Takes a range of the src array, and sorts just that range.
        /// If memory is available, allocates a temporary array of the same size as the src array, and uses a faster not-in-place Merge Sort.
        /// Otherwise, uses a purely in-place but slower Parallel Merge Sort.
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        static public void SortMergeInPlacePar<T>(this T[] src, int startIndex, int length, IComparer<T> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            try
            {
                T[] dst = new T[src.Length];
                if ((parallelThreshold * Environment.ProcessorCount) < src.Length)
                    parallelThreshold = src.Length / Environment.ProcessorCount;

                src.SortMergeInnerPar<T>(startIndex, startIndex + length - 1, dst, false, comparer, parallelThreshold);
            }
            catch (System.OutOfMemoryException)
            {
                src.SortMergeInPlacePureInnerPar<T>(startIndex, startIndex + length - 1, comparer, parallelThreshold);
            }
        }
        /// <summary>
        /// Purely in-place Parallel Merge Sort, but not an in-place algorithm. Takes a range of the src array, and sorts just that range.
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        public static void SortMergeInPlacePurePar<T>(this T[] src, IComparer<T> comparer = null, int parallelThreshold = 16 * 1024)
        {
            SortMergeInPlacePureInnerPar<T>(src, 0, src.Length - 1, comparer, parallelThreshold);
        }
        /// <summary>
        /// Purely in-place Parallel Merge Sort, but not an in-place algorithm. Takes a range of the src array, and sorts just that range.
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        public static void SortMergeInPlacePurePar<T>(this T[] src, int startIndex, int length, IComparer<T> comparer = null, int parallelThreshold = 16 * 1024)
        {
            SortMergeInPlacePureInnerPar<T>(src, startIndex, startIndex + length - 1, comparer, parallelThreshold);
        }
        // start and end indexes are inclusive
        private static void SortMergeInPlacePureInnerPar<T>(this T[] src, int startIndex, int endIndex, IComparer<T> comparer = null, int threshold0 = 16 * 1024, int threshold1 = 256 * 1024, int threshold2 = 256 * 1024 )
        {
            //Console.WriteLine("merge sort: start = {0}, length = {1}", startIndex, length);
            int length = endIndex - startIndex + 1;
            if (length <= 1) return;
            if (length <= threshold0)
            {
                Array.Sort(src, startIndex, length, comparer);
                return;
            }
            int midIndex = (endIndex + startIndex) / 2;
            Parallel.Invoke(
                () => { SortMergeInPlacePureInnerPar<T>(src, startIndex,   midIndex, comparer, threshold0, threshold1, threshold2); },  // recursive call left  half
                () => { SortMergeInPlacePureInnerPar<T>(src, midIndex + 1, endIndex, comparer, threshold0, threshold1, threshold2); }   // recursive call right half
            );
            MergeDivideAndConquerInPlacePar(src, startIndex, midIndex, endIndex, comparer, threshold1, threshold2);     // merge the results
        }
        /// <summary>
        /// In-place interface Parallel Merge Sort of array of keys and an array of items, but not an in-place algorithm.
        /// Allocates a temporary array of the same size as the keys array and another as the items array.
        /// </summary>
        /// <typeparam name="T1">data type of each array element</typeparam>
        /// <typeparam name="T2">type for items array</typeparam>
        /// <param name="keys">source/destination array</param>
        /// <param name="items">array of items to be sorted by keys</param>
        /// <param name="comparer">method to compare keys</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        public static void SortMergeInPlacePar<T1, T2>(this T1[] keys, T2[] items, IComparer<T1> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            T1[] dstKeys  = new T1[ keys.Length];
            T2[] dstItems = new T2[items.Length];
            if ((parallelThreshold * Environment.ProcessorCount) < items.Length)
                parallelThreshold = items.Length / Environment.ProcessorCount;

            SortMergeInnerPar<T1, T2>(keys, items, 0, keys.Length - 1, dstKeys, dstItems, false, comparer, parallelThreshold);
        }
        /// <summary>
        /// In-place interface Parallel Merge Sort. Takes a range of the src array, and sorts just that range.
        /// Allocates a temporary array of the same size as the src array for keys and items.
        /// </summary>
        /// <typeparam name="T1">type for keys array</typeparam>
        /// <typeparam name="T2">type for items array</typeparam>
        /// <param name="keys">array of keys used to sort by</param>
        /// <param name="items">array of items to be sorted by keys</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        static public void SortMergeInPlacePar<T1, T2>(this T1[] keys, T2[] items, int startIndex, int length, IComparer<T1> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            T1[] dstKeys  = new T1[ keys.Length];
            T2[] dstItems = new T2[items.Length];
            if ((parallelThreshold * Environment.ProcessorCount) < items.Length)
                parallelThreshold = items.Length / Environment.ProcessorCount;

            SortMergeInnerPar<T1, T2>(keys, items, startIndex, startIndex + length - 1, dstKeys, dstItems, false, comparer, parallelThreshold);
        }
        /// <summary>
        /// In-place interface Parallel Merge Sort (stable).
        /// Allocates a temporary array of the same size as the src array.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="array">source/destination array</param>
        /// <param name="comparer">method to compare array elements</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        public static void SortMergeInPlaceStablePar<T>(this T[] array, IComparer<T> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            T[] dst = new T[array.Length];
            if ((parallelThreshold * Environment.ProcessorCount) < array.Length)
                parallelThreshold = array.Length / Environment.ProcessorCount;

            SortMergeStableInnerPar<T>(array, 0, array.Length - 1, dst, false, comparer, parallelThreshold);
        }
        /// <summary>
        /// In-place interface Parallel Merge Sort (stable). Takes a range of the src array, and sorts just that range.
        /// Allocates a temporary array of the same size as the src array for keys and items.
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="array">source/destination array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        static public void SortMergeInPlaceStablePar<T>(this T[] array, int startIndex, int length, IComparer<T> comparer = null, Int32 parallelThreshold = 8 * 1024)
        {
            T[] dst = new T[array.Length];
            if ((parallelThreshold * Environment.ProcessorCount) < array.Length)
                parallelThreshold = array.Length / Environment.ProcessorCount;

            array.SortMergeStableInnerPar<T>(startIndex, startIndex + length - 1, dst, false, comparer, parallelThreshold);
        }
        /// <summary>
        /// Parallel Merge Sort. Not in-place algorithm.
        /// Allocates the resulting array and returns it.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="comparer">method to compare List elements</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        public static List<T> SortMergePar<T>(this List<T> src, IComparer<T> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            T[] srcCopy = src.ToArrayPar();
            if ((parallelThreshold * Environment.ProcessorCount) < src.Count)
                parallelThreshold = src.Count / Environment.ProcessorCount;

            SortMergePar(srcCopy, comparer, parallelThreshold);
            List<T> dst = new List<T>(srcCopy);
            return dst;
        }
        /// <summary>
        /// Parallel Merge Sort (stable)
        /// Allocates the resulting array and returns it.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="comparer">method to compare List elements</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        /// <returns>returns a List of same length as the source</returns>
        public static List<T> SortMergeStablePar<T>(this List<T> src, IComparer<T> comparer = null, Int32 parallelThreshold = 8 * 1024)
        {
            T[] srcCopy = src.ToArrayPar();
            if ((parallelThreshold * Environment.ProcessorCount) < src.Count)
                parallelThreshold = src.Count / Environment.ProcessorCount;

            SortMergeStablePar(srcCopy, comparer, parallelThreshold);
            List<T> dst = new List<T>(srcCopy);
            return dst;
        }
        /// <summary>
        /// Parallel Merge Sort. Takes a range of the src List, sorts it, and then returns just the sorted range.
        /// Not in-place algorithm
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        /// <returns>returns an array of the legth specified</returns>
        static public List<T> SortMergePar<T>(this List<T> src, int startIndex, int length, IComparer<T> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            T[] srcTrimmed = src.ToArrayPar(startIndex, length);
            T[] dst        = new T[srcTrimmed.Length];
            if ((parallelThreshold * Environment.ProcessorCount) < src.Count)
                parallelThreshold = src.Count / Environment.ProcessorCount;

            srcTrimmed.SortMergeInnerPar<T>(0, length - 1, dst, true, comparer, parallelThreshold);

            return new List<T>(dst);
        }
        /// <summary>
        /// Parallel Merge Sort (stable). Takes a range of the src List, sorts it, and then returns just the sorted range
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        /// <returns>returns an array of length specified</returns>
        static public List<T> SortMergeStablePar<T>(this List<T> src, int startIndex, int length, IComparer<T> comparer = null, Int32 parallelThreshold = 8 * 1024)
        {
            T[] srcTrimmed = src.ToArrayPar(startIndex, length);
            T[] dst = new T[srcTrimmed.Length];
            if ((parallelThreshold * Environment.ProcessorCount) < src.Count)
                parallelThreshold = src.Count / Environment.ProcessorCount;

            srcTrimmed.SortMergeStableInnerPar<T>(0, length - 1, dst, true, comparer, parallelThreshold);

            return new List<T>(dst);
        }
        /// <summary>
        /// Parallel Merge Sort. Takes a range of the src List, sorts it, and then returns just the sorted range
        /// </summary>
        /// <typeparam name="T">List of type T</typeparam>
        /// <param name="list">source/destination List</param>
        /// <param name="startIndex">index within the src List where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two List elements of type T</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        /// <returns>returns an array of length specified</returns>
        static public void SortMergeInPlacePar<T>(ref List<T> list, int startIndex, int length, IComparer<T> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            T[] srcCopy = list.ToArrayPar();
            if ((parallelThreshold * Environment.ProcessorCount) < list.Count)
                parallelThreshold = list.Count / Environment.ProcessorCount;

            srcCopy.SortMergeInPlacePar(startIndex, length, comparer, parallelThreshold);
            list = new List<T>(srcCopy);
        }
        /// <summary>
        /// Parallel Merge Sort (stable). Takes a range of the src List, sorts it, and then returns just the sorted range
        /// </summary>
        /// <typeparam name="T">List of type T</typeparam>
        /// <param name="list">source/destination List</param>
        /// <param name="startIndex">index within the src List where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two List elements of type T</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        /// <returns>returns an array of length specified</returns>
        static public void SortMergeInPlaceStablePar<T>(ref List<T> list, int startIndex, int length, IComparer<T> comparer = null, Int32 parallelThreshold = 8 * 1024)
        {
            T[] srcCopy = list.ToArrayPar();
            if ((parallelThreshold * Environment.ProcessorCount) < list.Count)
                parallelThreshold = list.Count / Environment.ProcessorCount;

            srcCopy.SortMergeInPlaceStablePar(startIndex, length, comparer, parallelThreshold);
            list = new List<T>(srcCopy);
        }
        /// <summary>
        /// In-place Parallel Merge Sort
        /// Uses a not-in-place parallel merge sort implementation, allocating the same size array as the input array, releasing it when sorting has completed.
        /// </summary>
        /// <typeparam name="T">data type of each List element</typeparam>
        /// <param name="list">source/destination List</param>
        /// <param name="comparer">method to compare List elements</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        public static void SortMergeInPlacePar<T>(ref List<T> list, IComparer<T> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            T[] srcCopy = list.ToArrayPar();
            if ((parallelThreshold * Environment.ProcessorCount) < list.Count)
                parallelThreshold = list.Count / Environment.ProcessorCount;

            SortMergeInPlacePar(srcCopy, comparer, parallelThreshold);
            list = new List<T>(srcCopy);
        }
        /// <summary>
        /// In-place Parallel Merge Sort (stable)
        /// Uses a not-in-place parallel merge sort implementation, allocating the same size array as the input array, releasing it when sorting has completed.
        /// </summary>
        /// <typeparam name="T">data type of each List element</typeparam>
        /// <param name="list">source/destination List</param>
        /// <param name="comparer">method to compare List elements</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        public static void SortMergeInPlaceStablePar<T>(ref List<T> list, IComparer<T> comparer = null, Int32 parallelThreshold = 8 * 1024)
        {
            T[] srcCopy = list.ToArrayPar();
            if ((parallelThreshold * Environment.ProcessorCount) < list.Count)
                parallelThreshold = list.Count / Environment.ProcessorCount;

            SortMergeInPlaceStablePar(srcCopy, comparer);
            list = new List<T>(srcCopy);
        }

        private static void SortMergeHybridWithRadixInnerPar<T>(this T[] src, Int32 l, Int32 r, T[] dst, bool srcToDst, Func<T, UInt32> getKey, IComparer<T> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            if (r == l)
            {    // termination/base case of sorting a single element
                if (srcToDst) dst[l] = src[l];    // copy the single element from src to dst
                return;
            }
            if ((r - l) <= SortMergeParallelInsertionThreshold)
            {
                HPCsharp.Algorithm.InsertionSort<T>(src, l, r - l + 1, comparer);  // want to do dstToSrc, can just do it in-place, just sort the src, no need to copy
                if (srcToDst)
                    for (int i = l; i <= r; i++) dst[i] = src[i];	// copy from src to dst, when the result needs to be in dst
                return;
            }
            //else if ((r - l) <= parallelThreshold)
            //{
            //    Array.Sort<T>(src, l, r - l + 1, comparer);     // not a stable sort
            //    if (srcToDst)
            //        for (int i = l; i <= r; i++) dst[i] = src[i];	// copy from src to dst, when the result needs to be in dst
            //    return;
            //}
            else if (((r - l) <= parallelThreshold) && srcToDst)
            {
                // TODO: this version of the algorithm isn't the fastest two phase version
                HPCsharp.Algorithm.SortRadix<T>(src, l, r - l + 1, dst, getKey);
                //HPCsharp.Algorithm.SortRadixDerandomizedWrites<T>(src, l, r - l + 1, dst, getKey);
                return;
            }
            int m = ((r + l) / 2);
            Parallel.Invoke(
                () => { SortMergeHybridWithRadixInnerPar<T>(src, l,     m, dst, !srcToDst, getKey, comparer, parallelThreshold); },      // reverse direction of srcToDst for the next level of recursion
                () => { SortMergeHybridWithRadixInnerPar<T>(src, m + 1, r, dst, !srcToDst, getKey, comparer, parallelThreshold); }
            );
            if (srcToDst) MergeInnerPar<T>(dst, l, m, m + 1, r, src, l, comparer);
            else          MergeInnerPar<T>(src, l, m, m + 1, r, dst, l, comparer);
        }

        private static void SortMergeHybridWithRadixInnerPar(this UInt32[] src, Int32 l, Int32 r, UInt32[] dst, bool srcToDst, Int32 parallelThreshold = 24 * 1024)
        {
            if (r == l)
            {                       // termination/base case of sorting a single element
                if (srcToDst)
                    dst[l] = src[l];    // copy the single element from src to dst
                return;
            }
            if ((r - l) <= SortMergeParallelInsertionThreshold)
            {
                HPCsharp.Algorithm.InsertionSort(src, l, r - l + 1);  // want to do dstToSrc, can just do it in-place, just sort the src, no need to copy
                if (srcToDst)
                    for (int i = l; i <= r; i++) dst[i] = src[i];
                return;
            }
            else if ((r - l) <= parallelThreshold)
            {
                // TODO: this version of the algorithm isn't the fastest two phase version
                HPCsharp.Algorithm.SortRadix(src, l, r - l + 1, dst);
                if (srcToDst)
                    for (int i = l; i <= r; i++) dst[i] = src[i];
                //HPCsharp.Algorithm.SortRadix(src, l, r - l + 1, dst);
                //HPCsharp.Algorithm.SortRadixDerandomizedWrites<T>(src, l, r - l + 1, dst, getKey);
                return;
            }
            int m = ((r + l) / 2);
            Parallel.Invoke(
                () => { SortMergeHybridWithRadixInnerPar(src, l,     m, dst, !srcToDst, parallelThreshold); },
                () => { SortMergeHybridWithRadixInnerPar(src, m + 1, r, dst, !srcToDst, parallelThreshold); }
            );
            if (srcToDst) MergeInnerPar(src, l, m, m + 1, r, dst, l);
            else          MergeInnerPar(dst, l, m, m + 1, r, src, l);
        }

        private static void SortMergeHybridWithRadixInnerPar<T>(this T[] src, Int32 l, Int32 r, T[] dst, bool srcToDst, IComparer<T> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            if (r == l)
            {    // termination/base case of sorting a single element
                if (srcToDst) dst[l] = src[l];    // copy the single element from src to dst
                return;
            }
            // TODO: This threshold may not be needed as C# sort already does it
            if ((r - l) <= SortMergeParallelInsertionThreshold)
            {
                HPCsharp.Algorithm.InsertionSort<T>(src, l, r - l + 1, comparer);  // want to do dstToSrc, can just do it in-place, just sort the src, no need to copy
                if (srcToDst)
                    for (int i = l; i <= r; i++) dst[i] = src[i];	// copy from src to dst, when the result needs to be in dst
                return;
            }
            //else if ((r - l) <= SortMergeParallelThreshold)
            //{
            //    Array.Sort<T>(src, l, r - l + 1, comparer);     // not a stable sort
            //    if (srcToDst)
            //        for (int i = l; i <= r; i++) dst[i] = src[i];	// copy from src to dst, when the result needs to be in dst
            //    return;
            //}
            else if ((r - l) <= parallelThreshold)
            {
                // TODO: Need a version that doesn't require a getKey function
                //HPCsharp.Algorithm.SortRadixNew<T>(src, l, r - l + 1, dst);
            }
            int m = ((r + l) / 2);
            Parallel.Invoke(
                () => { SortMergeHybridWithRadixInnerPar<T>(src, l,     m, dst, !srcToDst, comparer, parallelThreshold); },      // reverse direction of srcToDst for the next level of recursion
                () => { SortMergeHybridWithRadixInnerPar<T>(src, m + 1, r, dst, !srcToDst, comparer, parallelThreshold); }
            );
            // reverse direction of srcToDst for the next level of recursion
            if (srcToDst) MergeInnerPar<T>(src, l, m, m + 1, r, dst, l, comparer);
            else          MergeInnerPar<T>(dst, l, m, m + 1, r, src, l, comparer);
        }
        /// <summary>
        /// Parallel Merge Sort, which uses Radix Sort as the recursion base-case.
        /// Not in-place algorithm, but the interface is in-place.
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        /// <returns>returns an array of length specified</returns>
        public static void SortMergeHybridWithRadixPar<T>(this T[] src, Func<T, UInt32> getKey, IComparer<T> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            T[] workBuffer = new T[src.Length];
            //if ((parallelThreshold * Environment.ProcessorCount) < src.Length)
            //    parallelThreshold = src.Length / Environment.ProcessorCount;

// TODO: Having srcToDst = true here seems wrong as we are not returning the dst, but the src array instead
            src.SortMergeHybridWithRadixInnerPar<T>(0, src.Length - 1, workBuffer, true, getKey, comparer, parallelThreshold);
        }
        /// <summary>
        /// Parallel Merge Sort. Takes a range of the src array, sorts it, and then returns just the sorted range.
        /// Not in-place algorithm.
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        /// <returns>returns an array of length specified</returns>
        public static T[] SortMergeHybridWithRadixPar<T>(this T[] src, int startIndex, int length, Func<T, UInt32> getKey, IComparer<T> comparer = null, Int32 parallelThreshold = 24 * 1024)
        {
            T[] srcTrimmed = new T[length];
            T[] dst        = new T[length];
            if ((parallelThreshold * Environment.ProcessorCount) < src.Length)
                parallelThreshold = src.Length / Environment.ProcessorCount;

            Array.Copy(src, startIndex, srcTrimmed, 0, length);

            srcTrimmed.SortMergeHybridWithRadixInnerPar<T>(0, length - 1, dst, true, getKey, comparer, parallelThreshold);

            return srcTrimmed;
        }
        /// <summary>
        /// Parallel Merge Sort, which uses Radix Sort as the recursion base-case.
        /// Not in-place algorithm, but the interface is in-place.
        /// </summary>
        /// <param name="src">source array</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        /// <returns>returns an array of length specified</returns>
        public static void SortMergeHybridWithRadixPar(this UInt32[] src, Int32 parallelThreshold = 24 * 1024)
        {
            UInt32[] dst = new UInt32[src.Length];
            //if ((parallelThreshold * Environment.ProcessorCount) < src.Length)
            //    parallelThreshold = src.Length / Environment.ProcessorCount;

            src.SortMergeHybridWithRadixInnerPar(0, src.Length - 1, dst, false, parallelThreshold);
        }

        private static void SortMergeHybridWithRadixInplaceInnerPar(this UInt64[] src, Int32 l, Int32 r,
            Int32 threshold0 = 24 * 1024, Int32 threshold1 = 16 * 1024, Int32 threshold2 = 16 * 1024 )
        {
            if (r == l)
                return;
            else if ((r - l) <= threshold0)
            {
                Algorithm.SortRadixMsd(src, l, r - l + 1);
                return;
            }
            int m = ((r + l) / 2);
            Parallel.Invoke(
                () => { SortMergeHybridWithRadixInplaceInnerPar(src, l,     m, threshold0, threshold1, threshold2); },      // reverse direction of srcToDst for the next level of recursion
                () => { SortMergeHybridWithRadixInplaceInnerPar(src, m + 1, r, threshold0, threshold1, threshold2); }
            );
            MergeDivideAndConquerInPlacePar<ulong>(src, l, m, r, null, threshold1, threshold2);
        }
        /// <summary>
        /// Parallel Merge Sort, which uses Radix Sort as the recursion base-case.
        /// Truly in-place algorithm.
        /// </summary>
        /// <param name="src">source array</param>
        /// <param name="threshold0">arrays larger than this value will be sorted using multiple cores</param>
        /// <returns>returns an array of length specified</returns>
        public static void SortMergeHybridWithRadixInplacePar(this ulong[] src, Int32 threshold0 = 24 * 1024, Int32 threshold1 = 16 * 1024, Int32 threshold2 = 16 * 1024)
        {
            //if ((parallelThreshold * Environment.ProcessorCount) < src.Length)
            //    parallelThreshold = src.Length / Environment.ProcessorCount;

            src.SortMergeHybridWithRadixInplaceInnerPar(0, src.Length - 1, threshold0, threshold1, threshold2);
        }
    }
}
