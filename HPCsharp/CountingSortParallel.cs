﻿// TODO: Improve speed further for sorting small arrays by allocating memory on the stack if possible to use safely, such as spans.
// TODO: Figure out why the parallel Counting Sort varies in performance so wildly, but is so increadibly fast too at its peak. Is it false sharing of cache lines? To where we need
//       to divide-and-conquer on cache line boundary between tasks? Or, page boundary?
// TODO: Implement a partial integer Counting Sort - i.e. allow not just a 16-bit short-size arbitrary cutoff for Counting Sort, but let's say allow 24-bits or some number of
//       value range that is larger than 16-bits, because it should still pay off and allow us to keep raising applicability of Counting Sort as CPU cache sizes grow and memory size grows.
// TODO: Find out where the performance of Counting Sort (serial and parallel) falls off below Radix Sort - i.e. how many bits to sort on at once?
// TODO: These versions of Counting Sort could be in-place.

#pragma warning disable CA1510

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
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
            byte[] sortedArray = new byte[inputArray.Length];

            int[] counts = inputArray.HistogramPar();

            int startIndex = 0;
            for (uint countIndex = 0; countIndex < counts.Length; countIndex++)
            {
                sortedArray.FillPar((byte)countIndex, startIndex, counts[countIndex]);
                startIndex += counts[countIndex];
            }

            return sortedArray;
        }

        // Faster than not-in-place version!
        public static void SortCountingInPlacePar(this byte[] arrayToSort)
        {
            int[] counts = arrayToSort.HistogramPar();

            int startIndex = 0;
            for (uint countIndex = 0; countIndex < counts.Length; countIndex++)
            {
                // TODO: Fill needs to be a parallel function to be done in parallel, with each value done in parallel and within each value as well
                // Then use both of these combined to show that full memory bandwidth can be achieved in C# and C++. In C++ we can use memset() to do SSE
                //arrayToSort.FillUsingBlockCopy((byte)countIndex, startIndex, counts[countIndex]);   // faster than .Fill() by several times
                //arrayToSort.Fill((byte)countIndex, startIndex, counts[countIndex]);
                arrayToSort.FillPar((byte)countIndex, startIndex, counts[countIndex]);
                startIndex += counts[countIndex];
            }
        }

        public static byte[] SortCountingInPlaceFuncPar(this byte[] arrayToSort)
        {
            arrayToSort.SortCountingInPlacePar();
            return arrayToSort;
        }

        public static ushort[] SortCountingPar(this ushort[] inputArray)
        {
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
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

        private static ushort[] SortCountingInPlaceFuncPar(this ushort[] arrayToSort)
        {
            arrayToSort.SortCountingInPlacePar();
            return arrayToSort;
        }
    }
}
