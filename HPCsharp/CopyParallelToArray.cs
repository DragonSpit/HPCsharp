// TODO: These need to handle thresholds well, to allow developers to use them any time for any size of List without having to benchmark.
//       We may need to determine work quanta size and then possibly use Min(Array.Length/workQuanta, numberOfCores) number of cores to keep performance from dropping when going parallel.
// TODO: We may need to determine work quanta size and then use Min(Array.Length/workQuanta, numberOfCores) number of cores to keep performance from dropping when going parallel.
//       This is a nice ramp-up from scalar to fully parallel concept, where performance should not drop off as the Array size gets smaller. Show with benchmark data
//       this method to always be as good as scalar for small arrays and then ramp up to being way better for large arrays, where developers have nothing to prevent them
//       from using the parallel version everywhere and always (except for setting the Threshold/MinimumWorkQuanta value).
// TODO: Change Threshold to MinimumWorkQuanta to make it clearer on what the quantity really means.
// TODO: Determine the minimum work quanta where doing work by more than one core/worker uses less time than doing the same amount of work by a single core/worker.
// TODO: Start a document/paper with all of these parallel factors that matter, such as this minimum work quanta, paged in or not of memory, aligned or unaligned scalar
//       and SIMD/SSE size items from memory.
// TODO: Develop a good example for List.ToArrayPar() and Array.ToArray() and CopyTo() to show the performance benfit immediately
// TODO: See if List.CopyTo() can be implemented faster using the same methodology.
// TODO: https://stackoverflow.com/questions/1105990/is-it-better-to-call-tolist-or-toarray-in-linq-queries?rq=1 contribute to this entry with everything learned
// TODO: Contribute to https://stackoverflow.com/questions/6750447/c-toarray-performance and show performance and point to HCPsharp
// TODO: Figure out how to support .AsParallel() by supporting ParallelQuery<List<</List>>
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace HPCsharp.ParallelAlgorithms
{
    static public partial class Copy
    {
        private static void CopyParallelInnerDac<T>(List<T> src, Int32 srcStart, List<T> dst, Int32 dstStart, Int32 length, Int32 parallelThreshold = 16 * 1024)
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
                () => { CopyParallelInnerDac<T>(src, srcStart,                   dst, dstStart,                   lengthFirstHalf,  parallelThreshold); },
                () => { CopyParallelInnerDac<T>(src, srcStart + lengthFirstHalf, dst, dstStart + lengthFirstHalf, lengthSecondHalf, parallelThreshold); }
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
        private static void CopyToArrayParallelInnerDac<T>(this List<T> src, Int32 srcStart, T[] dst, Int32 dstStart, Int32 length, (Int32 parallelThreshold, Int32 degreeOfParallelism)? parSettings = null)
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
                () => { CopyToArrayParallelInnerDac<T>(src, srcStart,                   dst, dstStart,                   lengthFirstHalf,  (parallelThreshold, degreeOfParallelism)); },
                () => { CopyToArrayParallelInnerDac<T>(src, srcStart + lengthFirstHalf, dst, dstStart + lengthFirstHalf, lengthSecondHalf, (parallelThreshold, degreeOfParallelism)); }
            );
            return;
        }

        // Ramp up linearly from 1 to all cores, as the amount of work (number of elements) grows,
        // up to the number or cores developer requested, but not more than are useful for the size of the array.
        // For example, if there is only enough work for one core/worker, then use one worker. If there is enough for two, then use two, and so on...
        // until the array or list is big enough, where there is enough work for all the cores/workers
        public static int ComputeMaxDegreeOfParallelism(Int32 length, Int32 minWorkQuanta, Int32 degreeOfParallelism)
        {
            int maxNumOfUsefulCores = Math.Min((length + minWorkQuanta - 1) / minWorkQuanta, Environment.ProcessorCount);
            int maxDegreeOfParallelism = degreeOfParallelism <= 0 ? maxNumOfUsefulCores : Math.Min(maxNumOfUsefulCores, degreeOfParallelism);
            return maxDegreeOfParallelism;
        }

        private static void CopyToArrayParallelInner<T>(this List<T> src, Int32 srcStart, T[] dst, Int32 dstStart, Int32 length, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            if (length <= 0)      // zero elements to copy
                return;
            if (length > (src.Count - srcStart) || length > (dst.Length - dstStart))
                throw new ArgumentOutOfRangeException();

            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 0);      // default values for parallelThreshold and degreeOfParallelism to use all processor cores

            if (length <= minWorkQuanta || degreeOfParallelism == 1)    // process scalar request and small enough array to be scalar, with the least amount of overhead
            {
                src.CopyTo(srcStart, dst, dstStart, length);
                return;
            }

            var options = new ParallelOptions() { MaxDegreeOfParallelism = ComputeMaxDegreeOfParallelism(length, minWorkQuanta, degreeOfParallelism) };

            Parallel.ForEach(Partitioner.Create(srcStart, srcStart + length), options, range =>
            {
                //Console.WriteLine("Partition: start = {0}   end = {1}", range.Item1, range.Item2);
                src.CopyTo(range.Item1, dst, dstStart + (range.Item1 - srcStart), (range.Item2 - range.Item1));
            });
            return;
        }

        // TODO: Figure out how to support .AsParallel() by supporting ParallelQuery<List<</List>>. Below is only partially figured out yet.
        //public static T[] ToArray<T>(this ParallelQuery<List<T>> src, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        //{
        //    (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 0);      // default values for parallelThreshold and degreeOfParallelism
        //    //var sourceArray = src.;
        //    T[] dst = new T[src.Count()];
        //    CopyToArrayParallelInner<T>(src.Cast<List<T>>(), 0, dst, 0, src.Count(), (minWorkQuanta, degreeOfParallelism));
        //    return dst;
        //}

        /// <summary>
        /// Create a new Array from the source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static T[] ToArrayPar<T>(this List<T> src, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 0);      // default values for parallelThreshold and degreeOfParallelism
            T[] dst = new T[src.Count];
            CopyToArrayParallelInner<T>(src, 0, dst, 0, src.Count, (minWorkQuanta, degreeOfParallelism));
            return dst;
        }
        /// <summary>
        /// Create a new Array from a portion of source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="srcStart">starting index within src List</param>
        /// <param name="length">number of elements to be copied</param>
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static T[] ToArrayPar<T>(this List<T> src, Int32 srcStart, Int32 length, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null )
        {
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 0);      // default values for parallelThreshold and degreeOfParallelism
            T[] dst = new T[length];
            CopyToArrayParallelInner<T>(src, srcStart, dst, 0, length, (minWorkQuanta, degreeOfParallelism));
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
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static T[] ToArrayPar<T>(this List<T> src, Int32 srcStart, Int32 dstStart, Int32 length, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 0);      // default values for parallelThreshold and degreeOfParallelism
            T[] dst = new T[src.Count];
            CopyToArrayParallelInner<T>(src, srcStart, dst, dstStart, length, (minWorkQuanta, degreeOfParallelism));
            return dst;
        }
        /// <summary>
        /// Copy to an existing Array from the source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="dst">destination array</param>
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static void CopyToPar<T>(this List<T> src, T[] dst, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 0);      // default values for parallelThreshold and degreeOfParallelism
            CopyToArrayParallelInner<T>(src, 0, dst, 0, src.Count, (minWorkQuanta, degreeOfParallelism));
        }
        /// <summary>
        /// Copy to an existing Array from a portion of source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="dst">destination array</param>
        /// <param name="dstStart">starting index within dst Array</param>
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static void CopyToPar<T>(this List<T> src, T[] dst, Int32 dstStart, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 0);      // default values for parallelThreshold and degreeOfParallelism
            CopyToArrayParallelInner<T>(src, 0, dst, dstStart, src.Count, (minWorkQuanta, degreeOfParallelism));
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
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static void CopyToPar<T>(this List<T> src, Int32 srcStart, T[] dst, Int32 dstStart, Int32 length, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 0);      // default values for parallelThreshold and degreeOfParallelism
            CopyToArrayParallelInner<T>(src, srcStart, dst, dstStart, length, (minWorkQuanta, degreeOfParallelism));
        }
    }
}
