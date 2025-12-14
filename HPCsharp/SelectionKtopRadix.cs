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
        // Move elements in the region to the right of the k-th bin (Rok) (i.e. greater than k-th bin) that belong in k-th bin into the k-th bin.
        // Swap if the element in the k-th bin belongs to the Rok region.
        // Generic implementation that work for regions to the left or to the right of the k-th bin, and for any digit size.
        private static int MoveRokElementsIntoKthBin(uint[] a, int startOfKthBin, int lengthOfKthBin, int startOfRok, int lengthOfRok, int shiftRightAmount, uint bitMask, int kthBin)
        {
            int endOfKthBin = startOfKthBin + lengthOfKthBin - 1;
            int endOfRok    = startOfRok    + lengthOfRok    - 1;
            int _current_rok = startOfRok, _current_ib = startOfKthBin; // _rok = right of k-th-bin, _ib = inside of bin
            while (true)
            {
                // Look for the element that belongs in the bin that k is in, to move into that bin
                for (; _current_rok <= endOfRok; _current_rok++)
                    if (((a[_current_rok] >> shiftRightAmount) & bitMask) == kthBin) break;
                // Look for the first location in the bin that k is in, which has an element that does not belong to the right of k-th bin
                if (_current_rok <= endOfRok)
                    for (; _current_ib <= endOfKthBin; _current_ib++)
                        if (((a[_current_ib] >> shiftRightAmount) & bitMask) != kthBin) break;

                if (_current_rok > endOfRok || _current_ib > endOfKthBin) break; // All the element outside the bin have been exhausted or the bin that k is in is full or
                if (((a[_current_ib] >> shiftRightAmount) & bitMask) > kthBin)
                    (a[_current_ib], a[_current_rok]) = (a[_current_rok++], a[_current_ib++]);  // Swap
                else
                    a[_current_ib++] = a[_current_rok++];    // Move the element that belongs in the bin into the bin
            }
            return _current_ib;
        }
        // Move elements in the k-th bin which belong in the region to the right of the k-th bin (Rok) (i.e. greater than k-th bin) into the Rok region.
        // Generic implementation that work for regions to the left or to the right of the k-th bin, and for any digit size.
        private static void MoveRokElementsOutOfKthBin(uint[] a, int startOfKthBin, int lengthOfKthBin, int startOfRok, int lengthOfRok, int shiftRightAmount, uint bitMask, int kthBin)
        {
            int endOfKthBin = startOfKthBin + lengthOfKthBin - 1;
            int endOfRok    = startOfRok    + lengthOfRok    - 1;
            int _current_rok = startOfRok, _current_ib = startOfKthBin; // _rok = right of k-th-bin, _ib = inside of bin
            while (true)
            {
                // Look for the element that belongs in the bin that k is in, to move into that bin
                for (; _current_ib <= endOfKthBin; _current_ib++)
                    if (((a[_current_ib] >> shiftRightAmount) & bitMask) > kthBin) break;
                // Look for the first location in the bin that k is in, which has an element that does not belong to the right of k-th bin
                if (_current_ib <= endOfKthBin)
                    for (; _current_rok <= endOfRok; _current_rok++)
                        if (((a[_current_rok] >> shiftRightAmount) & bitMask) < kthBin) break;

                if (_current_rok > endOfRok || _current_ib > endOfKthBin) break; // All the element outside the bin have been exhausted or the bin that k is in is full or
                a[_current_rok++] = a[_current_ib++];   // Move the element that belongs to the right of the k-th bin into that region
            }
        }
        // Move elements outside (to the left of) the k-th bin, the bin that k is in, which belong to the k-th bin, into the k-th bin.
        // Move elements outside (to the left of) the k-th bin, the bin that k is in, which belong to the right of the k-th bin, to the right of the k-th bin.
        // Generic implementation that works for any digit size.
        private static void MoveLobElements(uint[] a, int startOfLob, int lengthOfLob, int startOfKthBin, int lengthOfKthBin, int startOfKp1thBin, int lengthOfRob, int shiftRightAmount, uint bitMask, int kthBin)
        {
            int endOfKthBin = startOfKthBin   + lengthOfKthBin - 1;
            int endOfLob    = startOfLob      + lengthOfLob    - 1;
            int endOfRob    = startOfKp1thBin + lengthOfRob    - 1;
            int _current_lob = startOfLob, _current_ib = startOfKthBin, _current_rob = startOfKp1thBin, found_ib, found_rob; // _lob = outside of bin, _ib = inside of bin, _rob = right outside of bin
            while (true)
            {
                // Look for the element that belongs in the bin that k is in, or belong to the right of the k-th bin
                for (found_ib = 0, found_rob = 0; _current_lob <= endOfLob; _current_lob++)
                {
                    if (((a[_current_lob] >> shiftRightAmount) & bitMask) == kthBin) { found_ib  = 1; break; }
                    if (((a[_current_lob] >> shiftRightAmount) & bitMask) >  kthBin) { found_rob = 1; break; }
                }
                if (_current_lob > endOfLob) break; // All the element outside the bin have been exhausted or the bin that k is in is full or 
                if (found_ib == 1)  // Look for the first location in the bin that k is in, which has an element that does not belong in that bin, which will only be ones less than kthBin
                {
                    for (; _current_ib <= endOfKthBin; _current_ib++)
                        if (((a[_current_ib] >> shiftRightAmount) & bitMask) != kthBin) break;
                    a[_current_ib++] = a[_current_lob++];     // Move the element that belongs in the bin into the bin
                }
                if (found_rob == 1)  // Look for the first location to the right of the bin that k is in, which has an element that does not belong in that region
                {
                    for (; _current_rob <= endOfRob; _current_rob++)
                        if (((a[_current_rob] >> shiftRightAmount) & bitMask) <= kthBin) break;  // Tricky part, as moving element out of a region in previous step doesn't mean they are gone from that region. A copy of them is still there.
                    a[_current_rob++] = a[_current_lob++];    // Move the element that belongs to the right of the k-th bin into that region
                }
            }
        }
        // Process 16-bit digits at a time, since the count array fits in modern CPU cache.
        private static void RadixSelectionKtopWordInner(uint[] a, int first, int length, int shiftRightAmount, int k)
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
            int kthBin = 0, _current_ib;
            for (; kthBin < PowerOfTwoRadix_loc; kthBin++)
            {
                int binLength = startOfBin[kthBin + 1] - startOfBin[kthBin];
                if (binLength == 0) continue; // skip empty bins
                if (k >= startOfBin[kthBin] && k <= (startOfBin[kthBin + 1] - 1)) break;
            }

            _current_ib = MoveRokElementsIntoKthBin(a, startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], startOfBin[kthBin + 1], last - startOfBin[kthBin + 1] + 1, shiftRightAmount, bitMask, kthBin);
            // TODO: use _current_ib to optimize the following call. Use it instead of startOfBin[kthBin]
            MoveRokElementsOutOfKthBin(             a, startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], startOfBin[kthBin + 1], last - startOfBin[kthBin + 1] + 1, shiftRightAmount, bitMask, kthBin);
            MoveLobElements(a, first, startOfBin[kthBin] - first, startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], startOfBin[kthBin + 1], last - startOfBin[kthBin + 1] + 1, shiftRightAmount, bitMask, kthBin);

            if (shiftRightAmount > 0)          // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix_loc) shiftRightAmount -= Log2ofPowerOfTwoRadix_loc;
                else shiftRightAmount = 0;
                // Only recurse into the bin that contains the k-th smallest element and if more than one element is in that bin
                if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) > 1)
                    RadixSelectionKtopWordInner(a, startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], shiftRightAmount, k);
                else if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) == 1) return; // Only one element in the bin that k is in, so it must be the k-th smallest element
                else throw new Exception("RadixSelectiontMsdInner: No elements in the bin that k is in, which should never happen");
            }
        }
        /// <summary>
        /// In-place Radix Selection of the k-th element in an array. Processes one word-digits at a time.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be selected from in place</param>
        /// <param name="start">starting index of the subarray</param>
        /// <param name="length">length of the subarray</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static void SelectRadixKtopWord(this uint[] arrayToBeSelected, Int32 start, Int32 length, Int32 k)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (start < 0 || length <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < start || k > (start + arrayToBeSelected.Length))
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            const int Log2ofPowerOfTwoRadix_loc = 16;
            int shiftRightAmount = (sizeof(uint) / 2 * 16) - Log2ofPowerOfTwoRadix_loc;
            RadixSelectionKtopWordInner(arrayToBeSelected, start, length, shiftRightAmount, k);
        }
        /// <summary>
        /// In-place Radix Selection of the k-th element in an array. Processes one word-digits at a time.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be sorted in place</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static void SelectRadixKtopWord(this uint[] arrayToBeSelected, Int32 k)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (arrayToBeSelected.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBeSelected.Length), "array length is invalid");
            if (k < 0 || k > arrayToBeSelected.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            const int Log2ofPowerOfTwoRadix_loc = 16;
            int shiftRightAmount = (sizeof(uint) / 2 * 16) - Log2ofPowerOfTwoRadix_loc;
            RadixSelectionKtopWordInner(arrayToBeSelected, 0, arrayToBeSelected.Length, shiftRightAmount, k);
        }
    }
}
