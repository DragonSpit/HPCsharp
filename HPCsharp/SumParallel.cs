// TODO: To speedup summing up of long to decimal accumulation, Josh suggested using a long accumulator and catching the overflow exception and then adding to decimal - i.e. most of the time accumulate to long and once in
//       a while accumulate to decimal instead of always accumulating to decimal (offer this version as an alternate)
// TODO: Implement a for loop instead of divide-and-conquer, since they really accomplish the same thing, and the for loop will be more efficient and easier to make cache line boundary divisible.
//       Combining will be slightly harder, but we could create an array of sums, where each task has its own array element to fill in, then we combine all of the sums at the end serially. Still has the issue of managed memory
//       where the array may move and not be cache aligned any more, requiring fixing the array in memory to not move (an unsafe version).
// TODO: Implement nullable versions of Sum, only faster than the standard C# ones. Should be able to still use SSE and multi-core, but need to check out each element for null before adding it. These
//       will be much slower than non-nullable. Hmmm.. Need to test what Linq.Sum() returns for nullable, as I bet the sum becomes null, since null is really an unknown and if one value of an array is unknown, then the Sum() becomes unknown.
//       If that's the case for Linq.Sum() for nullable types, then SIMD acceleration is still possible, but will be much slower, since we'll need to test each Vector.Count array items for null, and only if they all are not-null then add them to the sum.
//       From research on the web, Linq.Sum() skips null values by default - i.e. treats them as zeroes. This is inconsistent with adding a null value to a number.
// TODO: See if SSEandScalar version is faster when the array is entirely inside the cache, to make sure that it's not just being memory bandwidth limited and hiding ILP speedup. Port it to C++ and see
//       if it speeds up. Run many times over the same array using .Sum() and provide the average and minimum timing. Also, could test using a single core, where memory bandwidth is not the limiter.
// TODO: Return a tupple (sum and c) from each parallel Neumaier result and figure out how to combine these results for a more accurate and possibly perfect overall result that will match serial array processing result.
// TODO: Since C# has support for BigInteger data type in System.Numerics, then provide accurate .Sum() all the way to these for decimal[], float[] and double[]. Basically, provide a consistent story for .Sum() where every type can be
//       summed with perfect accuracy when needed. Make sure naming of functions is consistent for all of this and across all data types, multi-core and SSE implementations, to make it simple, logical and consistent to use.
//       Sadly, this idea won't work, since we need a BigDecimal or BigFloatingPoint to capture perfect accumulation for both of these non-integer types.
// TODO: Implement .Sum() for summing a field of Objects/UserDefinedTypes, if it's possible to add value by possibly aggregating elements into a local array of Vector size and doing an SSE sum. Is it possible to abstract it well and to
//       perform well to support extracting all numeric data types, so that performance and abstraction are competitive and as simple or simpler than Linq?
// TODO: Write a blog on floating-point .Sum() and all of it's capabilities, options and trade-offs in performance and accuracy (Submitted a paper proposal to the MSDN journal. Waiting for response first.)
// TODO: Rename Neumaier .Sum() to sum_kbn as Julia language does, since the original implementation was done by Kahan-Babuska and KBN would give all three creators credit
// TODO: Re-use the new generic divide-and-conquer function, and it could even be a lambda function for some implementations (like non-Kahan-Neumaier addition). For float and double summation, we just need to pass in function of double or float.
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
// TODO: Need to implement integer and unsigned integer .SumPar() - i.e. without SSE, but multi-core
// TODO: Study parallel solution presented here (parallel for), which may be better in some cases: https://stackoverflow.com/questions/2419343/how-to-sum-up-an-array-of-integers-in-c-sharp/54794753#54794753

using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System;

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
        public static long SumSse(this sbyte[] arrayToSum)
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
        public static long SumSse(this sbyte[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static long SumSseInner(this sbyte[] arrayToSum, int l, int r)
        {
            var sumVector000 = new Vector<long>();
            var sumVector001 = new Vector<long>();
            var sumVector010 = new Vector<long>();
            var sumVector011 = new Vector<long>();
            var sumVector100 = new Vector<long>();
            var sumVector101 = new Vector<long>();
            var sumVector110 = new Vector<long>();
            var sumVector111 = new Vector<long>();
            var shortLow  = new Vector<short>();
            var shortHigh = new Vector<short>();
            var int00 = new Vector<int>();
            var int01 = new Vector<int>();
            var int10 = new Vector<int>();
            var int11 = new Vector<int>();
            var long000 = new Vector<long>();
            var long001 = new Vector<long>();
            var long010 = new Vector<long>();
            var long011 = new Vector<long>();
            var long100 = new Vector<long>();
            var long101 = new Vector<long>();
            var long110 = new Vector<long>();
            var long111 = new Vector<long>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<sbyte>.Count) * Vector<sbyte>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<sbyte>.Count)
            {
                var inVector = new Vector<sbyte>(arrayToSum, i);
                Vector.Widen(inVector, out shortLow, out shortHigh);
                Vector.Widen(shortLow,  out int00, out int01);
                Vector.Widen(shortHigh, out int10, out int11);
                Vector.Widen(int00, out long000, out long001);
                Vector.Widen(int01, out long010, out long011);
                Vector.Widen(int10, out long100, out long101);
                Vector.Widen(int11, out long110, out long111);
                sumVector000 += long000;
                sumVector001 += long001;
                sumVector010 += long010;
                sumVector011 += long011;
                sumVector100 += long100;
                sumVector101 += long101;
                sumVector110 += long110;
                sumVector111 += long111;
            }
            long overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            sumVector000 += sumVector001;
            sumVector010 += sumVector011;
            sumVector000 += sumVector010;
            sumVector100 += sumVector101;
            sumVector110 += sumVector111;
            sumVector100 += sumVector110;
            sumVector000 += sumVector100;
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVector000[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of byte[] array, which uses a ulong accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumSse(this byte[] arrayToSum)
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
        public static ulong SumSse(this byte[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static ulong SumSseInner(this byte[] arrayToSum, int l, int r)
        {
            var sumVector000 = new Vector<ulong>();
            var sumVector001 = new Vector<ulong>();
            var sumVector010 = new Vector<ulong>();
            var sumVector011 = new Vector<ulong>();
            var sumVector100 = new Vector<ulong>();
            var sumVector101 = new Vector<ulong>();
            var sumVector110 = new Vector<ulong>();
            var sumVector111 = new Vector<ulong>();
            var shortLow  = new Vector<ushort>();
            var shortHigh = new Vector<ushort>();
            var int00 = new Vector<uint>();
            var int01 = new Vector<uint>();
            var int10 = new Vector<uint>();
            var int11 = new Vector<uint>();
            var long000 = new Vector<ulong>();
            var long001 = new Vector<ulong>();
            var long010 = new Vector<ulong>();
            var long011 = new Vector<ulong>();
            var long100 = new Vector<ulong>();
            var long101 = new Vector<ulong>();
            var long110 = new Vector<ulong>();
            var long111 = new Vector<ulong>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<byte>.Count) * Vector<byte>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<byte>.Count)
            {
                var inVector = new Vector<byte>(arrayToSum, i);
                Vector.Widen(inVector, out shortLow, out shortHigh);
                Vector.Widen(shortLow, out int00, out int01);
                Vector.Widen(shortHigh, out int10, out int11);
                Vector.Widen(int00, out long000, out long001);
                Vector.Widen(int01, out long010, out long011);
                Vector.Widen(int10, out long100, out long101);
                Vector.Widen(int11, out long110, out long111);
                sumVector000 += long000;
                sumVector001 += long001;
                sumVector010 += long010;
                sumVector011 += long011;
                sumVector100 += long100;
                sumVector101 += long101;
                sumVector110 += long110;
                sumVector111 += long111;
            }
            ulong overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            sumVector000 += sumVector001;
            sumVector010 += sumVector011;
            sumVector000 += sumVector010;
            sumVector100 += sumVector101;
            sumVector110 += sumVector111;
            sumVector100 += sumVector110;
            sumVector000 += sumVector100;
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVector000[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of short[] array, which uses a long accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static long SumSse(this short[] arrayToSum)
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
        public static long SumSse(this short[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static long SumSseInner(this short[] arrayToSum, int l, int r)
        {
            var sumVector00 = new Vector<long>();
            var sumVector01 = new Vector<long>();
            var sumVector10 = new Vector<long>();
            var sumVector11 = new Vector<long>();
            var intLow    = new Vector<int>();
            var intHigh   = new Vector<int>();
            var long00 = new Vector<long>();
            var long01 = new Vector<long>();
            var long10 = new Vector<long>();
            var long11 = new Vector<long>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<short>.Count) * Vector<short>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<short>.Count)
            {
                var inVector = new Vector<short>(arrayToSum, i);
                Vector.Widen(inVector, out intLow, out intHigh);
                Vector.Widen(intLow,   out long00, out long01);
                Vector.Widen(intHigh,  out long10, out long11);
                sumVector00 += long00;
                sumVector01 += long01;
                sumVector10 += long10;
                sumVector11 += long11;
            }
            long overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            sumVector00 += sumVector01;
            sumVector10 += sumVector11;
            sumVector00 += sumVector10;
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVector00[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of ushort[] array, which uses a ulong accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumSse(this ushort[] arrayToSum)
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
        public static ulong SumSse(this ushort[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static ulong SumSseInner(this ushort[] arrayToSum, int l, int r)
        {
            var sumVector00 = new Vector<ulong>();
            var sumVector01 = new Vector<ulong>();
            var sumVector10 = new Vector<ulong>();
            var sumVector11 = new Vector<ulong>();
            var intLow  = new Vector<uint>();
            var intHigh = new Vector<uint>();
            var long00 = new Vector<ulong>();
            var long01 = new Vector<ulong>();
            var long10 = new Vector<ulong>();
            var long11 = new Vector<ulong>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<short>.Count) * Vector<short>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<short>.Count)
            {
                var inVector = new Vector<ushort>(arrayToSum, i);
                Vector.Widen(inVector, out intLow, out intHigh);
                Vector.Widen(intLow, out long00, out long01);
                Vector.Widen(intHigh, out long10, out long11);
                sumVector00 += long00;
                sumVector01 += long01;
                sumVector10 += long10;
                sumVector11 += long11;
            }
            ulong overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            sumVector00 += sumVector01;
            sumVector10 += sumVector11;
            sumVector00 += sumVector10;
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVector00[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of int[] array, which uses a long accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static long SumSse(this int[] arrayToSum)
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
        public static long SumSse(this int[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static long SumSseInner(this int[] arrayToSum, int l, int r)
        {
            var sumVectorLower = new Vector<long>();
            var sumVectorUpper = new Vector<long>();
            var longLower = new Vector<long>();
            var longUpper = new Vector<long>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<int>.Count) * Vector<int>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVector = new Vector<int>(arrayToSum, i);
                Vector.Widen(inVector, out longLower, out longUpper);
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
            var longLower      = new Vector<long>();
            var longUpper      = new Vector<long>();
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
                Vector.Widen(inVector, out longLower, out longUpper);
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

        public static long SumSse(this int?[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        public static long SumSse(this int?[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseInner(start, start + length - 1);
        }

        private static long SumSseAndScalarInner(this int[] arrayToSum, int l, int r)
        {
            const int numScalarOps = 2;
            var sumVectorLower = new Vector<long>();
            var sumVectorUpper = new Vector<long>();
            var longLower      = new Vector<long>();
            var longUpper      = new Vector<long>();
            int lengthForVector = (r - l + 1) / (Vector<int>.Count + numScalarOps) * Vector<int>.Count;
            int numFullVectors = lengthForVector / Vector<int>.Count;
            long partialScalarSum0 = 0;
            long partialScalarSum1 = 0;
            int i = l;
            int scalarIndex = l + numFullVectors * Vector<int>.Count;
            int sseIndexEnd = scalarIndex;
            //System.Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", arrayToSum.Length, Vector<int>.Count, lengthForVector, numFullVectors, scalarIndex, sseIndexEnd);
            for (; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVector = new Vector<int>(arrayToSum, i);
                Vector.Widen(inVector, out longLower, out longUpper);
                partialScalarSum0 += arrayToSum[scalarIndex++];          // interleave SSE and Scaler operations
                sumVectorLower += longLower;
                partialScalarSum1 += arrayToSum[scalarIndex++];
                sumVectorUpper += longUpper;
            }
            //System.Console.WriteLine("{0}", scalarIndex);
            for (i = scalarIndex; i <= r; i++)
                partialScalarSum0 += arrayToSum[i];
            partialScalarSum0 += partialScalarSum1;
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<long>.Count; i++)
                partialScalarSum0 += sumVectorLower[i];
            return partialScalarSum0;
        }

        private static long SumSseAndScalar(this int[] arrayToSum)
        {
            //return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
            return arrayToSum.SumSseAndScalarInner(0, arrayToSum.Length - 1);
        }

        private static long SumSseAndScalar(this int[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseAndScalarInner(start, start + length - 1);
        }

        /// <summary>
        /// Summation of uint[] array, which uses a ulong accumulator for perfect accuracy, using data parallel SIMD/SSE instructions for higher performance on a single core.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumSse(this uint[] arrayToSum)
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
        public static ulong SumSse(this uint[] arrayToSum, int startIndex, int length)
        {
            return arrayToSum.SumSseInner(startIndex, startIndex + length - 1);
        }

        private static ulong SumSseInner(this uint[] arrayToSum, int l, int r)
        {
            var sumVectorLower = new Vector<ulong>();
            var sumVectorUpper = new Vector<ulong>();
            var longLower      = new Vector<ulong>();
            var longUpper      = new Vector<ulong>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<uint>.Count) * Vector<uint>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVector = new Vector<uint>(arrayToSum, i);
                Vector.Widen(inVector, out longLower, out longUpper);
                sumVectorLower += longLower;
                sumVectorUpper += longUpper;
            }
            ulong overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVectorLower[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of long[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
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
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVector[i];
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
        // About 5% faster on my quad-core laptop
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
        /// Summation of ulong[] array, using data parallel SIMD/SSE instructions for higher performance on a single core.
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
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        public static float SumSse(this float[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        public static float SumSse(this float[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseInner(start, start + length - 1);
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

        public static double SumSseDbl(this float[] arrayToSum)
        {
            return arrayToSum.SumSseDoubleInner(0, arrayToSum.Length - 1);
        }

        public static double SumSseDbl(this float[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseDoubleInner(start, start + length - 1);
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

        public static double SumSse(this double[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        public static double SumSse(this double[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseInner(start, start + length - 1);
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

        public static float SumSseNeumaier(this float[] arrayToSum)
        {
            return arrayToSum.SumSseNeumaierInner(0, arrayToSum.Length - 1);
        }

        public static float SumSseNeumaier(this float[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseNeumaierInner(start, start + length - 1);
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

        public static double SumSseNeumaierDbl(this float[] arrayToSum)
        {
            return arrayToSum.SumSseNeumaierDoubleInner(0, arrayToSum.Length - 1);
        }

        public static double SumSseNeumaierDbl(this float[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseNeumaierDoubleInner(start, start + length - 1);
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

        public static double SumSseNeumaier(this double[] arrayToSum)
        {
            return arrayToSum.SumSseNeumaierInner(0, arrayToSum.Length - 1);
        }

        public static double SumSseNeumaier(this double[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseNeumaierInner(start, start + length - 1);
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

        public static int ThresholdParallelSum { get; set; } = 16 * 1024;

        private static long SumSseParInner(this sbyte[] arrayToSum, int l, int r)
        {
            long sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSse(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            long sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft = SumSseParInner(arrayToSum, l, m); },
                () => { sumRight = SumSseParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static ulong SumSseParInner(this byte[] arrayToSum, int l, int r)
        {
            ulong sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSse(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            ulong sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSseParInner(arrayToSum, l,     m); },
                () => { sumRight = SumSseParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static long SumSseParInner(this short[] arrayToSum, int l, int r)
        {
            long sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSse(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            long sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSseParInner(arrayToSum, l,     m); },
                () => { sumRight = SumSseParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static ulong SumSseParInner(this ushort[] arrayToSum, int l, int r)
        {
            ulong sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSse(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            ulong sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSseParInner(arrayToSum, l,     m); },
                () => { sumRight = SumSseParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static long SumSseParInner(this int[] arrayToSum, int l, int r)
        {
            long sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSse(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            long sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSseParInner(arrayToSum, l,     m); },
                () => { sumRight = SumSseParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static ulong SumSseParInner(this uint[] arrayToSum, int l, int r)
        {
            ulong sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSse(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            ulong sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSseParInner(arrayToSum, l,     m); },
                () => { sumRight = SumSseParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static long SumSseParInner(this long[] arrayToSum, int l, int r)
        {
            long sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSse(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            long sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSseParInner(arrayToSum, l,     m); },
                () => { sumRight = SumSseParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static ulong SumSseParInner(this ulong[] arrayToSum, int l, int r)
        {
            ulong sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSse(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            ulong sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSseParInner(arrayToSum, l,     m); },
                () => { sumRight = SumSseParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static float SumParInner(this float[] arrayToSum, int l, int r)
        {
            float sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return Algorithms.Sum.SumHpc(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            float sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumParInner(arrayToSum, l,     m); },
                () => { sumRight = SumParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static double SumDblParInner(this float[] arrayToSum, int l, int r)
        {
            double sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return Algorithms.Sum.SumDblHpc(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            double sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumDblParInner(arrayToSum, l, m); },
                () => { sumRight = SumDblParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        // Generic enough to be used for scalar multi-core, SSE multi-core, Kahan/Neumaier multi-core, and SSE Kahan/Neumaier multi-core
        // Sadly, C# generics do not support limiting to only certain numeric types, making it impossible to implement an even further generic function for all numeric types
        private static float SumParInner(this float[] arrayToSum, int l, int r, Func<float[], int, int, float> baseCase, Func<float, float, float> reduce)
        {
            float sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return baseCase(arrayToSum, l, r);

            int m = (r + l) / 2;

            float sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumParInner(arrayToSum, l,     m, baseCase, reduce); },
                () => { sumRight = SumParInner(arrayToSum, m + 1, r, baseCase, reduce); }
            );
            return reduce(sumLeft, sumRight);
        }

        private static double SumDblParInner(this float[] arrayToSum, int l, int r, Func<float[], int, int, double> baseCase, Func<double, double, double> reduce)
        {
            double sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return baseCase(arrayToSum, l, r);

            int m = (r + l) / 2;

            double sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumDblParInner(arrayToSum, l,     m, baseCase, reduce); },
                () => { sumRight = SumDblParInner(arrayToSum, m + 1, r, baseCase, reduce); }
            );
            return reduce(sumLeft, sumRight);
        }

        private static double SumSseParInner(this float[] arrayToSum, int l, int r)
        {
            double sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSse(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            double sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSseParInner(arrayToSum, l,     m); },
                () => { sumRight = SumSseParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static float SumNeumaierParInner(this float[] arrayToSum, int l, int r)
        {
            float sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return Algorithms.Sum.SumNeumaier(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            float sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumNeumaierParInner(arrayToSum, l,     m); },
                () => { sumRight = SumNeumaierParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return Algorithms.Sum.SumNeumaier(sumLeft, sumRight);
        }

        private static double SumNeumaierDoubleParInner(this float[] arrayToSum, int l, int r)
        {
            double sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return Algorithms.Sum.SumNeumaierDbl(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            double sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumNeumaierDoubleParInner(arrayToSum, l,     m); },
                () => { sumRight = SumNeumaierDoubleParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return Algorithms.Sum.SumNeumaier(sumLeft, sumRight);
        }

        private static float SumSseNeumaierParInner(this float[] arrayToSum, int l, int r)
        {
            float sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSseNeumaier(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            float sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSseNeumaierParInner(arrayToSum, l,     m); },
                () => { sumRight = SumSseNeumaierParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return Algorithms.Sum.SumNeumaier(sumLeft, sumRight);
        }

        private static double SumSseNeumaierDoubleParInner(this float[] arrayToSum, int l, int r)
        {
            double sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSseNeumaierDbl(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            double sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSseNeumaierDoubleParInner(arrayToSum, l,     m); },
                () => { sumRight = SumSseNeumaierDoubleParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return Algorithms.Sum.SumNeumaier(sumLeft, sumRight);
        }

        private static double SumParInner(this double[] arrayToSum, int l, int r, Func<double[], int, int, double> baseCase, Func<double, double, double> reduce)
        {
            double sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return baseCase(arrayToSum, l, r);

            int m = (r + l) / 2;

            double sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumParInner(arrayToSum, l,     m, baseCase, reduce); },
                () => { sumRight = SumParInner(arrayToSum, m + 1, r, baseCase, reduce); }
            );
            return reduce(sumLeft, sumRight);
        }

        private static T SumParInner<T>(this T[] arrayToSum, int l, int r, Func<T[], int, int, T> baseCase, Func<T, T, T> reduce, int thresholdParSum = 16 * 1024)
        {
            T sumLeft = default(T);

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= thresholdParSum)
                return baseCase(arrayToSum, l, r);

            int m = (r + l) / 2;

            T sumRight = default(T);

            Parallel.Invoke(
                () => { sumLeft  = SumParInner(arrayToSum, l,     m, baseCase, reduce, thresholdParSum); },
                () => { sumRight = SumParInner(arrayToSum, m + 1, r, baseCase, reduce, thresholdParSum); }
            );
            return reduce(sumLeft, sumRight);
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

        private static double SumParInner(this double[] arrayToSum, int l, int r)
        {
            double sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return Algorithms.Sum.SumHpc(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            double sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumParInner(arrayToSum, l,     m); },
                () => { sumRight = SumParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static double SumSseParInner(this double[] arrayToSum, int l, int r)
        {
            double sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSse(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            double sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSseParInner(arrayToSum, l,     m); },
                () => { sumRight = SumSseParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static double SumNeumaierParInner(this double[] arrayToSum, int l, int r)
        {
            double sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return Algorithms.Sum.SumNeumaier(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            double sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumNeumaierParInner(arrayToSum, l,     m); },
                () => { sumRight = SumNeumaierParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return Algorithms.Sum.SumNeumaier(sumLeft, sumRight);
        }

        private static double SumSseNeumaierParInner(this double[] arrayToSum, int l, int r)
        {
            double sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSseNeumaier(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            double sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSseNeumaierParInner(arrayToSum, l,     m); },
                () => { sumRight = SumSseNeumaierParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return Algorithms.Sum.SumNeumaier(sumLeft, sumRight);
        }

        private static decimal SumParInner(this decimal[] arrayToSum, int l, int r)
        {
            decimal sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return Algorithms.Sum.SumHpc(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            decimal sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumParInner(arrayToSum, l,     m); },
                () => { sumRight = SumParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static decimal SumParInner(this long[] arrayToSum, int l, int r)
        {
            decimal sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return Algorithms.Sum.SumDecimalHpc(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            decimal sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumParInner(arrayToSum, l,     m); },
                () => { sumRight = SumParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        private static decimal SumParInner(this ulong[] arrayToSum, int l, int r)
        {
            decimal sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return Algorithms.Sum.SumDecimalHpc(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            decimal sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumParInner(arrayToSum, l,     m); },
                () => { sumRight = SumParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return sumLeft + sumRight;
        }

        public static long SumSsePar(this sbyte[] arrayToSum)
        {
            return SumSseParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static long SumSsePar(this sbyte[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseParInner(start, start + length - 1);
        }

        public static ulong SumSsePar(this byte[] arrayToSum)
        {
            return SumSseParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static ulong SumSsePar(this byte[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseParInner(start, start + length - 1);
        }

        public static long SumSsePar(this short[] arrayToSum)
        {
            return SumSseParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static long SumSsePar(this short[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseParInner(start, start + length - 1);
        }

        public static ulong SumSsePar(this ushort[] arrayToSum)
        {
            return SumSseParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static ulong SumSsePar(this ushort[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseParInner(start, start + length - 1);
        }

        public static long SumSsePar(this int[] arrayToSum)
        {
            return SumSseParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static long SumSsePar(this int[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseParInner(start, start + length - 1);
        }

        public static ulong SumSsePar(this uint[] arrayToSum)
        {
            return SumSseParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static ulong SumSsePar(this uint[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseParInner(start, start + length - 1);
        }

        public static long SumSsePar(this long[] arrayToSum)
        {
            return SumSseParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static long SumSsePar(this long[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseParInner(start, start + length - 1);
        }

        public static decimal SumDecimalPar(this long[] arrayToSum)
        {
            return SumParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static decimal SumDecimalPar(this long[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumParInner(start, start + length - 1);
        }

        public static decimal SumDecimalPar(this ulong[] arrayToSum)
        {
            return SumParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }
        public static decimal SumDecimalPar(this ulong[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumParInner(start, start + length - 1);
        }

        public static ulong SumSsePar(this ulong[] arrayToSum)
        {
            return SumSseParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static ulong SumSsePar(this ulong[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseParInner(start, start + length - 1);
        }

        public static float SumPar(this float[] arrayToSum)
        {
            return arrayToSum.SumParInner(0, arrayToSum.Length - 1);
            //return arrayToSum.SumParInner(0, arrayToSum.Length - 1, Algorithm.SumLR, (x, y) => x + y);
        }

        public static float SumPar(this float[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumParInner(start, start + length - 1);
            //return arrayToSum.SumParInner(start, start + length - 1, Algorithm.SumLR, (x, y) => x + y);
        }

        public static double SumDblPar(this float[] arrayToSum)
        {
            return arrayToSum.SumDblParInner(0, arrayToSum.Length - 1);
            //return arrayToSum.SumDblParInner(0, arrayToSum.Length - 1, Algorithm.SumDblLR, (x, y) => x + y);
        }

        public static double SumDblPar(this float[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumDblParInner(start, start + length - 1);
            //return arrayToSum.SumDblParInner(start, start + length - 1, Algorithm.SumDblLR, (x, y) => x + y);
        }

        public static double SumSsePar(this float[] arrayToSum)
        {
            return SumSseParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }
        
        public static double SumSsePar(this float[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseParInner(start, start + length - 1);
        }
        public static float SumNeumaierPar(this float[] arrayToSum)
        {
            return SumNeumaierParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static float SumNeumaierPar(this float[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumNeumaierParInner(start, start + length - 1);
        }
        public static double SumNeumaierDblPar(this float[] arrayToSum)
        {
            return SumNeumaierDoubleParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static double SumNeumaierDblPar(this float[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumNeumaierDoubleParInner(start, start + length - 1);
        }

        public static float SumSseNeumaierPar(this float[] arrayToSum)
        {
            return SumSseNeumaierParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static float SumSseNeumaierPar(this float[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseNeumaierParInner(start, start + length - 1);
        }

        public static double SumSseNeumaierDblPar(this float[] arrayToSum)
        {
            return SumSseNeumaierDoubleParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static double SumSseNeumaierDblPar(this float[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseNeumaierDoubleParInner(start, start + length - 1);
        }

        public static double SumPar(this double[] arrayToSum)
        {
            return SumParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static double SumPar(this double[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumParInner(start, start + length - 1);
        }

        public static double SumSsePar(this double[] arrayToSum)
        {
            return SumSseParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static double SumSsePar(this double[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseParInner(start, start + length - 1);
        }

        public static double SumNeumaierPar(this double[] arrayToSum)
        {
            return SumNeumaierParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static double SumNeumaierPar(this double[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumNeumaierParInner(start, start + length - 1);
        }

        public static double SumSseNeumaierPar(this double[] arrayToSum)
        {
            return SumSseNeumaierParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static double SumSseNeumaierPar(this double[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseNeumaierParInner(start, start + length - 1);
        }
        public static decimal SumPar(this decimal[] arrayToSum)
        {
            return SumParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static decimal SumPar(this decimal[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumParInner(start, start + length - 1);
        }

#if false
        public static void FillGenericSse<T>(this T[] arrayToFill, T value, int startIndex, int length) where T : struct
        {
            var fillVector = new Vector<T>(value);
            int numFullVectorsIndex = (length / Vector<T>.Count) * Vector<T>.Count;
            int i;
            for (i = startIndex; i < numFullVectorsIndex; i += Vector<T>.Count)
                fillVector.CopyTo(arrayToFill, i);
            for (; i < arrayToFill.Length; i++)
                arrayToFill[i] = value;
        }

        public static void FillSse(this byte[] arrayToFill, byte value)
        {
            var fillVector = new Vector<byte>(value);
            int endOfFullVectorsIndex = (arrayToFill.Length / Vector<byte>.Count) * Vector<byte>.Count;
            ulong numBytesUnaligned = 0;
            unsafe
            {
                byte* ptrToArray = (byte *)arrayToFill[0];
                numBytesUnaligned = ((ulong)ptrToArray) & 63;
            }
            //Console.WriteLine("Pointer offset = {0}", numBytesUnaligned);
            int i;
            for (i = 0; i < endOfFullVectorsIndex; i += Vector<byte>.Count)
                fillVector.CopyTo(arrayToFill, i);
            for (; i < arrayToFill.Length; i++)
                arrayToFill[i] = value;
        }

        public static void FillSse(this byte[] arrayToFill, byte value, int startIndex, int length)
        {
            var fillVector = new Vector<byte>(value);
            int endOfFullVectorsIndex, numBytesUnaligned, i = startIndex;
            unsafe
            {
                fixed (byte* ptrToArray = &arrayToFill[startIndex])
                {
                    numBytesUnaligned = (int)((ulong)ptrToArray & (ulong)(Vector<byte>.Count- 1));
                    int endOfByteUnaligned = (numBytesUnaligned == 0) ? 0 : Vector<byte>.Count;
                    int numBytesFilled = 0;
                    for (int j = numBytesUnaligned; j < endOfByteUnaligned; j++, i++, numBytesFilled++)
                    {
                        if (numBytesFilled < length)
                            arrayToFill[i] = value;
                        else
                            break;
                    }
                    endOfFullVectorsIndex = i + ((length - numBytesFilled) / Vector<byte>.Count) * Vector<byte>.Count;
                    //Console.WriteLine("Pointer offset = {0}  ptr = {1:X}  startIndex = {2}  i = {3} endIndex = {4} length = {5} lengthLeft = {6}",
                    //    numBytesUnaligned, (ulong)ptrToArray, startIndex, i, endOfFullVectorsIndex, length, length - numBytesFilled);
                    for (; i < endOfFullVectorsIndex; i += Vector<byte>.Count)
                        fillVector.CopyTo(arrayToFill, i);
                }
            }
            //Console.WriteLine("After fill using Vector, i = {0}", i);
            for (; i < startIndex + length; i++)
                arrayToFill[i] = value;
        }
#endif
    }
}
