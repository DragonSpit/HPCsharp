// TODO: Use some suggestions from https://stackoverflow.com/questions/1897555/what-is-the-equivalent-of-memset-in-c and also parallelize if bandwidth is not used up by
//       a single core. Hope this works for ushort too.
// TODO: Can we allocate the count array on the stack to make sorting small arrays faster?
// TODO: Benchmark Fill() for different data types from byte (8-bits) to ulong (64-bits) to see if all of memory bandwidth can be used up. If not then go parallel.
// TODO: If Fill() benchmarks well for larger data types, then reading the array 64-bits at a time will most likely pay off as well. Yes it does! Great direction to go in.
// TODO: Consider using SIMD instructions to read and write even more bits per iteration - e.g. 256-bits is 32 bytes and 512-bits is an entire cache line.
//       https://stackoverflow.com/questions/31999479/using-simd-operation-from-c-sharp-in-net-framework-4-6-is-slower
// TODO: Make sure to mention that .NET core has a Fill method implemented already. Modify my version to have the same interface, and provide a parallel version that's even faster.
// TODO: Benchmark my Fill version on a quad-memory channel system to see how much bandwidth it provides - the fill rate! ;-) kinda like graphics.
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        public static byte[] SortCounting(this byte[] inputArray)
        {
            byte[] sortedArray = new byte[inputArray.Length];

            int[] counts = inputArray.CountValues();

            int startIndex = 0;
            for (uint countIndex = 0; countIndex < counts.Length; countIndex++)
            {
                sortedArray.Fill(startIndex, counts[countIndex], (byte)countIndex);
                startIndex += counts[countIndex];
            }

            return sortedArray;
        }

        public static void SortCountingInPlace(this byte[] arrayToSort)
        {
            int[] counts = arrayToSort.CountValues();

            int startIndex = 0;
            for (uint countIndex = 0; countIndex < counts.Length; countIndex++)
            {
                arrayToSort.Fill(startIndex, counts[countIndex], (byte)countIndex);
                startIndex += counts[countIndex];
            }
        }

        public static int[] CountValues(this byte[] arrayToCount)
        {
            int numberOfBins = 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < arrayToCount.Length; currIndex++)
                counts[arrayToCount[currIndex]]++;

            return counts;
        }

        public static int[] CountValues(this ushort[] arrayToCount)
        {
            int numberOfBins = 256 * 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < arrayToCount.Length; currIndex++)
                counts[arrayToCount[currIndex]]++;

            return counts;
        }

        public static int[] CountValues(this short[] arrayToCount)
        {
            int numberOfBins = 256 * 256;
            int[] counts = new int[numberOfBins];

            // TODO: Since short values are signed and can be negative, we need to figure out a way to handle negative indexes
            for (uint currIndex = 0; currIndex < arrayToCount.Length; currIndex++)
                counts[arrayToCount[currIndex]]++;

            return counts;
        }

        public static void Fill<T>(this T[] arrayToFill, T value)
        {
            for (int i = 0; i < arrayToFill.Length; i++)
                arrayToFill[i] = value;
        }

        public static void FillPar<T>(this T[] arrayToFill, T value)
        {
            int m = arrayToFill.Length / 2;
            int lengthOf2ndHalf = arrayToFill.Length - m;
            Parallel.Invoke(
                () => { Fill<T>(arrayToFill, 0, m,               value); },
                () => { Fill<T>(arrayToFill, m, lengthOf2ndHalf, value); }
            );
        }

        public static void Fill<T>(this T[] arrayToFill, int startIndex, int length, T value)
        {
            int index    = startIndex;
            int endIndex = startIndex + length;
            for (int i = startIndex; i < endIndex; i++)
                arrayToFill[i] = value;
        }

        public static void FillSse(this byte[] arrayToFill, byte value)
        {
            var fillVector = new Vector<byte>(value);
            int numFullVectorsIndex = (arrayToFill.Length / Vector<byte>.Count) * Vector<byte>.Count;
            int i;
            for(i = 0; i < numFullVectorsIndex; i += Vector<byte>.Count)
                fillVector.CopyTo(arrayToFill, i);
            for (; i < arrayToFill.Length; i++)
                arrayToFill[i] = value;
        }
        // From StackOverflow fast fill question https://stackoverflow.com/questions/1897555/what-is-the-equivalent-of-memset-in-c
        public static void MemSetBlockX2(int[] array, int value)
        {
            int block = 32, index = 0;
            int length = Math.Min(block, array.Length);

            while (index < length)          // Fill the initial array
                array[index++] = value;

            length = array.Length;
            while (index < length)
            {
                int actualBlockSize = Math.Min(block, length - index);
                Buffer.BlockCopy(array, 0, array, index << 2, actualBlockSize << 2);
                index += block;
                block *= 2;
            }
        }
        public static void MemSetBlockX2(byte[] array, byte value)
        {
            int block = 32, index = 0;
            int length = Math.Min(block, array.Length);

            while (index < length)          // Fill the initial array
                array[index++] = value;

            length = array.Length;
            while (index < length)
            {
                int actualBlockSize = Math.Min(block, length - index);
                Buffer.BlockCopy(array, 0, array, index, actualBlockSize);
                index += block;
                block *= 2;
            }
        }
    }
}
