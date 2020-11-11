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
// TODO: If .ToArrayPar() works for List.ToArrayPar() and it speeds it up, that would be amazing and something that everyone benefits from, everywhere in our code.
// TODO: We may need to determine work quanta size and then use Min(Array.Length/workQuanta, numberOfCores) number of cores to keep performance from dropping when going parallel.
//       This is a nice ramp-up from scalar to fully parallel concept, where performance should not drop off as the Array size gets smaller.
// TODO: Parallelize List.ToList() copy and compare performance to List<Int32> copy = new List<Int32>(original); constructor copying. Can List be constructed from an Array? Is it faster
//       to copy a List.ToArray() and then Array.ToList(), if such operations exist?
// TODO: See if List.CopyTo() can be implemented faster using the same methodology. Note on performance: reading List by multiple threads has some sort of a performance problem.
// TODO: Document that HPCsharp.ToArray() is more flexible for List and Array than C# standard functions, allowing developers to take a section of the source List
//       or an array and create a destination Array out of it in one step, and in parallel. Document in Readme and in the Blog.

// Performance details:
// Notes: When benchmarking "not pageInDstArray" you will see occasionally (and somewhat regularly) performance spike to a much higher level. Theory: these data points are where
// the previously allocated array is being re-used and has already been created and has been paged into system memory, raising the performance to the paged-in benchmark level.
// SSE implementation when run on all cores tops out lower than scalar. But does it ramp up faster?
// TODO: Develop graphs of scalar versus SSE/SIMD copy to see if SSE ramps up faster and allows to use fewer cores, but tops out lower than scalar.
// TODO: When developing Parallel Merge Sort, found that C#'s Array.Copy() is significantly faster than using a for loop, at least for integer types, and was as
//       fast as using SSE instructions to copy. This maybe something to do everywhere in HCPsharp, wherever copying is being performed.
// TODO: Could we create a List from an Array in parallel? Possibly create a .CopyToPar() and/or srcArray.ToListPar() and bring parallelism to construction.
// TODO: Compare performance of List to List copy to one https://stackoverflow.com/questions/1952185/how-do-i-copy-items-from-list-to-list-without-foreach which uses
//       List<Int32> copy = new List<Int32>(original); Could we use this idea to speed up List to List copy by making it parallel? If faster, then contribute to this
//       stackoverflow answer
// TODO: For List copying, could we convert to Array, copy the Array in parallel, and copy back to a List? Would all this copying pay off?
// TODO: For those functions that create a new Array that is going to be returned, would it help to page that new Array in? Could we page it in using multiple cores faster
//       than using a single core?

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Numerics;

namespace HPCsharp.ParallelAlgorithms
{
    static public partial class Copy
    {
        // Pays off on quad memory channel CPU, where one core may not be able to use all memory bandwidth with a single core
        // Not only do Xeon's have quad-channel memory, but also some high end i7's and AMD EPYC and earlier generation AMD desktop Zen CPUs
        private static void CopyParallelInnerDac<T>(this T[] src, Int32 srcStart, T[] dst, Int32 dstStart, Int32 length, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            if (length <= 0)      // zero elements to copy
                return;
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, Environment.ProcessorCount);      // default values for parallelThreshold and degreeOfParallelism
            if (length <= minWorkQuanta || degreeOfParallelism == 1)
            {
                Array.Copy(src, srcStart, dst, dstStart, length);
                return;
            }
            int lengthFirstHalf = length / 2;
            int lengthSecondHalf = length - lengthFirstHalf;
            Parallel.Invoke(
                () => { CopyParallelInnerDac<T>(src, srcStart, dst, dstStart, lengthFirstHalf, (minWorkQuanta, degreeOfParallelism)); },
                () => { CopyParallelInnerDac<T>(src, srcStart + lengthFirstHalf, dst, dstStart + lengthFirstHalf, lengthSecondHalf, (minWorkQuanta, degreeOfParallelism)); }
            );
            return;
        }
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
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        private static void CopyParFor<T>(this T[] src, Int32 srcStart, T[] dst, Int32 dstStart, Int32 length, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            if (length <= 0)      // zero elements to copy
                return;
            if (length > (src.Length - srcStart) || length > (dst.Length - dstStart))
                throw new ArgumentOutOfRangeException();

            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, 0);      // default values for parallelThreshold and degreeOfParallelism

            if (length < minWorkQuanta || degreeOfParallelism == 1)
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
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static void CopyPar<T>(this T[] src, Int32 srcStart, T[] dst, Int32 dstStart, Int32 length, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            if (length <= 0)      // zero elements to copy
                return;
            if ((srcStart + length) > src.Length || (dstStart + length) > dst.Length)
                throw new ArgumentOutOfRangeException();

            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, Environment.ProcessorCount / SystemAttributes.HyperthreadingNumberOfWays);      // default values for parallelThreshold and degreeOfParallelism

            if ((minWorkQuanta * degreeOfParallelism) < src.Length)
                minWorkQuanta = src.Length / degreeOfParallelism;

            CopyParallelInnerDac<T>(src, srcStart, dst, dstStart, length, (minWorkQuanta, degreeOfParallelism));
        }
        /// <summary>
        /// Copy elements from the source array to the destination array, using multiple processor cores.
        /// Performance is even higher whenever the destination array can be reused several times.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="dst">destination array</param>
        /// <param name="length">number of array elements to copy</param>
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static void CopyPar<T>(this T[] src, T[] dst, Int32 length, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            if (length <= 0)      // zero elements to copy
                return;
            if (length > src.Length || length > dst.Length)
                throw new ArgumentOutOfRangeException();

            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, Environment.ProcessorCount / SystemAttributes.HyperthreadingNumberOfWays);      // default values for parallelThreshold and degreeOfParallelism

            if ((minWorkQuanta * degreeOfParallelism) < src.Length)
                minWorkQuanta = src.Length / degreeOfParallelism;

            CopyParallelInnerDac<T>(src, 0, dst, 0, length, (minWorkQuanta, degreeOfParallelism));
        }

        /// <summary>
        /// Copy elements from the source array to the destination array, using multiple processor cores.
        /// Performance is even higher whenever the destination array can be reused several times.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="dst">destination array</param>
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static void CopyPar<T>(this T[] src, T[] dst, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            if (src.Length > dst.Length)
                throw new ArgumentOutOfRangeException();

            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, Environment.ProcessorCount / SystemAttributes.HyperthreadingNumberOfWays);      // default values for parallelThreshold and degreeOfParallelism

            if ((minWorkQuanta * degreeOfParallelism) < src.Length)
                minWorkQuanta = src.Length / degreeOfParallelism;

            CopyParallelInnerDac<T>(src, 0, dst, 0, src.Length, (minWorkQuanta, degreeOfParallelism));
        }
        /// <summary>
        /// Copy elements from the source array to the destination array, starting at an index within the destination, using multiple processor cores.
        /// Performance is even higher whenever the destination array can be reused several times.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="dst">destination array</param>
        /// <param name="dstStart">starting index of the destination array</param>
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        // Note: Private because of a conflict between several overloaded functions. Need to resolve.
        public static void CopyToPar<T>(this T[] src, T[] dst, Int32 dstStart, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            if (src.Length > (dst.Length - dstStart))
                throw new ArgumentOutOfRangeException();

            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, Environment.ProcessorCount / SystemAttributes.HyperthreadingNumberOfWays);      // default values for parallelThreshold and degreeOfParallelism

            if ((minWorkQuanta * degreeOfParallelism) < src.Length)
                minWorkQuanta = src.Length / degreeOfParallelism;

            CopyParallelInnerDac<T>(src, 0, dst, dstStart, src.Length, (minWorkQuanta, degreeOfParallelism));
        }
        // Note: Same speed as Array.Copy()
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
            if (length <= 0)      // zero elements to copy
                return;
            if (length > (src.Length - srcStart) || length > (dst.Length - dstStart))
                throw new ArgumentOutOfRangeException();

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
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static void CopySsePar(this int[] src, Int32 srcStart, int[] dst, Int32 dstStart, Int32 length, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            if (length <= 0)      // zero elements to copy
                return;
            if (length > (src.Length - srcStart) || length > (dst.Length - dstStart))
                throw new ArgumentOutOfRangeException();

            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, 0);      // default values for parallelThreshold and degreeOfParallelism

            if (length < minWorkQuanta || degreeOfParallelism == 1)
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
        /// <summary>
        /// Copy elements from the source array to the allocated destination array, using multiple processor cores.
        /// Slower than the version with destination array argument, because the newly allocated destination array has not yet been paged in.
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="srcStart">source array starting index</param>
        /// <param name="length">number of array elements to copy</param>
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static T[] ToArrayPar<T>(this T[] src, Int32 srcStart, Int32 length, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            if (length <= 0)      // zero elements to copy
                return new T[0];
            if (length > (src.Length - srcStart))
                throw new ArgumentOutOfRangeException();

            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, Environment.ProcessorCount / SystemAttributes.HyperthreadingNumberOfWays);      // default values for parallelThreshold and degreeOfParallelism

            if ((minWorkQuanta * degreeOfParallelism) < src.Length)
                minWorkQuanta = src.Length / degreeOfParallelism;

            T[] dst = new T[length];
            CopyParallelInnerDac<T>(src, srcStart, dst, 0, length, (minWorkQuanta, degreeOfParallelism));
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
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static T[] ToArrayPar<T>(this T[] src, Int32 length, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            if (length <= 0)      // zero elements to copy
                return new T[0];
            if (length > src.Length)
                throw new ArgumentOutOfRangeException();

            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, Environment.ProcessorCount / SystemAttributes.HyperthreadingNumberOfWays);      // default values for parallelThreshold and degreeOfParallelism

            if ((minWorkQuanta * degreeOfParallelism) < src.Length)
                minWorkQuanta = src.Length / degreeOfParallelism;

            T[] dst = new T[src.Length];
            CopyParallelInnerDac<T>(src, 0, dst, 0, length, (minWorkQuanta, degreeOfParallelism));
            return dst;
        }
        /// <summary>
        /// Copy elements from the source array to a newly allocated destination array, using multiple processor cores.
        /// Slower than the version with destination array argument, because the newly allocated destination array has not yet been paged in.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static T[] ToArrayPar<T>(this T[] src, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, Environment.ProcessorCount / SystemAttributes.HyperthreadingNumberOfWays);      // default values for parallelThreshold and degreeOfParallelism

            if ((minWorkQuanta * degreeOfParallelism) < src.Length)
                minWorkQuanta = src.Length / degreeOfParallelism;

            T[] dst = new T[src.Length];
            CopyParallelInnerDac<T>(src, 0, dst, 0, src.Length, (minWorkQuanta, degreeOfParallelism));
            return dst;
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

        public static void CopyTo<T>(this List<T> src, Int32 srcStart, List<T> dst, Int32 dstStart, Int32 length)
        {
            for (Int32 i = 0; i < length; i++)
                dst[dstStart++] = src[srcStart++];
        }

#if false
        /// <summary>
        /// Copy elements from the source List to a new destination List, using multiple processor cores.
        /// Slower than the version with destination List argument, because the newly allocated destination List has not yet been paged in.
        /// </summary>
        /// <typeparam name="T">data type of each List element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="parSettings">minWorkQuanta = number of List elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static List<T> ToListPar<T>(this List<T> src, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, Environment.ProcessorCount / SystemAttributes.HyperthreadingNumberOfWays);      // default values for parallelThreshold and degreeOfParallelism

            List<T> dst = new List<T>(src.Count);
            src.CopyToPar<T>(dst, (minWorkQuanta, degreeOfParallelism));
            return dst;
        }
        private static void CopyParallelInnerDac<T>(this List<T> src, Int32 srcStart, List<T> dst, Int32 dstStart, Int32 length, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            if (length <= 0)      // zero elements to copy
                return;
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, 2);      // default values for parallelThreshold and degreeOfParallelism
            if (length <= minWorkQuanta || degreeOfParallelism == 1)
            {
                src.CopyTo(srcStart, dst, dstStart, length);
                return;
            }

            int lengthFirstHalf = length / 2;
            int lengthSecondHalf = length - lengthFirstHalf;
            Parallel.Invoke(
                () => { CopyParallelInnerDac<T>(src, srcStart,                   dst, dstStart,                   lengthFirstHalf,  (minWorkQuanta, degreeOfParallelism)); },
                () => { CopyParallelInnerDac<T>(src, srcStart + lengthFirstHalf, dst, dstStart + lengthFirstHalf, lengthSecondHalf, (minWorkQuanta, degreeOfParallelism)); }
            );
            return;
        }
        /// <summary>
        /// Copy to an existing List from the source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="dst">destination List</param>
        /// <param name="parSettings">minWorkQuanta = number of List elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static void CopyToPar<T>(this List<T> src, List<T> dst, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, Environment.ProcessorCount / SystemAttributes.HyperthreadingNumberOfWays);      // default values for parallelThreshold and degreeOfParallelism
            if ((minWorkQuanta * degreeOfParallelism) < src.Count)
                minWorkQuanta = src.Count / degreeOfParallelism;
            CopyParallelInnerDac<T>(src, 0, dst, 0, src.Count, (minWorkQuanta, degreeOfParallelism));
        }
        /// <summary>
        /// Copy to an existing List from a portion of source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="dst">destination List</param>
        /// <param name="dstStart">starting index within dst List</param>
        /// <param name="parSettings">minWorkQuanta = number of List elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static void CopyToPar<T>(this List<T> src, List<T> dst, Int32 dstStart, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (164 * 1024, Environment.ProcessorCount / SystemAttributes.HyperthreadingNumberOfWays);      // default values for parallelThreshold and degreeOfParallelism
            if ((minWorkQuanta * degreeOfParallelism) < src.Count)
                minWorkQuanta = src.Count / degreeOfParallelism;
            CopyParallelInnerDac<T>(src, 0, dst, dstStart, src.Count, (minWorkQuanta, degreeOfParallelism));
        }
        /// <summary>
        /// Copy to an existing List from a portion of source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="srcStart">source List starting index</param>
        /// <param name="dst">destination List</param>
        /// <param name="dstStart">destination List starting index</param>
        /// <param name="length">number of List elements to copy</param>
        /// <param name="parSettings">minWorkQuanta = number of List elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static void CopyToPar<T>(this List<T> src, Int32 srcStart, List<T> dst, Int32 dstStart, Int32 length, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, Environment.ProcessorCount / SystemAttributes.HyperthreadingNumberOfWays);      // default values for parallelThreshold and degreeOfParallelism
           if ((minWorkQuanta * degreeOfParallelism) < src.Count)
                minWorkQuanta = src.Count / degreeOfParallelism;
            CopyParallelInnerDac<T>(src, srcStart, dst, dstStart, length, (minWorkQuanta, degreeOfParallelism));
        }
#endif
    }
}
