// TODO: Implement in a stable way by using SelectRadix as the starting point and then keep the elements which are < k-th bin on the left of k-th bin and move elements which are > k-th bin on the right of k-th bin.
// TODO: Implement numpy.partition like functionality where the k-th element is an actual value of array[k] instead of an index. (Note: The current implementation works like C++'s nth_element API).
// TODO: Implement a k[] array verstion of the partitioning, as the current PartitionRadixMsdUIntInner method most likely already supports this, but it needs to be verified and tested.
// TODO: Instead of iterating over all bins, we can iterate only over the unique digits present in kDigits to optimize the recursion.
//       One way to do this is to sort kDigits and then iterate over the unique values, or use a HashSet to get unique digits.
//       This may only be possible for k[] array of values, but not for k[] array of indexes, since it's unkown which bin the k[] indexes will fall into.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HPCsharp
{
    public static partial class Algorithm
    {
        private static void PartitionRadixBasedOnElementsMsdUIntInner(uint[] a, int start, int length, int shiftRightAmount, uint[] kPartitionElements, int kPartitionStart, int kPartitionLength, Action<uint[], int, int> baseCaseInPlaceSort, int threshold = 1024)
        {
            if (length < threshold)
            {
                baseCaseInPlaceSort(a, start, length);
                return;
            }
            int last = start + length - 1;
            const uint bitMask = PowerOfTwoRadix - 1;

            var count = HPCsharp.Algorithm.HistogramOneByteComponent(a, start, last, shiftRightAmount);

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin = new int[PowerOfTwoRadix];
            int nextBin = 1;
            startOfBin[0] = endOfBin[0] = start; startOfBin[PowerOfTwoRadix] = -1;         // sentinal
            for (int i = 1; i < PowerOfTwoRadix; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];

            for (int _current = start; _current <= last;)
            {
                uint digit;
                uint current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                while (endOfBin[digit = (current_element >> shiftRightAmount) & bitMask] != _current)
                    (a[endOfBin[digit]], current_element) = (current_element, a[endOfBin[digit]++]);
                a[_current] = current_element;

                endOfBin[digit]++;
                while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                _current = endOfBin[nextBin - 1];
            }
            if (shiftRightAmount > 0)          // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else shiftRightAmount = 0;

                // TODO: Simplification idea: Instead of iterating over all bins, we can iterate only over the unique digits present in kPartitionDigits
                // TODO: One way to do this is to sort kPartitionElements and then iterate over the result.
                // Recurse only into a bin which contains one or more of the kPartitionElements[] elements
                for (int bin = 0, partitionElementsIndex = kPartitionStart; bin < PowerOfTwoRadix && partitionElementsIndex < (kPartitionStart + kPartitionLength); bin++)
                {
                    if (startOfBin[bin] <= endOfBin[bin])   // only non-empty bins
                    {
                        if (((kPartitionElements[partitionElementsIndex] >> shiftRightAmount) & bitMask) == bin)
                        {
                            int kNewStart = partitionElementsIndex++;
                            int kNewLength = 1; // at least one of the kPartitionElements[] elements is in this bin. Determine if more kPartitionElements[] element are in this bin, and pass them into the recursive call for this bin.
                            while (partitionElementsIndex < (kPartitionStart + kPartitionLength) && ((kPartitionElements[partitionElementsIndex] >> shiftRightAmount) & bitMask) == bin)
                            {
                                kNewLength++;
                                partitionElementsIndex++;
                            }
                            PartitionRadixBasedOnElementsMsdUIntInner(a, startOfBin[bin], endOfBin[bin] - startOfBin[bin], shiftRightAmount, kPartitionElements, kNewStart, kNewLength, baseCaseInPlaceSort);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// In-place Partition using Radix by the elements at k[]-th indexes in an array. Not a stable algorithm.
        /// </summary>
        /// <param name="arrayToBePartitioned">array that is to be selected from in place</param>
        /// <param name="kPartitionIndices">array of indexes of the desired elements to be partitioned by, in ascending order</param>
        /// <param name="threshold">for array size smaller than threshold Array.Sort will be used instead of MSD Radix Sort like algorithm</param>
        public static void PartitionByElementRadix(this uint[] arrayToBePartitioned, Int32[] kPartitionIndices, Int32 threshold = 1024)
        {
            if (arrayToBePartitioned == null)
                throw new ArgumentNullException(nameof(arrayToBePartitioned));
            if (kPartitionIndices == null)
                throw new ArgumentNullException(nameof(kPartitionIndices));
            if (arrayToBePartitioned.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBePartitioned.Length), "array length is invalid");
            if (kPartitionIndices.Length > arrayToBePartitioned.Length)
                throw new ArgumentException("kPartitionIndices array length cannot be greater than the array to be selected length");
            if (kPartitionIndices.Length == 0) return;
            uint[] kPartitionElements = new uint[kPartitionIndices.Length];
            for (int i = 0; i < kPartitionIndices.Length; i++)
            {
                if (kPartitionIndices[i] < 0 || kPartitionIndices[i] >= arrayToBePartitioned.Length)
                    throw new ArgumentOutOfRangeException(nameof(kPartitionIndices), "kPartitionIndices is out of array bounds");
                kPartitionElements[i] = arrayToBePartitioned[kPartitionIndices[i]];
            }
            SortRadixMsd(kPartitionElements);  // Hybrid version which calls a Array.Sort for small arrays
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            PartitionRadixBasedOnElementsMsdUIntInner(arrayToBePartitioned, 0, arrayToBePartitioned.Length, shiftRightAmount, kPartitionElements, 0, kPartitionElements.Length, Array.Sort);
        }
    }
}
