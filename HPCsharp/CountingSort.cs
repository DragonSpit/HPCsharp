// TODO: Improve speed further for sorting small arrays by allocating memory on the stack if possible to use safely, such as spans.
// TODO: Provide documentation explaining usage and variations
// TODO: Add unit tests for 0 length to 100 length to random, to make sure it works for 0 and 1 and less than 32 lengths when using FillSse, since that one is tricky
// TODO: Create a generic version of Counting Sort where the Fill function is passed in, so that we can easily switch between these implementations
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

            int[] counts = inputArray.Histogram();

            int startIndex = 0;
            for (uint countIndex = 0; countIndex < counts.Length; countIndex++)
            {
                //sortedArray.FillUsingBlockCopy((byte)countIndex, startIndex, counts[countIndex]);
                sortedArray.FillSse((byte)countIndex, startIndex, counts[countIndex]);
                startIndex += counts[countIndex];
            }

            return sortedArray;
        }

        public static void SortCountingInPlace(this byte[] arrayToSort)
        {
            int[] counts = arrayToSort.Histogram();

            int startIndex = 0;
            for (uint countIndex = 0; countIndex < counts.Length; countIndex++)
            {
                //arrayToSort.FillUsingBlockCopy((byte)countIndex, startIndex, counts[countIndex]);
                arrayToSort.FillSse((byte)countIndex, startIndex, counts[countIndex]);
                startIndex += counts[countIndex];
            }
        }

        public static byte[] SortCountingInPlaceFunctional(this byte[] arrayToSort)
        {
            arrayToSort.SortCountingInPlace();
            return arrayToSort;
        }

        public static ushort[] SortCounting(this ushort[] inputArray)
        {
            ushort[] sortedArray = new ushort[inputArray.Length];

            int[] counts = inputArray.Histogram();

            int startIndex = 0;
            for (uint countIndex = 0; countIndex < counts.Length; countIndex++)
            {
                sortedArray.FillUsingBlockCopy((ushort)countIndex, startIndex, counts[countIndex]);
                startIndex += counts[countIndex];
            }

            return sortedArray;
        }

        public static void SortCountingInPlace(this ushort[] arrayToSort)
        {
            int[] counts = arrayToSort.Histogram();

            int startIndex = 0;
            for (uint countIndex = 0; countIndex < counts.Length; countIndex++)
            {
                arrayToSort.FillUsingBlockCopy((ushort)countIndex, startIndex, counts[countIndex]);
                startIndex += counts[countIndex];
            }
        }

        public static ushort[] SortCountingInPlaceFunctional(this ushort[] arrayToSort)
        {
            arrayToSort.SortCountingInPlace();
            return arrayToSort;
        }

    }
}
