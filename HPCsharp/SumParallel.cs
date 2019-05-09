// TODO: To speedup summing up of long to decimal accumulation, Josh suggested using a long accumulator and catching the overflow exception and then adding to decimal - i.e. most of the time accumulate to long and once in
//       a while accumulate to decimal instead of always accumulating to decimal (offer this version as an alternate)
// TODO: Implement aligned SIMD sum, since memory alignment is critical for SIMD instructions. So, do scalar first until we are SIMD aligned and then do SIMD, followed by more scarlar to finish all
//       left over elements that are not SIMD size divisible. First simple step is to check alignment of SIMD portion of the sum. See cache-aligned entry below, which may solve this problem.
// TODO: Implement a cache-aligned divide-and-conquer split. This is very useful for more consitent performance for SIMD/SSE .Sum() and other operations, and is fundamental to improve consistency of performance - i.e.
//       reduce veriability in performance, since SIME/SSE performs better when each SSE instruction is aligned in memory on the size of the instruction boundary (e.g. 512-bit instruction performs better when it's aligned
//       on 512-bit/64-byte boundary, which is a cache-line boundary).
// TODO: Develop a method to split an array on a cache line (64 byte) boundary. Make it public, since it will be useful in many cases.
// TODO: Implement a for loop instead of divide-and-conquer, since they really accomplish the same thing, and the for loop will be more efficient and easier to make cache line boundary divisible.
//       Combining will be slightly harder, but we could create an array of sums, where each task has its own array element to fill in, then we combine all of the sums at the end serially.
// TODO: Implement nullable versions of Sum, only faster than the standard C# ones. Should be able to still use SSE and multi-core, but need to check out each element for null before adding it. These
//       will be much slower than non-nullable
// TODO: See if SSEandScalar version is faster when the array is entirely inside the cache, to make sure that it's not just being memory bandwidth limited and hiding ILP speedup. Port it to C++ and see
//       if it speeds up. Run many times over the same array using .Sum() and provide the average and minimum timing.
// TODO: Implement Neumaier .Sum() using SIMD/SSE including handling of the if/conditional, as it is possible.
// TODO: Return a tupple from each parallel Neumaier result and figure out how to combine these results for a more accurate and possibly perfect overall result that will match serial array processing result.
// TODO: It may be simpler to do a parallelFor style parallelism for parallel Neumaier, where we process chunks in parallel but in order and then combine the results from these chunks in the same order, as
//       if it was done serially in a serial for loop.
// TODO: Implement the divide-and-conquer method for simple floating-point additions, since that increases accuracy O(longN) error, and lower additional performance overhead (potentially). If it doesn't turn
//       out then make an argument on Wikipedia and blog about it.
// TODO: Since C# has support for BigInteger data type in System.Numerics, then provide accurate .Sum() all the way to these for decimal[], float[] and double[]. Basically, provide a consistent story for .Sum() where every type can be
//       summed with perfect accuracy when needed. Make sure naming of functions is consistent for all of this and across all data types, multi-core and SSE implementations, to make it simple, logical and consistent to use.
// TODO: Implement .Sum() for summing a field of Objects/UserDefinedTypes, if it's possible to add value by possibly aggregating elements into a local array of Vector size and doing an SSE sum. Is it possible to abstract it well and to
//       perform well to support extracting all numeric data types, so that performance and abstraction are competitive and as simple or simpler than Linq?
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
        public static long SumSse(this sbyte[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        public static long SumSse(this sbyte[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseInner(start, start + length - 1);
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

        public static ulong SumSse(this byte[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        public static ulong SumSse(this byte[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseInner(start, start + length - 1);
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

        public static long SumSse(this short[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        public static long SumSse(this short[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseInner(start, start + length - 1);
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

        public static ulong SumSse(this ushort[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        public static ulong SumSse(this ushort[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseInner(start, start + length - 1);
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

        public static long SumSse(this int[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        public static long SumSse(this int[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseInner(start, start + length - 1);
        }

        private static long SumSseInner(this int[] arrayToSum, int l, int r)
        {
            var sumVectorLower = new Vector<long>();
            var sumVectorUpper = new Vector<long>();
            var longLower      = new Vector<long>();
            var longUpper      = new Vector<long>();
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

        public static ulong SumSse(this uint[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        public static ulong SumSse(this uint[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseInner(start, start + length - 1);
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

        public static long SumSse(this long[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        public static long SumSse(this long[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseInner(start, start + length - 1);
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

        public static ulong SumSse(this ulong[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        public static ulong SumSse(this ulong[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseInner(start, start + length - 1);
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

        public static double SumSse(this float[] arrayToSum)
        {
            return arrayToSum.SumSseInner(0, arrayToSum.Length - 1);
        }

        public static double SumSse(this float[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseInner(start, start + length - 1);
        }

        private static double SumSseInner(this float[] arrayToSum, int l, int r)
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

        private static double SumSseNeumaierInner(this double[] arrayToSum, int l, int r)
        {
            var sumVector = new Vector<double>();
            var cVector   = new Vector<double>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<double>.Count) * Vector<double>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<double>.Count)
            {
                var inVector = new Vector<double>(arrayToSum, i);
                var tVector = sumVector + inVector;
                Vector<long> gteMask = Vector.GreaterThanOrEqual(Vector.Abs(sumVector), Vector.Abs(inVector));  // if true then 0xFFFFFFFFFFFFFFFFL else 0L at each element of the Vector<long> 
                // Several ways to implement:
                // 1. to select which arguments are chosen for the first +, which requires selection for two arguments
                // 2. to select which arguments are chosen for the last  +, which requires two computational paths, followed by selection of one argument
                // Should implement both ways, since there are only two and choose the highest performing method.
#if false
                var ifThenResult = sumVector - tVector + inVector;
                var ifElseResult = inVector  - tVector + sumVector;
                cVector += Vector.ConditionalSelect(gteMask, ifThenResult, ifElseResult);
#else
                cVector += Vector.ConditionalSelect(gteMask, sumVector, inVector) - tVector + Vector.ConditionalSelect(gteMask, inVector, sumVector);
#endif
                //if (Math.Abs(sum) >= Math.Abs(arrayToSum[i]))
                //    c += (sum - t) + arrayToSum[i];         // If sum is bigger, low-order digits of input[i] are lost.
                //else
                //    c += (arrayToSum[i] - t) + sum;         // Else low-order digits of sum are lost

                sumVector = tVector;
            }
// TODO: This portion needs to be Neumaier algorithm too, all the way down
            double overallSum = 0;
            for (; i <= r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<double>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        private static long SumSseAndScalar(this int[] arrayToSum, int l, int r)
        {
            const int numScalarOps = 2;
            var sumVectorLower = new Vector<long>();
            var sumVectorUpper = new Vector<long>();
            var longLower      = new Vector<long>();
            var longUpper      = new Vector<long>();
            int lengthForVector = (r - l + 1) / (Vector<int>.Count + numScalarOps) * Vector<int>.Count;
            int numFullVectors = lengthForVector / Vector<int>.Count;
            long partialSum0 = 0;
            long partialSum1 = 0;
            int i = l;
            int numScalarAdditions = (arrayToSum.Length - numFullVectors * Vector<int>.Count) / numScalarOps;
            int numIterations = System.Math.Min(numFullVectors, numScalarAdditions);
            int scalarIndex = l + numIterations * Vector<int>.Count;
            int sseIndexEnd = scalarIndex;
            //System.Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", arrayToSum.Length, lengthForVector, numFullVectors, numScalarAdditions, numIterations, scalarIndex);
            for (; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVector = new Vector<int>(arrayToSum, i);
                Vector.Widen(inVector, out longLower, out longUpper);
                partialSum0      += arrayToSum[scalarIndex++];          // interleave SSE and Scalar operations
                sumVectorLower   += longLower;
                partialSum1      += arrayToSum[scalarIndex++];
                sumVectorUpper   += longUpper;
            }
            for (i = scalarIndex; i <= r; i++)
                partialSum0 += arrayToSum[i];
            partialSum0    += partialSum1;
            sumVectorLower += sumVectorUpper;
            for (i = 0; i < Vector<long>.Count; i++)
                partialSum0 += sumVectorLower[i];
            return partialSum0;
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
                () => { sumLeft = SumSseParInner(arrayToSum, l, m); },
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
                () => { sumLeft = SumSseParInner(arrayToSum, l, m); },
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
                () => { sumLeft = SumSseParInner(arrayToSum, l, m); },
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
                () => { sumLeft = SumSseParInner(arrayToSum, l, m); },
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
                () => { sumLeft  = SumSseParInner(arrayToSum, l, m); },
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

        private static double SumNeumaierParInner(this float[] arrayToSum, int l, int r)
        {
            double sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return HPCsharp.Algorithm.SumNeumaier(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            double sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumNeumaierParInner(arrayToSum, l,     m); },
                () => { sumRight = SumNeumaierParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return HPCsharp.Algorithm.SumNeumaier(sumLeft, sumRight);
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
                return HPCsharp.Algorithm.SumNeumaier(arrayToSum, l, r - l + 1);

            int m = (r + l) / 2;

            double sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumNeumaierParInner(arrayToSum, l,     m); },
                () => { sumRight = SumNeumaierParInner(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            return HPCsharp.Algorithm.SumNeumaier(sumLeft, sumRight);
        }

        private static decimal SumParInner(this decimal[] arrayToSum, int l, int r)
        {
            decimal sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return HPCsharp.Algorithm.SumHpc(arrayToSum, l, r - l + 1);

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
                return HPCsharp.Algorithm.SumDecimalHpc(arrayToSum, l, r - l + 1);

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
                return HPCsharp.Algorithm.SumDecimalHpc(arrayToSum, l, r - l + 1);

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

        public static double SumSsePar(this float[] arrayToSum)
        {
            return SumSseParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }
        
        public static double SumSsePar(this float[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumSseParInner(start, start + length - 1);
        }
        public static double SumNeumaierPar(this float[] arrayToSum)
        {
            return SumNeumaierParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static double SumNeumaierPar(this float[] arrayToSum, int start, int length)
        {
            return arrayToSum.SumNeumaierParInner(start, start + length - 1);
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
