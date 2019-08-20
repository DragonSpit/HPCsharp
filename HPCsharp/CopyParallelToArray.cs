// TODO: Detect and use C# well implemented cases for List, and use our code for the cases where C# does not implement well
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HPCsharp.ParallelAlgorithms
{
    static public partial class Copy
    {
        /// <summary>
        /// List smaller than this value will not be copied using a parallel copy
        /// </summary>
        public static Int32 CopyParListThreshold { get; set; } = 16 * 1024;
        /// <summary>
        /// Copy elements from the source List to the destination List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="srcStart">source List starting index</param>
        /// <param name="dst">destination List</param>
        /// <param name="dstStart">destination List starting index</param>
        /// <param name="length">number of List elements to copy</param>
        private static void CopyParallelInner<T>(List<T> src, Int32 srcStart, List<T> dst, Int32 dstStart, Int32 length)
        {
            if (length <= 0)      // zero elements to copy
                return;
            if (length < CopyParListThreshold)
            {
                for (Int32 i = 0; i < length; i++)
                    dst[dstStart++] = src[srcStart++];
                return;
            }

            int lengthFirstHalf = length / 2;
            int lengthSecondHalf = length - lengthFirstHalf;
            Parallel.Invoke(
                () => { CopyParallelInner<T>(src, srcStart,                   dst, dstStart,                   lengthFirstHalf ); },
                () => { CopyParallelInner<T>(src, srcStart + lengthFirstHalf, dst, dstStart + lengthFirstHalf, lengthSecondHalf); }
            );
            return;
        }
#if false
        // TODO: Keep in mind that List is different than Array and can expand in size. We may want to allow for this in our implementation to make it
        //       more thoughtful and more general and more useful
        // Idea: Create two versions: one that does not grow the destination List and one that does, possibly with one where its an error
        //       to not have enough elements in the source or needing to clip the source. Examine each possible case one by one and figure out
        //       the appropriate action for List. Then if we need to create separate functions to handle them, then just do it.
        public static void CopyPar<T>(List<T> src, Int32 srcStart, List<T> dst, Int32 dstStart, Int32 length)
        {
            if (srcStart == 0 && dstStart == 0)
            {
                if (src.Count == dst.Count && src.Count == length)
                {
                    dst.Clear();
                    dst.AddRange(src);
                }
                else if ()
                {

                }
                
                // use List.Delete and List.Insert
                // or if dst List is empty use .AddRange
            }
            else
            {

            }
        }
#endif
        /// <summary>
        /// List smaller than this value will not be copied using a parallel copy
        /// </summary>
        public static Int32 CopyParallelListToArrayThreshold { get; set; } = 16 * 1024;    // TODO: don't guess, find threshold
        /// <summary>
        /// Amount of parallelism used for array copy, since most of the time it's not necessary to use all cores to use all of memory bandwidth
        /// </summary>
        public static Int32 CopyToParParallelism { get; set; } = 2;
        private static void CopyParallelInner<T>(this List<T> src, Int32 srcStart, T[] dst, Int32 dstStart, Int32 length)
        {
            if (length <= 0)      // zero elements to copy
                return;
            if (length < CopyParallelListToArrayThreshold)
            {
                src.CopyTo(srcStart, dst, dstStart, length);
                return;
            }
            int lengthFirstHalf  = length / 2;
            int lengthSecondHalf = length - lengthFirstHalf;
            var options = new ParallelOptions { MaxDegreeOfParallelism = CopyToParParallelism };
            Parallel.Invoke(options,
                () => { CopyParallelInner<T>(src, srcStart,                   dst, dstStart,                   lengthFirstHalf ); },
                () => { CopyParallelInner<T>(src, srcStart + lengthFirstHalf, dst, dstStart + lengthFirstHalf, lengthSecondHalf); }
            );
            return;
        }
        /// <summary>
        /// Copy elements from the source List to the destination Array
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="srcStart">source List starting index</param>
        /// <param name="dst">destination Array</param>
        /// <param name="dstStart">destination Array starting index</param>
        /// <param name="length">number of Array elements to copy</param>
        public static void CopyToPar<T>(this List<T> src, Int32 srcStart, T[] dst, Int32 dstStart, Int32 length)
        {
            CopyParallelInner<T>(src, srcStart, dst, dstStart, length);
        }
        /// <summary>
        /// Copy elements from the source List to the destination Array
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="dst">destination Array</param>
        /// <param name="length">number of elements to copy</param>
        public static void CopyToPar<T>(this List<T> src, T[] dst, Int32 length)
        {
            if (length > src.Count || length > dst.Length)
                throw new ArgumentOutOfRangeException();
            CopyParallelInner<T>(src, 0, dst, 0, length);
        }
        /// <summary>
        /// Copy elements from the source List to the destination Array
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="dst">destination Array</param>
        public static void CopyToPar<T>(this List<T> src, T[] dst)
        {
            if (src.Count > dst.Length)
                throw new ArgumentOutOfRangeException();
            CopyParallelInner<T>(src, 0, dst, 0, src.Count);
        }
        /// <summary>
        /// Create a new Array from the source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        public static T[] ToArrayPar<T>(this List<T> src)
        {
            T[] dst = new T[src.Count];
            CopyParallelInner<T>(src, 0, dst, 0, src.Count);
            return dst;
        }
        /// <summary>
        /// Create a new Array from a portion of source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="srcStart">starting index within src List</param>
        /// <param name="length">number of elements to be copied</param>
        public static T[] ToArrayPar<T>(this List<T> src, Int32 srcStart, Int32 length)
        {
            T[] dst = new T[length];
            CopyParallelInner<T>(src, srcStart, dst, 0, length);
            return dst;
        }
    }
}
