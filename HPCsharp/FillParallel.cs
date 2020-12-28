// TODO: Use some suggestions from https://stackoverflow.com/questions/1897555/what-is-the-equivalent-of-memset-in-c and also parallelize if bandwidth is not used up by
//       a single core. Hope this works for ushort too.
// TODO: Benchmark Fill() for different data types from byte (8-bits) to ulong (64-bits) to see if all of memory bandwidth can be used up. If not then go parallel.
// TODO: If Fill() benchmarks well for larger data types, then reading the array 64-bits at a time will most likely pay off as well. Yes it does! Great direction to go in.
// TODO: Make sure to mention that .NET core has a Fill method implemented already. Modify my version to have the same interface, and provide a parallel version that's even faster.
// TODO: Benchmark my Fill version on a quad-memory channel system to see how much bandwidth it provides - the fill rate! ;-) kinda like graphics.
// TODO: Create generic versions and data type specific version, since data specific versions seem to be significantly faster than generic.
// TODO: Implement FillSse for ushort
// TODO: For parallel versions reduce false sharing by either splitting the array on cache-line boundaries (this type of a split function would be useful for many algorithms), or
//       create a parallel for loop functionality, where we make an array/list of tasks (I already do this in spots), each one on cache-line boundary and then hand that array to TPL to execute in parallel.
// TODO: For generic implementation of SSE limit the data types to numeric, like generic scalar version does.
// TODO: Benchmark Fill on quad-channel and 8-channel and higher machines in AWS, to see how well these scale.
// TODO: If FillUsingBlockCopy runs faster than scalar Fill, then implement a parallel version using it.

using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
        public static void FillPar<T>(this T[] arrayToFill, T value) where T : struct
        {
            int m = arrayToFill.Length / 2;
            int lengthOf2ndHalf = arrayToFill.Length - m;
            Parallel.Invoke(
                () => { Algorithm.Fill<T>(arrayToFill, value, 0, m              ); },
                () => { Algorithm.Fill<T>(arrayToFill, value, m, lengthOf2ndHalf); }
            );
        }

        public static void FillPar<T>(this T[] arrayToFill, T value, int startIndex, int length) where T : struct
        {
            int m = length / 2;
            int lengthOf2ndHalf = length - m;
            Parallel.Invoke(
                () => { Algorithm.Fill<T>(arrayToFill, value, startIndex,     m              ); },
                () => { Algorithm.Fill<T>(arrayToFill, value, startIndex + m, lengthOf2ndHalf); }
            );
        }

        // TODO: Figure out why these are not scaling well
        private static void FillPar(this byte[] arrayToFill, byte value)
        {
            int m = arrayToFill.Length / 2;
            int lengthOf2ndHalf = arrayToFill.Length - m;
            Parallel.Invoke(
                () => { Algorithm.Fill(arrayToFill, value, 0, m              ); },
                () => { Algorithm.Fill(arrayToFill, value, m, lengthOf2ndHalf); }
            );
        }

        private static void FillPar(this byte[] arrayToFill, byte value, int startIndex, int length)
        {
            int m = length / 2;
            int lengthOf2ndHalf = length - m;
            Parallel.Invoke(
                () => { Algorithm.Fill(arrayToFill, value, startIndex, m); },
                () => { Algorithm.Fill(arrayToFill, value, startIndex + m, lengthOf2ndHalf); }
            );
        }

        public static void FillGenericSse<T>(this T[] arrayToFill, T value) where T : struct
        {
            var fillVector = new Vector<T>(value);
            int numFullVectorsIndex = (arrayToFill.Length / Vector<T>.Count) * Vector<T>.Count;
            int i;
            for(i = 0; i < numFullVectorsIndex; i += Vector<T>.Count)
                fillVector.CopyTo(arrayToFill, i);
            for (; i < arrayToFill.Length; i++)
                arrayToFill[i] = value;
        }

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
