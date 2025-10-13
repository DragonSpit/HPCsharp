// Explanation and Reasoning:
// This file implements an in-place Radix Selection algorithm using the Most Significant Digit (MSD) approach for unsigned integers (uint).
// It provides linear order and in-place operation for the Selection algorithm. It is possible because only one bin
// or half is needed (for QuickSelect), while elements in other bins (or the other half) can be ignored or thrown away, which is not the case with sorting algorithms.

using System;
using System.Collections;
using System.Collections.Generic;

namespace HPCsharp
{
    public static partial class Algorithm
    {
        private static void RadixSelectiontMsdUIntInner(uint[] a, int first, int length, int shiftRightAmount, int k, Action<uint[], int, int> baseCaseInPlaceSort, int threshold = 1024)
        {
            if (length < threshold)
            {
                baseCaseInPlaceSort(a, first, length);
                return;
            }
            int last = first + length - 1;
            const uint bitMask = PowerOfTwoRadix - 1;

            var count = HistogramOneByteComponent(a, first, last, shiftRightAmount);

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin   = new int[PowerOfTwoRadix];
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadix] = last + 1;
            for (int i = 1; i < PowerOfTwoRadix; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];

            // Determine which bin contains the k-th smallest element. kthBin will hold the bin number.
            int kthBin = 0;
            for (; kthBin < PowerOfTwoRadix; kthBin++)
                if (k < startOfBin[kthBin])
                {
                    kthBin--; break;
                }
            int _current_ob, _current_ib; // _ob = outside of bin, _ib = inside of bin
            while (true) // process elements outside the bin that k is in, which are to the left of that bin
            {
                // Look for the element that belongs in the bin that k is in, to move into that bin
                for (_current_ob = first; _current_ob < startOfBin[kthBin]; _current_ob++)
                    if (((a[_current_ob] >> shiftRightAmount) & bitMask) == kthBin) break;
                // Look for the first location in the bin that k is in, which has an element that does not belong in that bin
                for (_current_ib = startOfBin[kthBin]; _current_ib < startOfBin[kthBin + 1]; _current_ib++)
                    if (((a[_current_ib] >> shiftRightAmount) & bitMask) != kthBin) break;

                a[_current_ib++] = a[_current_ob++];    // Move the element that belongs in the bin into the bin
                if (_current_ib >= startOfBin[kthBin + 1] || _current_ob >= startOfBin[kthBin]) break; // The bin that k is in is full or all the element outside the bin to the left have been exhausted
            }

            while (true) // process elements outside the bin that k is in, which are to the right of that bin
            {
                // Look for the element that belongs in the bin that k is in, to move into that bin
                for (_current_ob = startOfBin[kthBin + 1]; _current_ob <= last; _current_ob++)
                    if (((a[_current_ob] >> shiftRightAmount) & bitMask) == kthBin) break;
                // Look for the first location in the bin that k is in, which has an element that does not belong in that bin
                for (_current_ib = startOfBin[kthBin]; _current_ib < startOfBin[kthBin + 1]; _current_ib++)
                    if (((a[_current_ib] >> shiftRightAmount) & bitMask) != kthBin) break;

                a[_current_ib++] = a[_current_ob++];    // Move the element that belongs in the bin into the bin
                if (_current_ib >= startOfBin[kthBin + 1] || _current_ob > last) break; // The bin that k is in is full or all the element outside the bin to the right have been exhausted
            }

            if (shiftRightAmount > 0)          // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else shiftRightAmount = 0;
                // Only recurse into the bin that contains the k-th smallest element
                RadixSelectiontMsdUIntInner(a, startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], shiftRightAmount, k, baseCaseInPlaceSort);
            }
        }
        /// <summary>
        /// In-place Radix Selection (Most Significant Digit).
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be selected from in place</param>
        /// <param name="threshold">for array size smaller than threshold Array.Sort will be used instead</param>
        public static void SelectRadixMsd(this uint[] arrayToBeSelected, Int32 start, Int32 length, Int32 k, Int32 threshold = 1024)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (start < 0 || length <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < start || k > (start + arrayToBeSelected.Length))
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            // Insertion Sort or Heap Sort could be passed in as another base case since they are both in-place
            RadixSelectiontMsdUIntInner(arrayToBeSelected, start, length, shiftRightAmount, k, Array.Sort, threshold);
            // The following does not work: Need to figure out how to pass InsertionSort method as an Action
            //RadixSortMsdUIntInner(arrayToBeSorted, start, length, shiftRightAmount, (arr, startIndex, lengthOfArray) => InsertionSort(arrayToBeSorted, start, length), threshold);
        }
        /// <summary>
        /// In-place Radix Selection (Most Significant Digit), not a stable sort.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be sorted in place</param>
        /// <param name="threshold">for array size smaller than threshold Array.Sort will be used instead</param>
        public static void SelectRadixMsd(this uint[] arrayToBeSelected, Int32 k, Int32 threshold = 1024)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (arrayToBeSelected.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < 0 || k > arrayToBeSelected.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            // Insertion Sort or Heap Sort could be passed in as another base case since they are both in-place
            RadixSelectiontMsdUIntInner(arrayToBeSelected, 0, arrayToBeSelected.Length, shiftRightAmount, k, Array.Sort, threshold);
            // The following does not work: Need to figure out how to pass InsertionSort method as an Action
            //RadixSortMsdUIntInner(arrayToBeSorted, 0, arrayToBeSorted.Length, shiftRightAmount, (arr, startIndex, lengthOfArray) => { InsertionSort(arrayToBeSorted, 0, arrayToBeSorted.Length); }, threshold);
        }

    }
}
