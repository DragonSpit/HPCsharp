// TODO: Implement support for Comparison functions as well as Comparer (eek - double the fun), as Standard C# libraries support both methods.
// TODO: See if implementing stable IEnumerable sorting is faster than LINQ sorting, since that's the only stable one.
// TODO: Compare performance of sorting arrays and lists of classes with .Sort and LINQ to see if the advantages are bigger or smaller
// TODO: Expose all of the thresholds for users to be able to conrol. These can just be a List of thresholds that match up with the list of algorithms - i.e. a pair of algorithm and threshold.
//       Thresholds could be allowed to be negative or zero disable the associated algorithm.
// TODO: Test whether Merge is also stable, so that we don't have to resort to DivideAndConquerMerge for stability and give up performance in the process.
using System;
using System.Collections.Generic;
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
            int length1 = m - l       + 1;
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
            T[] dst        = new T[length];

            Array.Copy(source, startIndex, srcTrimmed, 0, length);

            srcTrimmed.SortMergeInner<T>(0, length - 1, dst, true, comparer);

            return dst;
        }

        /// <summary>
        /// Take the source array, sort it using the Merge Sort algorithm, and return a sorted array of full length
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

        /// <summary>
        /// Take a segment of the source array, and sort it in place using the Merge Sort algorithm
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="array">source array</param>
        /// <param name="startIndex">index within the array where sorting starts, inclusive</param>
        /// <param name="length">number of elements to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        static public void SortMergeInPlace<T>(this T[] array, int startIndex, int length, IComparer<T> comparer = null)
        {
            T[] dst = new T[array.Length];

            array.SortMergeInner<T>(startIndex, startIndex + length - 1, dst, false, comparer);
        }

        /// <summary>
        /// Take the source array, and sort all of it in place using the Merge Sort algorithm
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="array">source and result array</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        static public void SortMergeInPlace<T>(this T[] array, IComparer<T> comparer = null)
        {
            T[] dst = new T[array.Length];

            array.SortMergeInner<T>(0, array.Length - 1, dst, false, comparer);
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
        static public void SortMergeInPlace<T>(ref List<T> list, int startIndex, int length, IComparer<T> comparer = null)
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
        public static void SortMergeInPlace<T>(ref List<T> list, IComparer<T> comparer = null)
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
    }
}
