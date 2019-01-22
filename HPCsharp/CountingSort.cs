// TODO: Improve speed further for sorting small arrays by allocating memory on the stack if possible to use safely, such as spans.
// TODO: Provide documentation explaining usage and variations
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
                sortedArray.FillUsingBlockCopy((byte)countIndex, startIndex, counts[countIndex]);
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
                arrayToSort.FillUsingBlockCopy((byte)countIndex, startIndex, counts[countIndex]);
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
