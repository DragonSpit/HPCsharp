using System;
using System.Collections.Generic;

namespace HPCsharp
{
    public struct SortedSpan
    {
        public Int32 Start;
        public Int32 End;
    }

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
        static public void Merge<T>(List<T> a,   Int32 aStart, Int32 aEnd,
                                    List<T> b,   Int32 bStart, Int32 bEnd,
                                    List<T> dst, Int32 dstStart,
                                    Comparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            while (aStart <= aEnd && bStart <= bEnd)
            {
                // a[aStart] <= b[bStart]
                if (equalityComparer.Compare(a[aStart], b[bStart]) <= 0)   	// if elements are equal, then a[] element is output
                    dst[dstStart++] = a[aStart++];
                else
                    dst[dstStart++] = b[bStart++];
            }
            while (aStart <= aEnd) dst[dstStart++] = a[aStart++];
            while (bStart <= bEnd) dst[dstStart++] = b[bStart++];
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
        static public void Merge(int[] a,   Int32 aStart, Int32 aEnd,
                                 int[] b,   Int32 bStart, Int32 bEnd,
                                 int[] dst, Int32 dstStart)
        {
            while (aStart <= aEnd && bStart <= bEnd)
            {
                if (a[aStart] <= b[bStart])   	    // if elements are equal, then a[] element is output
                    dst[dstStart++] = a[aStart++];
                else
                    dst[dstStart++] = b[bStart++];
            }
            //Array.Copy(a, aStart, dst, dstStart, aEnd - aStart + 1);
            while (aStart <= aEnd) dst[dstStart++] = a[aStart++];      // copy(a[aStart, aEnd] to dst[dstStart]
            //Array.Copy(b, bStart, dst, dstStart, bEnd - bStart + 1);
            while (bStart <= bEnd) dst[dstStart++] = b[bStart++];
        }
        static public void Merge(int[] src, Int32 aStart, Int32 aEnd, Int32 bStart, Int32 bEnd,
                                 int[] dst, Int32 dstStart)
        {
            while (aStart <= aEnd && bStart <= bEnd)
            {
                if (src[aStart] <= src[bStart])   	    // if elements are equal, then a[] element is output
                    dst[dstStart++] = src[aStart++];
                else
                    dst[dstStart++] = src[bStart++];
            }
            //Array.Copy(src, aStart, dst, dstStart, aEnd - aStart + 1);
            while (aStart <= aEnd) dst[dstStart++] = src[aStart++];       // copy(a[aStart, aEnd] to dst[dstStart]
            //Array.Copy(src, bStart, dst, dstStart, bEnd - bStart + 1);
            while (bStart <= bEnd) dst[dstStart++] = src[bStart++];
        }
        // A slightly faster implementation, which performs only a single comparison for each loop
        static public void Merge2(int[] src, Int32 aStart, Int32 aEnd, Int32 bStart, Int32 bEnd,
                                  int[] dst, Int32 dstStart)
        {
            //Console.WriteLine("Merge integer2");
            if (aStart <= aEnd && bStart <= bEnd)
            {
                while (true)
                    if (src[aStart] <= src[bStart])   	    // if elements are equal, then a[] element is output
                    {
                        dst[dstStart++] = src[aStart++];
                        if (aStart > aEnd) break;
                    }
                    else
                    {
                        dst[dstStart++] = src[bStart++];
                        if (bStart > bEnd) break;
                    }
            }
            //Array.Copy(src, aStart, dst, dstStart, aEnd - aStart + 1);
            while (aStart <= aEnd) dst[dstStart++] = src[aStart++];       // copy(a[aStart, aEnd] to dst[dstStart]
            //Array.Copy(src, bStart, dst, dstStart, bEnd - bStart + 1);
            while (bStart <= bEnd) dst[dstStart++] = src[bStart++];
        }
        // Is loop unrolling worthwhile?
        static public void Merge3(int[] src, Int32 aStart, Int32 aEnd, Int32 bStart, Int32 bEnd,
                                  int[] dst, Int32 dstStart)
        {
	        int aLength = aEnd - aStart + 1;
            int bLength = bEnd - bStart + 1;
            int lengthMin = Math.Min(aLength, bLength);

	        while(lengthMin > 10)
            {
		        dst[dstStart++] = (src[aStart] <= src[bStart]) ? src[aStart++] : src[bStart++];	// if elements are equal, then a[] element is output
                dst[dstStart++] = (src[aStart] <= src[bStart]) ? src[aStart++] : src[bStart++];
                dst[dstStart++] = (src[aStart] <= src[bStart]) ? src[aStart++] : src[bStart++];
                dst[dstStart++] = (src[aStart] <= src[bStart]) ? src[aStart++] : src[bStart++];
                dst[dstStart++] = (src[aStart] <= src[bStart]) ? src[aStart++] : src[bStart++];
                dst[dstStart++] = (src[aStart] <= src[bStart]) ? src[aStart++] : src[bStart++];
                dst[dstStart++] = (src[aStart] <= src[bStart]) ? src[aStart++] : src[bStart++];
                dst[dstStart++] = (src[aStart] <= src[bStart]) ? src[aStart++] : src[bStart++];
                dst[dstStart++] = (src[aStart] <= src[bStart]) ? src[aStart++] : src[bStart++];
                dst[dstStart++] = (src[aStart] <= src[bStart]) ? src[aStart++] : src[bStart++];
		        aLength = aEnd - aStart + 1;
		        bLength = bEnd - bStart + 1;
		        lengthMin = Math.Min(aLength, bLength);
            }
            // TODO: Should call Merge2 at this point
            if (aStart <= aEnd && bStart <= bEnd)
            {
                while (true)
                    if (src[aStart] <= src[bStart])   	    // if elements are equal, then a[] element is output
                    {
                        dst[dstStart++] = src[aStart++];
                        if (aStart >= aEnd) break;
                    }
                    else
                    {
                        dst[dstStart++] = src[bStart++];
                        if (bStart >= bEnd) break;
                    }
            }
            //Array.Copy(src, aStart, dst, dstStart, aEnd - aStart + 1);
            while (aStart <= aEnd) dst[dstStart++] = src[aStart++];       // copy(a[aStart, aEnd] to dst[dstStart]
            //Array.Copy(src, bStart, dst, dstStart, bEnd - bStart + 1);
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
        static public void Merge<T>(T[] a,   Int32 aStart, Int32 aEnd,
                                    T[] b,   Int32 bStart, Int32 bEnd,
                                    T[] dst, Int32 dstStart,
                                    Comparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            while (aStart <= aEnd && bStart <= bEnd)
            {
                // a[aStart] <= b[bStart]
                if (equalityComparer.Compare(a[aStart], b[bStart]) <= 0)   	// if elements are equal, then a[] element is output
                    dst[dstStart++] = a[aStart++];
                else
                    dst[dstStart++] = b[bStart++];
            }
            //Array.Copy(a, aStart, dst, dstStart, aEnd - aStart);
            while (aStart <= aEnd) dst[dstStart++] = a[aStart++];    // copy(a[aStart, aEnd] to dst[dstStart]
            //Array.Copy(b, bStart, dst, dstStart, bEnd - bStart);
            while (bStart <= bEnd) dst[dstStart++] = b[bStart++];
        }

        /// <summary>
        /// Merge two sorted Arrays within a range, placing the result into a destination List, starting at an index.
        //  The destination array must be as big as the source, otherwise an ArgumentException is thrown.
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
        static public void Merge<T>(T[] src, List<SortedSpan> srcSpans,
                                    T[] dst,
                                    Comparer<T> comparer = null)
        {
            // TODO: Check that the destination is of equal size as the source, otherwise throw an exception
            if (dst.Length != src.Length)
            {
                throw new ArgumentException("Destination array must be the same size as the source array");
            }
            if (srcSpans == null || srcSpans.Count == 0)    // nothing to merge
            {
                return;
            }
            else
            {
                bool srcToDst = true;
                while (srcSpans.Count >= 1)
                {
                    if (srcSpans.Count == 1)
                    {
                        if (srcToDst)
                            Array.Copy(src, srcSpans[0].Start, dst, srcSpans[0].Start, srcSpans[0].End - srcSpans[0].Start + 1);
                        return;
                    }

                    var dstSpans = new List<SortedSpan>();
                    Int32 i = 0;

                    // Merge neighboring pairs of spans
                    Int32 numPairs = srcSpans.Count / 2;
                    for (Int32 p = 0; p < numPairs; p++)
                    {
                        Merge<T>(src, srcSpans[i    ].Start, srcSpans[i    ].End,
                                 src, srcSpans[i + 1].Start, srcSpans[i + 1].End,
                                 dst, srcSpans[i    ].Start,
                                 comparer);
                        dstSpans.Add(new SortedSpan { Start = srcSpans[i].Start, End = srcSpans[i + 1].End });
                        i += 2;
                    }
                    // Copy the last left over odd segment (if there is one) from src to dst and add it to dstSpans
                    if (i > srcSpans.Count)
                    {
                        Array.Copy(src, srcSpans[i - 1].Start, dst, srcSpans[i - 1].Start, srcSpans[i - 1].End - srcSpans[i - 1].Start + 1);
                        dstSpans.Add(new SortedSpan { Start = srcSpans[i - 1].Start, End = srcSpans[i - 1].End });
                    }
                    srcSpans = dstSpans;
                    var tmp = src;          // swap src and dst arrays
                    src = dst;
                    dst = tmp;
                    srcToDst = srcToDst ? false : true; // keep track of merge direction
                }
            }
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
            Console.WriteLine("Merge generic");
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
        static public void MergeThree<T>(T[] a,   Int32 aStart, Int32 aEnd,
                                         T[] b,   Int32 bStart, Int32 bEnd,
                                         T[] c,   Int32 cStart, Int32 cEnd,
                                         T[] dst, Int32 dstStart,
                                         Comparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            while (aStart <= aEnd && bStart <= bEnd && cStart <= cEnd)
            {
                // a[aStart] <= b[bStart]
                if (equalityComparer.Compare(a[aStart], b[bStart]) <= 0)
                {   // a <= b
                    if (equalityComparer.Compare(a[aStart], c[cStart]) <= 0)
                    {   // a is smallest
                        dst[dstStart++] = a[aStart++];
                    }
                    else
                    {   // c is smallest
                        dst[dstStart++] = c[cStart++];
                    }
                }
                else
                {   // b < a
                    if (equalityComparer.Compare(b[bStart], c[cStart]) <= 0)
                    {   // b is smallest
                        dst[dstStart++] = b[bStart++];
                    }
                    else
                    {   // c is smallest
                        dst[dstStart++] = c[cStart++];
                    }
                }
            }
            // Ran out of elements in one of the segments - i.e. 2 segments are available for merging, but which 2
            if (aStart > aEnd)
            {
                Merge(b, bStart, bEnd, c, cStart, cEnd, dst, dstStart, equalityComparer);
            }
            else if (bStart > bEnd)
            {
                Merge(a, aStart, aEnd, c, cStart, cEnd, dst, dstStart, equalityComparer);
            }
            else   // (cStart > cEnd)
            {
                Merge(a, aStart, aEnd, b, bStart, bEnd, dst, dstStart, equalityComparer);
            }
        }
        // Strategy is to handle 4 segments while 4 are available, 3 while 3 are available, 2 while 2 are available
        // This extends the strategy used for merging two segments nicely
        static public void MergeFour<T>(T[] a,   Int32   aStart, Int32 aEnd,
                                        T[] b,   Int32   bStart, Int32 bEnd,
                                        T[] c,   Int32   cStart, Int32 cEnd,
                                        T[] d,   Int32   dStart, Int32 dEnd,
                                        T[] dst, Int32 dstStart,
                                        Comparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            while (aStart <= aEnd && bStart <= bEnd && cStart <= cEnd && dStart <= dEnd)
            {
                // a[aStart] <= b[bStart]
                if (equalityComparer.Compare(a[aStart], b[bStart]) <= 0)
                {   // a <= b
                    if (equalityComparer.Compare(c[cStart], d[dStart]) <= 0)
                    {   // c <= d
                        if (equalityComparer.Compare(a[aStart], c[cStart]) <= 0)
                        {   // a is smallest
                            dst[dstStart++] = a[aStart++];
                        }
                        else
                        {   // c is smallest
                            dst[dstStart++] = c[cStart++];
                        }
                    }
                    else
                    {   // d < c
                        if (equalityComparer.Compare(a[aStart], d[dStart]) <= 0)
                        {   // a is smallest
                            dst[dstStart++] = a[aStart++];
                        }
                        else
                        {   // d is smallest
                            dst[dstStart++] = d[dStart++];
                        }
                    }
                }
                else
                {   // b < a
                    if (equalityComparer.Compare(c[cStart], d[dStart]) <= 0)
                    {   // c <= d
                        if (equalityComparer.Compare(b[bStart], c[cStart]) <= 0)
                        {   // b is smallest
                            dst[dstStart++] = b[bStart++];
                        }
                        else
                        {   // c is smallest
                            dst[dstStart++] = c[cStart++];
                        }
                    }
                    else
                    {   // d < c
                        if (equalityComparer.Compare(b[bStart], d[dStart]) <= 0)
                        {   // b is smallest
                            dst[dstStart++] = b[bStart++];
                        }
                        else
                        {   // d is smallest
                            dst[dstStart++] = d[dStart++];
                        }
                    }
                }
            }
            // Ran out of elements in one of the segments - i.e. 3 segments are available for merging, but which 3
            if (aStart > aEnd)
            {
                MergeThree(b, bStart, bEnd, c, cStart, cEnd, d, dStart, dEnd, dst, dstStart, equalityComparer);
            }
            else if (bStart > bEnd)
            {
                MergeThree(a, aStart, aEnd, c, cStart, cEnd, d, dStart, dEnd, dst, dstStart, equalityComparer);
            }
            else if (cStart > cEnd)
            {
                MergeThree(a, aStart, aEnd, b, bStart, bEnd, d, dStart, dEnd, dst, dstStart, equalityComparer);
            }
            else   // (dStart > dEnd)
            {
                MergeThree(a, aStart, aEnd, b, bStart, bEnd, c, cStart, cEnd, dst, dstStart, equalityComparer);
            }
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