// TODO: Detect and use C# well implemented cases for List, and use our code for the cases where C# does not implement well
// TODO: Strengthen argument value and combinations error checking
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HPCsharp.ParallelAlgorithms
{
    static public partial class Copy
    {
        private static void CopyParallelInner<T>(List<T> src, Int32 srcStart, List<T> dst, Int32 dstStart, Int32 length, Int32 parallelThreshold = 16 * 1024)
        {
            if (length <= 0)      // zero elements to copy
                return;
            if (length < parallelThreshold)
            {
                for (Int32 i = 0; i < length; i++)
                    dst[dstStart++] = src[srcStart++];
                return;
            }

            int lengthFirstHalf = length / 2;
            int lengthSecondHalf = length - lengthFirstHalf;
            Parallel.Invoke(
                () => { CopyParallelInner<T>(src, srcStart,                   dst, dstStart,                   lengthFirstHalf,  parallelThreshold); },
                () => { CopyParallelInner<T>(src, srcStart + lengthFirstHalf, dst, dstStart + lengthFirstHalf, lengthSecondHalf, parallelThreshold); }
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
        private static void CopyToArrayParallelInner<T>(this List<T> src, Int32 srcStart, T[] dst, Int32 dstStart, Int32 length, (Int32 parallelThreshold, Int32 degreeOfParallelism)? parSettings = null)
        {
            if (length <= 0)      // zero elements to copy
                return;
            (Int32 parallelThreshold, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 2);      // default values for parallelThreshold and degreeOfParallelism
            if (length < parallelThreshold)
            {
                src.CopyTo(srcStart, dst, dstStart, length);
                return;
            }
            int lengthFirstHalf  = length / 2;
            int lengthSecondHalf = length - lengthFirstHalf;
            var options = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
            Parallel.Invoke(options,
                () => { CopyToArrayParallelInner<T>(src, srcStart,                   dst, dstStart,                   lengthFirstHalf,  (parallelThreshold, degreeOfParallelism)); },
                () => { CopyToArrayParallelInner<T>(src, srcStart + lengthFirstHalf, dst, dstStart + lengthFirstHalf, lengthSecondHalf, (parallelThreshold, degreeOfParallelism)); }
            );
            return;
        }
        /// <summary>
        /// Create a new Array from the source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="parSettings">parallelThreshold = array size larger than this threshold will use multiple cores; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static T[] ToArrayPar<T>(this List<T> src, (Int32 parallelThreshold, Int32 degreeOfParallelism)? parSettings = null)
        {
            (Int32 parallelThreshold, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 2);      // default values for parallelThreshold and degreeOfParallelism
            T[] dst = new T[src.Count];
            CopyToArrayParallelInner<T>(src, 0, dst, 0, src.Count, (parallelThreshold, degreeOfParallelism));
            return dst;
        }
        /// <summary>
        /// Create a new Array from a portion of source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="srcStart">starting index within src List</param>
        /// <param name="length">number of elements to be copied</param>
        /// <param name="parSettings">parallelThreshold = array size larger than this threshold will use multiple cores; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static T[] ToArrayPar<T>(this List<T> src, Int32 srcStart, Int32 length, (Int32 parallelThreshold, Int32 degreeOfParallelism)? parSettings = null )
        {
            (Int32 parallelThreshold, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 2);      // default values for parallelThreshold and degreeOfParallelism
            T[] dst = new T[length];
            CopyToArrayParallelInner<T>(src, srcStart, dst, 0, length, (parallelThreshold, degreeOfParallelism));
            return dst;
        }
        /// <summary>
        /// Copy elements from the source List to the destination Array
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="srcStart">source List starting index</param>
        /// <param name="dstStart">destination Array starting index</param>
        /// <param name="length">number of Array elements to copy</param>
        /// <param name="parSettings">parallelThreshold = array size larger than this threshold will use multiple cores; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static T[] ToArrayPar<T>(this List<T> src, Int32 srcStart, Int32 dstStart, Int32 length, (Int32 parallelThreshold, Int32 degreeOfParallelism)? parSettings = null)
        {
            (Int32 parallelThreshold, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 2);      // default values for parallelThreshold and degreeOfParallelism
            T[] dst = new T[src.Count];
            CopyToArrayParallelInner<T>(src, srcStart, dst, dstStart, length, (parallelThreshold, degreeOfParallelism));
            return dst;
        }
        /// <summary>
        /// Copy to an existing Array from the source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="dst">destination array</param>
        /// <param name="parSettings">parallelThreshold = array size larger than this threshold will use multiple cores; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static void CopyToPar<T>(this List<T> src, T[] dst, (Int32 parallelThreshold, Int32 degreeOfParallelism)? parSettings = null)
        {
            (Int32 parallelThreshold, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 2);      // default values for parallelThreshold and degreeOfParallelism
            CopyToArrayParallelInner<T>(src, 0, dst, 0, src.Count, (parallelThreshold, degreeOfParallelism));
        }
        /// <summary>
        /// Copy to an existing Array from a portion of source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="dst">destination array</param>
        /// <param name="dstStart">starting index within dst Array</param>
        /// <param name="parSettings">parallelThreshold = array size larger than this threshold will use multiple cores; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static void CopyToPar<T>(this List<T> src, T[] dst, Int32 dstStart, (Int32 parallelThreshold, Int32 degreeOfParallelism)? parSettings = null)
        {
            (Int32 parallelThreshold, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 2);      // default values for parallelThreshold and degreeOfParallelism
            CopyToArrayParallelInner<T>(src, 0, dst, dstStart, src.Count, (parallelThreshold, degreeOfParallelism));
        }
        /// <summary>
        /// Copy to an existing Array from a portion of source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="srcStart">source List starting index</param>
        /// <param name="dst">destination array</param>
        /// <param name="dstStart">destination Array starting index</param>
        /// <param name="length">number of Array elements to copy</param>
        /// <param name="parSettings">parallelThreshold = array size larger than this threshold will use multiple cores; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static void CopyToPar<T>(this List<T> src, Int32 srcStart, T[] dst, Int32 dstStart, Int32 length, (Int32 parallelThreshold, Int32 degreeOfParallelism)? parSettings = null)
        {
            (Int32 parallelThreshold, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 2);      // default values for parallelThreshold and degreeOfParallelism
            CopyToArrayParallelInner<T>(src, srcStart, dst, dstStart, length, (parallelThreshold, degreeOfParallelism));
        }
    }
}
