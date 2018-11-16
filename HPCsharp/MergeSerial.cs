// TODO: Implement stable Merge Sort, possibly by using the divide-and-conquer implementation of Merge, since it's stable. Compare performance to
//       not-stable .Sort and stable LINQ sorting for fair comparison.
// TODO: See if implementing stable IEnumerable sorting is faster than LINQ sorting, since that's the only stable one.
// TODO: Compare performance of sorting arrays and lists of classes with .Sort and LINQ to see if the advantages are bigger or smaller
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
        static public void Merge<T>(List<T> a,   Int32 aStart, Int32 aLength,
                                    List<T> b,   Int32 bStart, Int32 bLength,
                                    List<T> dst, Int32 dstStart,
                                    Comparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
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
        /// <param name="aStart">starting index of the first sorted List segment, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="bStart">starting index of the second sorted List segment, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dst">destination List where the result of two merged Lists is to be placed</param>
        /// <param name="dstStart">starting index within the destination List where the merged sorted List is to be placed</param>
        /// <param name="comparer">optional method to compare array elements</param>
        static public void Merge<T>(List<T> src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
                                    List<T> dst, Int32 dstStart, Comparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
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
        static public void Merge(int[] a,   Int32 aStart, Int32 aLength,
                                 int[] b,   Int32 bStart, Int32 bLength,
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
        static public void Merge<T>(T[] a,   Int32 aStart, Int32 aLength,
                                    T[] b,   Int32 bStart, Int32 bLength,
                                    T[] dst, Int32 dstStart,
                                    Comparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
            while (aStart <= aEnd && bStart <= bEnd)
            {
                // a[aStart] <= b[bStart]
                if (equalityComparer.Compare(a[aStart], b[bStart]) <= 0)   	// if elements are equal, then a[] element is output
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
        /// Merge multiple sorted array spans, placing the result into a destination array as a single sorted span.
        /// The destination array must be as big as the source, otherwise an ArgumentException is thrown.
        /// The source array is modified during processing.
        /// </summary>
        /// <param name="src">source Array to be merged</param>
        /// <param name="srcSpans">List of sorted segments, specified by starting index and length</param>
        /// <param name="dst">destination Array where the result of merged segments is to be placed</param>
        /// <param name="comparer">optional method to compare array elements</param>
        static public void Merge<T>(T[] src, List<SortedSpan> srcSpans,
                                    T[] dst,
                                    Comparer<T> comparer = null)
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
                        Merge<T>(src, srcSpans[i    ].Start, srcSpans[i    ].Length,
                                 src, srcSpans[i + 1].Start, srcSpans[i + 1].Length,
                                 dst, srcSpans[i    ].Start,
                                 comparer);
                        dstSpans.Add(new SortedSpan { Start = srcSpans[i].Start, Length = srcSpans[i + 1].Length });
                        i += 2;
                    }
                    // Copy the last left over odd segment (if there is one) from src to dst and add it to dstSpans
                    if (i > srcSpans.Count)
                    {
                        Array.Copy(src, srcSpans[i - 1].Start, dst, srcSpans[i - 1].Start, srcSpans[i - 1].Length);
                        dstSpans.Add(new SortedSpan { Start = srcSpans[i - 1].Start, Length = srcSpans[i - 1].Length });
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
        /// <param name="aStart">starting index of the first sorted segment, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="bStart">starting index of the second sorted segment, inclusive</param>
        /// <param name="bLength">length of the first sorted segment</param>
        /// <param name="dst">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        /// <param name="comparer">optional method to compare array elements</param>
        static public void Merge<T>(T[] src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
                                    T[] dst, Int32 dstStart, Comparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
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

        static public void MergeThree<T>(T[] a,   Int32 aStart, Int32 aLength,
                                         T[] b,   Int32 bStart, Int32 bLength,
                                         T[] c,   Int32 cStart, Int32 cLength,
                                         T[] dst, Int32 dstStart,
                                         Comparer<T> comparer = null)
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
        static public void MergeFour<T>(T[] a,   Int32   aStart, Int32 aLength,
                                        T[] b,   Int32   bStart, Int32 bLength,
                                        T[] c,   Int32   cStart, Int32 cLength,
                                        T[] d,   Int32   dStart, Int32 dLength,
                                        T[] dst, Int32 dstStart,
                                        Comparer<T> comparer = null)
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

// TODO: Make Inner routines of "internal" access instead of public. Or, Move the "Inner" routines into a class of their own AlgorithmsInner, to be able to use them by others inside, but not have to expose them
// TODO: Rename SeqMerge to MergeSerial
// TODO: Split off Merge Sorting into MergeSortSerial, which will follow the file naming pattern of parallel ones.
// TODO: Change file names of others to follow the same pattern.

        /// <summary>
        /// Smaller than threshold will use non-divide-and-conquer algorithm to merge arrays
        /// </summary>
        static Int32 MergeArrayThreshold { get; set; } = 8192;
        /// <summary>
        /// Divide-and-Conquer Merge of two ranges of source array src[ p1 .. r1 ] and src[ p2 .. r2 ] into destination array starting at index p3.
        /// This is a stable merge
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
        internal static void MergeDivideAndConquer<T>(T[] src, Int32 aStart, Int32 aEnd, Int32 bStart, Int32 bEnd, T[] dst, Int32 p3, Comparer<T> comparer = null)
        {
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
            if ((length1 + length2) <= MergeArrayThreshold)
            {
                //Console.WriteLine("#3 " + p1 + " " + length1 + " " + p2 + " " + length2 + " " + p3);
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
                MergeDivideAndConquer(src, aStart, q1 - 1, bStart, q2 - 1, dst, p3, comparer);
                MergeDivideAndConquer(src, q1 + 1, aEnd, q2, bEnd, dst, q3 + 1, comparer);
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
