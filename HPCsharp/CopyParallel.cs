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

namespace HPCsharp.ParallelAlgorithms
{
    static public partial class ArrayHpc
    {
        // Pays off on quad memory channel CPU, where one core may not be able to use all memory bandwidth with a single core
        // Not only do Xeon's have quad-channel memory, but also some high end i7's and AMD EPYC and earlier generation AMD desktop Zen CPUs
        /// <summary>
        /// Array smaller than this value will not be copied using a parallel copy
        /// </summary>
        public static Int32 CopyParArrayThreshold { get; set; } = 8 * 1024;
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
        public static void CopyPar<T>(this T[] src, Int32 srcStart, T[] dst, Int32 dstStart, Int32 length)
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
        public static T[] CopyPar<T>(this T[] src, Int32 srcStart, Int32 dstStart, Int32 length)
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
    }
}
