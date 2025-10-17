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
        private static void RadixSelectiontMsdUIntInner(uint[] a, int first, int length, int shiftRightAmount, int k, int threshold = 1024)
        {
            // TODO: Replace with QuickSelect random version for < threshold
            //if (length < threshold)
            //{
            //    baseCaseInPlaceSort(a, first, length);
            //    return;
            //}
            int last = first + length - 1;
            const uint bitMask = PowerOfTwoRadix - 1;

            var count = HPCsharp.Algorithm.HistogramOneByteComponent(a, first, last, shiftRightAmount);

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin = new int[PowerOfTwoRadix];
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadix] = last + 1;
            for (int i = 1; i < PowerOfTwoRadix; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];

            // Determine which bin contains the k-th smallest element. kthBin will hold the bin number.
            int kthBin = 0;
            for (; kthBin < PowerOfTwoRadix; kthBin++)
            {
                int binLength = startOfBin[kthBin + 1] - startOfBin[kthBin];
                if (binLength == 0) continue; // skip empty bins
                if (k >= startOfBin[kthBin] && k <= (startOfBin[kthBin + 1] - 1)) break;
            }
            int _current_ob = first, _current_ib = startOfBin[kthBin], found_ob; // _ob = outside of bin, _ib = inside of bin
            while (true) // process elements outside the bin that k is in, which are to the left of that bin
            {
                // Look for the element that belongs in the bin that k is in, to move into that bin
                for (found_ob = 0; _current_ob < startOfBin[kthBin]; _current_ob++)
                    if (((a[_current_ob] >> shiftRightAmount) & bitMask) == kthBin) { found_ob = 1; break; }
                // Look for the first location in the bin that k is in, which has an element that does not belong in that bin
                if (found_ob == 1)
                    for (; _current_ib < startOfBin[kthBin + 1]; _current_ib++)
                        if (((a[_current_ib] >> shiftRightAmount) & bitMask) != kthBin) break;

                if (_current_ib >= startOfBin[kthBin + 1] || _current_ob >= startOfBin[kthBin]) break; // The bin that k is in is full or all the element outside the bin to the left have been exhausted
                a[_current_ib++] = a[_current_ob++];    // Move the element that belongs in the bin into the bin
            }
            _current_ob = startOfBin[kthBin + 1]; _current_ib = startOfBin[kthBin];
            while (true) // process elements outside the bin that k is in, which are to the right of that bin
            {
                // Look for the element that belongs in the bin that k is in, to move into that bin
                for (found_ob = 0; _current_ob <= last; _current_ob++)
                    if (((a[_current_ob] >> shiftRightAmount) & bitMask) == kthBin) { found_ob = 1; break; }
                // Look for the first location in the bin that k is in, which has an element that does not belong in that bin
                if (found_ob == 1)
                    for (; _current_ib < startOfBin[kthBin + 1]; _current_ib++)
                        if (((a[_current_ib] >> shiftRightAmount) & bitMask) != kthBin) break;

                if (_current_ib >= startOfBin[kthBin + 1] || _current_ob > last) break; // The bin that k is in is full or all the element outside the bin to the right have been exhausted
                a[_current_ib++] = a[_current_ob++];    // Move the element that belongs in the bin into the bin
            }
            if (shiftRightAmount > 0)          // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else shiftRightAmount = 0;
                // Only recurse into the bin that contains the k-th smallest element and if more than one element is in that bin
                if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) > 1)
                    RadixSelectiontMsdUIntInner(a, startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], shiftRightAmount, k);
                else if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) == 1)
                {
                    return; // Only one element in the bin that k is in, so it must be the k-th smallest element
                }
                else
                {
                    // No elements in the bin that k is in, which should never happen
                    throw new Exception("RadixSelectiontMsdUIntInner: No elements in the bin that k is in, which should never happen");
                }
            }
        }
        /// <summary>
        /// In-place Radix Selection (Most Significant Digit).
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be selected from in place</param>
        /// <param name="threshold">for array size smaller than threshold Array.Sort will be used instead</param>
        public static uint SelectRadixMsd(this uint[] arrayToBeSelected, Int32 start, Int32 length, Int32 k, Int32 threshold = 1024)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (start < 0 || length <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < start || k > (start + arrayToBeSelected.Length))
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            // Insertion Sort or Heap Sort could be passed in as another base case since they are both in-place
            RadixSelectiontMsdUIntInner(arrayToBeSelected, start, length, shiftRightAmount, k, threshold);
            return arrayToBeSelected[k];
        }
        /// <summary>
        /// In-place Radix Selection (Most Significant Digit), not a stable sort.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be sorted in place</param>
        /// <param name="threshold">for array size smaller than threshold Array.Sort will be used instead</param>
        public static uint SelectRadixMsd(this uint[] arrayToBeSelected, Int32 k, Int32 threshold = 1024)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (arrayToBeSelected.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < 0 || k > arrayToBeSelected.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            //Console.WriteLine("Radix Selection:  k = {0}   Array length = {1}", k, arrayToBeSelected.Length);
            // Insertion Sort or Heap Sort could be passed in as another base case since they are both in-place
            RadixSelectiontMsdUIntInner(arrayToBeSelected, 0, arrayToBeSelected.Length, shiftRightAmount, k, threshold);
            return arrayToBeSelected[k];
        }

    }
}
