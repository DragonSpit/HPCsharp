// TODO: Implement a knob for the merge algorithm, to use multi-merge and specify how many way to split the merge, for divide-and-conquer too.
// TODO: Multi-merge should use 2-way, 3-way and possibly 4-way before using the more general multi-way merge to go faster.
// TODO: 3-way and 4-way merge need to be implemented using the algorithm that uses fewest boundary comparisons, as it may pay off as the way merge increases.
// TODO: Is it faster to copy a List to an Array, then do the merge and then to copy the result back to a List? Currently, List merge runs at 1/2 the speed of Array merge.
// TODO: Does it pay off to use the parallel copy.
// TODO: Change all in-place sorting functions to return the original reference to improve Functional composition and pipelining, since it's returning void anyways.
// TODO: Refactor other methods to possibly return a destination, if it makes sense.
using System;
using System.Collections.Generic;

namespace HPCsharp
{
    public struct SortedSpan
    {
        public Int32 Start;
        public Int32 Length;
    }

    static public partial class Algorithm
    {
        /// <summary>
        /// Merge two sorted Lists within a range, placing the result into a destination List, starting at an index.
        /// </summary>
        /// <param name="a">first source List to be merged</param>
        /// <param name="aStart">starting index of the first sorted List segment, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="b">second source List to be merged</param>
        /// <param name="bStart">starting index of the second sorted List segment, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dst">destination List where the result of two merged Lists is to be placed</param>
        /// <param name="dstStart">starting index within the destination List where the merged sorted List is to be placed</param>
        /// <param name="comparer">optional method to compare array elements</param>
        static public void Merge<T>(List<T> a, Int32 aStart, Int32 aLength,
                                    List<T> b, Int32 bStart, Int32 bLength,
                                    List<T> dst, Int32 dstStart,
                                    IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
            while (aStart <= aEnd && bStart <= bEnd)
            {
                while (true)
                {
                    // a[aStart] <= b[bStart]
                    if (equalityComparer.Compare(a[aStart], b[bStart]) <= 0)   	// if elements are equal, then a[] element is output
                    {
                        dst[dstStart++] = a[aStart++];
                        if (aStart > aEnd) break;
                    }
                    else
                    {
                        dst[dstStart++] = b[bStart++];
                        if (bStart > bEnd) break;
                    }
                }
            }
            while (aStart <= aEnd) dst[dstStart++] = a[aStart++];
            while (bStart <= bEnd) dst[dstStart++] = b[bStart++];
        }
        /// <summary>
        /// Merge two sorted ranges of a List, placing the result into a destination List, starting at an index.
        /// </summary>
        /// <param name="src">source List</param>
        /// <param name="aStart">starting index of the first sorted List segment, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="bStart">starting index of the second sorted List segment, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dst">destination List where the result of two merged Lists is to be placed</param>
        /// <param name="dstStart">starting index within the destination List where the merged sorted List is to be placed</param>
        /// <param name="comparer">optional method to compare array elements</param>
        static public void Merge<T>(List<T> src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
                                    List<T> dst, Int32 dstStart, IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
            while (aStart <= aEnd && bStart <= bEnd)
            {
                while (true)
                {
                    // a[aStart] <= b[bStart]
                    if (equalityComparer.Compare(src[aStart], src[bStart]) <= 0)   	// if elements are equal, then a[] element is output
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
            }
            //Array.Copy(src, aStart, dst, dstStart, aEnd - aStart + 1);
            //dst = src.GetRange(aStart, aEnd - aStart + 1);
            while (aStart <= aEnd) dst[dstStart++] = src[aStart++];
            while (bStart <= bEnd) dst[dstStart++] = src[bStart++];
        }
        /// <summary>
        /// Merge two sorted Arrays of int's within a range, placing the result into a destination Array, starting at an index.
        /// </summary>
        /// <param name="a">first source Array to be merged</param>
        /// <param name="aStart">starting index of the first sorted Array segment, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="b">second source Array to be merged</param>
        /// <param name="bStart">starting index of the second sorted Array segment, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dst">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        static public void Merge(int[] a, Int32 aStart, Int32 aLength,
                                 int[] b, Int32 bStart, Int32 bLength,
                                 int[] dst, Int32 dstStart)
        {
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
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
        /// <summary>
        /// Merge two sorted spans of an Array of int's, placing the result into a destination Array, starting at an index.
        /// </summary>
        /// <param name="src">source Array of int's with two sorted segments to be merged</param>
        /// <param name="aStart">starting index of the first sorted Array, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="bStart">starting index within the second sorted Array, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dst">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        static public void Merge(int[] src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
                                 int[] dst, Int32 dstStart)
        {
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
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
        /// <summary>
        /// A slightly faster implementation, which performs only a single comparison for each loop
        /// Merge two sorted segments of an Array of int's, placing the result into a destination Array, starting at an index.
        /// </summary>
        /// <param name="src">source Array of int's with two sorted segments to be merged</param>
        /// <param name="aStart">starting index of the first sorted Array, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="bStart">starting index within the second sorted Array, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dst">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        static public void Merge2(int[] src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
                                  int[] dst, Int32 dstStart)
        {
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
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

            while (lengthMin > 10)
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
        /// Merge two sorted Array segments, placing the result into a destination Array, starting at an index.
        /// </summary>
        /// <param name="a">first source Array to be merged</param>
        /// <param name="aStart">starting index of the first sorted Array, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="b">second source Array to be merged</param>
        /// <param name="bStart">starting index of the second sorted Array, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dst">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        /// <param name="comparer">optional method to compare array elements</param>
        static public void Merge<T>(T[] a, Int32 aStart, Int32 aLength,
                                    T[] b, Int32 bStart, Int32 bLength,
                                    T[] dst, Int32 dstStart,
                                    IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;

            while (aStart <= aEnd && bStart <= bEnd)
            {
                if (equalityComparer.Compare(a[aStart], b[bStart]) <= 0)   	    // if elements are equal, then a[] element is output
                    dst[dstStart++] = a[aStart++];
                else
                    dst[dstStart++] = b[bStart++];
            }

            //Array.Copy(a, aStart, dst, dstStart, aEnd - aStart + 1);
            while (aStart <= aEnd) dst[dstStart++] = a[aStart++];    // copy(a[aStart, aEnd] to dst[dstStart]
            //Array.Copy(b, bStart, dst, dstStart, bEnd - bStart + 1);
            while (bStart <= bEnd) dst[dstStart++] = b[bStart++];
        }
        /// <summary>
        /// Merge two sorted Array segments, placing the result into a destination Array, starting at an index.
        /// </summary>
        /// <param name="a">first source Array to be merged</param>
        /// <param name="aStart">starting index of the first sorted Array, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="b">second source Array to be merged</param>
        /// <param name="bStart">starting index of the second sorted Array, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dst">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        /// <param name="comparer">optional method to compare array elements</param>
        static public void Merge2<T>(T[] a, Int32 aStart, Int32 aLength,
                                     T[] b, Int32 bStart, Int32 bLength,
                                     T[] dst, Int32 dstStart,
                                     IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
            while (aStart <= aEnd && bStart <= bEnd)
            {
                while (true)
                {
                    // a[aStart] <= b[bStart]
                    if (equalityComparer.Compare(a[aStart], b[bStart]) <= 0)   	// if elements are equal, then a[] element is output
                    {
                        dst[dstStart++] = a[aStart++];
                        if (aStart > aEnd) break;
                    }
                    else
                    {
                        dst[dstStart++] = b[bStart++];
                        if (bStart > bEnd) break;
                    }
                }
            }
            //Array.Copy(a, aStart, dst, dstStart, aEnd - aStart + 1);
            while (aStart <= aEnd) dst[dstStart++] = a[aStart++];    // copy(a[aStart, aEnd] to dst[dstStart]
            //Array.Copy(b, bStart, dst, dstStart, bEnd - bStart + 1);
            while (bStart <= bEnd) dst[dstStart++] = b[bStart++];
        }
        /// <summary>
        /// Merge two sorted Array segments, placing the result into a destination Array, starting at an index.
        /// </summary>
        /// <param name="a">first source Array to be merged</param>
        /// <param name="aStart">starting index of the first sorted Array, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="bStart">starting index of the second sorted Array, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dst">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        /// <param name="comparer">optional method to compare array elements</param>
        static public void Merge<T>(T[] a, Int32 aStart, Int32 aLength,
                                           Int32 bStart, Int32 bLength,
                                    T[] dst, Int32 dstStart,
                                    IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;

            while (aStart <= aEnd && bStart <= bEnd)
            {
                if (equalityComparer.Compare(a[aStart], a[bStart]) <= 0)   	    // if elements are equal, then a[] element is output
                    dst[dstStart++] = a[aStart++];
                else
                    dst[dstStart++] = a[bStart++];
            }

            //Array.Copy(a, aStart, dst, dstStart, aEnd - aStart + 1);
            while (aStart <= aEnd) dst[dstStart++] = a[aStart++];           // copy(a[aStart, aEnd] to dst[dstStart]
            //Array.Copy(b, bStart, dst, dstStart, bEnd - bStart + 1);
            while (bStart <= bEnd) dst[dstStart++] = a[bStart++];
        }
        /// <summary>
        /// Merge two sorted ranges of an Array, placing the result into a destination Array, starting at an index.
        /// </summary>
        /// <param name="src">source List</param>
        /// <param name="aStart">starting index of the first sorted segment, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="bStart">starting index of the second sorted segment, inclusive</param>
        /// <param name="bLength">length of the first sorted segment</param>
        /// <param name="dst">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        /// <param name="comparer">optional method to compare array elements</param>
        static public void Merge2<T>(T[] src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
                                     T[] dst, Int32 dstStart, IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
            while (aStart <= aEnd && bStart <= bEnd)
            {
                while (true)
                {
                    // a[aStart] <= b[bStart]
                    if (equalityComparer.Compare(src[aStart], src[bStart]) <= 0)   	// if elements are equal, then a[] element is output
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
            }
            //Array.Copy(src, aStart, dst, dstStart, aEnd - aStart + 1);
            while (aStart <= aEnd) dst[dstStart++] = src[aStart++];    // copy(a[aStart, aEnd] to dst[dstStart]
            //Array.Copy(src, bStart, dst, dstStart, bEnd - bStart + 1);
            while (bStart <= bEnd) dst[dstStart++] = src[bStart++];
        }
        /// <summary>
        /// Merge multiple sorted array segments, placing the result into a destination array as a single sorted span.
        /// The destination array must be as big as the source, otherwise an ArgumentException is thrown.
        /// The source array is modified during processing.
        /// Uses multiple passes of 2-way merges
        /// </summary>
        /// <param name="src">source Array to be merged</param>
        /// <param name="srcSpans">List of sorted segments, specified by starting index and length</param>
        /// <param name="dst">destination Array where the result of merged segments is to be placed</param>
        /// <param name="comparer">optional method to compare array elements</param>
        static public void Merge<T>(T[] src, List<SortedSpan> srcSpans,
                                    T[] dst,
                                    IComparer<T> comparer = null)
        {
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
                            Array.Copy(src, srcSpans[0].Start, dst, srcSpans[0].Start, srcSpans[0].Length);
                        return;
                    }

                    var dstSpans = new List<SortedSpan>();
                    Int32 i = 0;

                    // Merge neighboring pairs of spans
                    Int32 numPairs = srcSpans.Count / 2;
                    for (Int32 p = 0; p < numPairs; p++)
                    {
                        Merge<T>(src, srcSpans[i].Start, srcSpans[i].Length,
                                 src, srcSpans[i + 1].Start, srcSpans[i + 1].Length,
                                 dst, srcSpans[i].Start,
                                 comparer);
                        dstSpans.Add(new SortedSpan { Start = srcSpans[i].Start, Length = srcSpans[i].Length + srcSpans[i + 1].Length });
                        i += 2;
                    }
                    // Copy the last left over odd segment (if there is one) from src to dst and add it to dstSpans
                    if (i == (srcSpans.Count - 1))
                    {
                        Array.Copy(src, srcSpans[i].Start, dst, srcSpans[i].Start, srcSpans[i].Length);
                        dstSpans.Add(new SortedSpan { Start = srcSpans[i].Start, Length = srcSpans[i].Length });
                    }
                    srcSpans = dstSpans;
                    var tmp = src;          // swap src and dst arrays
                    src = dst;
                    dst = tmp;
                    srcToDst = srcToDst ? false : true; // keep track of merge direction
                }
            }
        }

        // This implementation uses a fixed size priority queue to extract the min element in O(1) time and to insert a new element O(lgK) time, where K is the K-way merge and K is known in advance
        // since we know how many spans are being merged. Performs multi-way merge in one pass, from the source to destination.
        static public void MergeMulti<T>(T[] src, List<SortedSpan> srcSpans,
                                         T[] dst,
                                         IComparer<T> comparer = null)
        {
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
                while (srcSpans.Count > 2)
                {
                    var dstSpans = new List<SortedSpan>();
                    Int32 i = 0;

                    // Merge neighboring pairs of spans
                    Int32 numPairs = srcSpans.Count / 2;
                    for (Int32 p = 0; p < numPairs; p++)
                    {
                        Merge<T>(src, srcSpans[i].Start, srcSpans[i].Length,
                                 src, srcSpans[i + 1].Start, srcSpans[i + 1].Length,
                                 dst, srcSpans[i].Start,
                                 comparer);
                        dstSpans.Add(new SortedSpan { Start = srcSpans[i].Start, Length = srcSpans[i].Length + srcSpans[i + 1].Length });
                        i += 2;
                    }
                    // Copy the last left over odd segment (if there is one) from src to dst and add it to dstSpans
                    if (i == (srcSpans.Count - 1))
                    {
                        Array.Copy(src, srcSpans[i].Start, dst, srcSpans[i].Start, srcSpans[i].Length);
                        dstSpans.Add(new SortedSpan { Start = srcSpans[i].Start, Length = srcSpans[i].Length });
                    }
                    srcSpans = dstSpans;
                    var tmp = src;          // swap src and dst arrays
                    src = dst;
                    dst = tmp;
                    srcToDst = srcToDst ? false : true; // keep track of merge direction
                }
                if (srcSpans.Count == 2)
                {
                    // TODO: call a 2-way merge
                }
                else if (srcSpans.Count == 1)
                {
                    Array.Copy(src, srcSpans[0].Start, dst, srcSpans[0].Start, srcSpans[0].Length);
                }
            }
        }

        static public void MergeThree<T>(T[] a, Int32 aStart, Int32 aLength,
                                         T[] b, Int32 bStart, Int32 bLength,
                                         T[] c, Int32 cStart, Int32 cLength,
                                         T[] dst, Int32 dstStart,
                                         IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
            Int32 cEnd = cStart + cLength - 1;
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
                Merge(b, bStart, bLength, c, cStart, cLength, dst, dstStart, equalityComparer);
            }
            else if (bStart > bEnd)
            {
                Merge(a, aStart, aLength, c, cStart, cLength, dst, dstStart, equalityComparer);
            }
            else   // (cStart > cEnd)
            {
                Merge(a, aStart, aLength, b, bStart, bLength, dst, dstStart, equalityComparer);
            }
        }
        // Strategy is to handle 4 segments while 4 are available, 3 while 3 are available, 2 while 2 are available
        // This extends the strategy used for merging two segments nicely
        static public void MergeFour<T>(T[] a, Int32 aStart, Int32 aLength,
                                        T[] b, Int32 bStart, Int32 bLength,
                                        T[] c, Int32 cStart, Int32 cLength,
                                        T[] d, Int32 dStart, Int32 dLength,
                                        T[] dst, Int32 dstStart,
                                        IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
            Int32 cEnd = cStart + cLength - 1;
            Int32 dEnd = dStart + dLength - 1;
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
                MergeThree(b, bStart, bLength, c, cStart, cLength, d, dStart, dLength, dst, dstStart, equalityComparer);
            }
            else if (bStart > bEnd)
            {
                MergeThree(a, aStart, aLength, c, cStart, cLength, d, dStart, dLength, dst, dstStart, equalityComparer);
            }
            else if (cStart > cEnd)
            {
                MergeThree(a, aStart, aLength, b, bStart, bLength, d, dStart, dLength, dst, dstStart, equalityComparer);
            }
            else   // (dStart > dEnd)
            {
                MergeThree(a, aStart, aLength, b, bStart, bLength, c, cStart, cLength, dst, dstStart, equalityComparer);
            }
        }

        /// <summary>
        /// Smaller than threshold will use non-divide-and-conquer algorithm to merge arrays
        /// </summary>
        public static Int32 MergeArrayThreshold { get; set; } = 4 * 1024;
        /// <summary>
        /// Divide-and-Conquer Merge of two ranges of source array src[ p1 .. r1 ] and src[ p2 .. r2 ] into destination array starting at index p3.
        /// This merge is not stable.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="aStart">starting index of the first  segment, inclusive</param>
        /// <param name="aEnd">ending   index of the first  segment, inclusive</param>
        /// <param name="bStart">starting index of the second segment, inclusive</param>
        /// <param name="bEnd">ending   index of the second segment, inclusive</param>
        /// <param name="dst">destination array</param>
        /// <param name="p3">starting index of the result</param>
        /// <param name="comparer">method to compare array elements</param>
        public static void MergeDivideAndConquer<T>(T[] src, Int32 aStart, Int32 aEnd, Int32 bStart, Int32 bEnd, T[] dst, Int32 p3, IComparer<T> comparer = null)
        {
            //Console.WriteLine("#1 " + aStart + " " + aEnd + " " + bStart + " " + bEnd);
            Int32 length1 = aEnd - aStart + 1;
            Int32 length2 = bEnd - bStart + 1;
            if (length1 < length2)
            {
                HPCsharp.ParallelAlgorithm.Exchange(ref aStart, ref bStart);
                HPCsharp.ParallelAlgorithm.Exchange(ref aEnd, ref bEnd);
                HPCsharp.ParallelAlgorithm.Exchange(ref length1, ref length2);
            }
            if (length1 == 0) return;
            if ((length1 + length2) <= MergeArrayThreshold)
            {
                //Console.WriteLine("Merge: aStart = {0} length1 = {1} bStart = {2} length2 = {3} p3 = {2}", aStart, length1, bStart, length2, p3);
                Merge<T>(src, aStart, length1,
                         src, bStart, length2,
                         dst, p3, comparer);            // in Dr. Dobb's Journal paper
            }
            else
            {
                Int32 q1 = (aStart + aEnd) / 2;
                Int32 q2 = Algorithm.BinarySearch(src[q1], src, bStart, bEnd, comparer);
                Int32 q3 = p3 + (q1 - aStart) + (q2 - bStart);
                dst[q3] = src[q1];
                MergeDivideAndConquer(src, aStart, q1 - 1, bStart, q2 - 1, dst, p3, comparer);  // lower half
                MergeDivideAndConquer(src, q1 + 1, aEnd, q2, bEnd, dst, q3 + 1, comparer);  // upper half
            }
        }
    }
}

// These are work in progress and should not be used until it has been moved to the Algorithm namespace
namespace HPCsharpExperimental
{
    static public partial class Algorithm
    {
        public static void MergeDivideAndConquerExperimental<T>(T[] src, Int32 aStart, Int32 aEnd, Int32 bStart, Int32 bEnd, T[] dst, Int32 p3, IComparer<T> comparer = null)
        {
            //var equalityComparer = comparer ?? Comparer<T>.Default;
            //Console.WriteLine("#1 " + p1 + " " + r1 + " " + p2 + " " + r2);
            Int32 length1 = aEnd - aStart + 1;
            Int32 length2 = bEnd - bStart + 1;
            if (length1 < length2)
            {
                HPCsharp.ParallelAlgorithm.Exchange(ref aStart, ref bStart);
                HPCsharp.ParallelAlgorithm.Exchange(ref aEnd, ref bEnd);
                HPCsharp.ParallelAlgorithm.Exchange(ref length1, ref length2);
            }
            if (length1 == 0) return;
            if ((length1 + length2) <= HPCsharp.Algorithm.MergeArrayThreshold)
            {
                Console.WriteLine("Merge: aStart = {0} length1 = {1} bStart = {2} length2 = {3} p3 = {2}", aStart, length1, bStart, length2, p3);
                HPCsharp.Algorithm.Merge<T>(src, aStart, length1,
                                            src, bStart, length2,
                                            dst, p3, comparer);            // in Dr. Dobb's Journal paper
            }
            else
            {
                Int32 q1 = (aStart + aEnd) / 2;
                Int32 q2 = HPCsharp.Algorithm.BinarySearch(src[q1], src, bStart, bEnd, comparer);
                Int32 q3 = p3 + (q1 - aStart) + (q2 - bStart);
                // TODO: The flaw seems to be when q2 == bStart. In this case, there may not be a split of the [bStart-bEnd] range, and we need to detect and account for this case
                //       as we always need the left part of the b-range-split to be strictly less than the q1 element. Otherwise, we may not even need to move the q1 element (maybe)
                Console.WriteLine("aStart = {0} aEnd = {1} bStart = {2} bEnd = {3} q1 = {4} q2 = {5} q3 = {6} p3 = {7}", aStart, aEnd, bStart, bEnd, q1, q2, q3, p3);
                // When the src[bStart] is equal to src[q1], BinarySearch will return bStart as q1
                // However, to support stability 
                if (q2 > bStart)
                {
                    dst[q3] = src[q1];
                    Console.Write("Before Merge: ");
                    foreach (var p in dst) Console.Write(p);
                    Console.WriteLine();
                    MergeDivideAndConquerExperimental(src, aStart, q1 - 1, bStart, q2 - 1, dst, p3, comparer);  // lower half
                    Console.Write("Between Merges: ");
                    foreach (var p in dst) Console.Write(p);
                    Console.WriteLine();
                    MergeDivideAndConquerExperimental(src, q1 + 1, aEnd, q2, bEnd, dst, q3 + 1, comparer);  // upper half
                    Console.Write("After Both Merges: ");
                    foreach (var p in dst) Console.Write(p);
                    Console.WriteLine();
                }
                else  // q2 <= bStart which implies no split for segment B
                {
                    //dst[q3] = src[q1];
                    Console.Write("Before Merge: ");
                    foreach (var p in dst) Console.Write(p);
                    Console.WriteLine();
                    MergeDivideAndConquerExperimental(src, aStart, q1 - 1, bStart, q2 - 1, dst, p3, comparer); // lower half
                    Console.Write("Between Merges: ");
                    foreach (var p in dst) Console.Write(p);
                    Console.WriteLine();
                    MergeDivideAndConquerExperimental(src, q1 + 1, aEnd, q2, bEnd, dst, q3 + 1, comparer);  // upper half
                    Console.Write("After Both Merges: ");
                    foreach (var p in dst) Console.Write(p);
                    Console.WriteLine();
                }
            }
        }
        // Unrolled version, which is useful for debug
        public static void MergeDivideAndConquerUnrolled<T>(T[] src, Int32 aStart, Int32 aEnd, Int32 bStart, Int32 bEnd, T[] dst, Int32 p3, IComparer<T> comparer = null)
        {
            //var equalityComparer = comparer ?? Comparer<T>.Default;
            //Console.WriteLine("#1 " + p1 + " " + r1 + " " + p2 + " " + r2);
            Int32 length1 = aEnd - aStart + 1;
            Int32 length2 = bEnd - bStart + 1;
            if (length1 >= length2)             // A segment is longer than B segment
            {
                Console.WriteLine("length1 >= length2");
                if (length1 == 0) return;
                if ((length1 + length2) <= HPCsharp.Algorithm.MergeArrayThreshold)
                {
                    Console.WriteLine("Merge: aStart = {0} length1 = {1} bStart = {2} length2 = {3} p3 = {2}", aStart, length1, bStart, length2, p3);
                    HPCsharp.Algorithm.Merge<T>(src, aStart, length1,
                                                src, bStart, length2,
                                                dst, p3, comparer);            // in Dr. Dobb's Journal paper
                }
                else
                {
                    Int32 q1 = (aStart + aEnd) / 2;
                    Int32 q2 = HPCsharp.Algorithm.BinarySearch(src[q1], src, bStart, bEnd, comparer);
                    Int32 q3 = p3 + (q1 - aStart) + (q2 - bStart);
                    // TODO: The flaw seems to be when q2 == bStart. In this case, there may not be a split of the [bStart-bEnd] range, and we need to detect and account for this case
                    //       as we always need the left part of the b-range-split to be strictly less than the q1 element. Otherwise, we may not even need to move the q1 element (maybe)
                    Console.WriteLine("aStart = {0} aEnd = {1} bStart = {2} bEnd = {3} q1 = {4} q2 = {5} q3 = {6} p3 = {7}", aStart, aEnd, bStart, bEnd, q1, q2, q3, p3);
                    dst[q3] = src[q1];
                    Console.Write("Before Merge: ");
                    foreach (var p in dst) Console.Write(p);
                    Console.WriteLine();
                    MergeDivideAndConquerUnrolled(src, aStart, q1 - 1, bStart, q2 - 1, dst, p3, comparer);  // lower half
                    Console.Write("Between Merges: ");
                    foreach (var p in dst) Console.Write(p);
                    Console.WriteLine();
                    MergeDivideAndConquerUnrolled(src, q1 + 1, aEnd, q2, bEnd, dst, q3 + 1, comparer);      // upper half
                    Console.Write("After Both Merges: ");
                    foreach (var p in dst) Console.Write(p);
                    Console.WriteLine();
                }
            }
            else  // length2 > length1        B segment is longer than A segment
            {
                Console.WriteLine("length2 > length1");
                if (length2 == 0) return;
                if ((length1 + length2) <= HPCsharp.Algorithm.MergeArrayThreshold)
                {
                    Console.WriteLine("Merge: aStart = {0} length1 = {1} bStart = {2} length2 = {3} p3 = {2}", aStart, length1, bStart, length2, p3);
                    HPCsharp.Algorithm.Merge<T>(src, aStart, length1,
                                                src, bStart, length2,
                                                dst, p3, comparer);            // in Dr. Dobb's Journal paper
                }
                else
                {
                    Int32 q1 = (bStart + bEnd) / 2;
                    Int32 q2 = HPCsharp.Algorithm.BinarySearch(src[q1], src, aStart, aEnd, comparer);
                    Int32 q3 = p3 + (q1 - bStart) + (q2 - aStart);
                    // TODO: The flaw seems to be when q2 == bStart. In this case, there may not be a split of the [bStart-bEnd] range, and we need to detect and account for this case
                    //       as we always need the left part of the b-range-split to be strictly less than the q1 element. Otherwise, we may not even need to move the q1 element (maybe)
                    Console.WriteLine("aStart = {0} aEnd = {1} bStart = {2} bEnd = {3} q1 = {4} q2 = {5} q3 = {6} p3 = {7}", aStart, aEnd, bStart, bEnd, q1, q2, q3, p3);
                    dst[q3] = src[q1];
                    Console.Write("Before Merge: ");
                    foreach (var p in dst) Console.Write(p);
                    Console.WriteLine();
                    MergeDivideAndConquerUnrolled(src, bStart, q1 - 1, aStart, q2 - 1, dst, p3, comparer);  // upper half
                    Console.Write("Between Merges: ");
                    foreach (var p in dst) Console.Write(p);
                    Console.WriteLine();
                    MergeDivideAndConquerUnrolled(src, q1 + 1, bEnd, q2, aEnd, dst, q3 + 1, comparer);      // lower half
                    Console.Write("After Both Merges: ");
                    foreach (var p in dst) Console.Write(p);
                    Console.WriteLine();
                }
            }
        }
    }

}