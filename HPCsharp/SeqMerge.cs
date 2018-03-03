using System;
using System.Collections.Generic;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        // Listing 1
        // _end pointer point not to the last element, but one past and never access it.
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
            // TODO: We could use some sort of a copy here to make this more efficient!
            //Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length);
            Array.Copy(a, aStart, dst, dstStart, aEnd - aStart);
            //while (aStart < aEnd) dst[dstStart++] = a[aStart++];    // copy(a[aStart, aEnd] to dst[dstStart]
            Array.Copy(b, bStart, dst, dstStart, bEnd - bStart);
            //while (bStart < bEnd) dst[dstStart++] = b[bStart++];
        }
    }
}