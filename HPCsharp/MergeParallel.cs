// TODO: Create a single multi-merge generic algorithm (inner) where the 2-way merge is passed in as a function parameter (serial or parallel)
//       The trouble is where does this generic algorithm live, ParallelAlgorithm or Algorithm class? Maybe we should have a single class
// TODO: For Divide-and-Conquer parallel merge split the array on cache line boundaries to eliminate sharing of cache lines between threads.
// TODO: Port my C++ parallel in-place merge algorithm from (https://www.drdobbs.com/parallel/parallel-in-place-merge/240008783?pgno=1) to C#,
//       as a user requested a truly in-place version, and it would be good to see how well it performs on 6, 14, and 32-core CPUs with 2, 4, 8 memory channels.
// TODO: Parallelize Algorithm.BlockSwapReversal(arr, q1, midIndex, q2 - 1); to use SSE/SIMD instructions and to use as few cores as necessary to use full memory bandwidth.
// TODO: Benchmark in-place versus not-in-place Merge and parallel Merge. Develop an adaptive in-place/not-in-place Merge and Parallel Merge if there is a large performance difference,
//       and use it inside Parallel Merge Sort and serial Merge Sort
// TODO: Parallel Merge running on all 32-core with hyperthreading (64-core in AWS) varied in performance dramatically from 200 Million to 1.7 Billion 
//       530-741 Million on a 6-core (with hyperthreading) is much more consitent in performance (on battery). Figure out why it's not scaling well - interference?
//       Single-core merge is 150-170 Million (very consistent) - on battery power; and 245-246 Million (extremely consistent - on wall power.
//       ParallelMerge was 750-770 Million when threshold set to be arra.Length / numberOfCores, and 770-1.1 Billion when set to 32K or 64K on 6-core laptop (with 128K possibly even better).
// TODO: Figure out why Merge and Merge2 are consistent in performance, with neither showing advantage, especially when order of measurement was swapped and then the other showed up as faster.
//       Seems to be a measurement flaw.
// TODO: Port my median-of-two-sorted-arrays algorithm from Dr. Dobb's to reduce the number of levels in the parallel merge to be exactly Log2(N).
// TODO: Improve termination case of Parallel Merge to a better measure than (length1 + length2) < threshold, to possibly include the case of one of the lengths being smaller than a threshold

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HPCsharp.ParallelAlgorithms;

namespace HPCsharp
{
    /// <summary>
    /// Parallel Algorithms operating on variety of containers, providing trade-off between abstraction and performance
    /// </summary>
    static public partial class ParallelAlgorithm
    {
        /// <summary>
        /// Smaller than threshold will use non-parallel algorithm to merge arrays
        /// </summary>
        public static Int32 MergeParallelArrayThreshold { get; set; } = 128 * 1024;
        /// <summary>
        /// Divide-and-Conquer Merge of two ranges of source array src[ p1 .. r1 ] and src[ p2 .. r2 ] into destination array starting at index p3.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="p1">starting index of the first  segment, inclusive</param>
        /// <param name="r1">ending   index of the first  segment, inclusive</param>
        /// <param name="p2">starting index of the second segment, inclusive</param>
        /// <param name="r2">ending   index of the second segment, inclusive</param>
        /// <param name="dst">destination array</param>
        /// <param name="p3">starting index of the result</param>
        /// <param name="comparer">method to compare array elements</param>
        internal static void MergeInnerPar<T>(T[] src, Int32 p1, Int32 r1, Int32 p2, Int32 r2, T[] dst, Int32 p3, IComparer<T> comparer = null)
        {
            //Console.WriteLine("#1 " + p1 + " " + r1 + " " + p2 + " " + r2);
            Int32 length1 = r1 - p1 + 1;
            Int32 length2 = r2 - p2 + 1;
            if (length1 < length2)
            {
                Algorithm.Swap(ref p1, ref p2);
                Algorithm.Swap(ref r1, ref r2);
                Algorithm.Swap(ref length1, ref length2);
            }
            if (length1 == 0) return;
            if ((length1 + length2) <= MergeParallelArrayThreshold)
            {
                //Console.WriteLine("#3 " + p1 + " " + length1 + " " + p2 + " " + length2 + " " + p3);
                HPCsharp.Algorithm.Merge<T>(src, p1, length1,
                                            src, p2, length2,
                                            dst, p3, comparer);  // in Dr. Dobb's Journal paper
                //HPCsharp.Algorithm.MergeFaster<T>(src, p1, length1,
                //                                       p2, length2,
                //                                  dst, p3, comparer);
            }
            else
            {
                Int32 q1 = (p1 + r1) / 2;
                Int32 q2 = Algorithm.BinarySearch(src[q1], src, p2, r2, comparer);
                Int32 q3 = p3 + (q1 - p1) + (q2 - p2);
                dst[q3] = src[q1];
                Parallel.Invoke(
                    () => { MergeInnerPar<T>(src, p1,     q1 - 1, p2, q2 - 1, dst, p3,     comparer); },
                    () => { MergeInnerPar<T>(src, q1 + 1, r1,     q2, r2,     dst, q3 + 1, comparer); }
                );
            }
        }

        internal static void MergeInnerParNew<T>(T[] src, Int32 p1, Int32 r1, Int32 p2, Int32 r2, T[] dst, Int32 p3, IComparer<T> comparer = null)
        {
            //Console.WriteLine("#1 " + p1 + " " + r1 + " " + p2 + " " + r2);
            Int32 length1 = r1 - p1 + 1;
            Int32 length2 = r2 - p2 + 1;
            if (length1 < length2)
            {
                Algorithm.Swap(ref p1, ref p2);
                Algorithm.Swap(ref r1, ref r2);
                Algorithm.Swap(ref length1, ref length2);
            }
            if (length1 == 0) return;
            if ((length1 + length2) <= MergeParallelArrayThreshold)
            {
                //Console.WriteLine("#3 " + p1 + " " + length1 + " " + p2 + " " + length2 + " " + p3);
                HPCsharp.Algorithm.MergeWithCopy<T>(src, p1, length1,
                                                    src, p2, length2,
                                                    dst, p3, comparer);
                //HPCsharp.Algorithm.MergeFaster<T>(src, p1, length1,
                //                                       p2, length2,
                //                                  dst, p3, comparer);
            }
            else
            {
                Int32 q1 = (p1 + r1) / 2;
                Int32 q2 = Algorithm.BinarySearch(src[q1], src, p2, r2, comparer);
                Int32 q3 = p3 + (q1 - p1) + (q2 - p2);
                dst[q3] = src[q1];
                Parallel.Invoke(
                    () => { MergeInnerPar<T>(src, p1, q1 - 1, p2, q2 - 1, dst, p3, comparer); },
                    () => { MergeInnerPar<T>(src, q1 + 1, r1, q2, r2, dst, q3 + 1, comparer); }
                );
            }
        }
        /// <summary>
        /// Divide-and-Conquer Merge of two ranges of source array src[ p1 .. r1 ] and src[ p2 .. r2 ] into destination array starting at index p3.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="p1">starting index of the first  segment, inclusive</param>
        /// <param name="r1">ending   index of the first  segment, inclusive</param>
        /// <param name="p2">starting index of the second segment, inclusive</param>
        /// <param name="r2">ending   index of the second segment, inclusive</param>
        /// <param name="dst">destination array</param>
        /// <param name="p3">starting index of the result</param>
        /// <param name="comparer">method to compare array elements</param>
        internal static void MergeInnerFasterPar<T>(T[] src, Int32 p1, Int32 r1, Int32 p2, Int32 r2, T[] dst, Int32 p3, IComparer<T> comparer = null, Int32 mergeParallelThreshold = 128 * 1024)
        {
            //Console.WriteLine("#1 " + p1 + " " + r1 + " " + p2 + " " + r2);
            Int32 length1 = r1 - p1 + 1;
            Int32 length2 = r2 - p2 + 1;
            if (length1 < length2)
            {
                Algorithm.Swap(ref p1, ref p2);
                Algorithm.Swap(ref r1, ref r2);
                Algorithm.Swap(ref length1, ref length2);
            }
            if (length1 == 0) return;
            if ((length1 + length2) <= mergeParallelThreshold)
            {
                //Console.WriteLine("#3 " + p1 + " " + length1 + " " + p2 + " " + length2 + " " + p3);
                HPCsharp.Algorithm.MergeFaster<T>(src, p1, length1,
                                                  src, p2, length2,
                                                  dst, p3, comparer);
            }
            else
            {
                Int32 q1 = (p1 + r1) / 2;
                Int32 q2 = Algorithm.BinarySearch(src[q1], src, p2, r2, comparer);
                Int32 q3 = p3 + (q1 - p1) + (q2 - p2);
                dst[q3] = src[q1];
                Parallel.Invoke(
                    () => { MergeInnerFasterPar<T>(src, p1, q1 - 1, p2, q2 - 1, dst, p3, comparer, mergeParallelThreshold); },
                    () => { MergeInnerFasterPar<T>(src, q1 + 1, r1, q2, r2, dst, q3 + 1, comparer, mergeParallelThreshold); }
                );
            }
        }
        /// <summary>
        /// Divide-and-Conquer Merge of two segments of a source array into destination array starting at index dstStart.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="aStart">starting index of the first  segment, inclusive</param>
        /// <param name="aLength">number of array elements in the first segment</param>
        /// <param name="bStart">starting index of the second segment, inclusive</param>
        /// <param name="bLength">number of array elements in the second segment</param>
        /// <param name="dst">destination array</param>
        /// <param name="dstStart">starting index of the destination/result</param>
        /// <param name="comparer">method to compare array elements</param>
        public static void MergePar<T>(T[] src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength, T[] dst, Int32 dstStart, Comparer<T> comparer = null)
        {
            MergeInnerPar<T>(src, aStart, aStart + aLength - 1, bStart, bStart + bLength - 1, dst, dstStart, comparer);
        }

        public static void MergeParNew<T>(T[] src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength, T[] dst, Int32 dstStart, Comparer<T> comparer = null)
        {
            MergeInnerParNew<T>(src, aStart, aStart + aLength - 1, bStart, bStart + bLength - 1, dst, dstStart, comparer);
        }

        /// <summary>
        /// Divide-and-Conquer Merge of two ranges of source array src[ p1 .. r1 ] and src[ p2 .. r2 ] into destination array starting at index p3.
        /// </summary>
        /// <typeparam name="T1">data type of each key element</typeparam>
        /// <typeparam name="T2">data type of each array element</typeparam>
        /// <param name="srcKeys">source array of keys used for sorting</param>
        /// <param name="srcItems">source array of items that will be sorted along with keys</param>
        /// <param name="p1">starting index of the first  segment, inclusive</param>
        /// <param name="r1">ending   index of the first  segment, inclusive</param>
        /// <param name="p2">starting index of the second segment, inclusive</param>
        /// <param name="r2">ending   index of the second segment, inclusive</param>
        /// <param name="dstKeys">destination array of sorted keys</param>
        /// <param name="dstItems">destination array of sorted items</param>
        /// <param name="p3">starting index of the result</param>
        /// <param name="comparer">method to compare array elements</param>
        internal static void MergeInnerPar<T1, T2>(T1[] srcKeys, T2[] srcItems, Int32 p1, Int32 r1, Int32 p2, Int32 r2, T1[] dstKeys, T2[] dstItems, Int32 p3, IComparer<T1> comparer = null)
        {
            //Console.WriteLine("merge: #1 " + p1 + " " + r1 + " " + p2 + " " + r2);
            Int32 length1 = r1 - p1 + 1;
            Int32 length2 = r2 - p2 + 1;
            if (length1 < length2)
            {
                Algorithm.Swap(ref p1, ref p2);
                Algorithm.Swap(ref r1, ref r2);
                Algorithm.Swap(ref length1, ref length2);
            }
            if (length1 == 0) return;
            if ((length1 + length2) <= MergeParallelArrayThreshold)
            {
                //Console.WriteLine("merge: #3 " + p1 + " " + length1 + " " + p2 + " " + length2 + " " + p3);
                HPCsharp.Algorithm.Merge<T1, T2>(srcKeys, srcItems, p1, length1,
                                                 srcKeys, srcItems, p2, length2,
                                                 dstKeys, dstItems, p3, comparer);
            }
            else
            {
                Int32 q1 = (p1 + r1) / 2;
                Int32 q2 = Algorithm.BinarySearch(srcKeys[q1], srcKeys, p2, r2, comparer);
                Int32 q3 = p3 + (q1 - p1) + (q2 - p2);
                dstKeys[ q3] = srcKeys[ q1];
                dstItems[q3] = srcItems[q1];
                Parallel.Invoke(
                    () => { MergeInnerPar<T1, T2>(srcKeys, srcItems, p1,     q1 - 1, p2, q2 - 1, dstKeys, dstItems, p3,     comparer); },
                    () => { MergeInnerPar<T1, T2>(srcKeys, srcItems, q1 + 1, r1,     q2, r2,     dstKeys, dstItems, q3 + 1, comparer); }
                );
            }
        }
        // Merge two ranges of source array T[ l .. m, m+1 .. r ] in-place.
        // Based on not-in-place algorithm in 3rd ed. of "Introduction to Algorithms" p. 798-802, extending it to be in-place
        // and my Dr. Dobb's paper https://www.drdobbs.com/parallel/parallel-in-place-merge/240008783
        public static void MergeDivideAndConquerInPlacePar<T>(T[] arr, int startIndex, int midIndex, int endIndex, IComparer<T> comparer = null, int threshold0 = 16 * 1024, int threshold1 = 16 * 1024)
        {
            //Console.WriteLine("MergeDivideAndConquerInPlacePar: start = {0}, mid = {1}, end = {2}", startIndex, midIndex, endIndex);
            int length1 = midIndex - startIndex + 1;
            int length2 = endIndex - midIndex;
            if (length1 >= length2)
            {
                if (length2 <= 0) return;                       // if the smaller segment has zero elements, then nothing to merge
                int q1 = (startIndex + midIndex) / 2;           // q1 is mid-point of the larger segment. length1 >= length2 > 0
                int q2 = Algorithm.BinarySearch(arr[q1], arr, midIndex + 1, endIndex, comparer);  // q2 is q1 partitioning element within the smaller sub-array (and q2 itself is part of the sub-array that does not move)
                int q3 = q1 + (q2 - midIndex - 1);
                //BlockSwapReversalPar(arr, q1, midIndex, q2 - 1, threshold0);
                //Algorithm.BlockSwapReversal(arr, q1, midIndex, q2 - 1);
                Algorithm.BlockSwapGriesMills(arr, q1, midIndex, q2 - 1);

                if (length1 < threshold1)
                {
                    MergeDivideAndConquerInPlacePar(arr, startIndex, q1 - 1, q3 - 1,   comparer);
                    MergeDivideAndConquerInPlacePar(arr, q3 + 1,     q2 - 1, endIndex, comparer);
                }
                else
                {
                    Parallel.Invoke(
                        () => { MergeDivideAndConquerInPlacePar(arr, startIndex, q1 - 1, q3 - 1,   comparer); },   // note that q3 is now in its final place and no longer participates in further processing
                        () => { MergeDivideAndConquerInPlacePar(arr, q3 + 1,     q2 - 1, endIndex, comparer); }
                    );
                }
            }
            else
            {   // length1 < length2
                if (length1 <= 0) return;                       // if the smaller segment has zero elements, then nothing to merge
                int q1 = (midIndex + 1 + endIndex) / 2;         // q1 is mid-point of the larger segment.  length2 > length1 > 0
                int q2 = Algorithm.BinarySearch(arr[q1], arr, startIndex, midIndex, comparer);    // q2 is q1 partitioning element within the smaller sub-array (and q2 itself is part of the sub-array that does not move)
                int q3 = q2 + (q1 - midIndex - 1);
                //BlockSwapReversalPar(arr, q2, midIndex, q1, threshold0);
                //Algorithm.BlockSwapReversal(arr, q2, midIndex, q1);
                Algorithm.BlockSwapGriesMills(arr, q2, midIndex, q1);

                if (length1 < threshold1)
                {
                    MergeDivideAndConquerInPlacePar(arr, startIndex, q2 - 1, q3 - 1, comparer);
                    MergeDivideAndConquerInPlacePar(arr, q3 + 1, q1, endIndex, comparer);
                }
                else
                {
                    Parallel.Invoke(
                        () => { MergeDivideAndConquerInPlacePar(arr, startIndex, q2 - 1, q3 - 1,   comparer); },   // note that q3 is now in its final place and no longer participates in further processing
                        () => { MergeDivideAndConquerInPlacePar(arr, q3 + 1,     q1,     endIndex, comparer); }
                    );
                }
            }
        }

        /// <summary>
        /// Smaller than threshold will use non-parallel algorithm to merge arrays
        /// </summary>
        static Int32 MergeParallelListThreshold { get; set; } = 64000;
#if false
        // TODO: Figure out why this is so slow and does not accelerate
        /// <summary>
        /// Divide-and-Conquer Merge of two ranges of source List src[ p1 .. r1 ] and src[ p2 .. r2 ] into destination List starting at index p3.
        /// </summary>
        /// <typeparam name="T">data type of each List element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="p1">starting index of the first  segment, inclusive</param>
        /// <param name="r1">ending   index of the first  segment, inclusive</param>
        /// <param name="p2">starting index of the second segment, inclusive</param>
        /// <param name="r2">ending   index of the second segment, inclusive</param>
        /// <param name="dst">destination List</param>
        /// <param name="p3">starting index of the result</param>
        /// <param name="comparer">method to compare array elements</param>
        public static void MergeParallel<T>(List<T> src, Int32 p1, Int32 r1, Int32 p2, Int32 r2, List<T> dst, Int32 p3, Comparer<T> comparer = null)
        {
            Int32 length1 = r1 - p1 + 1;
            Int32 length2 = r2 - p2 + 1;
            if (length1 < length2)
            {
                Exchange(ref p1, ref p2);
                Exchange(ref r1, ref r2);
                Exchange(ref length1, ref length2);
            }
            if (length1 == 0) return;
            if ((length1 + length2) <= MergeParallelListThreshold)
            {   // 8192 threshold is much better than 16 (is it for C#)
                HPCsharp.Algorithm.Merge<T>(src, p1, p1 + length1, src, p2, p2 + length2, dst, p3, comparer);  // in DDJ paper
            }
            else
            {
                Int32 q1 = (p1 + r1) / 2;
                Int32 q2 = Algorithm.BinarySearch(src[q1], src, p2, r2);
                Int32 q3 = p3 + (q1 - p1) + (q2 - p2);
                dst[q3] = src[q1];
                Parallel.Invoke(
                    () => { MergeParallel<T>(src, p1,     q1 - 1, p2, q2 - 1, dst, p3,     comparer); },
                    () => { MergeParallel<T>(src, q1 + 1, r1,     q2, r2,     dst, q3 + 1, comparer); }
                );
            }
        }
#endif
        /// <summary>
        /// Divide-and-Conquer Merge of two segments of source List into destination List starting at index p3.
        /// </summary>
        /// <typeparam name="T">data type of each List element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="aStart">starting index of the first  segment, inclusive</param>
        /// <param name="aLength">number of array elements in the first segment</param>
        /// <param name="bStart">starting index of the second segment, inclusive</param>
        /// <param name="bLength">number of array elements in the second segment</param>
        /// <param name="dstStart">starting index of the destination/result</param>
        /// <param name="comparer">method to compare array elements</param>
        public static List<T> MergePar<T>(List<T> src, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength, Int32 dstStart, Comparer<T> comparer = null)
        {
            T[] srcCopy = src.ToArrayPar();
            T[] dstCopy = new T[src.Count];
            MergePar(srcCopy, aStart, aLength, bStart, bLength, dstCopy, dstStart, comparer);
            return new List<T>(dstCopy);
        }

        /// <summary>
        /// Merge two or more sorted array spans, placing the result into a destination array as a single sorted span.
        /// The destination array must be as big as the source, otherwise an ArgumentException is thrown.
        /// </summary>
        /// <typeparam name="T">data type of each List element</typeparam>
        /// <param name="sourceArray">source array</param>
        /// <param name="sourceSpans">List of sorted spans, specified by starting and ending indexes (both inclusive)</param>
        /// <param name="destinationArray">destination Array where the result of merged spans is placed</param>
        /// <param name="comparer">(optional) method to compare array elements</param>
        static public void MergePar<T>( T[] sourceArray, List<SortedSpan> sourceSpans,
                                        T[] destinationArray,
                                        Comparer<T> comparer = null)
        {
            if (destinationArray.Length != sourceArray.Length)
            {
                throw new ArgumentException("Destination array must be the same size as the source array");
            }
            if (sourceSpans == null || sourceSpans.Count == 0)    // nothing to merge
            {
                return;
            }
            else
            {
                bool srcToDst = true;
                while (sourceSpans.Count >= 1)
                {
                    if (sourceSpans.Count == 1)
                    {
                        if (srcToDst)
                            Array.Copy(sourceArray, sourceSpans[0].Start, destinationArray, sourceSpans[0].Start, sourceSpans[0].Length);
                        return;
                    }

                    var dstSpans = new List<SortedSpan>();
                    Int32 i = 0;

                    // Merge neighboring pairs of spans
                    Int32 numPairs = sourceSpans.Count / 2;
                    for (Int32 p = 0; p < numPairs; p++)
                    {
                        MergePar<T>(sourceArray,      sourceSpans[i    ].Start, sourceSpans[i    ].Length,
                                                      sourceSpans[i + 1].Start, sourceSpans[i + 1].Length,
                                    destinationArray, sourceSpans[i    ].Start,
                                    comparer);
                        dstSpans.Add(new SortedSpan { Start = sourceSpans[i].Start, Length = sourceSpans[i].Length + sourceSpans[i + 1].Length });
                        i += 2;
                    }
                    // Copy the last left over odd segment (if there is one) from src to dst and add it to dstSpans
                    if (i == (sourceSpans.Count - 1))
                    {
                        Array.Copy(sourceArray, sourceSpans[i].Start, destinationArray, sourceSpans[i].Start, sourceSpans[i].Length);
                        dstSpans.Add(new SortedSpan { Start = sourceSpans[i].Start, Length = sourceSpans[i].Length });
                    }
                    sourceSpans = dstSpans;
                    var tmp = sourceArray;          // swap src and dst arrays
                    sourceArray = destinationArray;
                    destinationArray = tmp;
                    srcToDst = srcToDst ? false : true; // keep track of merge direction
                }
            }
        }
    }
}
