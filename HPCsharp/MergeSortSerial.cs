// TODO: Implement stable Merge Sort, possibly by using the divide-and-conquer implementation of Merge, since it's stable. Compare performance to
//       not-stable .Sort and stable LINQ sorting for fair comparison.
// TODO: See if implementing stable IEnumerable sorting is faster than LINQ sorting, since that's the only stable one.
// TODO: Compare performance of sorting arrays and lists of classes with .Sort and LINQ to see if the advantages are bigger or smaller
// TODO: Expose all of the thresholds for users to be able to conrol
using System;
using System.Collections.Generic;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        static private void MergeSortInner<T>(this T[] src, int l, int r, T[] dst, bool srcToDst = true, Comparer<T> comparer = null)
        {
            if (r == l)
            {    // termination/base case of sorting a single element
                if (srcToDst) dst[l] = src[l];    // copy the single element from src to dst
                return;
            }
            else if ((r - l) < 16)
            {
                HPCsharp.Algorithm.InsertionSort<T>(src, l, r - l + 1, comparer);  // want to do dstToSrc, can just do it in-place, just sort the src, no need to copy
                if (srcToDst)
                    for (int i = l; i <= r; i++) dst[i] = src[i];	// copy from src to dst, when the result needs to be in dst
                return;
            }

            int m = (r + l) / 2;
            int length1 = m - l       + 1;
            int length2 = r - (m + 1) + 1;

            MergeSortInner(src, l,     m, dst, !srcToDst, comparer);		// reverse direction of srcToDst for the next level of recursion
            MergeSortInner(src, m + 1, r, dst, !srcToDst, comparer);

            if (srcToDst) Merge(src, l, length1, m + 1, length2, dst, l, comparer);
            else          Merge(dst, l, length1, m + 1, length2, src, l, comparer);
        }

        internal static void MergeSortStableInner<T>(this T[] src, int l, int r, T[] dst, bool srcToDst = true, Comparer<T> comparer = null)
        {
            if (r == l)
            {    // termination/base case of sorting a single element
                if (srcToDst) dst[l] = src[l];    // copy the single element from src to dst
                return;
            }
            else if ((r - l) < 16)
            {
                HPCsharp.Algorithm.InsertionSort<T>(src, l, r - l + 1, comparer);  // want to do dstToSrc, can just do it in-place, just sort the src, no need to copy
                if (srcToDst)
                    for (int i = l; i <= r; i++) dst[i] = src[i];	// copy from src to dst, when the result needs to be in dst
                return;
            }

            int m = (r + l) / 2;

            MergeSortStableInner(src, l,     m, dst, !srcToDst, comparer);		// reverse direction of srcToDst for the next level of recursion
            MergeSortStableInner(src, m + 1, r, dst, !srcToDst, comparer);

            if (srcToDst) MergeDivideAndConquer(src, l, m, m + 1, r, dst, l, comparer);
            else          MergeDivideAndConquer(dst, l, m, m + 1, r, src, l, comparer);
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
        static public T[] SortMerge<T>(this T[] source, int startIndex, int length, Comparer<T> comparer = null)
        {
            T[] srcTrimmed = new T[length];
            T[] dst        = new T[length];

            Array.Copy(source, startIndex, srcTrimmed, 0, length);

            srcTrimmed.MergeSortInner<T>(0, length - 1, dst, true, comparer);

            return dst;
        }

        /// <summary>
        /// Take a segment of the src array, sort it using the Merge Sort (stable) algorithm, and return just the sorted range
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="source">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns a sorted array of length specified</returns>
        static public T[] SortMergeStable<T>(this T[] source, int startIndex, int length, Comparer<T> comparer = null)
        {
            T[] srcTrimmed = new T[length];
            T[] dst = new T[length];

            Array.Copy(source, startIndex, srcTrimmed, 0, length);

            srcTrimmed.MergeSortStableInner<T>(0, length - 1, dst, true, comparer);

            return dst;
        }

        /// <summary>
        /// Take the source array, sort it using the Merge Sort algorithm, and return a sorted array of full length
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="source">source array</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns a sorted array of full length</returns>
        static public T[] SortMerge<T>(this T[] source, Comparer<T> comparer = null)
        {
            T[] dst = new T[source.Length];

            source.MergeSortInner<T>(0, source.Length - 1, dst, true, comparer);

            return dst;
        }

        /// <summary>
        /// Take the source array, sort it using the Merge Sort (stable) algorithm, and return a sorted array of full length
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="source">source array</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns a sorted array of full length</returns>
        static public T[] SortMergeStable<T>(this T[] source, Comparer<T> comparer = null)
        {
            T[] dst = new T[source.Length];

            source.MergeSortStableInner<T>(0, source.Length - 1, dst, true, comparer);

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
        static public void SortMergeInPlace<T>(this T[] array, int startIndex, int length, Comparer<T> comparer = null)
        {
            T[] dst = new T[array.Length];

            array.MergeSortInner<T>(startIndex, startIndex + length - 1, dst, false, comparer);
        }

        /// <summary>
        /// Take a segment of the array, and sort it in place using the Merge Sort (stable) algorithm
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="array">source and result array</param>
        /// <param name="startIndex">index within the array where sorting starts</param>
        /// <param name="length">number of elements to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        static public void SortMergeInPlaceStable<T>(this T[] array, int startIndex, int length, Comparer<T> comparer = null)
        {
            T[] dst = new T[array.Length];

            array.MergeSortStableInner<T>(startIndex, startIndex + length - 1, dst, false, comparer);
        }

        /// <summary>
        /// Take the source array, and sort all of it in place using the Merge Sort algorithm
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="array">source and result array</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        static public void SortMergeInPlace<T>(this T[] array, Comparer<T> comparer = null)
        {
            T[] dst = new T[array.Length];

            array.MergeSortInner<T>(0, array.Length - 1, dst, false, comparer);
        }

        /// <summary>
        /// Take the source array, and sort all of it in place using the Merge Sort (stable) algorithm
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="array">source and result array</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        static public void SortMergeInPlaceStable<T>(this T[] array, Comparer<T> comparer = null)
        {
            T[] dst = new T[array.Length];

            array.MergeSortStableInner<T>(0, array.Length - 1, dst, false, comparer);
        }
    }
}
