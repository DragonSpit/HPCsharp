using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HPCsharp
{
    /// <summary>
    /// Parallel Algorithms operating on variety of containers, providing trade-off between abstraction and performance
    /// </summary>
    static public partial class ParallelAlgorithm
    {
        private static void Exchange<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
        /// <summary>
        /// Smaller than threshold will use non-parallel algorithm to merge arrays
        /// </summary>
        static Int32 MergeParallelArrayThreshold { get; set; } = 64000;
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
        public static void MergePar<T>(T[] src, Int32 p1, Int32 r1, Int32 p2, Int32 r2, T[] dst, Int32 p3, Comparer<T> comparer = null)
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
            if ((length1 + length2) <= MergeParallelArrayThreshold)
            {   // 8192 threshold is much better than 16 (is it for C#)
                HPCsharp.Algorithm.Merge<T>(src, p1, p1 + length1,
                                            src, p2, p2 + length2,
                                            dst, p3, comparer);  // in Dr. Dobb's Journal paper
            }
            else
            {
                Int32 q1 = (p1 + r1) / 2;
                Int32 q2 = Algorithm.BinarySearch(src[q1], src, p2, r2, comparer);
                Int32 q3 = p3 + (q1 - p1) + (q2 - p2);
                dst[q3] = src[q1];
                Parallel.Invoke(
                    () => { MergePar<T>(src, p1,     q1 - 1, p2, q2 - 1, dst, p3,     comparer); },
                    () => { MergePar<T>(src, q1 + 1, r1,     q2, r2,     dst, q3 + 1, comparer); }
                );
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
        /// Divide-and-Conquer Merge of two ranges of source List src[ p1 .. r1 ] and src[ p2 .. r2 ] into destination List starting at index p3.
        /// </summary>
        /// <typeparam name="T">data type of each List element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="p1">starting index of the first  segment, inclusive</param>
        /// <param name="r1">ending   index of the first  segment, inclusive</param>
        /// <param name="p2">starting index of the second segment, inclusive</param>
        /// <param name="r2">ending   index of the second segment, inclusive</param>
        /// <param name="dstStart">starting index of the result</param>
        /// <param name="comparer">method to compare array elements</param>
        public static List<T> MergePar<T>(List<T> src, Int32 p1, Int32 r1, Int32 p2, Int32 r2, Int32 dstStart, Comparer<T> comparer = null)
        {
            T[] srcCopy = src.ToArrayPar();
            T[] dstCopy = new T[src.Count];
            MergePar(srcCopy, p1, r1, p2, r2, dstCopy, dstStart, comparer);
            return new List<T>(dstCopy);
        }
    }
}
