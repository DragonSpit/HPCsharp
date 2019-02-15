// TODO: Implement Sum algorithm for basic data types that not only uses multi-core, but also uses SIMD instructions, because that's a fun one as it can harness
//       data parallelism and multi-threading parallelism, and is a commonly used function. Can also harness computational unit parallelism (scalar and SIMD) in parallel.
//       and ILP. Average can be easily implemented too, since that depends on sum. Then some of the basic statistics could also be accelerated.using System;
// TODO: Provide a function to sum a field within a user defined type
// TODO: Sum should also provide two types: one for the data types being sumed and the other data type of the sum. For instance, sum up an array of longs, but use a double as the sum to not overflow.
//       Or, summ up an array of int32's, but use int64 for the sum to not overflow.
// TODO: Implement aligned SIMD sum, since memory alignment is critical for SIMD instructions. So, do scalar first until we are SIMD aligned and then do SIMD, followed by more scarlar to finish all
//       left over elements that are not SIMD size divisible.
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
        public static long SumSseIntToLong(this int[] arrayToSum)
        {
            var sumVector = new Vector<int>();
            int numFullVectors = (arrayToSum.Length / Vector<int>.Count) * Vector<int>.Count;
            int i;
            for (i = 0; i < numFullVectors; i += Vector<int>.Count)
                sumVector += new Vector<int>(arrayToSum, i);
            long overallSum = 0;
            for (; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<int>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        public static long SumSseIntToLong(this int[] arrayToSum, int l, int r)
        {
            var sumVector = new Vector<int>();
            int numFullVectors = ((r - l + 1) / Vector<int>.Count) * Vector<int>.Count;
            int i;
            for (i = l; i < numFullVectors; i += Vector<int>.Count)
                sumVector += new Vector<int>(arrayToSum, i);
            long overallSum = 0;
            for (; i < r; i++)
                overallSum += arrayToSum[i];
            for (i = 0; i < Vector<int>.Count; i++)
                overallSum += sumVector[i];
            return overallSum;
        }

        public static int ThresholdParallelSum { get; set; } = 1024;

        public static long SumSseIntToLongPar(int[] arrayToSum, int l, int r)
        {
            long sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= ThresholdParallelSum)
                return SumSseIntToLong(arrayToSum, l, r);

            int m = (r + l) / 2;

            long sumRight = 0;

            Parallel.Invoke(
                () => { sumLeft  = SumSseIntToLongPar(arrayToSum, l,     m); },
                () => { sumRight = SumSseIntToLongPar(arrayToSum, m + 1, r); }
            );
            // Combine left and right results
            sumLeft += sumRight;
            return sumLeft;
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
