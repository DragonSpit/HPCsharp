// TODO: Use some suggestions from https://stackoverflow.com/questions/1897555/what-is-the-equivalent-of-memset-in-c and also parallelize if bandwidth is not used up by
//       a single core. Hope this works for ushort too.
// TODO: Can we allocate the count array on the stack to make sorting small arrays faster?
// TODO: Benchmark Fill() for different data types from byte (8-bits) to ulong (64-bits) to see if all of memory bandwidth can be used up. If not then go parallel.
using System;
using System.Collections.Generic;
using System.Text;

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

        public static void Fill<T>(this T[] arrayToFill, int startIndex, int length, T value)
        {
            int index    = startIndex;
            int endIndex = startIndex + length;
            for (int i = startIndex; i < endIndex; i++)
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
    }
}
