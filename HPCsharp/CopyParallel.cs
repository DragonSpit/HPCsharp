// TODO: Optimize parallel copy using our new statistical methods, since these are paying off for sorting and merging.
// TODO: Figure out when parallel copy makes sense and gains performance, and under what conditions: already paged in or not paged in or when reusing dst and src arrays
// TODO: Using SSE instructions for possible higher bandwidth thru each CPU core is worth experimenting with, to see if it beats Array.Copy
// TODO: Strengthen argument error checking for each function
// TODO: Make sure to support ToArrayPar() naming well, even for array to array (maybe) if arrays have a ToArray() built-in function, then create a parallel version.
//       also document well in the Readme as Array.CopyPar() and ToArrayPar(), and put .ToArrayPar() first since that's much more recognizable by C# coders.
// TODO: Look for faster copying or ToArray() in stackOverflow https://stackoverflow.com/questions/12380266/how-this-toarray-implementation-more-optimized definitely can answer that question better
//       by using List.CopyTo() to an array which is C# built-in function, and then point them to HPCsharp for CopyToPar() multi-threaded version.
// TODO: Is there a native .AsParallel().CopyTo() or native .AsParallel().ToArray() in C#?
// TODO: Make interfaces exactly the same as C#'s .Copy() and .CopyTo() and .ToInteger(). These have a startIndex
//       and length, whereas ours don't and need to
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
        // Pays off on quad memory channel CPU, where one core may not be able to use all memory bandwidth with a single core
        // Not only do Xeon's have quad-channel memory, but also some high end i7's
        /// <summary>
        /// Array smaller than this value will not be copied using a parallel copy
        /// </summary>
        public static Int32 CopyParArrayThreshold { get; set; } = 8 * 1024;
        /// <summary>
        /// List smaller than this value will not be copied using a parallel copy
        /// </summary>
        public static Int32 CopyParListThreshold { get; set; } = 16 * 1024;
        /// <summary>
        /// Amount of parallelism used for array copy, since most of the time it's not necessary to use all cores to use all of memory bandwidth
        /// </summary>
        public static Int32 CopyParParallelism { get; set; } = 2;
        /// <summary>
        /// Copy elements from the source array to the destination array.
        /// Faster version, especially whenever the destination can be reused several times.
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="srcStart">source array starting index</param>
        /// <param name="dst">destination array</param>
        /// <param name="dstStart">destination array starting index</param>
        /// <param name="length">number of array elements to copy</param>
        public static void CopyPar<T>(T[] src, Int32 srcStart, T[] dst, Int32 dstStart, Int32 length)
        {
            if (length <= 0)      // zero elements to copy
                return;
            if (length < CopyParArrayThreshold)
            {
                Array.Copy(src, srcStart, dst, dstStart, length);
                return;
            }

            int lengthFirstHalf = length / 2;
            int lengthSecondHalf = length - lengthFirstHalf;
            Parallel.Invoke(
                () => { CopyPar<T>(src, srcStart,                   dst, dstStart,                   lengthFirstHalf ); },
                () => { CopyPar<T>(src, srcStart + lengthFirstHalf, dst, dstStart + lengthFirstHalf, lengthSecondHalf); }
            );
            return;
        }
        /// <summary>
        /// Copy elements from the source array to the destination array
        /// Faster version, especially whenever the destination can be reused several times.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="dst">destination array</param>
        /// <param name="length">number of array elements to copy</param>
        public static void CopyPar<T>(this T[] src, T[] dst, Int32 length)
        {
            if (length > src.Length || length > dst.Length)
                throw new ArgumentOutOfRangeException();
            CopyPar<T>(src, 0, dst, 0, length);
        }
        /// <summary>
        /// Copy elements from the source array to the destination array
        /// Faster version, especially whenever the destination can be reused several times.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="dst">destination array</param>
        public static void CopyPar<T>(this T[] src, T[] dst)
        {
            if (src.Length > dst.Length)
                throw new ArgumentOutOfRangeException();
            CopyPar<T>(src, 0, dst, 0, src.Length);
        }
        /// <summary>
        /// Copy elements from the source array to the destination array.
        /// Slower than the version with destination array argument, because a new destination array has not yet
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="srcStart">source array starting index</param>
        /// <param name="dstStart">destination array starting index</param>
        /// <param name="length">number of array elements to copy</param>
        public static T[] CopyPar<T>(T[] src, Int32 srcStart, Int32 dstStart, Int32 length)
        {
            T[] dst = new T[src.Length];
            CopyPar<T>(src, srcStart, dst, dstStart, length);
            return dst;
        }
        /// <summary>
        /// Copy elements from the source array to a new array which is returned.
        /// Slower than the version with destination array argument, because a new destination array has not yet
        /// been paged in.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="length">number of array elements to copy</param>
        public static T[] CopyPar<T>(this T[] src, Int32 length)
        {
            if (length > src.Length)
                throw new ArgumentOutOfRangeException();
            T[] dst = new T[src.Length];
            CopyPar<T>(src, 0, dst, 0, length);
            return dst;
        }
        /// <summary>
        /// Copy elements from the source array to the destination array
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        public static T[] CopyPar<T>(this T[] src)
        {
            T[] dst = new T[src.Length];
            CopyPar<T>(src, 0, dst, 0, src.Length);
            return dst;
        }
        /// <summary>
        /// Copy elements from the source List to the destination List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="srcStart">source List starting index</param>
        /// <param name="dst">destination List</param>
        /// <param name="dstStart">destination List starting index</param>
        /// <param name="length">number of List elements to copy</param>
// TODO: Detect and use C# well implemented cases for List, and use our code for the cases where C# does not implement well
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
