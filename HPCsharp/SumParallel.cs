// TODO: Implement Sum algorithm for basic data types that not only uses multi-core, but also uses SIMD instructions, because that's a fun one as it can harness
//       data parallelism and multi-threading parallelism, and is a commonly used function. Can also harness computational unit parallelism (scalar and SIMD) in parallel.
//       and ILP. Average can be easily implemented too, since that depends on sum. Then some of the basic statistics could also be accelerated.using System;
// TODO: Provide a function to sum a field within a user defined type
// TODO: Sum should also provide two types: one for the data types being sumed and the other data type of the sum. For instance, sum up an array of longs, but use a double as the sum to not overflow.
//       Or, sum up an array of int32's, but use int64 for the sum to not overflow. Would decimal accumulator support summing up an array of long's without overflow and providing a perfectly accurate result?
//       Since decimal uses a 96-bit mantissa and 32-bit sign/exponent, it may be true that accumulating an array, which is limited to 32-bit index, and 64-bit elements would sum up perfectly without an overflow ever.
//       This could be easy to test on a machine with 64 GB of system memory, since 2 GigaElement array of 16-byte (decimal) elements would use 32 GBytes just for the array. Then we would fill the array with Int64.MaxValue
//       and see if the decimal accumulator overflows. Do the same for ulong array. Even using a smaller array will prove that it's more accurate, since we are summing Int64.MaxValues
// TODO: Implement aligned SIMD sum, since memory alignment is critical for SIMD instructions. So, do scalar first until we are SIMD aligned and then do SIMD, followed by more scarlar to finish all
//       left over elements that are not SIMD size divisible. First simple step is to check alignment of SIMD portion of the sum.
// TODO: Contribute to Sum C# stackoverflow page, since nobody considered overflow condition and using a larger range values for sum. Also, to https://stackoverflow.com/questions/9987560/comparing-sum-methods-in-c-sharp which
//       asks for a faster implementation, so we can point to SSE and multi-core implemenations.
// TODO: Develop a method to split an array on a cache line (64 byte) boundary. Make it public.
// TODO: Implement a for loop instead of divide-and-conquer, since they really accomplish the same thing, and the for loop will be more efficient and easier to make cache line boundary divisible.
//       Combining will be slightly harder, but we could create an array of sums, where each task has its own array element to fill in, then we combine all of the sums at the end serially.
// TODO: Implement nullable versions of Sum, only faster than the standard C# ones. Should be able to still use SSE and multi-core, but need to check out each element for null before adding it. These
//       will be much slower than non-nullable
// TODO: See if SSEandScalar version is faster when the array is entirely inside the cache, to make sure that it's not just being memory bandwidth limited and hiding ILP speedup. Port it to C++ and see
//       if it speeds up. Run many times over the same array using .Sum() and provide the average and minimum timing.
// TODO: Implement SSE versions for all numeric integer (signed and unsigned) including 64-bit ones.
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
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

        public static long SumSsePar(this int[] arrayToSum)
        {
            return SumSseParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static long SumSsePar(this int[] arrayToSum, int start, int length)
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

        public static ulong SumSsePar(this uint[] arrayToSum)
        {
            return SumSseParInner(arrayToSum, 0, arrayToSum.Length - 1);
        }

        public static ulong SumSsePar(this uint[] arrayToSum, int start, int length)
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
