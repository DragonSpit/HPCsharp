// TODO: Using SSE instructions for possible higher bandwidth thru each CPU core is worth experimenting with, to see if it beats Array.Copy
// TODO: Strengthen argument error checking for each function
// TODO: Make sure to support ToArrayPar() naming well, even for array to array (maybe) if arrays have a ToArray() built-in function, then create a parallel version.
//       also document well in the Readme as Array.CopyPar() and ToArrayPar(), and put .ToArrayPar() first since that's much more recognizable by C# coders.
// TODO: Look for faster copying or ToArray() in stackOverflow https://stackoverflow.com/questions/12380266/how-this-toarray-implementation-more-optimized definitely can answer that question better
//       by using List.CopyTo() to an array which is C# built-in function, and then point them to HPCsharp for CopyToPar() multi-threaded version.
// TODO: Is there a native .AsParallel().CopyTo() or native .AsParallel().ToArray() in C#?
// TODO: Make interfaces exactly the same as C#'s .Copy() and .CopyTo() and .ToInteger(). These have a startIndex
//       and length, whereas ours don't and need to
// TODO: Tune the parallelThreshold value
// TODO: An interesting case to optimize https://stackoverflow.com/questions/17571621/copying-a-list-to-a-new-list-more-efficient-best-practice
//       How about List.ToArrayPar() and then create a new List out of that array
// TODO: Answer https://stackoverflow.com/questions/5099604/any-faster-way-of-copying-arrays-in-c?noredirect=1&lq=1
// TODO: Add support for .AsParallel() and WithDegreeOfParallelism(), to simplify the interface for all parallel implementations

// Performance details:
// Notes: When benchmarking "not pageInDstArray" you will see occasionally (and somewhat regularly) performance spike to a much higher level. Theory: these data points are where
// the previously allocated array is being re-used and has already been created and has been paged into system memory, raising the performance to the paged-in benchmark level.
// SSE implementation when run on all cores tops out lower than scalar. But does it ramp up faster?

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Numerics;

namespace HPCsharp.ParallelAlgorithms
{
    static public partial class ArrayHpc
    {
        /// <summary>
        /// Copy elements from the source array to the destination array, using multiple processor cores.
        /// Performance is substantially higher, whenever the destination can be reused several times.
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="srcStart">source array starting index</param>
        /// <param name="dst">destination array</param>
        /// <param name="dstStart">destination array starting index</param>
        /// <param name="length">number of array elements to copy</param>
        /// <param name="degreeOfParallelism">number of cores to use. Setting to 0 means use all available cores</param>
        /// <param name="parallelThreshold">array size larger than this threshold will use multiple cores</param>
        public static void CopyPar<T>(this T[] src, Int32 srcStart, T[] dst, Int32 dstStart, Int32 length, ushort degreeOfParallelism = 0, Int32 parallelThreshold = 8 * 1024)
        {
            if (length <= 0)      // zero elements to copy
                return;
            if (length > (src.Length - srcStart) || length > (dst.Length - dstStart))
                throw new ArgumentOutOfRangeException();
            if (length < parallelThreshold || degreeOfParallelism == 1)
            {
                Array.Copy(src, srcStart, dst, dstStart, length);
                return;
            }

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(srcStart, srcStart + length), options, range =>
            {
                //Console.WriteLine("Partition: start = {0}   end = {1}", range.Item1, range.Item2);
                Array.Copy(src, range.Item1, dst, dstStart + (range.Item1 - srcStart), (range.Item2 - range.Item1));
            });

            return;
        }
        /// <summary>
        /// Copy elements from the source array to the destination array, using multiple processor cores.
        /// Performance is even higher whenever the destination array can be reused several times.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="dst">destination array</param>
        /// <param name="length">number of array elements to copy</param>
        /// <param name="degreeOfParallelism">number of cores to use. Setting to 0 means use all available cores</param>
        /// <param name="parallelThreshold">array size larger than this threshold will use multiple cores</param>
        public static void CopyPar<T>(this T[] src, T[] dst, Int32 length, ushort degreeOfParallelism = 0, Int32 parallelThreshold = 8 * 1024)
        {
            if (length > src.Length || length > dst.Length)
                throw new ArgumentOutOfRangeException();
            CopyPar<T>(src, 0, dst, 0, length, degreeOfParallelism, parallelThreshold);
        }
        /// <summary>
        /// Copy elements from the source array to the destination array, using multiple processor cores.
        /// Performance is even higher whenever the destination array can be reused several times.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="dst">destination array</param>
        /// <param name="parallelThreshold">array size larger than this threshold will use multiple cores</param>
        public static void CopyPar<T>(this T[] src, T[] dst, ushort degreeOfParallelism = 0, Int32 parallelThreshold = 8 * 1024)
        {
            if (src.Length > dst.Length)
                throw new ArgumentOutOfRangeException();
            CopyPar<T>(src, 0, dst, 0, src.Length, degreeOfParallelism, parallelThreshold);
        }
        /// <summary>
        /// Copy elements from the source array to the destination array, starting at an index within the destination, using multiple processor cores.
        /// Performance is even higher whenever the destination array can be reused several times.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="dst">destination array</param>
        /// <param name="startDstIndex">starting index of the destination array</param>
        /// <param name="parallelThreshold">array size larger than this threshold will use multiple cores</param>
        private static void CopyToPar<T>(this T[] src, T[] dst, Int32 startDstIndex, ushort degreeOfParallelism = 0, Int32 parallelThreshold = 8 * 1024)
        {
            CopyPar<T>(src, 0, dst, startDstIndex, src.Length, degreeOfParallelism, parallelThreshold);
        }
        /// <summary>
        /// Copy elements from the source array of integers to the destination array, using a single core with data parallel SIMD/SSE instructions.
        /// Performance is substantially higher, whenever the destination can be reused several times.
        /// </summary>
        /// <param name="src">source array</param>
        /// <param name="srcStart">source array starting index</param>
        /// <param name="dst">destination array</param>
        /// <param name="dstStart">destination array starting index</param>
        /// <param name="length">number of array elements to copy</param>
        public static void CopySse(this int[] src, Int32 srcStart, int[] dst, Int32 dstStart, Int32 length)
        {
            int sseSrcIndexEnd = srcStart + (length / Vector<int>.Count) * Vector<int>.Count;
            int i, j;
            for (i = srcStart, j = dstStart; i < sseSrcIndexEnd; i += Vector<int>.Count, j += Vector<int>.Count)
            {
                var inVector = new Vector<int>(src, i);
                inVector.CopyTo(dst, j);
            }
            int r = srcStart + length - 1;
            for (; i <= r; i++, j++)
                dst[j] = src[i];
        }
        /// <summary>
        /// Copy elements from the source array of integers to the destination array, using a single core with data parallel SIMD/SSE instructions.
        /// Performance is substantially higher, whenever the destination can be reused several times.
        /// </summary>
        /// <param name="src">source array</param>
        /// <param name="srcStart">source array starting index</param>
        /// <param name="dst">destination array</param>
        /// <param name="dstStart">destination array starting index</param>
        /// <param name="length">number of array elements to copy</param>
        /// <param name="degreeOfParallelism">number of cores to use. Setting to 0 means use all available cores</param>
        /// <param name="parallelThreshold">array size larger than this threshold will use multiple cores</param>
        public static void CopySsePar(this int[] src, Int32 srcStart, int[] dst, Int32 dstStart, Int32 length, int degreeOfParallelism = 0, Int32 parallelThreshold = 8 * 1024)
        {
            if (length <= 0)      // zero elements to copy
                return;
            if (length < parallelThreshold || degreeOfParallelism == 1)
            {
                CopySse(src, srcStart, dst, dstStart, length);
                return;
            }

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(srcStart, srcStart + length), options, range =>
            {
                //Console.WriteLine("Partition: start = {0}   end = {1}", range.Item1, range.Item2);
                CopySse(src, range.Item1, dst, dstStart + (range.Item1 - srcStart), (range.Item2 - range.Item1));
            });

            return;
        }
        // Pays off on quad memory channel CPU, where one core may not be able to use all memory bandwidth with a single core
        // Not only do Xeon's have quad-channel memory, but also some high end i7's and AMD EPYC and earlier generation AMD desktop Zen CPUs
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
        /// <param name="parallelThreshold">array size larger than this threshold will use multiple cores</param>
        private static void CopyPar<T>(this T[] src, Int32 srcStart, T[] dst, Int32 dstStart, Int32 length, Int32 parallelThreshold = 8 * 1024)
        {
            if (length <= 0)      // zero elements to copy
                return;
            if (length < parallelThreshold)
            {
                Array.Copy(src, srcStart, dst, dstStart, length);
                return;
            }

            int lengthFirstHalf = length / 2;
            int lengthSecondHalf = length - lengthFirstHalf;
            Parallel.Invoke(
                () => { CopyPar<T>(src, srcStart, dst, dstStart, lengthFirstHalf, parallelThreshold); },
                () => { CopyPar<T>(src, srcStart + lengthFirstHalf, dst, dstStart + lengthFirstHalf, lengthSecondHalf, parallelThreshold); }
            );
            return;
        }
        /// <summary>
        /// Copy elements from the source array to the allocated destination array, using multiple processor cores.
        /// Slower than the version with destination array argument, because the newly allocated destination array has not yet been paged in.
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="srcStart">source array starting index</param>
        /// <param name="dstStart">destination array starting index</param>
        /// <param name="length">number of array elements to copy</param>
        /// <param name="degreeOfParallelism">number of cores to use. Setting to 0 means use all available cores</param>
        /// <param name="parallelThreshold">array size larger than this threshold will use multiple cores</param>
        public static T[] ToArrayPar<T>(this T[] src, Int32 srcStart, Int32 dstStart, Int32 length, ushort degreeOfParallelism = 0, Int32 parallelThreshold = 8 * 1024)
        {
            if (length > (src.Length - srcStart) || length > (src.Length - dstStart))
                throw new ArgumentOutOfRangeException();
            T[] dst = new T[src.Length];
            CopyPar<T>(src, srcStart, dst, dstStart, length, degreeOfParallelism, parallelThreshold);
            return dst;
        }
        /// <summary>
        /// Copy elements from the source array to the allocated destination array, using multiple processor cores.
        /// Slower than the version with destination array argument, because the newly allocated destination array has not yet been paged in.
        /// been paged in.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="length">number of array elements to copy</param>
        /// <param name="degreeOfParallelism">number of cores to use. Setting to 0 means use all available cores</param>
        /// <param name="parallelThreshold">array size larger than this threshold will use multiple cores</param>
        public static T[] ToArrayPar<T>(this T[] src, Int32 length, ushort degreeOfParallelism = 0, Int32 parallelThreshold = 8 * 1024)
        {
            if (length > src.Length)
                throw new ArgumentOutOfRangeException();
            T[] dst = new T[src.Length];
            CopyPar<T>(src, 0, dst, 0, length, degreeOfParallelism, parallelThreshold);
            return dst;
        }
        /// <summary>
        /// Copy elements from the source array to the allocated destination array, using multiple processor cores.
        /// Slower than the version with destination array argument, because the newly allocated destination array has not yet been paged in.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="degreeOfParallelism">number of cores to use. Setting to 0 means use all available cores</param>
        /// <param name="parallelThreshold">array size larger than this threshold will use multiple cores</param>
        public static T[] ToArrayPar<T>(this T[] src, ushort degreeOfParallelism = 0, Int32 parallelThreshold = 8 * 1024)
        {
            T[] dst = new T[src.Length];
            CopyPar<T>(src, 0, dst, 0, src.Length, degreeOfParallelism, parallelThreshold);
            return dst;
        }
    }
}
