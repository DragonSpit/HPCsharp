// TODO: Improve speed further for sorting small arrays by allocating memory on the stack if possible to use safely, such as spans.
// TODO: Figure out why the parallel Counting Sort varies in performance so wildly, but is so increadibly fast too at its peak. Is it false sharing of cache lines? To where we need
//       to divide-and-conquer on cache line boundary between tasks? Or, page boundary?
// TODO: Implement a partial integer Counting Sort - i.e. allow not just a 16-bit short-size arbitrary cutoff for Counting Sort, but let's say allow 24-bits or some number of
//       value range that is larger than 16-bits, because it should still pay off and allow us to keep raising applicability of Counting Sort as CPU cache sizes grow and memory size grows.
// TODO: Move parallel Histogram algorithm into its own file (HistogramParallel.cs)
// TODO: Implement FillSse for ushort
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
        // Ludicrous speed algorithm!
        public static byte[] SortCountingPar(this byte[] inputArray)
        {
            byte[] sortedArray = new byte[inputArray.Length];

            int[] counts = inputArray.HistogramPar();

            int startIndex = 0;
            for (uint countIndex = 0; countIndex < counts.Length; countIndex++)
            {
                sortedArray.FillSse((byte)countIndex, startIndex, counts[countIndex]);
                startIndex += counts[countIndex];
            }

            return sortedArray;
        }

        // Ludicrous speed algorithm!
        private static void SortCountingInPlacePar(this byte[] arrayToSort)
        {
            int[] counts = arrayToSort.HistogramPar();

            int startIndex = 0;
            for (uint countIndex = 0; countIndex < counts.Length; countIndex++)
            {
                arrayToSort.FillSse((byte)countIndex, startIndex, counts[countIndex]);
                startIndex += counts[countIndex];
            }
        }

        public static ushort[] SortCountingPar(this ushort[] inputArray)
        {
            ushort[] sortedArray = new ushort[inputArray.Length];

            int[] counts = inputArray.HistogramPar();

            int startIndex = 0;
            for (uint countIndex = 0; countIndex < counts.Length; countIndex++)
            {
                sortedArray.FillUsingBlockCopy((ushort)countIndex, startIndex, counts[countIndex]);
                startIndex += counts[countIndex];
            }

            return sortedArray;
        }

        private static void SortCountingInPlacePar(this ushort[] arrayToSort)
        {
            int[] counts = arrayToSort.HistogramPar();

            int startIndex = 0;
            for (uint countIndex = 0; countIndex < counts.Length; countIndex++)
            {
                arrayToSort.FillUsingBlockCopy((ushort)countIndex, startIndex, counts[countIndex]);
                startIndex += counts[countIndex];
            }
        }

        public static Int32 ThresholdByteCountPar { get; set; } = 64 * 1024;

        public static int[] HistogramInnerPar(byte[] inArray, Int32 l, Int32 r)
        {
            int numberOfBins = 256;
            int[] countLeft = new int[numberOfBins];

            if (l > r)      // zero elements to compare
                return countLeft;
            if ((r - l + 1) <= ThresholdByteCount)
            {
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    countLeft[inArray[current]]++;
                return countLeft;
            }

            int m = (r + l) / 2;

            int[] countRight = new int[numberOfBins];

            Parallel.Invoke(
                () => { countLeft  = HistogramInnerPar(inArray, l,     m); },
                () => { countRight = HistogramInnerPar(inArray, m + 1, r); }
            );
            
            for (int j = 0; j < numberOfBins; j++)      // Combine left and right results into a single count/histogram
                countLeft[j] += countRight[j];

            return countLeft;
        }

        public static int[] HistogramPar(this byte[] inArray)
        {
            return HistogramInnerPar(inArray, 0, inArray.Length - 1);
        }

        public static Int32 ThresholdShortCountPar { get; set; } = 64 * 1024;

        public static int[] HistogramInnerPar(ushort[] inArray, Int32 l, Int32 r)
        {
            int numberOfBins = 256 * 256;
            int[] countLeft = new int[numberOfBins];

            if (l > r)      // zero elements to compare
                return countLeft;
            if ((r - l + 1) <= ThresholdShortCountPar)
            {
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    countLeft[inArray[current]]++;
                return countLeft;
            }

            int m = (r + l) / 2;

            int[] countRight = new int[numberOfBins];

            Parallel.Invoke(
                () => { countLeft  = HistogramInnerPar(inArray, l,     m); },
                () => { countRight = HistogramInnerPar(inArray, m + 1, r); }
            );

            for (int j = 0; j < numberOfBins; j++)      // Combine left and right results into a single count/histogram
                countLeft[j] += countRight[j];

            return countLeft;
        }

        public static int[] HistogramPar(this ushort[] inArray)
        {
            return HistogramInnerPar(inArray, 0, inArray.Length - 1);
        }
    }
}
