// TODO: To speedup summing up of long to decimal accumulation, Josh suggested using a long accumulator and catching the overflow exception and then adding to decimal - i.e. most of the time accumulate to long and once in
//       a while accumulate to decimal instead of always accumulating to decimal (offer this version as an alternate)
// TODO: Implement a for loop instead of divide-and-conquer, since they really accomplish the same thing, and the for loop will be more efficient and easier to make cache line boundary divisible.
//       Combining will be slightly harder, but we could create an array of sums, where each task has its own array element to fill in, then we combine all of the sums at the end serially. Still has the issue of managed memory
//       where the array may move and not be cache aligned any more, requiring fixing the array in memory to not move (an unsafe version).
// TODO: Study parallel solution presented here (parallel for), which may be better in some cases: https://stackoverflow.com/questions/2419343/how-to-sum-up-an-array-of-integers-in-c-sharp/54794753#54794753
//       This method should control the degree of parallelism better than divide-and-conquer (DAC), since DAC we found to be unable to control degree of parallelism
// TODO: Implement nullable versions of Sum, only faster than the standard C# ones. Should be able to still use SSE and multi-core, but need to check out each element for null before adding it. These
//       will be much slower than non-nullable. Hmmm.. Need to test what Linq.Sum() returns for nullable, as I bet the sum becomes null, since null is really an unknown and if one value of an array is unknown, then the Sum() becomes unknown.
//       If that's the case for Linq.Sum() for nullable types, then SIMD acceleration is still possible, but will be much slower, since we'll need to test each Vector.Count array items for null, and only if they all are not-null then add them to the sum.
//       From research on the web, Linq.Sum() skips null values by default - i.e. treats them as zeroes. This is inconsistent with adding a null value to a number.
// TODO: See if SSEandScalar version is faster when the array is entirely inside the cache, to make sure that it's not just being memory bandwidth limited and hiding ILP speedup. Port it to C++ and see
//       if it speeds up. Run many times over the same array using .Sum() and provide the average and minimum timing. Also, could test using a single core, where memory bandwidth is not the limiter.
// TODO: Return a tupple (sum and c) from each parallel Neumaier result and figure out how to combine these results for a more accurate and possibly perfect overall result that will match serial array processing result.
// TODO: Implement .Sum() for summing a field of Objects/UserDefinedTypes, if it's possible to add value by possibly aggregating elements into a local array of Vector size and doing an SSE sum. Is it possible to abstract it well and to
//       perform well to support extracting all numeric data types, so that performance and abstraction are competitive and as simple or simpler than Linq?
// TODO: Write a blog on floating-point .Sum() and all of it's capabilities, options and trade-offs in performance and accuracy (Submitted a paper proposal to the MSDN journal. Waiting for response first.)
// TODO: Re-use the new generic divide-and-conquer function, and it could even be a lambda function for some implementations (like non-Kahan-Neumaier addition). For float and double summation, we just need to pass in function of double or float.
//       This would reduce the code base within this file by a very large amount, as most if not all divide-and-conquer repeated implementations would go away. Then we can claim that we use our own divide-and-conquer abstraction inside HPCsharp itself.
// TODO: Note that by default parallel .Sum() implementations are pairwise summation, since it does divide-and-conquer. This needs to be noted in the Readme or somehow be communicated to the user, and bloged about and in the parallel section of
//       wikipedia pairwise summation.
// TODO: One of the issues with C# nullable type is that it consists of a byte for the boolean and of another type. This makes
//       it convenient for single elements, but difficult to process in a data parallel fashion with higher performance.
//       A better array structure would be an array for booleans and an array to another type. This helps data parallel instruction
//       since these work only on data of the same size. Maybe that's what we could suggest and implement it to provide
//       a higher performance alternative.
// TODO: Apply the same technique of double the memory load as used for ZeroDetection by loading two per loop.
//       Wonder if having two or more independent loop counters would also help, along with two or more memory loads.
//       Need to see if SSE on single core is memory limited. Need to add to table single-core performance.
// TODO: Need to implement integer and unsigned integer .SumPar() - i.e. without SSE, but multi-core.
// TODO: Implement pair-wise floating-point summation that is multi-core and SSE, with separate SSE implementation which recursively combines an SSE-word all the way down possibly, as this eliminates the problem of base-case function being non-pair,
//       as this most likely could be just about as fast, or we could develop one that is just as fast and keeps the pairing using SSE all the way to the bottom of recursion.
// TODO: One idea that Josh and I came up with to detect overflow for SSE instructions if they saturate for addition is
//       to subtract and see if the result is the same as the original. If C# chooses the wrap around SSE instructions then
//       the same technique may still work, or we may need to come up with a different technique to detect wrap around,
//       possibly when the addition to a positive value makes the result go negative.
// TODO: Benchmark all Int64.MaxValue to show the worst case of BigInteger and Decimal summation of long[]
// TODO: Benchmark smaller arrays that fit into cache to show even a higher level of acceleration for a common user case
//       where the previous step in functional flow will most likely put the result inside the cache.
// TODO: Implement long[] to decimal and BigInteger in SSE with overflow detection in s/w. For ulong[] detections can
//       possibly be done by checking if the sum after the addition is smaller than the sum before the addition.
//       The same can be done with scalar code, to avoid the overhead of checking for overflow, raising exception, catching it
//       and then dealing with it - instead just checking for it all the time, which shouold be just as expensive/cheap
//       as doing checked. If this works, this would make a great blog post or part of an article.
//       We will have to figure out a similar method for long[] since it's signed.
// TODO: Don't forget for ulong Sum of ulong[] needs to throw an overflow exception, along with long Sum of long[]. Now, that we've developed
//       a way to detect it that is pretty cheap.
// TODO: Create a checked ulong[] .Sum and long[] .Sum that simply use SSE instructions, but now check for numerical overflow and throw an exception if it happens, and don't do anything about it, but at
//       at least report it. This will match functionality of Linq implementation, but now with SSE, and at least ulong[] checked SSE Sum will be way faster.
// TODO: We could also do SSE equivalent versions of int[] .Sum() in SSE, and uint[] in SSE that are fast and check
//       for numeric overflow. We could even do it for other smaller data types, if it's worthwhile to do. This may provide
//       higher performance, especially for uint[] checked, which would be equivalent in functionality to LInq and be higher
//       performance than expanding to long implementation in SSE - i.e. still safe, but eliminating overflow, just checking for it.
// TODO: Create a checkedAdd() for Vector<long> addition and Vector<ulong> addition cases, since we now know exactly what to do to work around
//       the lack of this capability by the checked block key word, and throw an overflow exception in the detected cases, with minimal overhead.
// TODO: Make sure to look at this https://stackoverflow.com/questions/49308115/c-sharp-vectordouble-copyto-barely-faster-than-non-simd-version?rq=1
// TODO: Contribute to this https://stackoverflow.com/questions/2056445/no-overflow-exception-for-int-in-c and point them to my question of
//       no overflow even when checked for SSE instructions.
// TODO: Suggest to Intel/AMD how to improve SSE instructions to make them better for addition, such as like multiplication, put the carry bits in another result, or produce a bigger result. The overflow bit seems like a really
//       outdated and inefficient way of computing. We can do better.
// TODO: Figure out the bug with "BigInteger += longValue" that Microsoft seems to have, possibly posting to StackOverflow, as it seems to produce wrong result, but "BigInteger = BigInteger + longValue" seems to work fine.
// TODO: Implement checked SSE subtraction, which is a bit less useful, but completes checked Addition/Subtraction coverage
// TODO: Figure out how .AsParallel() works by studying .NET Core open source code, and make HPCsharp compattible with it.
//       Add a new .AsDataParallel() that implements it in SIMD/SSE as an option.
// TODO: Check out this blog and links that it points to https://devblogs.microsoft.com/dotnet/hardware-intrinsics-in-net-core/
// TODO: Apply the new overflow detecting SSE implementation which has no if statements to byte, ushort, uint .SumSse()
//       as this may be faster, because + would be done at native data type size (byte, ushort, uint) instead of
//       spending instruction to expand bytes to ulong, which takes many steps. The way to do this for byte is to use a byte (same data type)
//       for carry-out bits and make an inner loop be fixed at 256 times, and figure out ahead of time how many of these 256 times loops we
//       can do. Then outside the loop accumulate these carry-out's into several 32-bit split SSE registers. This way the very inner loop is
//       minimal and the outer loops happens less often, amortized over the 256 times loop. This method should also work well for ushort. Uint won't need it.
// TODO: Benchmark the above improved SSE .Sum() implementation inside CPU cache only, by doing many loops accross
//       an array that fits into the cache completely, to see how much faster it really runs when not being
//       limited by speed of system memory. This may change how we do Functional programming efficiently in the future!
// TODO: Apply the new SSE worst-case optimization for ulong[].Sum() to BigInteger as well, as this will speedup even more than Decimal in the worst-case.
// TODO: Document in the Readme that .Sum() is a next higher level of abstraction for Sum(), since it takes care of more details internally, such as
//       removes the need to be concerned about arithmetic overflow, as that is handled inside the HPCsharp .Sum() functions.
// TODO: Implement .AsParallel() and .AsSafe() methods, which transform input array into whatever output .AsParallel() transforms normally to.
//       Also develop a special data type for .AsSafe() to transform the output to, and handle this data type in HPCsharp functions. This will make it
//       simpler for the developers to use instead of dealing with naming of functions.
// TODO: Benchmarks of unrolled SSE .Sum() show > 2X speedup for arrays inside the cache. Create routines using this methodology and show even faster
//       overall performance for arrays inside the cache using multi-core or single core. Single-core unrolled is running at almost 7 GigaAdds/sec for long[].
//       Multi-core performance seems to get limited to 4 GigaAdds/sec, possibly due to being limited by memory contention. Unrolled SSE multi-core seems
//       to perform at the same level as single SSE instruction per loop (not-unrolled). Thus, it's beneficial to use unrolled SSE, since it gains speed
//       when the array is inside the cache, especially for single core.
// TODO: 8-way unrolling of SIMD for long .SUM slowed things down when compared to 4-way unrolling, which provides 2X speedup over no unrolling.
//       for small arrays that fit into L2 cache. Need to try 2-way and 3-way unroll to see if these provide even higher performance.
// TODO: Write a blog comparing SumToLongParFor(intToLong) with HPCsharp using only two cores versus this one with 2 thru 6 cores, since HCPsharp uses SIMD/SSE.
//       Great comparison versus Lambda's too, since Lambda's have function call overhead per array element. This would be a great blog on its own - Prefer ParallelFor to Lambda's for Performance.
// TODO: Perform a random search of the best split of arrays for multi-core performance. Is it on cache-line boundaries, page boundaries, or relatively prime to
//       each other in some way. Another approach is to study why some array sizes and thus their splits perform better than others and see if there is a patterns
//       Maybe the first place to check is if the top performers are consistenly top performing.
// TODO: Figure out why BigInteger sum from long and ulong FasterPar divide-and-conquer generic is throwing an type conversion exception when using the Generic Divide and Conquer.

using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace HPCsharp.ParallelAlgorithms
{
    static public partial class Sum
    {
        /// <summary>
        /// Summation of sbyte[] array, which uses a long accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLongSse(this sbyte[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of sbyte[] array, which uses a long accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLongSse(this sbyte[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static long SumSseInner(this sbyte[] arrayToSum, int l, int r)
        {
            var sumVector = new Vector<long>();

            int sseIndexEnd = l + ((r - l + 1) / (256 * Vector<sbyte>.Count)) * (256 * Vector<sbyte>.Count);

            int incr = Vector<sbyte>.Count;
            int i;
            for (i = l; i < sseIndexEnd;)
            {
                var shortSumLow0  = new Vector<short>();
                var shortSumHigh0 = new Vector<short>();
                for (int j = 0; j < 256; j++, i += incr)
                {
                    var inVector0 = new Vector<sbyte>(arrayToSum, i);
                    Vector.Widen(inVector0, out var shortLow0, out var shortHigh0);
                    shortSumLow0  += shortLow0;
                    shortSumHigh0 += shortHigh0;
                }
                Vector.Widen(shortSumLow0, out var int0, out var int1);
                int0 += int1;
                Vector.Widen(shortSumHigh0, out var int2, out var int3);
                int0 += int2;
                int0 += int3;

                Vector.Widen(int0, out var long0, out var long1);
                sumVector += long0;
                sumVector += long1;
            }

            long overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of byte[] array, which uses a ulong accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongSse(this byte[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of byte[] array, which uses a ulong accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongSse(this byte[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static ulong SumSseInner(this byte[] arrayToSum, int l, int r)
        {
            var sumVector = new Vector<ulong>();

            int sseIndexEnd = l + ((r - l + 1) / (256 * Vector<byte>.Count)) * (256 * Vector<byte>.Count);

            int incr = Vector<byte>.Count;
            int i;
            for (i = l; i < sseIndexEnd; )
            {
                var ushortSumLow0  = new Vector<ushort>();
                var ushortSumHigh0 = new Vector<ushort>();
                for (int j = 0; j < 256; j++, i += incr)
                {
                    var inVector0 = new Vector<byte>(arrayToSum, i);
                    Vector.Widen(inVector0, out var ushortLow0, out var ushortHigh0);
                    ushortSumLow0  += ushortLow0;
                    ushortSumHigh0 += ushortHigh0;
                }
                Vector.Widen(ushortSumLow0,  out var uint0, out var uint1);
                uint0 += uint1;
                Vector.Widen(ushortSumHigh0, out var uint2, out var uint3);
                uint0 += uint2;
                uint0 += uint3;

                Vector.Widen(uint0, out var ulong0, out var ulong1);
                sumVector += ulong0;
                sumVector += ulong1;
            }

            ulong overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<ulong>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        private static ulong SumSseInnerUnrolled(this byte[] arrayToSum, int l, int r)
        {
            var sumVector = new Vector<ulong>();

            int sseIndexEnd = l + ((r - l + 1) / (256 * Vector<byte>.Count)) * (256 * Vector<byte>.Count);

            int incr = Vector<byte>.Count;
            int incrTimes2 = 2 * Vector<byte>.Count;
            int i;
            for (i = l; i < sseIndexEnd;)
            {
                var ushortSum00 = new Vector<ushort>();
                var ushortSum01 = new Vector<ushort>();
                var ushortSum10 = new Vector<ushort>();
                var ushortSum11 = new Vector<ushort>();
                for (int j = 0; j < 256; j += 2, i += incrTimes2)
                {
                    var inVector0 = new Vector<byte>(arrayToSum, i);
                    var inVector1 = new Vector<byte>(arrayToSum, i + incr);
                    Vector.Widen(inVector0, out var ushort00, out var ushort01);
                    Vector.Widen(inVector1, out var ushort10, out var ushort11);
                    ushortSum00 += ushort00;
                    ushortSum01 += ushort01;
                    ushortSum10 += ushort10;
                    ushortSum11 += ushort11;
                }
                Vector.Widen(ushortSum00, out var uint0, out var uint1);
                uint0 += uint1;
                Vector.Widen(ushortSum01, out var uint2, out var uint3);
                uint0 += uint2;
                uint0 += uint3;
                Vector.Widen(ushortSum10, out uint2, out uint3);
                uint0 += uint2;
                uint0 += uint3;
                Vector.Widen(ushortSum11, out uint2, out uint3);
                uint0 += uint2;
                uint0 += uint3;

                Vector.Widen(uint0, out var ulong0, out var ulong1);
                sumVector += ulong0;
                sumVector += ulong1;
            }

            ulong overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<ulong>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of short[] array, which uses a long accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLongSse(this short[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of short[] array, which uses a long accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLongSse(this short[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static long SumSseInner(this short[] arrayToSum, int l, int r)
        {
            var sumVector = new Vector<long>();

            int sseIndexEnd = l + ((r - l + 1) / (256 * Vector<short>.Count)) * (256 * Vector<short>.Count);

            int incr = Vector<short>.Count;
            int i;
            for (i = l; i < sseIndexEnd;)
            {
                var intSumLow0  = new Vector<int>();
                var intSumHigh0 = new Vector<int>();
                for (int j = 0; j < 256; j++, i += incr)
                {
                    var inVector0 = new Vector<short>(arrayToSum, i);
                    Vector.Widen(inVector0, out var intLow0, out var intHigh0);
                    intSumLow0  += intLow0;
                    intSumHigh0 += intHigh0;
                }
                Vector.Widen(intSumLow0, out var long0, out var long1);
                sumVector += long0;
                sumVector += long1;
                Vector.Widen(intSumHigh0, out long0, out long1);
                sumVector += long0;
                sumVector += long1;
            }

            long overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of ushort[] array, which uses a ulong accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongSse(this ushort[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of ushort[] array, which uses a ulong accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongSse(this ushort[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static ulong SumSseInner(this ushort[] arrayToSum, int l, int r)
        {
            var sumVector = new Vector<ulong>();

            int sseIndexEnd = l + ((r - l + 1) / (256 * Vector<ushort>.Count)) * (256 * Vector<ushort>.Count);

            int incr = Vector<ushort>.Count;
            int i;
            for (i = l; i < sseIndexEnd;)
            {
                var uintSumLow  = new Vector<uint>();
                var uintSumHigh = new Vector<uint>();
                for (int j = 0; j < 256; j++, i += incr)
                {
                    var inVector = new Vector<ushort>(arrayToSum, i);
                    Vector.Widen(inVector, out var uintLow, out var uintHigh);
                    uintSumLow  += uintLow;
                    uintSumHigh += uintHigh;
                }
                Vector.Widen(uintSumLow, out var ulong0, out var ulong1);
                sumVector += ulong0;
                sumVector += ulong1;
                Vector.Widen(uintSumHigh, out ulong0, out ulong1);
                sumVector += ulong0;
                sumVector += ulong1;
            }

            ulong overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<ulong>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of int[] array, which uses a long accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLongSse(this int[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of int[] array, which uses a long accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLongSse(this int[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static long SumSseInner(this int[] arrayToSum, int l, int r)
        {
            var sumVectorLower = new Vector<long>();
            var sumVectorUpper = new Vector<long>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<int>.Count) * Vector<int>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVector = new Vector<int>(arrayToSum, i);
                Vector.Widen(inVector, out var longLower, out var longUpper);
                sumVectorLower += longLower;
                sumVectorUpper += longUpper;
            }
            long overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVectorLower[i];
            return overallSum;
        }

        // TODO: Implement aligned SIMD sum, since memory alignment is critical for SIMD instructions. So, do scalar first until we are SIMD aligned and then do SIMD, followed by more scarlar to finish all
        //       left over elements that are not SIMD size divisible. First simple step is to check alignment of SIMD portion of the sum. See cache-aligned entry below, which may solve this problem.
        // Conclusion: In modern Intel CPUs this issue seems to have been resolved. It used to be an issue in earlier generations when SIMD/SSE was young.
        private static long SumSseOffset(this int[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        private static long SumSseOffsetInner(this int[] arrayToSum, int l, int r)
        {
            var sumVectorLower = new Vector<long>();
            var sumVectorUpper = new Vector<long>();
            int offset = 3;
            long overallSum = 0;
            int i;
            for (i = l; i < (l + offset); i++)
                overallSum += arrayToSum[i];
            l += offset;
            int sseIndexEnd = l + ((r - l + 1) / Vector<int>.Count) * Vector<int>.Count;
            for (i = l; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVector = new Vector<int>(arrayToSum, i);
                Vector.Widen(inVector, out var longLower, out var longUpper);
                sumVectorLower += longLower;
                sumVectorUpper += longUpper;
            }
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVectorLower[i];
            return overallSum;
        }

        private static long SumSseInner(this int?[] arrayToSum, int l, int r)
        {
            var sumVectorLower = new Vector<long>();
            var sumVectorUpper = new Vector<long>();
            var longLower      = new Vector<long>();
            var longUpper      = new Vector<long>();
            var intLocalVector = new int[Vector<int>.Count];
            var intLocalZero   = new Vector<int>();
            intLocalZero = default(Vector<int>);
            int sseIndexEnd = l + ((r - l + 1) / Vector<int>.Count) * Vector<int>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<int>.Count)
            {
#if true
                intLocalZero.CopyTo(intLocalVector, 0);
                for (int j = 0, k = i; j < intLocalVector.Length; j++, k++)
                    if (arrayToSum[k] != null)
                        intLocalVector[j] = (int)arrayToSum[k];
                var inVector = new Vector<int>(intLocalVector, 0);
#else
                for (int j = 0, k = i; j < intLocalVector.Length; j++, k++)
                    intLocalVector[j] = arrayToSum[k] ?? 0;
                var inVector = new Vector<int>(intLocalVector, 0);
#endif
                Vector.Widen(inVector, out longLower, out longUpper);
                sumVectorLower += longLower;
                sumVectorUpper += longUpper;
            }
            long overallSum = 0;
            for (; i <= r; i++)
                if (arrayToSum[i] != null)
                    overallSum += (int)arrayToSum[i];
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVectorLower[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of int?[] nullable array, which uses a long accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLongSse(this int?[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of int?[] nullable array, which uses a long accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLongSse(this int?[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        public static long SumSseAndScalarSingleStream(this int[] arrayToSum)
        {
            //return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
            return arrayToSum.SumSseAndScalarSingleStreamInner(0, arrayToSum.Length - 1);
        }

        public static long SumSseAndScalarSingleStream(this int[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseAndScalarSingleStreamInner(start, start + length - 1);
        }

        // 256-bit vector means eight 32-bit, which is the minimal size scalar needs to do
        // and then some number of vectors as well.
        private static long SumSseAndScalarSingleStreamInner(this int[] arrayToSum, int l, int r)
        {
            //const int numScalarOps = 2;
            var sumVectorLower0 = new Vector<long>();
            var sumVectorUpper0 = new Vector<long>();
            var sumVectorLower1 = new Vector<long>();
            var sumVectorUpper1 = new Vector<long>();
            var sumVectorLower2 = new Vector<long>();
            var sumVectorUpper2 = new Vector<long>();
            var sumVectorLower3 = new Vector<long>();
            var sumVectorUpper3 = new Vector<long>();
            var longLower0 = new Vector<long>();
            var longUpper0 = new Vector<long>();
            var longLower1 = new Vector<long>();
            var longUpper1 = new Vector<long>();
            var longLower2 = new Vector<long>();
            var longUpper2 = new Vector<long>();
            var longLower3 = new Vector<long>();
            var longUpper3 = new Vector<long>();
            int sseIndexEnd = l + ((r - l + 1) / (5 * Vector<int>.Count)) * Vector<int>.Count;
            //int numFullVectors = lengthForVector / Vector<int>.Count;
            long partialScalarSum0 = 0;
            long partialScalarSum1 = 0;
            long partialScalarSum2 = 0;
            long partialScalarSum3 = 0;
            int i = l;
            //int scalarIndex = l + numFullVectors * Vector<int>.Count;
            //int sseIndexEnd = scalarIndex;
            //System.Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", arrayToSum.Length, Vector<int>.Count, lengthForVector, numFullVectors, scalarIndex, sseIndexEnd);
            for (; i < sseIndexEnd; )
            {
                var inVector0 = new Vector<int>(arrayToSum, i);
                i += Vector<int>.Count;
                Vector.Widen(inVector0, out longLower0, out longUpper0);
                var inVector1 = new Vector<int>(arrayToSum, i);
                i += Vector<int>.Count;
                Vector.Widen(inVector1, out longLower1, out longUpper1);
                var inVector2 = new Vector<int>(arrayToSum, i);
                i += Vector<int>.Count;
                Vector.Widen(inVector2, out longLower2, out longUpper2);
                var inVector3 = new Vector<int>(arrayToSum, i);
                i += Vector<int>.Count;
                Vector.Widen(inVector3, out longLower3, out longUpper3);
                // Vector number of bits / 32-bits = number of scalar additions need to perform - i.e. one Vector's worth of scalar work
                partialScalarSum0 += arrayToSum[i++];
                sumVectorLower0 += longLower0;
                partialScalarSum1 += arrayToSum[i++];
                sumVectorUpper0 += longUpper0;
                partialScalarSum2 += arrayToSum[i++];
                sumVectorLower1 += longLower1;
                partialScalarSum3 += arrayToSum[i++];
                sumVectorUpper1 += longUpper1;
                partialScalarSum0 += arrayToSum[i++];
                sumVectorLower2 += longLower2;
                partialScalarSum1 += arrayToSum[i++];
                sumVectorUpper2 += longUpper2;
                partialScalarSum2 += arrayToSum[i++];
                sumVectorLower3 += longLower3;
                partialScalarSum3 += arrayToSum[i++];
                sumVectorUpper3 += longUpper3;
            }
            //System.Console.WriteLine("{0}", scalarIndex);
            for (; i <= r; i++)
                partialScalarSum0 += arrayToSum[i];
            partialScalarSum0 += partialScalarSum1;
            partialScalarSum0 += partialScalarSum2;
            partialScalarSum0 += partialScalarSum3;
            sumVectorLower0 += sumVectorUpper0;
            sumVectorLower1 += sumVectorUpper1;
            sumVectorLower2 += sumVectorUpper2;
            sumVectorLower3 += sumVectorUpper3;
            sumVectorLower0 += sumVectorLower1;
            sumVectorLower0 += sumVectorLower2;
            sumVectorLower0 += sumVectorLower3;
            for (i = 0; i < Vector<long>.Count; i++)
                partialScalarSum0 += sumVectorLower0[i];
            return partialScalarSum0;
        }

        public static long SumSseAndScalar(this int[] arrayToSum)
        {
            //return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
            return arrayToSum.SumSseAndScalarInner(0, arrayToSum.Length - 1);
        }

        public static long SumSseAndScalar(this int[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseAndScalarInner(start, start + length - 1);
        }
        // Sadly, even in-cache small arrays are not speeding up with this interleaving idea :-(
        // Yeah, sadly it's about 15% slower
        private static long SumSseAndScalarInner(this int[] arrayToSum, int l, int r)
        {
            const int numScalarOps = 1;
            var sumVectorLower = new Vector<long>();
            var sumVectorUpper = new Vector<long>();
            var longLower      = new Vector<long>();
            var longUpper      = new Vector<long>();
            int lengthForVector = (r - l + 1) / (Vector<int>.Count + numScalarOps) * Vector<int>.Count;
            int numFullVectors = lengthForVector / Vector<int>.Count;
            long partialScalarSum0 = 0;
            //long partialScalarSum1 = 0;
            int i = l;
            int scalarIndex = l + numFullVectors * Vector<int>.Count;
            int sseIndexEnd = scalarIndex;
            //System.Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", arrayToSum.Length, Vector<int>.Count, lengthForVector, numFullVectors, scalarIndex, sseIndexEnd);
            for (; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVector = new Vector<int>(arrayToSum, i);
                Vector.Widen(inVector, out longLower, out longUpper);
                partialScalarSum0 += arrayToSum[scalarIndex++];          // interleave SSE and Scaler operations
                sumVectorLower    += longLower;
                //partialScalarSum1 += arrayToSum[scalarIndex++];
                sumVectorUpper    += longUpper;
            }
            //System.Console.WriteLine("{0}", scalarIndex);
            for (i = scalarIndex; i <= r; i++)
                partialScalarSum0 += arrayToSum[i];
            //partialScalarSum0 += partialScalarSum1;
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<long>.Count; i++)
                partialScalarSum0 += sumVectorLower[i];
            return partialScalarSum0;
        }

        public static ulong SumSseAndScalarSingleStream(this ulong[] arrayToSum)
        {
            //return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
            return arrayToSum.SumSseAndScalarSingleStreamInner(0, arrayToSum.Length - 1);
        }

        public static ulong SumSseAndScalarSingleStream(this ulong[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseAndScalarSingleStreamInner(start, start + length - 1);
        }

        // 256-bit vector means eight 32-bit, which is the minimal size scalar needs to do
        // and then some number of vectors as well.
        // Sadly, this idea also doesn't seem to work and slows down performance.
        private static ulong SumSseAndScalarSingleStreamInner(this ulong[] arrayToSum, int l, int r)
        {
            //const int numScalarOps = 2;
            var sumVector0 = new Vector<ulong>();
            //var sumVector1 = new Vector<ulong>();
            //var sumVector2 = new Vector<ulong>();
            //var sumVector3 = new Vector<ulong>();
            int sseIndexEnd = l + ((r - l + 1) / (1 * Vector<ulong>.Count)) * Vector<ulong>.Count;
            //int numFullVectors = lengthForVector / Vector<int>.Count;
            ulong partialScalarSum0 = 0;
            //ulong partialScalarSum1 = 0;
            //ulong partialScalarSum2 = 0;
            //ulong partialScalarSum3 = 0;
            int i = l;
            //int j = l + (4 * Vector<ulong>.Count);
            //int j = l + 1 * Vector<ulong>.Count;
            //int k = l + 2 * Vector<ulong>.Count;
            //int m = l + 3 * Vector<ulong>.Count;
            int increment = 1 * Vector<ulong>.Count;
            //int scalarIndex = l + numFullVectors * Vector<int>.Count;
            //int sseIndexEnd = scalarIndex;
            //System.Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", arrayToSum.Length, Vector<int>.Count, lengthForVector, numFullVectors, scalarIndex, sseIndexEnd);
            for (; i < sseIndexEnd; i += increment)
            {
                sumVector0 += new Vector<ulong>(arrayToSum, i);
                //sumVector1 += new Vector<ulong>(arrayToSum, j);
                //sumVector2 += new Vector<ulong>(arrayToSum, k);
                //sumVector3 += new Vector<ulong>(arrayToSum, m);

                //var inVector0 = new Vector<ulong>(arrayToSum, i);
                //i += Vector<ulong>.Count;
                //var inVector1 = new Vector<ulong>(arrayToSum, j);
                //i += Vector<ulong>.Count;
                //var inVector2 = new Vector<ulong>(arrayToSum, k);
                //i += Vector<ulong>.Count;
                //var inVector3 = new Vector<ulong>(arrayToSum, m);
                //i += Vector<ulong>.Count;
                // Vector number of bits / 32-bits = number of scalar additions need to perform - i.e. one Vector's worth of scalar work
                //partialScalarSum0 += arrayToSum[j++];
                //sumVector0        += inVector0;
                //partialScalarSum1 += arrayToSum[j++];
                //sumVector1        += inVector1;
                //partialScalarSum2 += arrayToSum[j++];
                //sumVector2        += inVector2;
                //partialScalarSum3 += arrayToSum[j++];
                //sumVector3        += inVector3;
                //i += increment;
                //j += increment;
                //k += increment;
                //m += increment;
                //j += 4 * Vector<ulong>.Count;
            }
            //System.Console.WriteLine("{0}", scalarIndex);
            for (; i <= r; i++)
                partialScalarSum0 += arrayToSum[i];
            //partialScalarSum0 += partialScalarSum1;
            //partialScalarSum0 += partialScalarSum2;
            //partialScalarSum0 += partialScalarSum3;
            //sumVector0 += sumVector1;
            //sumVector0 += sumVector2;
            //sumVector0 += sumVector3;
            for (i = 0; i < Vector<ulong>.Count; i++)
                partialScalarSum0 += sumVector0[i];
            return partialScalarSum0;
        }

        /// <summary>
        /// Summation of uint[] array, which uses a ulong accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongSse(this uint[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of uint[] array, which uses a ulong accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongSse(this uint[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static ulong SumSseInner(this uint[] arrayToSum, int l, int r)
        {
            var sumVectorLower = new Vector<ulong>();
            var sumVectorUpper = new Vector<ulong>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<uint>.Count) * Vector<uint>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVector = new Vector<uint>(arrayToSum, i);
                Vector.Widen(inVector, out var ulongLower, out var ulongUpper);
                sumVectorLower += ulongLower;
                sumVectorUpper += ulongUpper;
            }
            ulong overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<ulong>.Count; i++)
                overallSum += sumVectorLower[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of long[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Caution: Will not throw an overflow exception for the majority of the array, but instead will wrap around to negatives, when the sum goes beyond Int64.MaxValue
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than Int64.MaxValue</exception>
        public static long SumSse(this long[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of long[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Caution: Will not throw an overflow exception for the majority of the array, but instead will wrap around to negatives, when the sum goes beyond Int64.MaxValue
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than Int64.MaxValue</exception>
        public static long SumSse(this long[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static long SumSseInner(this long[] arrayToSum, int l, int r)
        {
            var sumVector = new Vector<long>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<long>.Count) * Vector<long>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<long>.Count)
            {
                var inVector = new Vector<long>(arrayToSum, i);
                sumVector += inVector;
            }
            long overallSum = 0;
            for (; i <= r; i++)
            {
                overallSum = checked(overallSum + arrayToSum[i]);
            }
            for (i = 0; i < Vector<long>.Count; i++)
            {
                overallSum = checked(overallSum + sumVector[i]);
            }
            return overallSum;
        }

        private static long SumSse2(this long[] arrayToSum)
        {
            return arrayToSum.SumSseInner2(0, arrayToSum.Length - 1);
        }

        private static long SumSse2(this long[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseInner2(start, start + length - 1);
        }

        private static long SumSseInner2(this long[] arrayToSum, int l, int r)
        {
            var sumVector = new Vector<long>();
            int concurrentAmount = 4;
            int sseIndexEnd = l + ((r - l + 1) / (Vector<long>.Count * concurrentAmount)) * (Vector<long>.Count * concurrentAmount);
            int offset1 = Vector<long>.Count;
            int offset2 = Vector<long>.Count * 2;
            int offset3 = Vector<long>.Count * 3;
            int i;
            int increment = Vector<long>.Count * concurrentAmount;
            for (i = l; i < sseIndexEnd; i += increment)
            {
                sumVector += new Vector<long>(arrayToSum, i);
                sumVector += new Vector<long>(arrayToSum, i + offset1);
                sumVector += new Vector<long>(arrayToSum, i + offset2);
                sumVector += new Vector<long>(arrayToSum, i + offset3);
            }
            long overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        private static long SumSse3(this long[] arrayToSum)
        {
            return arrayToSum.SumSseInner3(0, arrayToSum.Length - 1);
        }

        private static long SumSse3(this long[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseInner3(start, start + length - 1);
        }
        // More than 2X faster on my 6-core laptop when the array is inside the cache. Single-core runs at nearly 7 GigaAdds/sec, but multi-core isn't performing well
        // 8-way unrolling slowed performance way down. Wonder if 2 or 3-way unroll will provide a higher speedup?!
        private static long SumSseInner3(this long[] arrayToSum, int l, int r)
        {
            var sumVector0 = new Vector<long>();
            var sumVector1 = new Vector<long>();
            var sumVector2 = new Vector<long>();
            var sumVector3 = new Vector<long>();
            int concurrentAmount = 4;
            int sseIndexEnd = l + ((r - l + 1) / (Vector<long>.Count * concurrentAmount)) * (Vector<long>.Count * concurrentAmount);
            int offset1 = Vector<long>.Count;
            int offset2 = Vector<long>.Count * 2;
            int offset3 = Vector<long>.Count * 3;
            int i, j, k, m;
            int increment = Vector<long>.Count * concurrentAmount;
            for (i = l, j = l + offset1, k = l + offset2, m = l + offset3; i < sseIndexEnd; i += increment, j += increment, k += increment, m += increment)
            {
                sumVector0 += new Vector<long>(arrayToSum, i);
                sumVector1 += new Vector<long>(arrayToSum, j);
                sumVector2 += new Vector<long>(arrayToSum, k);
                sumVector3 += new Vector<long>(arrayToSum, m);
            }
            long overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            sumVector0 += sumVector1;
            sumVector0 += sumVector2;
            sumVector0 += sumVector3;
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVector0[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of long[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Throws a System.OverflowException when the sum goes beyond Int64.MaxValue, even for the portion of the array that is processed using SSE instructions.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than Int64.MaxValue</exception>
        public static long SumCheckedSse(this long[] arrayToSum)
        {
            return arrayToSum.SumCheckedSseInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of long[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Throws a System.OverflowException when the sum goes beyond Int64.MaxValue, even for the portion of the array that is processed using SSE instructions.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than Int64.MaxValue</exception>
        public static long SumCheckedSse(this long[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumCheckedSseInner(startIndex, startIndex + length - 1);
        }

        private static long SumCheckedSseInner(this long[] arrayToSum, int l, int r)
        {
            var sumVector     = new Vector<long>(0);
            var newSumVector  = new Vector<long>();
            var zeroVector    = new Vector<long>(0);
            var allOnesVector = new Vector<long>(-1L);
            int i;
            int sseIndexEnd = l + ((r - l + 1) / Vector<long>.Count) * Vector<long>.Count;
            for (i = l; i < sseIndexEnd; i += Vector<long>.Count)
            {
                var inVector = new Vector<long>(arrayToSum, i);
                var inVectorGteZeroMask  = Vector.GreaterThanOrEqual(inVector,  zeroVector);  // if true then 0xFFFFFFFFFFFFFFFFL else 0L at each element of the Vector<long> 
                var sumVectorGteZeroMask = Vector.GreaterThanOrEqual(sumVector, zeroVector);  // if true then 0xFFFFFFFFFFFFFFFFL else 0L at each element of the Vector<long> 
                var inVectorLtZeroMask   = Vector.OnesComplement(inVectorGteZeroMask);
                var sumVectorLtZeroMask  = Vector.OnesComplement(sumVectorGteZeroMask);

                // Optimize performance of paths which don't overflow or underflow, assuming that's the common case
                // if (inVector >= 0 && sumVector < 0)
                var inGteZeroAndSumLtZeroMask = Vector.BitwiseAnd(inVectorGteZeroMask, sumVectorLtZeroMask);
                // if (inVector < 0 && sumVector >= 0)
                var inLtZeroAndSumGteZeroMask = Vector.BitwiseAnd(inVectorLtZeroMask, sumVectorGteZeroMask);
                var orMask = Vector.BitwiseOr(inGteZeroAndSumLtZeroMask, inLtZeroAndSumGteZeroMask);
                if (Vector.EqualsAll(orMask, allOnesVector))
                {
                    sumVector += inVector;
                    continue;
                }

                newSumVector = sumVector + inVector;

                // if (inVector >= 0 && sumVector >= 0)
                var bothGteZeroMask = Vector.BitwiseAnd(inVectorGteZeroMask, sumVectorGteZeroMask);
                // if (inVector < 0 && sumVector < 0)
                var bothLtZeroMask = Vector.BitwiseAnd(inVectorLtZeroMask, sumVectorLtZeroMask);

                var newSumLtSumMask = Vector.LessThan(   newSumVector, sumVector);
                var newSumGtSumMask = Vector.GreaterThan(newSumVector, sumVector);

                var comb10Mask = Vector.BitwiseAnd(bothGteZeroMask, newSumLtSumMask);
                var comb01Mask = Vector.BitwiseAnd(bothLtZeroMask,  newSumGtSumMask);

                if (Vector.EqualsAny(comb10Mask, allOnesVector) || Vector.EqualsAny(comb01Mask, allOnesVector))
                    throw new OverflowException();

                sumVector = newSumVector;
            }
            long overallSum = 0;
            for (; i <= r; i++)
                overallSum = checked(overallSum + arrayToSum[i]);
            for (i = 0; i < Vector<ulong>.Count; i++)
                overallSum = checked(overallSum + sumVector[i]);
            return overallSum;
        }

        /// <summary>
        /// Summation of long[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Uses a long accumulator for faster performance while detecting overflow/underflow without exceptions and returning a decimal for perfect accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalSseFaster(this long[] arrayToSum)
        {
            return arrayToSum.SumToDecimalSseFasterInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of long[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Uses a long accumulator for faster performance while detecting overflow/underflow without exceptions and returning a decimal for perfect accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalSseFaster(this long[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumToDecimalSseFasterInner(startIndex, startIndex + length - 1);
        }

        private static decimal SumToDecimalSseFasterInner(this long[] arrayToSum, int l, int r)
        {
            decimal overallSum = 0;
            var sumVector     = new Vector<long>(0);
            var newSumVector  = new Vector<long>();
            var zeroVector    = new Vector<long>(0);
            var tmpVector     = new Vector<long>();
            var allOnesVector = Vector.OnesComplement(new Vector<long>(0));
            int sseIndexEnd = l + ((r - l + 1) / Vector<long>.Count) * Vector<long>.Count;
            int i;

            for (i = l; i < sseIndexEnd; i += Vector<long>.Count)
            {
                var inVector = new Vector<long>(arrayToSum, i);
                var sumVectorPositiveMask = Vector.GreaterThanOrEqual(sumVector, zeroVector);  // if true then 0xFFFFFFFFFFFFFFFFL else 0L at each element of the Vector<long> 
                var inVectorPositiveMask  = Vector.GreaterThanOrEqual(inVector,  zeroVector);  // if true then 0xFFFFFFFFFFFFFFFFL else 0L at each element of the Vector<long> 
                var sumVectorNegativeMask = Vector.OnesComplement(sumVectorPositiveMask);
                var inVectorNegativeMask  = Vector.OnesComplement(inVectorPositiveMask);

                // Optimize performance of paths which don't overflow or underflow, assuming that's the common case
                // if (sumVector >= 0 && inVector < 0)
                var sumPositiveAndInNegativeMask = Vector.BitwiseAnd(sumVectorPositiveMask, inVectorNegativeMask);
                // if (sumVector < 0 && inVector >= 0)
                var sumNegativeAndInPositiveMask = Vector.BitwiseAnd(sumVectorNegativeMask, inVectorPositiveMask);
                var oppositeSignsMask = Vector.BitwiseOr(sumNegativeAndInPositiveMask, sumPositiveAndInNegativeMask);

                sumVector = Vector.ConditionalSelect(oppositeSignsMask, sumVector + inVector, sumVector);

                newSumVector = sumVector + inVector;

                // if (inVector >= 0 && sumVector >= 0)
                var bothPositiveMask = Vector.BitwiseAnd(inVectorPositiveMask, sumVectorPositiveMask);
                // if (inVector < 0 && sumVector < 0)
                var bothNegativeMask = Vector.BitwiseAnd(inVectorNegativeMask, sumVectorNegativeMask);

                var newSumLtSumMask = Vector.LessThan(   newSumVector, sumVector);
                var newSumGtSumMask = Vector.GreaterThan(newSumVector, sumVector);

                var comb10Mask = Vector.BitwiseAnd(bothPositiveMask, newSumLtSumMask);
                var comb01Mask = Vector.BitwiseAnd(bothNegativeMask, newSumGtSumMask);

                if (Vector.EqualsAny(comb10Mask, allOnesVector))    // overflow occured in one of the vector elements
                {
                    for (int j = 0; j < Vector<ulong>.Count; j++)
                    {
                        if (comb10Mask[j] == -1L)    // this particular sum overflowed
                        {
                            overallSum += sumVector[j];
                            overallSum += inVector[j];
                        }
                    }
                    tmpVector = Vector.ConditionalSelect(newSumLtSumMask, zeroVector, newSumVector);
                }
                else
                    tmpVector = newSumVector;
                sumVector = Vector.ConditionalSelect(bothPositiveMask, tmpVector, sumVector);

                if (Vector.EqualsAny(comb01Mask, allOnesVector))    // underflow occured in one of the vector elements
                {
                    for (int j = 0; j < Vector<ulong>.Count; j++)
                    {
                        if (comb01Mask[j] == -1L)    // this particular sum underflowed
                        {
                            overallSum += sumVector[j];
                            overallSum += inVector[j];
                        }
                    }
                    tmpVector = Vector.ConditionalSelect(newSumGtSumMask, zeroVector, newSumVector);
                }
                else
                    tmpVector = newSumVector;
                sumVector = Vector.ConditionalSelect(bothNegativeMask, tmpVector, sumVector);
            }

            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of long[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Uses a long accumulator for faster performance while detecting overflow/underflow without exceptions and returning a decimal for perfect accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerSseFaster(this long[] arrayToSum)
        {
            return arrayToSum.SumToBigIntegerSseFasterInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of long[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Uses a long accumulator for faster performance while detecting overflow/underflow without exceptions and returning a decimal for perfect accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerSseFaster(this long[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumToBigIntegerSseFasterInner(startIndex, startIndex + length - 1);
        }

        private static BigInteger SumToBigIntegerSseFasterInner(this long[] arrayToSum, int l, int r)
        {
            BigInteger overallSum = 0;
            var sumVector    = new Vector<long>(0);
            var newSumVector = new Vector<long>();
            var zeroVector   = new Vector<long>(0);
            var tmpVector    = new Vector<long>();
            var allOnesVector = Vector.OnesComplement(new Vector<long>(0));
            int sseIndexEnd = l + ((r - l + 1) / Vector<long>.Count) * Vector<long>.Count;
            int i;

            for (i = l; i < sseIndexEnd; i += Vector<long>.Count)
            {
                var inVector = new Vector<long>(arrayToSum, i);
                var sumVectorPositiveMask = Vector.GreaterThanOrEqual(sumVector, zeroVector);   // if true then 0xFFFFFFFFFFFFFFFFL else 0L at each element of the Vector<long> 
                var inVectorPositiveMask  = Vector.GreaterThanOrEqual(inVector,  zeroVector);   // if true then 0xFFFFFFFFFFFFFFFFL else 0L at each element of the Vector<long> 
                var sumVectorNegativeMask = Vector.OnesComplement(sumVectorPositiveMask);
                var inVectorNegativeMask  = Vector.OnesComplement(inVectorPositiveMask);

                // Optimize performance of paths which don't overflow or underflow, assuming that's the common case
                // if (sumVector >= 0 && inVector < 0)
                var sumPositiveAndInNegativeMask = Vector.BitwiseAnd(sumVectorPositiveMask, inVectorNegativeMask);
                // if (sumVector < 0 && inVector >= 0)
                var sumNegativeAndInPositiveMask = Vector.BitwiseAnd(sumVectorNegativeMask, inVectorPositiveMask);
                var oppositeSignsMask = Vector.BitwiseOr(sumNegativeAndInPositiveMask, sumPositiveAndInNegativeMask);

                sumVector = Vector.ConditionalSelect(oppositeSignsMask, sumVector + inVector, sumVector);

                newSumVector = sumVector + inVector;

                // if (inVector >= 0 && sumVector >= 0)
                var bothPositiveMask = Vector.BitwiseAnd(inVectorPositiveMask, sumVectorPositiveMask);
                // if (inVector < 0 && sumVector < 0)
                var bothNegativeMask = Vector.BitwiseAnd(inVectorNegativeMask, sumVectorNegativeMask);

                var newSumLtSumMask = Vector.LessThan(   newSumVector, sumVector);
                var newSumGtSumMask = Vector.GreaterThan(newSumVector, sumVector);

                var comb10Mask = Vector.BitwiseAnd(bothPositiveMask, newSumLtSumMask);
                var comb01Mask = Vector.BitwiseAnd(bothNegativeMask, newSumGtSumMask);

                if (Vector.EqualsAny(comb10Mask, allOnesVector))    // overflow occured in one of the vector elements
                {
                    for (int j = 0; j < Vector<long>.Count; j++)
                    {
                        if (comb10Mask[j] == -1L)    // this particular sum overflowed
                        {
                            overallSum = overallSum + sumVector[j];
                            overallSum = overallSum + inVector[j];
                        }
                    }
                    tmpVector = Vector.ConditionalSelect(newSumLtSumMask, zeroVector, newSumVector);
                }
                else
                    tmpVector = newSumVector;
                sumVector = Vector.ConditionalSelect(bothPositiveMask, tmpVector, sumVector);

                if (Vector.EqualsAny(comb01Mask, allOnesVector))    // underflow occured in one of the vector elements
                {
                    for (int j = 0; j < Vector<long>.Count; j++)
                    {
                        if (comb01Mask[j] == -1L)    // this particular sum underflowed
                        {
                            overallSum = overallSum + sumVector[j];
                            overallSum = overallSum + inVector[j];
                        }
                    }
                    tmpVector = Vector.ConditionalSelect(newSumGtSumMask, zeroVector, newSumVector);
                }
                else
                    tmpVector = newSumVector;
                sumVector = Vector.ConditionalSelect(bothNegativeMask, tmpVector, sumVector);
            }

            for (; i <= r; i++)
                overallSum = overallSum + arrayToSum[i];
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum = overallSum + sumVector[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of ulong[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Caution: Will not throw an overflow exception for the majority of the array, but instead will wrap around to smaller values, when the sum goes beyond UInt64.MaxValue
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than UInt64.MaxValue</exception>
        public static ulong SumSse(this ulong[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of ulong[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Caution: Will not throw an overflow exception for the majority of the array, but instead will wrap around to smaller values, when the sum goes beyond UInt64.MaxValue
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than UInt64.MaxValue</exception>
        public static ulong SumSse(this ulong[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static ulong SumSseInner(this ulong[] arrayToSum, int l, int r)
        {
            var sumVector = new Vector<ulong>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<ulong>.Count) * Vector<ulong>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<long>.Count)
            {
                var inVector = new Vector<ulong>(arrayToSum, i);
                sumVector += inVector;
            }
            ulong overallSum = 0;
            for (; i <= r; i++)
            {
                checked
                {
                    overallSum += arrayToSum[i];
                }
            }
            for (i = 0; i < Vector<long>.Count; i++)
            {
                checked
                {
                    overallSum += sumVector[i];
                }
            }
            return overallSum;
        }

        /// <summary>
        /// Summation of ulong[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Throws a System.OverflowException when the sum goes beyond UInt64.MaxValue, even for the portion of the array that is processed using SSE instructions.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than UInt64.MaxValue</exception>
        public static ulong SumCheckedSse(this ulong[] arrayToSum)
        {
            return arrayToSum.SumCheckedSseInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of ulong[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Throws a System.OverflowException when the sum goes beyond UInt64.MaxValue, even for the portion of the array that is processed using SSE instructions.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than UInt64.MaxValue</exception>
        public static ulong SumCheckedSse(this ulong[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumCheckedSseInner(startIndex, startIndex + length - 1);
        }

        private static ulong SumCheckedSseInner(this ulong[] arrayToSum, int l, int r)
        {
            var sumVector     = new Vector<ulong>();
            var newSumVector  = new Vector<ulong>();
            var allOnesVector = new Vector<ulong>(0xFFFFFFFFFFFFFFFFL);
            int sseIndexEnd = l + ((r - l + 1) / Vector<ulong>.Count) * Vector<ulong>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<long>.Count)
            {
                var inVector = new Vector<ulong>(arrayToSum, i);
                newSumVector = sumVector + inVector;
                Vector<ulong> gteMask = Vector.GreaterThanOrEqual(newSumVector, sumVector);         // if true then 0xFFFFFFFFFFFFFFFFL else 0L at each element of the Vector<long> 
                if (Vector.EqualsAll(gteMask, allOnesVector))
                    sumVector = newSumVector;
                else
                    throw new System.OverflowException();
            }
            ulong overallSum = 0;
            for (; i <= r; i++)
            {
                overallSum = checked(overallSum + arrayToSum[i]);
            }
            for (i = 0; i < Vector<ulong>.Count; i++)
            {
                overallSum = checked(overallSum + sumVector[i]);
            }
            return overallSum;
        }

        /// <summary>
        /// Summation of ulong[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Uses a ulong accumulator for faster performance while detecting overflow without exceptions and returning a decimal for perfect accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalSseFaster(this ulong[] arrayToSum)
        {
            return arrayToSum.SumToDecimalSseFasterInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of ulong[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Uses a ulong accumulator for faster performance while detecting overflow without exceptions and returning a decimal for perfect accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalSseFaster(this ulong[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumToDecimalSseFasterInner(startIndex, startIndex + length - 1);
        }

        private static decimal SumToDecimalSseFasterInner(this ulong[] arrayToSum, int l, int r)
        {
            decimal overallSum = 0;
            var sumVector    = new Vector<ulong>();
            var newSumVector = new Vector<ulong>();
            var zeroVector   = new Vector<ulong>(0);
            int sseIndexEnd = l + ((r - l + 1) / Vector<ulong>.Count) * Vector<ulong>.Count;
            int i;

            for (i = l; i < sseIndexEnd; i += Vector<ulong>.Count)
            {
                var inVector = new Vector<ulong>(arrayToSum, i);
                newSumVector = sumVector + inVector;
                Vector<ulong> gteMask = Vector.GreaterThanOrEqual(newSumVector, sumVector);         // if true then 0xFFFFFFFFFFFFFFFFL else 0L at each element of the Vector<long> 
                if (Vector.EqualsAny(gteMask, zeroVector))
                {
                    for(int j = 0; j < Vector<ulong>.Count; j++)
                    {
                        if (gteMask[j] == 0)    // this particular sum overflowed, since sum decreased
                        {
                            overallSum += sumVector[j];
                            overallSum += inVector[ j];
                        }
                    }
                }
                sumVector = Vector.ConditionalSelect(gteMask, newSumVector, zeroVector);
            }
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<ulong>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of ulong[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Uses a 128-bit accumulator for faster performance while detecting overflow without exceptions and returning a decimal for perfect accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalSseEvenFaster(this ulong[] arrayToSum)
        {
            return arrayToSum.SumToDecimalSseEvenFasterInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of ulong[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Uses a 128-bit accumulator for faster performance while detecting overflow without exceptions and returning a decimal for perfect accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalSseEvenFaster(this ulong[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumToDecimalSseEvenFasterInner(startIndex, startIndex + length - 1);
        }

        private static decimal SumToDecimalSseEvenFasterInner(this ulong[] arrayToSum, int l, int r)
        {
            decimal overallSum = 0;
            var sumVector      = new Vector<ulong>();
            var upperSumVector = new Vector<ulong>();
            var newSumVector   = new Vector<ulong>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<ulong>.Count) * Vector<ulong>.Count;
            int i;

            for (i = l; i < sseIndexEnd; i += Vector<ulong>.Count)
            {
                var inVector = new Vector<ulong>(arrayToSum, i);
                newSumVector = sumVector + inVector;
                Vector<ulong> ltMask = Vector.LessThan(newSumVector, sumVector);         // if true then 0xFFFFFFFFFFFFFFFFL else 0L at each element of the Vector<long> 
                upperSumVector -= ltMask;
                sumVector = newSumVector;
            }
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<ulong>.Count; i++)
            {
                overallSum += sumVector[i];

                decimal multiplier = 0x8000_0000_0000_0000;
                overallSum += multiplier * (Decimal)2 * (Decimal)upperSumVector[i];   // uintUpperSum << 64
            }

            return overallSum;
        }

        private static BigInteger SumToBigIntegerSseEvenFasterInner(this ulong[] arrayToSum, int l, int r)
        {
            return (BigInteger)SumToDecimalSseEvenFasterInner(arrayToSum, l, r);
        }

        /// <summary>
        /// Summation of ulong[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Uses a 128-bit accumulator for faster performance while detecting overflow without exceptions and returning a BigInteger for perfect accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerSseEvenFaster(this ulong[] arrayToSum)
        {
            return (BigInteger)arrayToSum.SumToDecimalSseEvenFasterInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of ulong[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Uses a 128-bit accumulator for faster performance while detecting overflow without exceptions and returning a BigInteger for perfect accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerSseEvenFaster(this ulong[] arrayToSum, int startIndex, int length)
        {
            return (BigInteger)arrayToSum.SumToDecimalSseEvenFasterInner(startIndex, startIndex + length - 1);
        }

        /// <summary>
        /// Summation of ulong[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Uses a ulong accumulator for faster performance while detecting overflow without exceptions and returning a BigInteger for perfect accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerSseFaster(this ulong[] arrayToSum)
        {
            return arrayToSum.SumToBigIntegerSseFasterInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of ulong[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Uses a ulong accumulator for faster performance while detecting overflow without exceptions and returning a BigInteger for perfect accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerSseFaster(this ulong[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumToBigIntegerSseFasterInner(startIndex, startIndex + length - 1);
        }

        private static BigInteger SumToBigIntegerSseFasterInner(this ulong[] arrayToSum, int l, int r)
        {
            BigInteger overallSum = 0;
            var sumVector    = new Vector<ulong>();
            var newSumVector = new Vector<ulong>();
            var zeroVector   = new Vector<ulong>(0);
            int sseIndexEnd = l + ((r - l + 1) / Vector<ulong>.Count) * Vector<ulong>.Count;
            int i;

            for (i = l; i < sseIndexEnd; i += Vector<ulong>.Count)
            {
                var inVector = new Vector<ulong>(arrayToSum, i);
                newSumVector = sumVector + inVector;
                Vector<ulong> gteMask = Vector.GreaterThanOrEqual(newSumVector, sumVector);         // if true then 0xFFFFFFFFFFFFFFFFL else 0L at each element of the Vector<long> 
                if (Vector.EqualsAny(gteMask, zeroVector))
                {
                    for (int j = 0; j < Vector<ulong>.Count; j++)
                    {
                        if (gteMask[j] == 0)    // this particular sum overflowed, since sum decreased
                        {
                            overallSum += sumVector[j];
                            overallSum += inVector[j];
                        }
                    }
                }
                sumVector = Vector.ConditionalSelect(gteMask, newSumVector, zeroVector);
            }
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<ulong>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of float[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>float sum</returns>
        public static float SumSse(this float[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of float[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>float sum</returns>
        public static float SumSse(this float[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static float SumSseInner(this float[] arrayToSum, int l, int r)
        {
            var sumVector = new Vector<float>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<float>.Count) * Vector<float>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<float>.Count)
            {
                var inVector = new Vector<float>(arrayToSum, i);
                sumVector += inVector;
            }
            float overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<float>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of float[] array, using a double accumulator for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleSse(this float[] arrayToSum)
        {
            return arrayToSum.SumSseDoubleInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of float[] array, using a double accumulator for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleSse(this float[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseDoubleInner(startIndex, startIndex + length - 1);
        }

        private static double SumSseDoubleInner(this float[] arrayToSum, int l, int r)
        {
            var sumVectorLower = new Vector<double>();
            var sumVectorUpper = new Vector<double>();
            var longLower      = new Vector<double>();
            var longUpper      = new Vector<double>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<float>.Count) * Vector<float>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<float>.Count)
            {
                var inVector = new Vector<float>(arrayToSum, i);
                Vector.Widen(inVector, out longLower, out longUpper);
                sumVectorLower += longLower;
                sumVectorUpper += longUpper;
            }
            double overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<double>.Count; i++)
                overallSum += sumVectorLower[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of double[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumSse(this double[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of double[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumSse(this double[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static double SumSseInner(this double[] arrayToSum, int l, int r)
        {
            var sumVector = new Vector<double>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<double>.Count) * Vector<double>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<double>.Count)
            {
                var inVector = new Vector<double>(arrayToSum, i);
                sumVector += inVector;
            }
            double overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<double>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of float[] array, using Neumaier variation of Kahan summation for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>float sum</returns>
        public static float SumSseMostAccurate(this float[] arrayToSum)
        {
            return arrayToSum.SumSseNeumaierInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of float[] array, using Neumaier variation of Kahan summation for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>float sum</returns>
        public static float SumSseMostAccurate(this float[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseNeumaierInner(startIndex, startIndex + length - 1);
        }

        private static float SumSseNeumaierInner(this float[] arrayToSum, int l, int r)
        {
            var sumVector = new Vector<float>();
            var cVector   = new Vector<float>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<float>.Count) * Vector<float>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<float>.Count)
            {
                var inVector = new Vector<float>(arrayToSum, i);
                var tVector = sumVector + inVector;
                Vector<int> gteMask = Vector.GreaterThanOrEqual(Vector.Abs(sumVector), Vector.Abs(inVector));                                           // if true then 0xFFFFFFFFL else 0L at each element of the Vector<int> 
                cVector += Vector.ConditionalSelect(gteMask, sumVector, inVector) - tVector + Vector.ConditionalSelect(gteMask, inVector, sumVector);   // ConditionalSelect selects left for 0xFFFFFFFFL and right for 0x0L
                sumVector = tVector;
            }
            int iLast = i;
            // At this point we have sumVector and cVector, which have Vector<float>.Count number of sum's and c's
            // Reduce these Vector's to a single sum and a single c
            float sum = 0.0f;
            float c   = 0.0f;
            for (i = 0; i < Vector<float>.Count; i++)
            {
                float t = sum + sumVector[i];
                if (Math.Abs(sum) >= Math.Abs(sumVector[i]))
                    c += (sum - t) + sumVector[i];         // If sum is bigger, low-order digits of input[i] are lost.
                else
                    c += (sumVector[i] - t) + sum;         // Else low-order digits of sum are lost
                sum = t;
                c += cVector[i];
            }
            for (i = iLast; i <= r; i++)
            {
                float t = sum + arrayToSum[i];
                if (Math.Abs(sum) >= Math.Abs(arrayToSum[i]))
                    c += (sum - t) + arrayToSum[i];         // If sum is bigger, low-order digits of input[i] are lost.
                else
                    c += (arrayToSum[i] - t) + sum;         // Else low-order digits of sum are lost
                sum = t;
            }
            return sum + c;
        }

        /// <summary>
        /// Summation of float[] array, using Neumaier variation of Kahan summation along with double precision for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
       /// <returns>double sum</returns>
        public static double SumToDoubleSseMostAccurate(this float[] arrayToSum)
        {
            return arrayToSum.SumSseNeumaierDoubleInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of float[] array, using Neumaier variation of Kahan summation along with double precision for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleSseMostAccurate(this float[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseNeumaierDoubleInner(startIndex, startIndex + length - 1);
        }

        private static double SumSseNeumaierDoubleInner(this float[] arrayToSum, int l, int r)
        {
            var sumVector = new Vector<double>();
            var cVector   = new Vector<double>();
            var longLower = new Vector<double>();
            var longUpper = new Vector<double>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<float>.Count) * Vector<float>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<float>.Count)
            {
                var inVector = new Vector<float>(arrayToSum, i);
                Vector.Widen(inVector, out longLower, out longUpper);

                var tVector = sumVector + longLower;
                Vector<long> gteMask = Vector.GreaterThanOrEqual(Vector.Abs(sumVector), Vector.Abs(longLower));         // if true then 0xFFFFFFFFFFFFFFFFL else 0L at each element of the Vector<long> 
                cVector += Vector.ConditionalSelect(gteMask, sumVector, longLower) - tVector + Vector.ConditionalSelect(gteMask, longLower, sumVector);
                sumVector = tVector;

                tVector = sumVector + longUpper;
                gteMask = Vector.GreaterThanOrEqual(Vector.Abs(sumVector), Vector.Abs(longUpper));                      // if true then 0xFFFFFFFFFFFFFFFFL else 0L at each element of the Vector<long> 
                cVector += Vector.ConditionalSelect(gteMask, sumVector, longUpper) - tVector + Vector.ConditionalSelect(gteMask, longUpper, sumVector);
                sumVector = tVector;
            }
            int iLast = i;
            // At this point we have sumVector and cVector, which have Vector<double>.Count number of sum's and c's
            // Reduce these Vector's to a single sum and a single c
            double sum = 0.0;
            double c   = 0.0;
            for (i = 0; i < Vector<double>.Count; i++)
            {
                double t = sum + sumVector[i];
                if (Math.Abs(sum) >= Math.Abs(sumVector[i]))
                    c += (sum - t) + sumVector[i];         // If sum is bigger, low-order digits of input[i] are lost.
                else
                    c += (sumVector[i] - t) + sum;         // Else low-order digits of sum are lost
                sum = t;
                c += cVector[i];
            }
            for (i = iLast; i <= r; i++)
            {
                double t = sum + arrayToSum[i];
                if (Math.Abs(sum) >= Math.Abs(arrayToSum[i]))
                    c += (sum - t) + arrayToSum[i];         // If sum is bigger, low-order digits of input[i] are lost.
                else
                    c += (arrayToSum[i] - t) + sum;         // Else low-order digits of sum are lost
                sum = t;
            }
            return sum + c;
        }

        /// <summary>
        /// Summation of double[] array, using Neumaier variation of Kahan summation for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumSseMostAccurate(this double[] arrayToSum)
        {
            return arrayToSum.SumSseNeumaierInner(0, arrayToSum.Length - 1);
        }

        /// <summary>
        /// Summation of double[] array, using Neumaier variation of Kahan summation for higher accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumSseMostAccurate(this double[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseNeumaierInner(startIndex, startIndex + length - 1);
        }

        private static double SumSseNeumaierInner(this double[] arrayToSum, int l, int r)
        {
            var sumVector = new Vector<double>();
            var cVector = new Vector<double>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<double>.Count) * Vector<double>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<double>.Count)
            {
                var inVector = new Vector<double>(arrayToSum, i);
                var tVector = sumVector + inVector;
                Vector<long> gteMask = Vector.GreaterThanOrEqual(Vector.Abs(sumVector), Vector.Abs(inVector));  // if true then 0xFFFFFFFFFFFFFFFFL else 0L at each element of the Vector<long> 
                cVector += Vector.ConditionalSelect(gteMask, sumVector, inVector) - tVector + Vector.ConditionalSelect(gteMask, inVector, sumVector);
                sumVector = tVector;
            }
            int iLast = i;
            // At this point we have sumVector and cVector, which have Vector<double>.Count number of sum's and c's
            // Reduce these Vector's to a single sum and a single c
            double sum = 0.0;
            double c = 0.0;
            for (i = 0; i < Vector<double>.Count; i++)
            {
                double t = sum + sumVector[i];
                if (Math.Abs(sum) >= Math.Abs(sumVector[i]))
                    c += (sum - t) + sumVector[i];         // If sum is bigger, low-order digits of input[i] are lost.
                else
                    c += (sumVector[i] - t) + sum;         // Else low-order digits of sum are lost
                sum = t;
                c += cVector[i];
            }
            for (i = iLast; i <= r; i++)
            {
                double t = sum + arrayToSum[i];
                if (Math.Abs(sum) >= Math.Abs(arrayToSum[i]))
                    c += (sum - t) + arrayToSum[i];         // If sum is bigger, low-order digits of input[i] are lost.
                else
                    c += (arrayToSum[i] - t) + sum;         // Else low-order digits of sum are lost
                sum = t;
            }
            return sum + c;
        }

        private static ulong NumberOfBytesToNextCacheLine(float[] arrayToAlign)
        {
            ulong numBytesUnaligned = 0;
            unsafe
            {
                fixed (float* ptrToArray = &arrayToAlign[0])
                {
                    byte* ptrByteToArray = (byte*)ptrToArray;
                    numBytesUnaligned = ((ulong)ptrToArray) & 63;
                }
            }
            return numBytesUnaligned;
        }

        /// <summary>
        /// Summation of sbyte[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a long accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLongSseParDac(this sbyte[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, SumToLongSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of sbyte[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a long accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLongSseParDac(this sbyte[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, SumToLongSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of byte[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a ulong accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongSsePar(this byte[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, SumToUlongSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        public static ulong SumToUlongSseParInvoke(this byte[] arrayToSum, int degreeOfParallelism = 0)
        {
            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };
// TODO: Need to deal with small arrays and small quantas, possibly 0
            int quanta = arrayToSum.Length / maxDegreeOfPar;

            var concurrentSums = new ConcurrentBag<ulong>();
            //var startIndexList = new List<int>();
            //var lengthList = new List<int>();

            //ulong sumGolden = 0;
            //ulong sumLocal = 0;
            var listOfActions = new List<Action>();
            int startIndex = 0;
            int i = 0;
            for (i = 0; i < (maxDegreeOfPar - 1); i++, startIndex += quanta)
            {
                //sumGolden = 0;
                //for (int j = 0; j < quanta; j++)
                //    sumGolden += arrayToSum[startIndex + j];
                //sumLocal = SumToUlongSse(arrayToSum, startIndex, quanta);
                //Console.WriteLine("quanta = {0}   startIndex = {1}   sumGolden = {2}  sumLocal = {3}", quanta, startIndex, sumGolden, sumLocal);
                //Int32 startIndexLoc1 = new Int32();
                int startIndexLoc1 = startIndex;
                //startIndexList.Add(startIndexLoc1);
                //Int32 quantaLoc1 = new Int32();
                int quantaLoc1 = quanta;
                //lengthList.Add(quantaLoc1);
                listOfActions.Add(() => {
                    ulong sumOfAction = SumToUlongSse(arrayToSum, startIndexLoc1, quantaLoc1);
                    //Console.WriteLine("Action: sum = {0}   startIndex = {1}   length = {2}", sumOfAction, startIndex, quanta);
                    concurrentSums.Add(sumOfAction);
                });
            }
            //sumGolden = 0;
            //for (int j = 0; j < quanta; j++)
            //    sumGolden += arrayToSum[startIndex + j];
            //sumLocal = Algorithms.Sum.SumToUlong(arrayToSum, startIndex, arrayToSum.Length - startIndex);
            //Console.WriteLine("quanta = {0}   startIndex = {1}  length = {2}   sumGolden = {3}  sumLocal = {4}", quanta, startIndex, arrayToSum.Length - startIndex, sumGolden, sumLocal);
            //Console.WriteLine("quanta = {0}   startIndex = {1}  length = {2}", quanta, startIndex, arrayToSum.Length - startIndex);
            //Int32 startIndexLoc = new Int32();
            int startIndexLoc = startIndex;
            //startIndexList.Add(startIndexLoc);
            //Int32 quantaLoc = new Int32();
            int quantaLoc = arrayToSum.Length - startIndex;
            //lengthList.Add(quantaLoc);
            listOfActions.Add(() => {
                ulong sumOfLastAction = SumToUlongSse(arrayToSum, startIndexLoc, quantaLoc);
                //Console.WriteLine("Last Action: sum = {0}   startIndex = {1}   length = {2}", sumOfLastAction, startIndex, arrayToSum.Length - startIndex);
                concurrentSums.Add(sumOfLastAction);
            });       // the rest

            Parallel.Invoke(options, listOfActions.ToArray());

            ulong sum = 0;
            var sumsArray = concurrentSums.ToArray();
            for (int k = 0; k < sumsArray.Length; k++)
            {
                //Console.WriteLine("concurrentBag[{0}] = {1}", k, sumsArray[k]);
                sum += sumsArray[k];
            }

            return sum;
        }
        /// <summary>
        /// Summation of byte[] array, using multiple cores.
        /// Uses a ulong accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongPar(this byte[] arrayToSum, int degreeOfParallelism = 0)
        {
            return arrayToSum.SumToUlongPar(0, arrayToSum.Length, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of byte[] array, using multiple cores.
        /// Uses a ulong accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongPar(this byte[] arrayToSum, int startIndex, int length, int degreeOfParallelism = 0)
        {
            var concurrentSums = new ConcurrentBag<ulong>();

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(startIndex, startIndex + length), options, range =>
            {
                //Console.WriteLine("Partition: start = {0}   end = {1}", range.Item1, range.Item2);
                ulong localSum = 0;
                for (int i = range.Item1; i < range.Item2; i++)
                    localSum += arrayToSum[i];
                concurrentSums.Add(localSum);
            });

            ulong sum = 0;
            var sumsArray = concurrentSums.ToArray();
            for (int i = 0; i < sumsArray.Length; i++)
                sum += sumsArray[i];

            return sum;
        }
        /// <summary>
        /// Summation of ushort[] array, using multiple cores.
        /// Uses a ulong accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongPar(this ushort[] arrayToSum, int degreeOfParallelism = 0)
        {
            return arrayToSum.SumToUlongPar(0, arrayToSum.Length, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of byte[] array, using multiple cores.
        /// Uses a ulong accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongPar(this ushort[] arrayToSum, int startIndex, int length, int degreeOfParallelism = 0)
        {
            var concurrentSums = new ConcurrentBag<ulong>();

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(startIndex, startIndex + length), options, range =>
            {
                ulong localSum = 0;
                for (int i = range.Item1; i < range.Item2; i++)
                    localSum += arrayToSum[i];
                concurrentSums.Add(localSum);
            });

            ulong sum = 0;
            var sumsArray = concurrentSums.ToArray();
            for (int i = 0; i < sumsArray.Length; i++)
                sum += sumsArray[i];

            return sum;
        }
        /// <summary>
        /// Summation of uint[] array, using multiple cores.
        /// Uses a ulong accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongPar(this uint[] arrayToSum, int degreeOfParallelism = 0)
        {
            return arrayToSum.SumToUlongPar(0, arrayToSum.Length, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of uint[] array, using multiple cores.
        /// Uses a ulong accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongPar(this uint[] arrayToSum, int startIndex, int length, int degreeOfParallelism = 0)
        {
            var concurrentSums = new ConcurrentBag<ulong>();

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(startIndex, startIndex + length), options, range =>
            {
                ulong localSum = 0;
                for (int i = range.Item1; i < range.Item2; i++)
                    localSum += arrayToSum[i];
                concurrentSums.Add(localSum);
            });

            ulong sum = 0;
            var sumsArray = concurrentSums.ToArray();
            for (int i = 0; i < sumsArray.Length; i++)
                sum += sumsArray[i];

            return sum;
        }
        /// <summary>
        /// Summation of sbyte[] array, using multiple corese.
        /// Uses a long accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static long SumToLongPar(this sbyte[] arrayToSum, int degreeOfParallelism = 0)
        {
            return arrayToSum.SumToLongPar(0, arrayToSum.Length, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of sbyte[] array, using multiple cores.
        /// Uses a long accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static long SumToLongPar(this sbyte[] arrayToSum, int startIndex, int length, int degreeOfParallelism = 0)
        {
            var concurrentSums = new ConcurrentBag<long>();

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(startIndex, startIndex + length), options, range =>
            {
                long localSum = 0;
                for (int i = range.Item1; i < range.Item2; i++)
                    localSum += arrayToSum[i];
                concurrentSums.Add(localSum);
            });

            long sum = 0;
            var sumsArray = concurrentSums.ToArray();
            for (int i = 0; i < sumsArray.Length; i++)
                sum += sumsArray[i];

            return sum;
        }
        /// <summary>
        /// Summation of short[] array, using multiple cores.
        /// Uses a long accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static long SumToLongPar(this short[] arrayToSum, int degreeOfParallelism = 0)
        {
            return arrayToSum.SumToLongPar(0, arrayToSum.Length, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of short[] array, using multiple cores.
        /// Uses a long accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static long SumToLongPar(this short[] arrayToSum, int startIndex, int length, int degreeOfParallelism = 0)
        {
            var concurrentSums = new ConcurrentBag<long>();

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(startIndex, startIndex + length), options, range =>
            {
                long localSum = 0;
                for (int i = range.Item1; i < range.Item2; i++)
                    localSum += arrayToSum[i];
                concurrentSums.Add(localSum);
            });

            long sum = 0;
            var sumsArray = concurrentSums.ToArray();
            for (int i = 0; i < sumsArray.Length; i++)
                sum += sumsArray[i];

            return sum;
        }
        /// <summary>
        /// Summation of int[] array, using multiple cores.
        /// Uses a long accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static long SumToLongPar(this int[] arrayToSum, int degreeOfParallelism = 0)
        {
            return arrayToSum.SumToLongPar(0, arrayToSum.Length, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of short[] array, using multiple cores.
        /// Uses a long accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static long SumToLongPar(this int[] arrayToSum, int startIndex, int length, int degreeOfParallelism = 0)
        {
            var concurrentSums = new ConcurrentBag<long>();

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(startIndex, startIndex + length), options, range =>
            {
                long localSum = 0;
                for (int i = range.Item1; i < range.Item2; i++)
                    localSum += arrayToSum[i];
                concurrentSums.Add(localSum);
            });

            long sum = 0;
            var sumsArray = concurrentSums.ToArray();
            for (int i = 0; i < sumsArray.Length; i++)
                sum += sumsArray[i];

            return sum;
        }
        // 7.8 GigaAdds/sec on 6-core dual-memory-channel CPU, using this scalar algorithm = 31 GigaBytes/sec of memory bandwidth for large arrays
        // from https://stackoverflow.com/questions/2419343/how-to-sum-up-an-array-of-integers-in-c-sharp?noredirect=1&lq=1
        private static long SumToLongParForInterlocked(this int[] arrayToSum, int degreeOfParallelism = 0)
        {
            long sum = 0;

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(0, arrayToSum.Length), options, range =>
            {
                long localSum = 0;
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    localSum += arrayToSum[i];
                }
                Interlocked.Add(ref sum, localSum);
            });
            return sum;
        }
        /// <summary>
        /// Summation of sbyte[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a long accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static long SumToLongSsePar(this sbyte[] arrayToSum, int degreeOfParallelism = 0)
        {
            return arrayToSum.SumToLongSsePar(0, arrayToSum.Length, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of sbyte[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a long accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static long SumToLongSsePar(this sbyte[] arrayToSum, int startIndex, int length, int degreeOfParallelism = 0)
        {
            var concurrentSums = new ConcurrentBag<long>();

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(startIndex, startIndex + length), options, range =>
            {
                long localSum = SumSseInner(arrayToSum, range.Item1, range.Item2 - 1);
                concurrentSums.Add(localSum);
            });

            long sum = 0;
            var sumsArray = concurrentSums.ToArray();
            for (int i = 0; i < sumsArray.Length; i++)
                sum += sumsArray[i];

            return sum;
        }
        /// <summary>
        /// Summation of short[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a long accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static long SumToLongSsePar(this short[] arrayToSum, int degreeOfParallelism = 0)
        {
            return arrayToSum.SumToLongSsePar(0, arrayToSum.Length, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of short[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a long accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static long SumToLongSsePar(this short[] arrayToSum, int startIndex, int length, int degreeOfParallelism = 0)
        {
            var concurrentSums = new ConcurrentBag<long>();

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(startIndex, startIndex + length), options, range =>
            {
                long localSum = SumSseInner(arrayToSum, range.Item1, range.Item2 - 1);
                concurrentSums.Add(localSum);
            });

            long sum = 0;
            var sumsArray = concurrentSums.ToArray();
            for (int i = 0; i < sumsArray.Length; i++)
                sum += sumsArray[i];

            return sum;
        }
        /// <summary>
        /// Summation of int[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a long accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static long SumToLongSsePar(this int[] arrayToSum, int degreeOfParallelism = 0)
        {
            return arrayToSum.SumToLongSsePar(0, arrayToSum.Length, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of int[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a long accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static long SumToLongSsePar(this int[] arrayToSum, int startIndex, int length, int degreeOfParallelism = 0)
        {
            var concurrentSums = new ConcurrentBag<long>();

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(startIndex, startIndex + length), options, range =>
            {
                long localSum = SumSseInner(arrayToSum, range.Item1, range.Item2 - 1);
                concurrentSums.Add(localSum);
            });

            long sum = 0;
            var sumsArray = concurrentSums.ToArray();
            for (int i = 0; i < sumsArray.Length; i++)
                sum += sumsArray[i];

            return sum;
        }
        /// <summary>
        /// Summation of long[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a long accumulator for perfect accuracy.
        /// Warning: will not throw an arithmetic overflow exception, wrapping around to the opposite sign quietly.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static long SumSsePar(this long[] arrayToSum, int degreeOfParallelism = 0)
        {
            return arrayToSum.SumSsePar(0, arrayToSum.Length, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of long[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a long accumulator for perfect accuracy.
        /// Warning: will not throw an arithmetic overflow exception, wrapping around to the opposite sign quietly.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static long SumSsePar(this long[] arrayToSum, int startIndex, int length, int degreeOfParallelism = 0)
        {
            var concurrentSums = new ConcurrentBag<long>();

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(startIndex, startIndex + length), options, range =>
            {
                long localSum = SumSseInner(arrayToSum, range.Item1, range.Item2 - 1);
                concurrentSums.Add(localSum);
            });

            long sum = 0;
            var sumsArray = concurrentSums.ToArray();
            for (int i = 0; i < sumsArray.Length; i++)
                sum += sumsArray[i];

            return sum;
        }
        /// <summary>
        /// Summation of byte[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a ulong accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongSsePar(this byte[] arrayToSum, int degreeOfParallelism = 0)
        {
            return arrayToSum.SumToUlongSsePar(0, arrayToSum.Length, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of byte[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a ulong accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongSsePar(this byte[] arrayToSum, int startIndex, int length, int degreeOfParallelism = 0)
        {
            var concurrentSums = new ConcurrentBag<ulong>();

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(startIndex, startIndex + length), options, range =>
            {
                //Console.WriteLine("Partition: start = {0}   end = {1}", range.Item1, range.Item2);
                ulong localSum = SumSseInner(arrayToSum, range.Item1, range.Item2 - 1);
                concurrentSums.Add(localSum);
            });

            ulong sum = 0;
            var sumsArray = concurrentSums.ToArray();
            for (int i = 0; i < sumsArray.Length; i++)
                sum += sumsArray[i];

            return sum;
        }
        /// <summary>
        /// Summation of ushort[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a ulong accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongSsePar(this ushort[] arrayToSum, int degreeOfParallelism = 0)
        {
            return arrayToSum.SumToUlongSsePar(0, arrayToSum.Length, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of byte[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a ulong accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongSsePar(this ushort[] arrayToSum, int startIndex, int length, int degreeOfParallelism = 0)
        {
            var concurrentSums = new ConcurrentBag<ulong>();

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(startIndex, startIndex + length), options, range =>
            {
                ulong localSum = SumSseInner(arrayToSum, range.Item1, range.Item2 - 1);
                concurrentSums.Add(localSum);
            });

            ulong sum = 0;
            var sumsArray = concurrentSums.ToArray();
            for (int i = 0; i < sumsArray.Length; i++)
                sum += sumsArray[i];

            return sum;
        }
        /// <summary>
        /// Summation of uint[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a ulong accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongSsePar(this uint[] arrayToSum, int degreeOfParallelism = 0)
        {
            return arrayToSum.SumToUlongSsePar(0, arrayToSum.Length, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of uint[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a ulong accumulator for perfect accuracy. Will not throw an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlongSsePar(this uint[] arrayToSum, int startIndex, int length, int degreeOfParallelism = 0)
        {
            var concurrentSums = new ConcurrentBag<ulong>();

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(startIndex, startIndex + length), options, range =>
            {
                ulong localSum = SumSseInner(arrayToSum, range.Item1, range.Item2 - 1);
                concurrentSums.Add(localSum);
            });

            ulong sum = 0;
            var sumsArray = concurrentSums.ToArray();
            for (int i = 0; i < sumsArray.Length; i++)
                sum += sumsArray[i];

            return sum;
        }
        /// <summary>
        /// Summation of ulong[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a ulong accumulator for perfect accuracy.
        /// Warning: will not throw an arithmetic overflow exception, wrapping around to smaller values quietly.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static ulong SumSsePar(this ulong[] arrayToSum, int degreeOfParallelism = 0)
        {
            return arrayToSum.SumSsePar(0, arrayToSum.Length, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of ulong[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a ulong accumulator for perfect accuracy.
        /// Warning: will not throw an arithmetic overflow exception, wrapping around to smaller values quietly.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <param name="degreeOfParallelism">number of computational cores to use</param>
        /// <returns>ulong sum</returns>
        public static ulong SumSsePar(this ulong[] arrayToSum, int startIndex, int length, int degreeOfParallelism = 0)
        {
            var concurrentSums = new ConcurrentBag<ulong>();

            int maxDegreeOfPar = degreeOfParallelism <= 0 ? Environment.ProcessorCount : degreeOfParallelism;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfPar };

            Parallel.ForEach(Partitioner.Create(startIndex, startIndex + length), options, range =>
            {
                ulong localSum = SumSseInner(arrayToSum, range.Item1, range.Item2 - 1);
                concurrentSums.Add(localSum);
            });

            ulong sum = 0;
            var sumsArray = concurrentSums.ToArray();
            for (int i = 0; i < sumsArray.Length; i++)
                sum += sumsArray[i];

            return sum;
        }

        private static BigInteger SumToBigIntegerFasterParInner(this ulong[] arrayToSum, int l, int r, int thresholdParallel = 16 * 1024)
        {
            BigInteger sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= thresholdParallel)
                return Algorithms.Sum.SumToBigIntegerFaster(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            BigInteger sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumToBigIntegerFasterParInner(arrayToSum, l,     m, thresholdParallel); },
                () => { sumRight = SumToBigIntegerFasterParInner(arrayToSum, m + 1, r, thresholdParallel); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static BigInteger SumToBigIntegerFasterParInner(this long[] arrayToSum, int l, int r, int thresholdParallel = 16 * 1024)
        {
            BigInteger sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= thresholdParallel)
                return Algorithms.Sum.SumToBigIntegerFaster(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            BigInteger sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumToBigIntegerFasterParInner(arrayToSum, l,     m, thresholdParallel); },
                () => { sumRight = SumToBigIntegerFasterParInner(arrayToSum, m + 1, r, thresholdParallel); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static BigInteger SumToBigIntegerSseFasterParInner(this long[] arrayToSum, int l, int r, int thresholdParallel = 16 * 1024)
        {
            BigInteger sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= thresholdParallel)
                return ParallelAlgorithms.Sum.SumToBigIntegerSseFasterInner(arrayToSum, l, r);

            int m = (r + l) / 2;

            BigInteger sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumToBigIntegerSseFasterParInner(arrayToSum, l,     m, thresholdParallel); },
                () => { sumRight = SumToBigIntegerSseFasterParInner(arrayToSum, m + 1, r, thresholdParallel); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static BigInteger SumToBigIntegerSseFasterParInner(this ulong[] arrayToSum, int l, int r, int thresholdParallel = 16 * 1024)
        {
            BigInteger sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= thresholdParallel)
                return ParallelAlgorithms.Sum.SumToBigIntegerSseFasterInner(arrayToSum, l, r);

            int m = (r + l) / 2;

            BigInteger sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumToBigIntegerSseFasterParInner(arrayToSum, l,     m, thresholdParallel); },
                () => { sumRight = SumToBigIntegerSseFasterParInner(arrayToSum, m + 1, r, thresholdParallel); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        /// <summary>
        /// Summation of byte[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a ulong accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        private static ulong SumToUlongSseParDac(this byte[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, SumToUlongSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of short[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a long accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        private static long SumToLongSseParDac(this short[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, SumToLongSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of short[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a long accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        private static long SumToLongSseParDac(this short[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, SumToLongSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of ushort[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a ulong accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        private static ulong SumToUlongSseParDac(this ushort[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, SumToUlongSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of ushort[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a ulong accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        private static ulong SumToUlongSseParDac(this ushort[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, SumToUlongSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of int[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a long accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        private static long SumToLongSseParDac(this int[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, SumToLongSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of int[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a long accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        private static long SumToLongSsePar(this int[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, SumToLongSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of uint[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a ulong accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        private static ulong SumToUlongSseParDac(this uint[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, SumToUlongSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of uint[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a ulong accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        private static ulong SumToUlongSsePar(this uint[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, SumToUlongSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of long[] array, using multiple cores, and using data parallel SIMD/SSE instructions on each core, for higher performance within each core.
        /// Warning: this function will quietly overflow, not throwing an arithmetic overflow exception, wrapping around to the opposite sign value.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        private static long SumSsePar(this long[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, 0, arrayToSum.Length, SumSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of long[] array, using multiple cores, and using data parallel SIMD/SSE instructions on each core, for higher performance within each core.
        /// Warning: this function will quietly overflow, not throwing an arithmetic overflow exception, wrapping around to the opposite sign value.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        private static long SumSsePar(this long[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, startIndex, length, SumSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of long[] array, using multiple cores, for higher performance within each core.
        /// Uses a decimal accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalPar(this long[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, Algorithms.Sum.SumToDecimal, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of long[] array, using multiple cores, for higher performance within each core.
        /// Uses a decimal accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalPar(this long[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, Algorithms.Sum.SumToDecimal, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of long[] array, using multiple cores, for higher performance within each core.
        /// Uses a long accumulator for faster performance while detecting overflow without exceptions and returning a decimal for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalFasterPar(this long[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, Algorithms.Sum.SumToDecimalFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of long[] array, using multiple cores, for higher performance within each core.
        /// Uses a long accumulator for faster performance while detecting overflow without exceptions and returning a decimal for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalFasterPar(this long[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, Algorithms.Sum.SumToDecimalFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of long[] array, using multiple cores, and using data parallel SIMD/SSE instructions on each core, for higher performance within each core.
        /// Uses a long accumulator for faster performance while detecting overflow without exceptions and returning a decimal for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalSseFasterPar(this long[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, ParallelAlgorithms.Sum.SumToDecimalSseFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of long[] array, using multiple cores, and using data parallel SIMD/SSE instructions on each core, for higher performance within each core.
        /// Uses a long accumulator for faster performance while detecting overflow without exceptions and returning a decimal for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalSseFasterPar(this long[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, ParallelAlgorithms.Sum.SumToDecimalSseFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of long[] array, using multiple cores, for higher performance within each core.
        /// Uses a long accumulator for faster performance while detecting overflow without exceptions and returning a BigInteger for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerFasterPar(this long[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return SumToBigIntegerFasterParInner(arrayToSum, 0, arrayToSum.Length - 1, thresholdParallel);
            //return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, Algorithms.Sum.SumToBigIntegerFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of long[] array, using multiple cores, for higher performance within each core.
        /// Uses a long accumulator for faster performance while detecting overflow without exceptions and returning a BigInteger for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static BigInteger SumToBigIntegerFasterPar(this long[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return SumToBigIntegerFasterParInner(arrayToSum, startIndex, startIndex + length - 1, thresholdParallel);
            //return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, Algorithms.Sum.SumToBigIntegerFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of long[] array, using multiple cores, and using data parallel SIMD/SSE instructions on each core, for higher performance within each core.
        /// Uses a long accumulator for faster performance while detecting overflow without exceptions and returning a BigInteger for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerSseFasterPar(this long[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return SumToBigIntegerSseFasterParInner(arrayToSum, 0, arrayToSum.Length - 1, thresholdParallel);
            //return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, ParallelAlgorithms.Sum.SumToBigIntegerSseFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of long[] array, using multiple cores, and using data parallel SIMD/SSE instructions on each core, for higher performance within each core.
        /// Uses a long accumulator for faster performance while detecting overflow without exceptions and returning a BigInteger for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerSseFasterPar(this long[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return SumToBigIntegerSseFasterParInner(arrayToSum, startIndex, startIndex + length - 1, thresholdParallel);
            //return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, ParallelAlgorithms.Sum.SumToBigIntegerSseFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of ulong[] array, using multiple cores, for higher performance within each core.
        /// Uses a decimal accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalPar(this ulong[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, Algorithms.Sum.SumToDecimal, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of ulong[] array, using multiple cores, for higher performance within each core.
        /// Uses a decimal accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalPar(this ulong[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, Algorithms.Sum.SumToDecimal, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of ulong[] array, using multiple cores, for higher performance within each core.
        /// Uses a ulong accumulator for faster performance while detecting overflow without exceptions and returning a decimal for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalFasterPar(this ulong[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, Algorithms.Sum.SumToDecimalFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of ulong[] array, using multiple cores, for higher performance within each core.
        /// Uses a ulong accumulator for faster performance while detecting overflow without exceptions and returning a decimal for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalFasterPar(this ulong[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, Algorithms.Sum.SumToDecimalFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of ulong[] array, using multiple cores, and using data parallel SIMD/SSE instructions on each core, for higher performance within each core.
        /// Uses a ulong accumulator for faster performance while detecting overflow without exceptions and returning a decimal for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalSseFasterPar(this ulong[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, ParallelAlgorithms.Sum.SumToDecimalSseFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of ulong[] array, using multiple cores, and using data parallel SIMD/SSE instructions on each core, for higher performance within each core.
        /// Uses a ulong accumulator for faster performance while detecting overflow without exceptions and returning a decimal for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalSseFasterPar(this ulong[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, ParallelAlgorithms.Sum.SumToDecimalSseFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of ulong[] array, using multiple cores, and using data parallel SIMD/SSE instructions on each core, for higher performance within each core.
        /// Uses a 128-bit accumulator for faster performance while detecting overflow without exceptions and returning a decimal for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalSseEvenFasterPar(this ulong[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, ParallelAlgorithms.Sum.SumToDecimalSseEvenFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of ulong[] array, using multiple cores, and using data parallel SIMD/SSE instructions on each core, for higher performance within each core.
        /// Uses a 128-bit accumulator for faster performance while detecting overflow without exceptions and returning a decimal for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalSseEvenFasterPar(this ulong[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, ParallelAlgorithms.Sum.SumToDecimalSseEvenFasterInner, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of ulong[] array, using multiple cores, for higher performance within each core.
        /// Uses a BigInteger accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerPar(this ulong[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, Algorithms.Sum.SumToBigInteger, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of ulong[] array, using multiple cores, for higher performance within each core.
        /// Uses a BigInteger accumulator for perfect accuracy. Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerPar(this ulong[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, Algorithms.Sum.SumToBigInteger, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of ulong[] array, using multiple cores, for higher performance within each core.
        /// Uses a ulong accumulator for faster performance while detecting overflow without exceptions and returning a BigInteger for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerFasterPar(this ulong[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return SumToBigIntegerFasterParInner(arrayToSum, 0, arrayToSum.Length - 1, thresholdParallel);
            //return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, Algorithms.Sum.SumToBigIntegerFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of ulong[] array, using multiple cores, for higher performance within each core.
        /// Uses a ulong accumulator for faster performance while detecting overflow without exceptions and returning a BigInteger for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerFasterPar(this ulong[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return SumToBigIntegerFasterParInner(arrayToSum, startIndex, startIndex + length - 1, thresholdParallel);
            //return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, Algorithms.Sum.SumToBigIntegerFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of ulong[] array, using multiple cores, and using data parallel SIMD/SSE instructions on each core, for higher performance within each core.
        /// Uses a ulong accumulator for faster performance while detecting overflow without exceptions and returning a BigInteger for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerSseFasterPar(this ulong[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return SumToBigIntegerSseFasterParInner(arrayToSum, 0, arrayToSum.Length - 1, thresholdParallel);
            //return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, ParallelAlgorithms.Sum.SumToBigIntegerSseFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of ulong[] array, using multiple cores, and using data parallel SIMD/SSE instructions on each core, for higher performance within each core.
        /// Uses a ulong accumulator for faster performance while detecting overflow without exceptions and returning a BigInteger for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerSseFasterPar(this ulong[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return SumToBigIntegerSseFasterParInner(arrayToSum, startIndex, startIndex + length - 1, thresholdParallel);
            //return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, ParallelAlgorithms.Sum.SumToBigIntegerSseFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of ulong[] array, using multiple cores, and using data parallel SIMD/SSE instructions on each core, for higher performance within each core.
        /// Uses a 128-bit accumulator for faster performance while detecting overflow without exceptions and returning a BigInteger for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerSseEvenFasterPar(this ulong[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return (BigInteger)SumToDecimalSseEvenFasterPar(arrayToSum, thresholdParallel, degreeOfParallelism);
            //return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, ParallelAlgorithms.Sum.SumToBigIntegerSseEvenFaster, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }
        /// <summary>
        /// Summation of ulong[] array, using multiple cores, and using data parallel SIMD/SSE instructions on each core, for higher performance within each core.
        /// Uses a 128-bit accumulator for faster performance while detecting overflow without exceptions and returning a BigInteger for perfect accuracy.
        /// Will not trow an overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerSseEvenFasterPar(this ulong[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return (BigInteger)SumToDecimalSseEvenFasterPar(arrayToSum, startIndex, length, thresholdParallel, degreeOfParallelism);
            //return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, ParallelAlgorithms.Sum.SumToBigIntegerSseEvenFasterInner, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of ulong[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        private static ulong SumSseParDac(this ulong[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, 0, arrayToSum.Length, SumSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of ulong[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        private static ulong SumSseParDac(this ulong[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, startIndex, length, SumSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of float[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>float sum</returns>
        public static float SumPar(this float[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, 0, arrayToSum.Length, Algorithms.Sum.SumHpc, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of float[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>float sum</returns>
        public static float SumPar(this float[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, startIndex, length, Algorithms.Sum.SumHpc, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of float[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a double accumulator for higher accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>float sum</returns>
        public static double SumToDoublePar(this float[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, Algorithms.Sum.SumToDouble, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of float[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a double accumulator for higher accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>float sum</returns>
        public static double SumToDoublePar(this float[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, Algorithms.Sum.SumToDouble, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of float[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>float sum</returns>
        public static float SumSsePar(this float[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, 0, arrayToSum.Length, SumSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of float[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>float sum</returns>
        public static float SumSsePar(this float[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, startIndex, length, SumSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of float[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a double accumulator for higher accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleSsePar(this float[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, SumSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of float[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// Uses a double accumulator for higher accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleSsePar(this float[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, SumSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of double[] array, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumPar(this double[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, 0, arrayToSum.Length, Algorithms.Sum.SumHpc, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of double[] array, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumPar(this double[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, startIndex, length, Algorithms.Sum.SumHpc, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of double[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumSsePar(this double[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, 0, arrayToSum.Length, SumSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of double[] array, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumSsePar(this double[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, startIndex, length, SumSse, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation: more accurate than for loop summation.
        /// Summation of float[] array, using a more accurate Kahan summation algorithm, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>float sum</returns>
        public static float SumParMostAccurate(this float[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, Algorithms.Sum.SumMostAccurate, Algorithms.Sum.SumMostAccurate, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation: more accurate than for loop summation.
        /// Summation of float[] array, using a more accurate Kahan summation algorithm, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>float sum</returns>
        public static float SumParMostAccurate(this float[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, Algorithms.Sum.SumMostAccurate, Algorithms.Sum.SumMostAccurate, thresholdParallel, degreeOfParallelism);
        }
        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation: more accurate than for loop summation.
        /// Summation of float[] array, using a more accurate Kahan summation algorithm, using a double accumulator for higher accuracy, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleParMostAccurate(this float[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, Algorithms.Sum.SumToDoubleMostAccurate, Algorithms.Sum.SumMostAccurate, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation: more accurate than for loop summation.
        /// Summation of float[] array, using a more accurate Kahan summation algorithm, using a double accumulator for higher accuracy, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleParMostAccurate(this float[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, Algorithms.Sum.SumToDoubleMostAccurate, Algorithms.Sum.SumMostAccurate, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation: more accurate than for loop summation.
        /// Summation of float[] array, using a more accurate Kahan summation algorithm, using data parallel SIMD/SSE instructions for higher performance within each core, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>float sum</returns>
        public static float SumSseParMostAccurate(this float[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, 0, arrayToSum.Length, SumSseMostAccurate, Algorithms.Sum.SumMostAccurate, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation: more accurate than for loop summation.
        /// Summation of float[] array, using a more accurate Kahan summation algorithm, using data parallel SIMD/SSE instructions for higher performance within each core, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>float sum</returns>
        public static float SumSseParMostAccurate(this float[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, startIndex, length, SumSseMostAccurate, Algorithms.Sum.SumMostAccurate, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation: more accurate than for loop summation.
        /// Summation of float[] array, using a more accurate Kahan summation algorithm, using a double precision accumulator for higher accuracy, using data parallel SIMD/SSE instructions for higher performance within each core, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleSseParMostAccurate(this float[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, 0, arrayToSum.Length, SumToDoubleSseMostAccurate, Algorithms.Sum.SumMostAccurate, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation: more accurate than for loop summation.
        /// Summation of float[] array, using a more accurate Kahan summation algorithm, using a double precision accumulator for higher accuracy, using data parallel SIMD/SSE instructions for higher performance within each core, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleSseParMostAccurate(this float[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerTwoTypesPar(arrayToSum, startIndex, length, SumToDoubleSseMostAccurate, Algorithms.Sum.SumMostAccurate, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation: more accurate than for loop summation.
        /// Summation of double[] array, using a more accurate Kahan summation algorithm, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumParMostAccurate(this double[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, 0, arrayToSum.Length, Algorithms.Sum.SumMostAccurate, Algorithms.Sum.SumMostAccurate, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation: more accurate than for loop summation.
        /// Summation of double[] array, using a more accurate Kahan summation algorithm, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumParMostAccurate(this double[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, startIndex, length, Algorithms.Sum.SumMostAccurate, Algorithms.Sum.SumMostAccurate, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation: more accurate than for loop summation.
        /// Summation of double[] array, using a more accurate Kahan summation algorithm, using data parallel SIMD/SSE instructions for higher performance within each core, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumSseParMostAccurate(this double[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, 0, arrayToSum.Length, SumSseMostAccurate, Algorithms.Sum.SumMostAccurate, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation: more accurate than for loop summation.
        /// Summation of double[] array, using a more accurate Kahan summation algorithm, using data parallel SIMD/SSE instructions for higher performance within each core, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumSseParMostAccurate(this double[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, startIndex, length, SumSseMostAccurate, Algorithms.Sum.SumMostAccurate, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of decimal[] array, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumPar(this decimal[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, 0, arrayToSum.Length, Algorithms.Sum.SumHpc, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of decimal[] array, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumPar(this decimal[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, startIndex, length, Algorithms.Sum.SumHpc, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of BigInteger[] array, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumPar(this BigInteger[] arrayToSum, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, 0, arrayToSum.Length, Algorithms.Sum.SumHpc, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }

        /// <summary>
        /// Summation of BigInteger[] array, using multiple cores.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static BigInteger SumPar(this BigInteger[] arrayToSum, int startIndex, int length, int thresholdParallel = 16 * 1024, int degreeOfParallelism = 0)
        {
            return AlgorithmPatterns.DivideAndConquerPar(arrayToSum, startIndex, length, Algorithms.Sum.SumHpc, (x, y) => x + y, thresholdParallel, degreeOfParallelism);
        }
    }
}
