// Explanation and Reasoning:
// This file implements an in-place Radix Selection algorithm using the Most Significant Digit (MSD) approach for unsigned integers (uint).
// It provides linear order and in-place operation for the Selection algorithm. It is possible because only one bin
// or half is needed (for QuickSelect), while elements in other bins (or the other half) can be ignored or thrown away, which is not the case with sorting algorithms,
// where all bins (or both halves) are sorted.
// TODO: Improve the algorithm by doing the count for the next digit while moving elements into the bin that contains the k-th smallest element.

using System;

namespace HPCsharp
{
    public static partial class Algorithm
    {
        // Move elements outside the k-th bin, the bin that k is in, into the k-th bin
        private static int RadixSelectiontMsdParUIntScan(uint[] a, int startOfOb, int lengthOfOb, int startOfKthBin, int lengthOfKthBin,
                                                         int shiftRightAmount, uint bitMask, int kthBin)
        {
            int _current_ob = startOfOb, _current_ib = startOfKthBin, found_ob; // _ob = outside of bin, _ib = inside of bin
            while (true)
            {
                int endOfKthBin = startOfKthBin + lengthOfKthBin - 1;
                int endOfOb = startOfOb + lengthOfOb - 1;
                // Look for the element that belongs in the bin that k is in, to move into that bin
                for (found_ob = 0; _current_ob <= endOfOb; _current_ob++)
                    if (((a[_current_ob] >> shiftRightAmount) & bitMask) == kthBin) { found_ob = 1; break; }
                // Look for the first location in the bin that k is in, which has an element that does not belong in that bin
                if (found_ob == 1)
                    for (; _current_ib <= endOfKthBin; _current_ib++)
                        if (((a[_current_ib] >> shiftRightAmount) & bitMask) != kthBin) break;

                if (_current_ob > endOfOb || _current_ib > endOfKthBin) break; // The bin that k is in is full or all the element outside the bin have been exhausted
                a[_current_ib++] = a[_current_ob++];    // Move the element that belongs in the bin into the bin
            }
            return _current_ib;
        }

        private static void RadixSelectiontMsdInner(uint[] a, int first, int length, int shiftRightAmount, int k)
        {
            int last = first + length - 1;
            const uint bitMask = PowerOfTwoRadix - 1;

            var count = HPCsharp.Algorithm.HistogramOneByteComponent(a, first, last, shiftRightAmount);

            var startOfBin = new int[PowerOfTwoRadix + 1];
            startOfBin[0] = first; startOfBin[PowerOfTwoRadix] = last + 1;
            for (int i = 1; i < PowerOfTwoRadix; i++)
                startOfBin[i] = startOfBin[i - 1] + count[i - 1];

            // Determine which bin contains the k-th smallest element. kthBin will hold the bin number.
            int kthBin = 0, _current_ib;
            for (; kthBin < PowerOfTwoRadix; kthBin++)
            {
                int binLength = startOfBin[kthBin + 1] - startOfBin[kthBin];
                if (binLength == 0) continue; // skip empty bins
                if (k >= startOfBin[kthBin] && k <= (startOfBin[kthBin + 1] - 1)) break;
            }
            _current_ib = RadixSelectiontMsdParUIntScan(a, first, startOfBin[kthBin] - first, startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin],
                                                           shiftRightAmount, bitMask, kthBin);
            _current_ib = RadixSelectiontMsdParUIntScan(a, startOfBin[kthBin + 1], last - startOfBin[kthBin + 1] + 1, _current_ib, startOfBin[kthBin + 1] - _current_ib,
                                                           shiftRightAmount, bitMask, kthBin);

            if (shiftRightAmount > 0)          // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else shiftRightAmount = 0;
                // Only recurse into the bin that contains the k-th smallest element and if more than one element is in that bin
                if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) > 1)
                    RadixSelectiontMsdInner(a, startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], shiftRightAmount, k);
                else if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) == 1) return; // Only one element in the bin that k is in, so it must be the k-th smallest element
                else throw new Exception("RadixSelectiontMsdInner: No elements in the bin that k is in, which should never happen");
            }
        }
        /// <summary>
        /// In-place Radix Selection (Most Significant Digit).
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be selected from in place</param>
        /// <param name="start">starting index of the subarray</param>
        /// <param name="length">length of the subarray</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static uint SelectRadixMsd(this uint[] arrayToBeSelected, Int32 start, Int32 length, Int32 k)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (start < 0 || length <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < start || k > (start + arrayToBeSelected.Length))
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            RadixSelectiontMsdInner(arrayToBeSelected, start, length, shiftRightAmount, k);
            return arrayToBeSelected[k];
        }
        /// <summary>
        /// In-place Radix Selection (Most Significant Digit).
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be sorted in place</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static uint SelectRadixMsd(this uint[] arrayToBeSelected, Int32 k)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (arrayToBeSelected.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBeSelected.Length), "array length is invalid");
            if (k < 0 || k > arrayToBeSelected.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            RadixSelectiontMsdInner(arrayToBeSelected, 0, arrayToBeSelected.Length, shiftRightAmount, k);
            return arrayToBeSelected[k];
        }

        // Process 16-bit digits at a time, since the count array fits in modern CPU cache.
        private static void RadixSelectiontMsdWordInner(uint[] a, int first, int length, int shiftRightAmount, int k)
        {
            int last = first + length - 1;
            const int PowerOfTwoRadix_loc = 256 * 256;
            const int Log2ofPowerOfTwoRadix_loc = 16;
            const uint bitMask = PowerOfTwoRadix_loc - 1;

            var count = HPCsharp.Algorithm.HistogramOneWordComponent(a, first, last, shiftRightAmount);

            var startOfBin = new int[PowerOfTwoRadix_loc + 1];
            startOfBin[0] = first; startOfBin[PowerOfTwoRadix_loc] = last + 1;
            for (int i = 1; i < PowerOfTwoRadix_loc; i++)
                startOfBin[i] = startOfBin[i - 1] + count[i - 1];

            // Determine which bin contains the k-th smallest element. kthBin will hold the bin number.
            int kthBin = 0;
            for (; kthBin < PowerOfTwoRadix_loc; kthBin++)
            {
                int binLength = startOfBin[kthBin + 1] - startOfBin[kthBin];
                if (binLength == 0) continue; // skip empty bins
                if (k >= startOfBin[kthBin] && k <= (startOfBin[kthBin + 1] - 1)) break;
            }
            // TODO: Turn the two while loops into a function, since they do the same work
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
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix_loc) shiftRightAmount -= Log2ofPowerOfTwoRadix_loc;
                else shiftRightAmount = 0;
                // Only recurse into the bin that contains the k-th smallest element and if more than one element is in that bin
                if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) > 1)
                    RadixSelectiontMsdWordInner(a, startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], shiftRightAmount, k);
                else if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) == 1) return; // Only one element in the bin that k is in, so it must be the k-th smallest element
                else throw new Exception("RadixSelectiontMsdInner: No elements in the bin that k is in, which should never happen");
            }
        }
        /// <summary>
        /// In-place Radix Selection (Most Significant Digit).
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be selected from in place</param>
        /// <param name="start">starting index of the subarray</param>
        /// <param name="length">length of the subarray</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static uint SelectRadixMsdWord(this uint[] arrayToBeSelected, Int32 start, Int32 length, Int32 k)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (start < 0 || length <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < start || k > (start + arrayToBeSelected.Length))
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            const int Log2ofPowerOfTwoRadix_loc = 16;
            int shiftRightAmount = (sizeof(uint) / 2 * 16) - Log2ofPowerOfTwoRadix_loc;
            RadixSelectiontMsdWordInner(arrayToBeSelected, start, length, shiftRightAmount, k);
            return arrayToBeSelected[k];
        }
        /// <summary>
        /// In-place Radix Selection (Most Significant Digit).
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be sorted in place</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static uint SelectRadixMsdWord(this uint[] arrayToBeSelected, Int32 k)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (arrayToBeSelected.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBeSelected.Length), "array length is invalid");
            if (k < 0 || k > arrayToBeSelected.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            const int Log2ofPowerOfTwoRadix_loc = 16;
            int shiftRightAmount = (sizeof(uint) / 2 * 16) - Log2ofPowerOfTwoRadix_loc;
            RadixSelectiontMsdWordInner(arrayToBeSelected, 0, arrayToBeSelected.Length, shiftRightAmount, k);
            return arrayToBeSelected[k];
        }
    }

    public static partial class ParallelAlgorithm
    {
        private static void RadixSelectiontMsdParUIntInner(uint[] a, int first, int length, int shiftRightAmount, int k, int parallelThreshold = 16384)
        {
            int last = first + length - 1;
            const uint bitMask = PowerOfTwoRadix - 1;

            var count = HPCsharp.ParallelAlgorithm.HistogramOneByteComponentPar(a, first, last, shiftRightAmount, parallelThreshold);

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
            // TODO: Turn the two while loops into a function, since they do the same work
            // Process elements outside the bin that k is in, which are to the left of that bin
            int _current_ob = first, _current_ib = startOfBin[kthBin], found_ob; // _ob = outside of bin, _ib = inside of bin
            while (true)
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
            // Process elements outside the bin that k is in, which are to the right of that bin
            _current_ob = startOfBin[kthBin + 1]; _current_ib = startOfBin[kthBin];
            while (true)
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
                    RadixSelectiontMsdParUIntInner(a, startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], shiftRightAmount, k);
                else if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) == 1) return; // Only one element in the bin that k is in, so it must be the k-th smallest element
                else throw new Exception("RadixSelectiontMsdInner: No elements in the bin that k is in, which should never happen");
            }
        }
        /// <summary>
        /// In-place Radix Selection (Most Significant Digit).
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be selected from in place</param>
        /// <param name="start">starting index of the subarray</param>
        /// <param name="length">length of the subarray</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static uint SelectRadixMsd(this uint[] arrayToBeSelected, Int32 start, Int32 length, Int32 k, int parallelThreshold = 100000)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (start < 0 || length <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < start || k > (start + arrayToBeSelected.Length))
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            RadixSelectiontMsdParUIntInner(arrayToBeSelected, start, length, shiftRightAmount, k, parallelThreshold);
            return arrayToBeSelected[k];
        }
        /// <summary>
        /// In-place Radix Selection (Most Significant Digit).
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be sorted in place</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static uint SelectRadixMsd(this uint[] arrayToBeSelected, Int32 k, int parallelThreshold = 100000)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (arrayToBeSelected.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBeSelected.Length), "array length is invalid");
            if (k < 0 || k > arrayToBeSelected.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            RadixSelectiontMsdParUIntInner(arrayToBeSelected, 0, arrayToBeSelected.Length, shiftRightAmount, k, parallelThreshold);
            return arrayToBeSelected[k];
        }
    }
}
