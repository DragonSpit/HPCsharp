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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

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
        public static Int32 SortMergeParallelThreshold { get; set; } = 24 * 1024;
        /// <summary>
        /// Arrays or Lists smaller than this value will not be copied using a parallel copy
        /// </summary>
        public static Int32 SortMergeStableParallelThreshold { get; set; } = 8 * 1024;
        /// <summary>
        /// Arrays or Lists smaller than this value will not be copied using a parallel copy
        /// </summary>
        public static Int32 SortMergeParallelInsertionThreshold { get; set; } = 16;
        /// <summary>
        /// Parallel Merge Sort that is not-in-place
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="l">left  index of the source array, inclusive</param>
        /// <param name="r">right index of the source array, inclusive</param>
        /// <param name="dst">destination array</param>
        /// <param name="srcToDst">true => destination array will hold the sorted array; false => source array will hold the sorted array</param>
        /// <param name="comparer">method to compare array elements</param>
        private static void SortMergeInnerPar<T>(this T[] src, Int32 l, Int32 r, T[] dst, bool srcToDst = true, IComparer<T> comparer = null)
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
            else if ((r - l) <= SortMergeParallelThreshold)
            {
                Array.Sort<T>(src, l, r - l + 1, comparer);     // not a stable sort
                if (srcToDst)
                    for (int i = l; i <= r; i++) dst[i] = src[i];	// copy from src to dst, when the result needs to be in dst
                return;
            }
            int m = ((r + l) / 2);
            Parallel.Invoke(
                () => { SortMergeInnerPar<T>(src, l,     m, dst, !srcToDst, comparer); },      // reverse direction of srcToDst for the next level of recursion
                () => { SortMergeInnerPar<T>(src, m + 1, r, dst, !srcToDst, comparer); }
            );
            // reverse direction of srcToDst for the next level of recursion
            if (srcToDst) MergeInnerPar<T>(src, l, m, m + 1, r, dst, l, comparer);
            else          MergeInnerPar<T>(dst, l, m, m + 1, r, src, l, comparer);
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
        private static void SortMergeStableInnerPar<T>(this T[] src, Int32 l, Int32 r, T[] dst, bool srcToDst = true, IComparer<T> comparer = null)
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
            else if ((r - l) <= SortMergeStableParallelThreshold)
            {
                HPCsharp.Algorithm.SortMergeInner<T>(src, l, r, dst, srcToDst, comparer);
                return;
            }
            int m = ((r + l) / 2);
            Parallel.Invoke(
                () => { SortMergeStableInnerPar<T>(src, l,     m, dst, !srcToDst, comparer); },      // reverse direction of srcToDst for the next level of recursion
                () => { SortMergeStableInnerPar<T>(src, m + 1, r, dst, !srcToDst, comparer); }
            );
            // reverse direction of srcToDst for the next level of recursion
            if (srcToDst) HPCsharp.Algorithm.Merge<T>(src, l, m - l + 1, m + 1, r - m, dst, l, comparer);
            else          HPCsharp.Algorithm.Merge<T>(dst, l, m - l + 1, m + 1, r - m, src, l, comparer);
        }

        /// <summary>
        /// Parallel Merge Sort. Takes a range of the src array, sorts it, and then returns just the sorted range
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns an array of length specified</returns>
        static public T[] SortMergePar<T>(this T[] src, int startIndex, int length, IComparer<T> comparer = null)
        {
            T[] srcTrimmed = new T[length];
            T[] dst        = new T[length];

            Array.Copy(src, startIndex, srcTrimmed, 0, length);

            srcTrimmed.SortMergeInnerPar<T>(0, length - 1, dst, true, comparer);

            return dst;
        }
        /// <summary>
        /// Parallel Merge Sort (stable). Takes a range of the src array, sorts it, and then returns just the sorted range
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns an array of length specified</returns>
        static public T[] SortMergeStablePar<T>(this T[] src, int startIndex, int length, IComparer<T> comparer = null)
        {
            T[] srcTrimmed = new T[length];
            T[] dst = new T[length];

            Array.Copy(src, startIndex, srcTrimmed, 0, length);

            srcTrimmed.SortMergeStableInnerPar<T>(0, length - 1, dst, true, comparer);

            return dst;
        }
        /// <summary>
        /// Parallel Merge Sort. Allocates the resulting sorted array and returns it.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="comparer">method to compare array elements</param>
        public static T[] SortMergePar<T>(this T[] src, IComparer<T> comparer = null)
        {
            var dst = new T[src.Length];
            src.SortMergeInnerPar<T>(0, src.Length - 1, dst, true, comparer);
            return dst;
        }
        /// <summary>
        /// Parallel Merge Sort (stable). Allocates the resulting sorted array and returns it.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="comparer">method to compare array elements</param>
        public static T[] SortMergeStablePar<T>(this T[] src, IComparer<T> comparer = null)
        {
            var dst = new T[src.Length];
            src.SortMergeStableInnerPar<T>(0, src.Length - 1, dst, true, comparer);
            return dst;
        }
        /// <summary>
        /// In-place Parallel Merge Sort. Takes a range of the src array, and sorts just that range.
        /// Allocates a temporary array of the same size as the src array.
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns an array of length specified</returns>
        static public void SortMergeInPlacePar<T>(this T[] src, int startIndex, int length, IComparer<T> comparer = null)
        {
            T[] dst = new T[src.Length];
            src.SortMergeInnerPar<T>(startIndex, startIndex + length - 1, dst, false, comparer);
        }
        /// <summary>
        /// In-place Parallel Merge Sort (stable). Takes a range of the src array, and sorts just that range.
        /// Allocates a temporary array of the same size as the src array.
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="array">source/destination array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns an array of length specified</returns>
        static public void SortMergeInPlaceStablePar<T>(this T[] array, int startIndex, int length, IComparer<T> comparer = null)
        {
            T[] dst = new T[array.Length];
            array.SortMergeStableInnerPar<T>(startIndex, startIndex + length - 1, dst, false, comparer);
        }
        /// <summary>
        /// In-place Parallel Merge Sort.
        /// Allocates a temporary array of the same size as the src array.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="array">source/destination array</param>
        /// <param name="comparer">method to compare array elements</param>
        public static void SortMergeInPlacePar<T>(this T[] array, IComparer<T> comparer = null)
        {
            T[] dst = new T[array.Length];
            SortMergeInnerPar<T>(array, 0, array.Length - 1, dst, false, comparer);
        }
        /// <summary>
        /// In-place Parallel Merge Sort (stable).
        /// Allocates a temporary array of the same size as the src array.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="array">source/destination array</param>
        /// <param name="comparer">method to compare array elements</param>
        public static void SortMergeInPlaceStablePar<T>(this T[] array, IComparer<T> comparer = null)
        {
            T[] dst = new T[array.Length];
            SortMergeStableInnerPar<T>(array, 0, array.Length - 1, dst, false, comparer);
        }
        /// <summary>
        /// Parallel Merge Sort. Takes a range of the src List, sorts it, and then returns just the sorted range
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns an array of length specified</returns>
        static public List<T> SortMergePar<T>(this List<T> src, int startIndex, int length, IComparer<T> comparer = null)
        {
            T[] srcTrimmed = src.ToArrayPar(startIndex, length);
            T[] dst        = new T[srcTrimmed.Length];

            srcTrimmed.SortMergeInnerPar<T>(0, length - 1, dst, true, comparer);

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
        /// <returns>returns an array of length specified</returns>
        static public List<T> SortMergeStablePar<T>(this List<T> src, int startIndex, int length, IComparer<T> comparer = null)
        {
            T[] srcTrimmed = src.ToArrayPar(startIndex, length);
            T[] dst = new T[srcTrimmed.Length];

            srcTrimmed.SortMergeStableInnerPar<T>(0, length - 1, dst, true, comparer);

            return new List<T>(dst);
        }
        /// <summary>
        /// Parallel Merge Sort
        /// Allocates the resulting array and returns it.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="comparer">method to compare List elements</param>
        public static List<T> SortMergePar<T>(this List<T> src, IComparer<T> comparer = null)
        {
#if true
            T[] srcCopy = src.ToArrayPar();
            SortMergePar(srcCopy, comparer);
            List<T> dst = new List<T>(srcCopy);
            return dst;
#else
            if (dst == null || dst.Count != src.Count)
                dst = new List<T>(src);
            SortMergeParallel<T>(src, 0, src.Count - 1, dst, true, comparer);
#endif
        }
        /// <summary>
        /// Parallel Merge Sort (stable)
        /// Allocates the resulting array and returns it.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="comparer">method to compare List elements</param>
        public static List<T> SortMergeStablePar<T>(this List<T> src, IComparer<T> comparer = null)
        {
#if true
            T[] srcCopy = src.ToArrayPar();
            SortMergeStablePar(srcCopy, comparer);
            List<T> dst = new List<T>(srcCopy);
            return dst;
#else
            if (dst == null || dst.Count != src.Count)
                dst = new List<T>(src);
            SortMergeParallel<T>(src, 0, src.Count - 1, dst, true, comparer);
#endif
        }
        /// <summary>
        /// Parallel Merge Sort. Takes a range of the src List, sorts it, and then returns just the sorted range
        /// </summary>
        /// <typeparam name="T">List of type T</typeparam>
        /// <param name="list">source/destination List</param>
        /// <param name="startIndex">index within the src List where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two List elements of type T</param>
        /// <returns>returns an array of length specified</returns>
        static public void SortMergeInPlacePar<T>(ref List<T> list, int startIndex, int length, IComparer<T> comparer = null)
        {
            T[] srcCopy = list.ToArrayPar();
            srcCopy.SortMergeInPlacePar(startIndex, length, comparer);
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
        /// <returns>returns an array of length specified</returns>
        static public void SortMergeInPlaceStablePar<T>(ref List<T> list, int startIndex, int length, IComparer<T> comparer = null)
        {
            T[] srcCopy = list.ToArrayPar();
            srcCopy.SortMergeInPlaceStablePar(startIndex, length, comparer);
            list = new List<T>(srcCopy);
        }
        /// <summary>
        /// In-place Parallel Merge Sort
        /// Uses a not-in-place parallel merge sort implementation, allocating the same size array as the input array, releasing it when sorting has completed.
        /// </summary>
        /// <typeparam name="T">data type of each List element</typeparam>
        /// <param name="list">source/destination List</param>
        /// <param name="comparer">method to compare List elements</param>
        public static void SortMergeInPlacePar<T>(ref List<T> list, IComparer<T> comparer = null)
        {
#if true
            T[] srcCopy = list.ToArrayPar();
            SortMergeInPlacePar(srcCopy, comparer);
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
        /// <summary>
        /// In-place Parallel Merge Sort (stable)
        /// Uses a not-in-place parallel merge sort implementation, allocating the same size array as the input array, releasing it when sorting has completed.
        /// </summary>
        /// <typeparam name="T">data type of each List element</typeparam>
        /// <param name="list">source/destination List</param>
        /// <param name="comparer">method to compare List elements</param>
        public static void SortMergeInPlaceStablePar<T>(ref List<T> list, IComparer<T> comparer = null)
        {
#if true
            T[] srcCopy = list.ToArrayPar();
            SortMergeInPlaceStablePar(srcCopy, comparer);
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
    }
}
