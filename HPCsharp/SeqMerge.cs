using System;
using System.Collections.Generic;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        /// <summary>
        /// Merge two sorted Lists within a range, placing the result into a destination List, starting at an index.
        /// a: first sorted List to be merged
        /// aStart: left/starting index within the first  List where to source the elements, inclusive
        /// aEnd:   right/ending  index within the first  List, non-inclusive
        /// b: second sorted List to be merged
        /// bStart: left/starting index within the second List where to source the elements, inclusive
        /// bEnd:   right/ending  index within the second List, non-inclusive
        /// dst: destination List where the result of two merged Lists is placed
        /// dstStart: left/starting index within the destination List where the merged sorted List will be placed
        /// comparer: optional compare method
        /// </summary>
        static public void Merge<T>(List<T> a, Int32 aStart, Int32 aEnd,
                                    List<T> b, Int32 bStart, Int32 bEnd,
                                    List<T> dst, Int32 dstStart,
                                    Comparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            while (aStart < aEnd && bStart < bEnd)
            {
                // a[aStart] <= b[bStart]
                if (equalityComparer.Compare(a[aStart], b[bStart]) <= 0)   	// if elements are equal, then a[] element is output
                    dst[dstStart++] = a[aStart++];
                else
                    dst[dstStart++] = b[bStart++];
            }
            while (aStart < aEnd) dst[dstStart++] = a[aStart++];
            while (bStart < bEnd) dst[dstStart++] = b[bStart++];
        }
        /// <summary>
        /// Merge two sorted ranges of a List, placing the result into a destination List, starting at an index.
        /// </summary>
        /// <param name="src">source List</param>
        /// <param name="aStart">starting index of the first sorted range, inclusive</param>
        /// <param name="aEnd">ending index of the first sorted range, inclusive</param>
        /// <param name="bStart">starting index of the second dorted range, inclusive</param>
        /// <param name="bEnd">ending   index of the second sorted range, inclusive</param>
        /// <param name="dst">destination List</param>
        /// <param name="dstStart">starting index of the result</param>
        /// <param name="comparer">optional method to compare array elements</param>
        static public void Merge<T>(List<T> src, Int32 aStart, Int32 aEnd, Int32 bStart, Int32 bEnd,
                                    List<T> dst, Int32 dstStart, Comparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            while (aStart <= aEnd && bStart <= bEnd)
            {
                // a[aStart] <= b[bStart]
                if (equalityComparer.Compare(src[aStart], src[bStart]) <= 0)   	// if elements are equal, then a[] element is output
                    dst[dstStart++] = src[aStart++];
                else
                    dst[dstStart++] = src[bStart++];
            }
            //Array.Copy(src, aStart, dst, dstStart, aEnd - aStart + 1);
            //dst = src.GetRange(aStart, aEnd - aStart + 1);
            while (aStart <= aEnd) dst[dstStart++] = src[aStart++];
            while (bStart <= bEnd) dst[dstStart++] = src[bStart++];
        }
        /// <summary>
        /// Merge two sorted Arrays within a range, placing the result into a destination List, starting at an index.
        /// a: first sorted Array to be merged
        /// aStart: left/starting index within the first  Array where to source the elements, inclusive
        /// aEnd:   right/ending  index within the first  Array, non-inclusive
        /// b: second sorted Array to be merged
        /// bStart: left/starting index within the second Array where to source the elements, inclusive
        /// bEnd:   right/ending  index within the second Array, non-inclusive
        /// dst: destination Array where the result of two merged Arrays is placed
        /// dstStart: left/starting index within the destination Array where the merged sorted List will be placed
        /// comparer: optional compare method
        /// </summary>
        static public void Merge<T>(T[] a, Int32 aStart, Int32 aEnd,
                                    T[] b, Int32 bStart, Int32 bEnd,
                                    T[] dst, Int32 dstStart,
                                    Comparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            while (aStart < aEnd && bStart < bEnd)
            {
                // a[aStart] <= b[bStart]
                if (equalityComparer.Compare(a[aStart], b[bStart]) <= 0)   	// if elements are equal, then a[] element is output
                    dst[dstStart++] = a[aStart++];
                else
                    dst[dstStart++] = b[bStart++];
            }
            Array.Copy(a, aStart, dst, dstStart, aEnd - aStart);
            //while (aStart < aEnd) dst[dstStart++] = a[aStart++];    // copy(a[aStart, aEnd] to dst[dstStart]
            Array.Copy(b, bStart, dst, dstStart, bEnd - bStart);
            //while (bStart < bEnd) dst[dstStart++] = b[bStart++];
        }

        /// <summary>
        /// Merge two sorted ranges of an Array, placing the result into a destination Array, starting at an index.
        /// </summary>
        /// <param name="src">source List</param>
        /// <param name="aStart">starting index of the first sorted range, inclusive</param>
        /// <param name="aEnd">ending index of the first sorted range, inclusive</param>
        /// <param name="bStart">starting index of the second dorted range, inclusive</param>
        /// <param name="bEnd">ending   index of the second sorted range, inclusive</param>
        /// <param name="dst">destination List</param>
        /// <param name="dstStart">starting index of the result</param>
        /// <param name="comparer">optional method to compare array elements</param>
        static public void Merge<T>(T[] src, Int32 aStart, Int32 aEnd, Int32 bStart, Int32 bEnd,
                                    T[] dst, Int32 dstStart, Comparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            while (aStart <= aEnd && bStart <= bEnd)
            {
                // a[aStart] <= b[bStart]
                if (equalityComparer.Compare(src[aStart], src[bStart]) <= 0)   	// if elements are equal, then a[] element is output
                    dst[dstStart++] = src[aStart++];
                else
                    dst[dstStart++] = src[bStart++];
            }
            //Array.Copy(src, aStart, dst, dstStart, aEnd - aStart + 1);
            while (aStart <= aEnd) dst[dstStart++] = src[aStart++];    // copy(a[aStart, aEnd] to dst[dstStart]
            //Array.Copy(src, bStart, dst, dstStart, bEnd - bStart + 1);
            while (bStart <= bEnd) dst[dstStart++] = src[bStart++];
        }

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

            MergeSortInner(src, l,     m, dst, !srcToDst, comparer);		// reverse direction of srcToDst for the next level of recursion
            MergeSortInner(src, m + 1, r, dst, !srcToDst, comparer);

            if (srcToDst) Merge(src, l, m, m + 1, r, dst, l, comparer);
            else          Merge(dst, l, m, m + 1, r, src, l, comparer);
        }
        /// <summary>
        /// Take a range of the src array, sort it, and then return just the sorted range
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <returns>returns an array of length specified</returns>
        static public T[] SortMerge<T>(this T[] src, int startIndex, int length, Comparer<T> comparer = null)
        {
            T[] srcTrimmed = new T[length];
            T[] dst        = new T[length];

            Array.Copy(src, startIndex, srcTrimmed, 0, length);

            srcTrimmed.MergeSortInner<T>(0, length - 1, dst, true, comparer);

            return dst;
        }

        static public T[] SortMerge<T>(this T[] src, Comparer<T> comparer = null)
        {
            T[] dst = new T[src.Length];

            src.MergeSortInner<T>(0, src.Length - 1, dst, true, comparer);

            return dst;
        }

        static public void SortMergeInPlace<T>(this T[] src, int startIndex, int length, Comparer<T> comparer = null)
        {
            T[] dst = new T[src.Length];

            src.MergeSortInner<T>(startIndex, startIndex + length - 1, dst, false, comparer);
        }

        static public void SortMergeInPlace<T>(this T[] src, Comparer<T> comparer = null)
        {
            T[] dst = new T[src.Length];

            src.MergeSortInner<T>(0, src.Length - 1, dst, false, comparer);
        }
    }
}