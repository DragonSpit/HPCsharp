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
    }
}