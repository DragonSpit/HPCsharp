// TODO: Implement Sum algorithm for basic data types that not only uses multi-core, but also uses SIMD instructions, because that's a fun one as it can harness
//       data parallelism and multi-threading parallelism, and is a commonly used function. Can also harness computational unit parallelism (scalar and SIMD) in parallel.
//       and ILP. Average can be easily implemented too, since that depends on sum. Then some of the basic statistics could also be accelerated.using System;
// TODO: Provide a function to sum a field within a user defined type
// TODO: Sum should also provide two types: one for the data types being sumed and the other data type of the sum. For instance, sum up an array of longs, but use a double as the sum to not overflow.
//       Or, summ up an array of int32's, but use int64 for the sum to not overflow.
// TODO: Implement aligned SIMD sum, since memory alignment is critical for SIMD instructions. So, do scalar first until we are SIMD aligned and then do SIMD, followed by more scarlar to finish all
//       left over elements that are not SIMD size divisible.
// TODO: Contribute to Sum C# stackoverflow page, since nobody considered overflow condition and using a larger range values for sum
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
        // TODO: use the l to r implementation here to have a single core implementation
        public static long SumSse(this int[] arrayToSum)
        {
            var sumVector = new Vector<long>();
            var longLower = new Vector<long>();
            var longUpper = new Vector<long>();
            int sseLimit = (arrayToSum.Length / Vector<int>.Count) * Vector<int>.Count;
            int i;
            for (i = 0; i < sseLimit; i += Vector<int>.Count)
            {
                var inVector = new Vector<int>(arrayToSum, i);
                Vector.Widen(inVector, out longLower, out longUpper);
                sumVector += longLower + longUpper;
            }
            long overallSum = 0;
            for (; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<long>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        public static long SumSse(this int[] arrayToSum, int l, int r)
        {
            var sumVector0 = new Vector<long>();
            var sumVector1 = new Vector<long>();
            var longLower  = new Vector<long>();
            var longUpper  = new Vector<long>();
            int numFullVectors = ((r - l + 1) / Vector<int>.Count) * Vector<int>.Count;
            int i;
            for (i = l; i < numFullVectors; i += Vector<int>.Count)
            {
                var inVector = new Vector<int>(arrayToSum, i);
                Vector.Widen(inVector, out longLower, out longUpper);
                sumVector0 += longLower;
                sumVector1 += longUpper;
            }
            long overallSum = 0;
            for (; i < r; i++)
                overallSum += arrayToSum[i];
            sumVector0 += sumVector1;
            for (i = 0; i < Vector<int>.Count; i++)
                overallSum += sumVector0[i];
            return overallSum;
        }

        public static long SumSseAndScalar(this int[] arrayToSum, int l, int r)
        {
            const int numScalarOps = 2;
            var sumVector = new Vector<long>();
            var longLower = new Vector<long>();
            var longUpper = new Vector<long>();
            int lengthForVector = (r - l + 1) / (Vector<int>.Count + numScalarOps) * Vector<int>.Count;
            int numFullVectors = (lengthForVector / Vector<int>.Count) * Vector<int>.Count;
            long partialSum0 = 0;
            long partialSum1 = 0;
            int i = l;
            int j = numFullVectors * Vector<int>.Count;
            int numIterations = System.Math.Min(numFullVectors, arrayToSum.Length - numFullVectors * Vector<int>.Count);
            for (; i < numIterations; i += Vector<int>.Count)
            {
                var inVector = new Vector<int>(arrayToSum, i);
                Vector.Widen(inVector, out longLower, out longUpper);
                partialSum0 += arrayToSum[j++];
                sumVector   += longLower;
                partialSum1 += arrayToSum[j++];
                sumVector   += longUpper;
            }
            for (i = j; i < r; i++)
                partialSum0 += arrayToSum[i];
            for (i = 0; i < Vector<int>.Count; i++)
                partialSum0 += sumVector[i];
            partialSum0 += partialSum1;
            return partialSum0;
        }

        public static int ThresholdParallelSum { get; set; } = 1024;

        public static long SumSsePar(int[] arrayToSum, int l, int r)
        {
            long sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSse(arrayToSum, l, r);

            int m = (r + l) / 2;

            long sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSsePar(arrayToSum, l,     m); },
                () => { sumRight = SumSsePar(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            sumLeft += sumRight;
            return sumLeft;
        }

        public static long SumSsePar(int[] arrayToSum)
        {
            return SumSsePar(arrayToSum, 0, arrayToSum.Length - 1);
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
