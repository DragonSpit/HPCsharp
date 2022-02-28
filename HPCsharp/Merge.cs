// TODO: Implement a knob for the merge algorithm, to use multi-merge and specify how many way to split the merge, for divide-and-conquer too.
// TODO: Multi-merge should use 2-way, 3-way and possibly 4-way before using the more general multi-way merge to go faster.
// TODO: Is it faster to copy a List to an Array, then do the merge and then to copy the result back to a List? Currently, List merge runs at 1/2 the speed of Array merge.
// TODO: Does it pay off to use the parallel copy.
// TODO: Change all in-place sorting functions to return the original reference to improve Functional composition and pipelining, since it's returning void anyways.
// TODO: Refactor other methods to possibly return a destination, if it makes sense.
// TODO: Develop a faster version of Divide-And-Conquer in-place merge which uses a not-in-place merge as the recursion termination base case
//       allocating an array and copying back from it, as is done in C++ std::inplace_merge to provide a faster in-place merge
//       Offer two version of Divide-And-Conquer in-place merge "purely in-place" (no additional allocations) and "adaptive in-place" (additional allocation
//       when memory is available). May be able to do a single allocation of a full size array and use it for merging.
// TODO: Parallelize block-swap algorithm to speedup in-place merge, possibly using SSE with instruction to reverse order within an SSE Vector
// TODO: For inner merge could we process a chunk at a time without comparisons for the ending conditions? We could take the min(A.Length, B.Length) and run for that
//       with only element comparison and length comparison, and then switch to the Merge2 clever comparison reduction method to finish the operation, or do it again
//       with left-over pieces of A and B. This method would pay even higher dividends for multi-way merge. I may have done this already in C++ and need to check.
//       This idea may be more CPU architecture friendly, due to fewer mispredictions, since the for loop one is easy to predict.
// TODO: Test Array.Copy versus copying using a for loop for the merge algorithms. Figure out the size threshold when the for loop is faster.
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
            //Console.WriteLine("Merge: aStart = {0}  dstStart = {1}  length = {2}", aStart, dstStart, aEnd - aStart + 1);
            //Array.Copy(src, aStart, dst, dstStart, aEnd - aStart + 1);
            //ParallelAlgorithms.Copy.CopySse(src, aStart, dst, dstStart, aEnd - aStart + 1);
            while (aStart <= aEnd) dst[dstStart++] = src[aStart++];       // copy(a[aStart, aEnd] to dst[dstStart]
            //Console.WriteLine("Merge: bStart = {0}  dstStart = {1}  length = {2}", bStart, dstStart, bEnd - bStart + 1);
            //Array.Copy(src, bStart, dst, dstStart, bEnd - bStart + 1);
            //ParallelAlgorithms.Copy.CopySse(src, bStart, dst, dstStart, bEnd - bStart + 1);
            while (bStart <= bEnd) dst[dstStart++] = src[bStart++];
        }
        /// <summary>
        /// Uses the faster Copy() when copying the remainder of spans
        /// Merge two sorted spans of an Array of int's, placing the result into a destination Array, starting at an index.
        /// </summary>
        /// <param name="src">source Array of int's with two sorted segments to be merged</param>
        /// <param name="aStart">starting index of the first sorted Array, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="bStart">starting index within the second sorted Array, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dst">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        static public void MergeWithCopy(int[] src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
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
            //Console.WriteLine("Merge: aStart = {0}  dstStart = {1}  length = {2}", aStart, dstStart, aEnd - aStart + 1);
            Copy(src, aStart, dst, dstStart, aEnd - aStart + 1);
            //ParallelAlgorithms.Copy.CopySse(src, aStart, dst, dstStart, aEnd - aStart + 1);
            //while (aStart <= aEnd) dst[dstStart++] = src[aStart++];       // copy(a[aStart, aEnd] to dst[dstStart]
            //Console.WriteLine("Merge: bStart = {0}  dstStart = {1}  length = {2}", bStart, dstStart, bEnd - bStart + 1);
            Copy(src, bStart, dst, dstStart, bEnd - bStart + 1);
            //ParallelAlgorithms.Copy.CopySse(src, bStart, dst, dstStart, bEnd - bStart + 1);
            //while (bStart <= bEnd) dst[dstStart++] = src[bStart++];
        }
        /// <summary>
        /// A slightly faster implementation, which performs only a single array boundary comparison for each loop
        /// Merge two sorted segments of an Array of int's, placing the result into a destination Array, starting at an index.
        /// </summary>
        /// <param name="src">source Array of int's with two sorted segments to be merged</param>
        /// <param name="aStart">starting index of the first sorted Array, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="bStart">starting index within the second sorted Array, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dst">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        static public void MergeFaster(int[] src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
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
        /// <summary>
        /// A slightly faster implementation, which performs only a single array boundary comparison for each loop.
        /// Also performs an optimized copy of the remainder of each span.
        /// Merge two sorted segments of an Array of int's, placing the result into a destination Array, starting at an index.
        /// </summary>
        /// <param name="src">source Array of int's with two sorted segments to be merged</param>
        /// <param name="aStart">starting index of the first sorted Array, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="bStart">starting index within the second sorted Array, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dst">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        static public void MergeFasterWithCopy(int[] src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
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
            Copy(src, aStart, dst, dstStart, aEnd - aStart + 1);
            Copy(src, bStart, dst, dstStart, bEnd - bStart + 1);
        }
        /// <summary>
        /// Merge two sorted spans of an Array of int's, placing the result into a destination Array, starting at an index.
        /// Algorithm minimize boundary comparisons by comparing lengths of the two input spans and running a for loop for minimum of them.
        /// </summary>
        /// <param name="src">source Array of int's with two sorted segments to be merged</param>
        /// <param name="aStart">starting index of the first sorted Array, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="bStart">starting index within the second sorted Array, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dst">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        static public void MergeBySpans(int[] src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
                                        int[] dst, Int32 dstStart)
        {
            Int32 aEnd = aStart + aLength - 1;      // inclusive
            Int32 bEnd = bStart + bLength - 1;
            while (aLength > 0 && bLength > 0)
            {
                Int32 numElements = Math.Min(aLength, bLength);
                for (Int32 i = 0; i < numElements; i++)
                {
                    if (src[aStart] <= src[bStart])         // if elements are equal, then a[] element is output
                        dst[dstStart++] = src[aStart++];
                    else
                        dst[dstStart++] = src[bStart++];
                }
                aLength = aEnd - aStart + 1;
                bLength = bEnd - bStart + 1;
            }
            //Array.Copy(src, aStart, dst, dstStart, aEnd - aStart + 1);
            while (aStart <= aEnd) dst[dstStart++] = src[aStart++];       // copy(a[aStart, aEnd] to dst[dstStart]
            //Array.Copy(src, bStart, dst, dstStart, bEnd - bStart + 1);
            while (bStart <= bEnd) dst[dstStart++] = src[bStart++];
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
        static public void Merge5(int[] src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
                                  int[] dst, Int32 dstStart,
                                  Int32 threshold = 1024)
        {
            Int32 numElements;
            Int32 aEnd = aStart + aLength - 1;      // inclusive
            Int32 bEnd = bStart + bLength - 1;

            while (true)
            {
                if (aLength <= bLength)
                {
                    if (aLength < threshold)
                    {
                        MergeFaster(src, aStart, aLength, bStart, bLength, dst, dstStart);
                        return;
                    }
                    else
                        numElements = aLength;
                }
                else
                {
                    if (bLength < threshold)
                    {
                        MergeFaster(src, aStart, aLength, bStart, bLength, dst, dstStart);
                        return;
                    }
                    else
                        numElements = bLength;
                }

                for (Int32 i = 0; i < numElements; i++)     // more predictable comparisons, since these aren't data dependent, easier for branch prediction
                {
                    if (src[aStart] <= src[bStart])         // if elements are equal, then a[] element is output
                        dst[dstStart++] = src[aStart++];
                    else
                        dst[dstStart++] = src[bStart++];
                }
                aLength = aEnd - aStart + 1;
                bLength = bEnd - bStart + 1;
            }
        }

        static public void Merge6(int[] src, Int32 aStart, Int32 aLength,
                                             Int32 bStart, Int32 bLength,
                                  int[] dst, Int32 dstStart,
                                  Int32 threshold = 1024)
        {
            Int32 numElements;
            Int32 aEnd = aStart + aLength - 1;      // inclusive
            Int32 bEnd = bStart + bLength - 1;

            while (true)
            {
                if (aLength <= bLength)
                {
                    if (aLength < threshold)
                    {
                        MergeFaster(src, aStart, aLength, bStart, bLength, dst, dstStart);
                        return;
                    }
                    else
                        numElements = aLength;
                }
                else
                {
                    if (bLength < threshold)
                    {
                        MergeFaster(src, aStart, aLength, bStart, bLength, dst, dstStart);
                        return;
                    }
                    else
                        numElements = bLength;
                }

                Int32 dstEnd = dstStart + numElements - 1;

                while (dstStart <= dstEnd)                  // single comparison
                {
                    if (src[aStart] <= src[bStart])         // if elements are equal, then a[] element is output
                        dst[dstStart++] = src[aStart++];
                    else
                        dst[dstStart++] = src[bStart++];
                }
                aLength = aEnd - aStart + 1;
                bLength = bEnd - bStart + 1;
            }
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
                dst[dstStart++] = equalityComparer.Compare(a[aStart], b[bStart]) <= 0 ? a[aStart++] : b[bStart++];      // if elements are equal, then a[] element is output
            }
            if (aStart <= aEnd)
                Array.Copy(a, aStart, dst, dstStart, aEnd - aStart + 1);
            //while (aStart <= aEnd) dst[dstStart++] = a[aStart++];    // copy(a[aStart, aEnd] to dst[dstStart]
            if (bStart <= bEnd)
                Array.Copy(b, bStart, dst, dstStart, bEnd - bStart + 1);
            //while (bStart <= bEnd) dst[dstStart++] = b[bStart++];
        }

        /// <summary>
        /// Merge two sorted Array segments, placing the result into a destination Array, starting at an index.
        /// Uses an optimized copy, to copy the remainder of spans.
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
        static public void MergeWithCopy<T>(T[] a, Int32 aStart, Int32 aLength,
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
            if (aStart <= aEnd)
                Copy(a, aStart, dst, dstStart, aEnd - aStart + 1);
            //while (aStart <= aEnd) dst[dstStart++] = a[aStart++];    // copy(a[aStart, aEnd] to dst[dstStart]
            if (bStart <= bEnd)
                Copy(b, bStart, dst, dstStart, bEnd - bStart + 1);
            //while (bStart <= bEnd) dst[dstStart++] = b[bStart++];
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
        static public void MergeFaster<T>(T[] a, Int32 aStart, Int32 aLength,
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
            if (aStart <= aEnd)
                Array.Copy(a, aStart, dst, dstStart, aEnd - aStart + 1);
            //while (aStart <= aEnd) dst[dstStart++] = a[aStart++];    // copy(a[aStart, aEnd] to dst[dstStart]
            if (bStart <= bEnd)
                Array.Copy(b, bStart, dst, dstStart, bEnd - bStart + 1);
            //while (bStart <= bEnd) dst[dstStart++] = b[bStart++];
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
        static public void Merge<T>(T[] a, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
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
        static public void MergeFaster<T>(T[] src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
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
        /// Merge two sorted Array segments, placing the result into a destination Array, starting at an index.
        /// </summary>
        /// <param name="aKeys">first source Array to be merged</param>
        /// <param name="aStart">starting index of the first sorted Array, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="bKeys">second source Array to be merged</param>
        /// <param name="bStart">starting index of the second sorted Array, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dstKeys">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        /// <param name="comparer">optional method to compare array elements</param>
        static public void Merge<T1, T2>(T1[] aKeys,   T2[] aItems,   Int32 aStart, Int32 aLength,
                                         T1[] bKeys,   T2[] bItems,   Int32 bStart, Int32 bLength,
                                         T1[] dstKeys, T2[] dstItems, Int32 dstStart,
                                         IComparer<T1> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T1>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;

            while (aStart <= aEnd && bStart <= bEnd)
            {
                if (equalityComparer.Compare(aKeys[aStart], bKeys[bStart]) <= 0)        // if elements are equal, then a[] element is output
                {
                    dstKeys[ dstStart  ] = aKeys[ aStart  ];
                    dstItems[dstStart++] = aItems[aStart++];
                }
                else
                {
                    dstKeys[ dstStart  ] = bKeys[ bStart  ];
                    dstItems[dstStart++] = bItems[bStart++];
                }
            }

            //Array.Copy(a, aStart, dst, dstStart, aEnd - aStart + 1);
            while (aStart <= aEnd)
            {
                dstKeys[ dstStart  ] = aKeys[ aStart  ];    // copy(a[aStart, aEnd] to dst[dstStart]
                dstItems[dstStart++] = aItems[aStart++];
            }
            //Array.Copy(b, bStart, dst, dstStart, bEnd - bStart + 1);
            while (bStart <= bEnd)
            {
                dstKeys[ dstStart  ] = bKeys[ bStart  ];
                dstItems[dstStart++] = bItems[bStart++];
            }
        }

        static public void MergeBySpans<T>(T[] src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
                                           T[] dst, Int32 dstStart, IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;      // inclusive
            Int32 bEnd = bStart + bLength - 1;
            while (aLength > 0 && bLength > 0)
            {
                Int32 numElements = Math.Min(aLength, bLength);
                for (Int32 i = 0; i < numElements; i++)
                {
                    if (equalityComparer.Compare(src[aStart], src[bStart]) <= 0)   	// if elements are equal, then a[] element is output
                        dst[dstStart++] = src[aStart++];
                    else
                        dst[dstStart++] = src[bStart++];
                }
                aLength = aEnd - aStart + 1;
                bLength = bEnd - bStart + 1;
            }
            MergeFaster(src, aStart, aLength, bStart, bLength, dst, dstStart, comparer);
        }

        static public void MergeBySpans<T>(T[] src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
                                           T[] dst, Int32 dstStart, IComparer<T> comparer = null, Int32 threshold = 100)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;      // inclusive
            Int32 bEnd = bStart + bLength - 1;
            while (aLength > 0 && bLength > 0)
            {
                Int32 numElements = Math.Min(aLength, bLength);
                if (numElements > threshold)
                    break;
                for (Int32 i = 0; i < numElements; i++)
                {
                    if (equalityComparer.Compare(src[aStart], src[bStart]) <= 0)   	// if elements are equal, then a[] element is output
                        dst[dstStart++] = src[aStart++];
                    else
                        dst[dstStart++] = src[bStart++];
                }
                aLength = aEnd - aStart + 1;
                bLength = bEnd - bStart + 1;
            }
            //Array.Copy(src, aStart, dst, dstStart, aEnd - aStart + 1);
            while (aStart <= aEnd) dst[dstStart++] = src[aStart++];       // copy(a[aStart, aEnd] to dst[dstStart]
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

        /// <summary>
        /// This implementation uses a fixed size priority queue to extract the min element in O(1) time and to insert a new element O(lgK) time, where K is the K-way merge and K is known in advance
        /// since we know how many spans are being merged. Performs multi-way merge in one pass, from the source to destination.
        /// </summary>
        /// <param name="src">source Array to be merged</param>
        /// <param name="srcSpans">List of sorted segments, specified by starting index and length</param>
        /// <param name="dst">destination Array where the result of merged segments is to be placed</param>
        /// <param name="comparer">optional method to compare array elements</param>
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

        static public void MergeThreeWay<T>(T[] src, Int32 aStart, Int32 aLength,
                                                     Int32 bStart, Int32 bLength,
                                                     Int32 cStart, Int32 cLength,
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
                if (equalityComparer.Compare(src[aStart], src[bStart]) <= 0)
                {   // a <= b
                    dst[dstStart++] = equalityComparer.Compare(src[aStart], src[cStart]) <= 0 ? src[aStart++] : src[cStart++];
                }
                else
                {   // b < a
                    dst[dstStart++] = equalityComparer.Compare(src[bStart], src[cStart]) <= 0 ? src[bStart++] : src[cStart++];
                }
            }
            // Ran out of elements in one of the segments - i.e. 2 segments are available for merging, but which 2
            // Length needs to be adjusted, to be lengths left yet to be merged for each segment
            aLength = aEnd - aStart + 1;
            bLength = bEnd - bStart + 1;
            cLength = cEnd - cStart + 1;
            if (aStart > aEnd)
            {
                Merge(src, bStart, bLength, cStart, cLength, dst, dstStart, equalityComparer);
            }
            else if (bStart > bEnd)
            {
                Merge(src, aStart, aLength, cStart, cLength, dst, dstStart, equalityComparer);
            }
            else   // (cStart > cEnd)
            {
                Merge(src, aStart, aLength, bStart, bLength, dst, dstStart, equalityComparer);
            }
        }

        static public void MergeThreeWay2<T>(T[] src, Int32 aStart, Int32 aLength,
                                                      Int32 bStart, Int32 bLength,
                                                      Int32 cStart, Int32 cLength,
                                             T[] dst, Int32 dstStart,
                                             IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
            Int32 cEnd = cStart + cLength - 1;
            while (aStart <= aEnd && bStart <= bEnd && cStart <= cEnd)
            {
                while (true)
                {
                    // a[aStart] <= b[bStart]
                    if (equalityComparer.Compare(src[aStart], src[bStart]) <= 0)
                    {   // a <= b
                        if (equalityComparer.Compare(src[aStart], src[cStart]) <= 0)
                        {
                            dst[dstStart++] = src[aStart++];   // a is smallest
                            if (aStart > aEnd) break;
                        }
                        else
                        {
                            dst[dstStart++] = src[cStart++];   // c is smallest
                            if (cStart > cEnd) break;
                        }
                    }
                    else
                    {   // b < a
                        if (equalityComparer.Compare(src[bStart], src[cStart]) <= 0)
                        {
                            dst[dstStart++] = src[bStart++];   // b is smallest
                            if (bStart > bEnd) break;
                        }
                        else
                        {
                            dst[dstStart++] = src[cStart++];   // c is smallest
                            if (cStart > cEnd) break;
                        }
                    }
                }
            }
            // Ran out of elements in one of the segments - i.e. 2 segments are available for merging, but which 2
            // Length needs to be adjusted, to be lengths left yet to be merged for each segment
            aLength = aEnd - aStart + 1;
            bLength = bEnd - bStart + 1;
            cLength = cEnd - cStart + 1;
            if (aStart > aEnd)
            {
                Merge(src, bStart, bLength, cStart, cLength, dst, dstStart, equalityComparer);
            }
            else if (bStart > bEnd)
            {
                Merge(src, aStart, aLength, cStart, cLength, dst, dstStart, equalityComparer);
            }
            else   // (cStart > cEnd)
            {
                Merge(src, aStart, aLength, bStart, bLength, dst, dstStart, equalityComparer);
            }
        }
        // Strategy is to handle 4 segments while 4 are available, 3 while 3 are available, 2 while 2 are available
        // This extends the strategy used for merging two segments nicely
        static public void MergeFourWay<T>(T[] src, Int32 aStart, Int32 aLength,
                                                    Int32 bStart, Int32 bLength,
                                                    Int32 cStart, Int32 cLength,
                                                    Int32 dStart, Int32 dLength,
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
                if (equalityComparer.Compare(src[aStart], src[bStart]) <= 0)
                {   // a <= b
                    if (equalityComparer.Compare(src[cStart], src[dStart]) <= 0)
                    {   // c <= d
                        dst[dstStart++] = equalityComparer.Compare(src[aStart], src[cStart]) <= 0 ? src[aStart++] : src[cStart++];   // a or c is smallest
                    }
                    else
                    {   // d < c
                        dst[dstStart++] = equalityComparer.Compare(src[aStart], src[dStart]) <= 0 ? src[aStart++] : src[dStart++];   // a or d is smallest
                    }
                }
                else
                {   // b < a
                    if (equalityComparer.Compare(src[cStart], src[dStart]) <= 0)
                    {   // c <= d
                        dst[dstStart++] = equalityComparer.Compare(src[bStart], src[cStart]) <= 0 ? src[bStart++] : src[cStart++];  // b or c is smallest
                    }
                    else
                    {   // d < c
                        dst[dstStart++] = equalityComparer.Compare(src[bStart], src[dStart]) <= 0 ? src[bStart++] :src[dStart++];  // b or d is smallest
                    }
                }
            }
            // Ran out of elements in one of the four segments - i.e. 3 segments are available for merging, but which 3
            // Length needs to be adjusted, to be lengths left yet to be merged for each segment
            aLength = aEnd - aStart + 1;
            bLength = bEnd - bStart + 1;
            cLength = cEnd - cStart + 1;
            dLength = dEnd - dStart + 1;
            if (aStart > aEnd)
            {
                MergeThreeWay(src, bStart, bLength, cStart, cLength, dStart, dLength, dst, dstStart, equalityComparer);
            }
            else if (bStart > bEnd)
            {
                MergeThreeWay(src, aStart, aLength, cStart, cLength, dStart, dLength, dst, dstStart, equalityComparer);
            }
            else if (cStart > cEnd)
            {
                MergeThreeWay(src, aStart, aLength, bStart, bLength, dStart, dLength, dst, dstStart, equalityComparer);
            }
            else   // (dStart > dEnd)
            {
                MergeThreeWay(src, aStart, aLength, bStart, bLength, cStart, cLength, dst, dstStart, equalityComparer);
            }
        }
        // Strategy is to handle 4 segments while 4 are available, 3 while 3 are available, 2 while 2 are available
        // This extends the strategy used for merging two segments nicely
        static public void MergeFourWay2<T>(T[] src, Int32 aStart, Int32 aLength,
                                                     Int32 bStart, Int32 bLength,
                                                     Int32 cStart, Int32 cLength,
                                                     Int32 dStart, Int32 dLength,
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
                while (true)
                {
                    if (equalityComparer.Compare(src[aStart], src[bStart]) <= 0)
                    {   // a <= b
                        if (equalityComparer.Compare(src[cStart], src[dStart]) <= 0)
                        {   // c <= d
                            if (equalityComparer.Compare(src[aStart], src[cStart]) <= 0)
                            {
                                dst[dstStart++] = src[aStart++];   // a is smallest
                                if (aStart > aEnd) break;
                            }
                            else
                            {
                                dst[dstStart++] = src[cStart++];   // c is smallest
                                if (cStart > cEnd) break;
                            }
                        }
                        else
                        {   // d < c
                            if (equalityComparer.Compare(src[aStart], src[dStart]) <= 0)
                            {
                                dst[dstStart++] = src[aStart++];   // a is smallest
                                if (aStart > aEnd) break;
                            }
                            else
                            {
                                dst[dstStart++] = src[dStart++];   // d is smallest
                                if (dStart > dEnd) break;
                            }
                        }
                    }
                    else
                    {   // b < a
                        if (equalityComparer.Compare(src[cStart], src[dStart]) <= 0)
                        {   // c <= d
                            if (equalityComparer.Compare(src[bStart], src[cStart]) <= 0)
                            {
                                dst[dstStart++] = src[bStart++];   // b is smallest
                                if (bStart > bEnd) break;
                            }
                            else
                            {
                                dst[dstStart++] = src[cStart++];   // c is smallest
                                if (cStart > cEnd) break;
                            }
                        }
                        else
                        {   // d < c
                            if (equalityComparer.Compare(src[bStart], src[dStart]) <= 0)
                            {
                                dst[dstStart++] = src[bStart++];   // b is smallest
                                if (bStart > bEnd) break;
                            }
                            else
                            {
                                dst[dstStart++] = src[dStart++];   // d is smallest
                                if (dStart > dEnd) break;
                            }
                        }
                    }
                }
            }
            // Ran out of elements in one of the four segments - i.e. 3 segments are available for merging, but which 3
            // Length needs to be adjusted, to be lengths left yet to be merged for each segment
            aLength = aEnd - aStart + 1;
            bLength = bEnd - bStart + 1;
            cLength = cEnd - cStart + 1;
            dLength = dEnd - dStart + 1;
            if (aStart > aEnd)
            {
                MergeThreeWay2(src, bStart, bLength, cStart, cLength, dStart, dLength, dst, dstStart, equalityComparer);
            }
            else if (bStart > bEnd)
            {
                MergeThreeWay2(src, aStart, aLength, cStart, cLength, dStart, dLength, dst, dstStart, equalityComparer);
            }
            else if (cStart > cEnd)
            {
                MergeThreeWay2(src, aStart, aLength, bStart, bLength, dStart, dLength, dst, dstStart, equalityComparer);
            }
            else   // (dStart > dEnd)
            {
                MergeThreeWay2(src, aStart, aLength, bStart, bLength, cStart, cLength, dst, dstStart, equalityComparer);
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
                Swap(ref aStart, ref bStart);
                Swap(ref aEnd, ref bEnd);
                Swap(ref length1, ref length2);
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

        // Merge two ranges of source array T[ l .. m, m+1 .. r ] in-place.
        // Based on not-in-place algorithm in 3rd ed. of "Introduction to Algorithms" p. 798-802, extending it to be in-place
        // and my Dr. Dobb's paper https://www.drdobbs.com/parallel/parallel-in-place-merge/240008783
        public static void MergeInPlaceDivideAndConquer<T>(T[] arr, int startIndex, int midIndex, int endIndex, IComparer<T> comparer = null)
        {
            //Console.WriteLine("merge: start = {0}, mid = {1}, end = {2}", startIndex, midIndex, endIndex);
            int length1 = midIndex - startIndex + 1;
            int length2 = endIndex - midIndex;
            if (length1 >= length2)
            {
                if (length2 <= 0) return;                       // if the smaller segment has zero elements, then nothing to merge
                int q1 = (startIndex + midIndex) / 2;           // q1 is mid-point of the larger segment. length1 >= length2 > 0
                int q2 = Algorithm.BinarySearch(arr[q1], arr, midIndex + 1, endIndex, comparer);  // q2 is q1 partitioning element within the smaller sub-array (and q2 itself is part of the sub-array that does not move)
                int q3 = q1 + (q2 - midIndex - 1);
                if ((q2 - q1) < 1024)       // TODO: Not sure if this adaptive portion is worth it
                    Algorithm.BlockSwapReversal(arr, q1, midIndex, q2 - 1);
                else
                    Algorithm.BlockSwapGriesMills(arr, q1, midIndex, q2 - 1);
                MergeInPlaceDivideAndConquer(arr, startIndex, q1 - 1, q3 - 1,   comparer);        // note that q3 is now in its final place and no longer participates in further processing
                MergeInPlaceDivideAndConquer(arr, q3 + 1,     q2 - 1, endIndex, comparer);
            }
            else
            {   // length1 < length2
                if (length1 <= 0) return;                       // if the smaller segment has zero elements, then nothing to merge
                int q1 = (midIndex + 1 + endIndex) / 2;         // q1 is mid-point of the larger segment.  length2 > length1 > 0
                int q2 = Algorithm.BinarySearch(arr[q1], arr, startIndex, midIndex, comparer);    // q2 is q1 partitioning element within the smaller sub-array (and q2 itself is part of the sub-array that does not move)
                int q3 = q2 + (q1 - midIndex - 1);
                if ((q1 - q2) < 1024)
                    Algorithm.BlockSwapReversal(arr, q2, midIndex, q1);
                else
                    Algorithm.BlockSwapGriesMills(arr, q2, midIndex, q1);
                MergeInPlaceDivideAndConquer(arr, startIndex, q2 - 1, q3 - 1,   comparer);        // note that q3 is now in its final place and no longer participates in further processing
                MergeInPlaceDivideAndConquer(arr, q3 + 1,     q1,     endIndex, comparer);
            }
        }

        // Merge two ranges of source array T[ l .. m, m+1 .. r ] in-place.
        // Based on not-in-place algorithm in 3rd ed. of "Introduction to Algorithms" p. 798-802, extending it to be in-place
        // and my Dr. Dobb's paper https://www.drdobbs.com/parallel/parallel-in-place-merge/240008783
        private static void MergeInPlaceDivideAndConquerHybrid<T>(T[] arr, int startIndex, int midIndex, int endIndex, T[] buff, IComparer<T> comparer = null)
        {
            //Console.WriteLine("merge: start = {0}, mid = {1}, end = {2}", startIndex, midIndex, endIndex);
            int length1 = midIndex - startIndex + 1;
            int length2 = endIndex - midIndex;
            if (length1 <= 0 || length2 <= 0)
                return;
            if ((length1 + length2) <= buff.Length)
            {
                Merge(arr, startIndex,   length1,
                      arr, midIndex + 1, length2,
                      buff, 0, comparer);                       // from Dr. Dobb's Journal paper
                Array.Copy(buff, arr, length1 + length2);
                return;
            }
            if (length1 >= length2)
            {
                int q1 = (startIndex + midIndex) / 2;           // q1 is mid-point of the larger segment. length1 >= length2 > 0
                int q2 = Algorithm.BinarySearch(arr[q1], arr, midIndex + 1, endIndex, comparer);  // q2 is q1 partitioning element within the smaller sub-array (and q2 itself is part of the sub-array that does not move)
                int q3 = q1 + (q2 - midIndex - 1);
                Algorithm.BlockSwapReversal(arr, q1, midIndex, q2 - 1);
                MergeInPlaceDivideAndConquerHybrid(arr, startIndex, q1 - 1, q3 - 1,   buff, comparer);  // note that q3 is now in its final place and no longer participates in further processing
                MergeInPlaceDivideAndConquerHybrid(arr, q3 + 1,     q2 - 1, endIndex, buff, comparer);
            }
            else
            {   // length1 < length2
                int q1 = (midIndex + 1 + endIndex) / 2;         // q1 is mid-point of the larger segment.  length2 > length1 > 0
                int q2 = Algorithm.BinarySearch(arr[q1], arr, startIndex, midIndex, comparer);    // q2 is q1 partitioning element within the smaller sub-array (and q2 itself is part of the sub-array that does not move)
                int q3 = q2 + (q1 - midIndex - 1);
                Algorithm.BlockSwapReversal(arr, q2, midIndex, q1);
                MergeInPlaceDivideAndConquerHybrid(arr, startIndex, q2 - 1, q3 - 1,   buff, comparer);  // note that q3 is now in its final place and no longer participates in further processing
                MergeInPlaceDivideAndConquerHybrid(arr, q3 + 1,     q1,     endIndex, buff, comparer);
            }
        }
        /// <summary>
        /// Divide-and-Conquer In-Place Adaptive Merge of two ranges of source array src[ startIndex .. midIndex ] and src[ midIndex+1 .. endIndex ] in-place
        /// This merge is not stable.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="arr">source array</param>
        /// <param name="startIndex">starting index of the first  segment, inclusive</param>
        /// <param name="midIndex">ending   index of the first  segment, inclusive</param>
        /// <param name="endIndex">ending   index of the second segment, inclusive</param>
        /// <param name="comparer">method to compare array elements</param>
        public static void MergeInPlaceAdaptiveDivideAndConquer<T>(T[] arr, int startIndex, int midIndex, int endIndex, IComparer<T> comparer = null, int threshold = 16 * 1024)
        {
            if ((endIndex - startIndex) < threshold)
            {
                MergeInPlaceDivideAndConquer(arr, startIndex, midIndex, endIndex, comparer);
            }
            else
            {
                try
                {
                    T[] tmpBuff = new T[arr.Length];
                    MergeDivideAndConquer(arr, startIndex, midIndex, midIndex + 1, endIndex, tmpBuff, startIndex, comparer);
                    Array.Copy(tmpBuff, startIndex, arr, startIndex, endIndex - startIndex + 1);
                }
                catch (System.OutOfMemoryException)
                {
                    MergeInPlaceDivideAndConquer(arr, startIndex, midIndex, endIndex, comparer);
                }
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
                HPCsharp.Algorithm.Swap(ref aStart, ref bStart);
                HPCsharp.Algorithm.Swap(ref aEnd, ref bEnd);
                HPCsharp.Algorithm.Swap(ref length1, ref length2);
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