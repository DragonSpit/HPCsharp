// Explanation and Reasoning:
// This file implements an in-place Radix Selection algorithm using the Most Significant Digit (MSD) approach for unsigned integers (uint).
// It provides linear order and in-place operation for the Selection algorithm. It is possible because only one bin
// or half is needed (for QuickSelect), while elements in other bins (or the other half) can be ignored or thrown away, which is not the case with sorting algorithms,
// where all bins (or both halves) are sorted.
// TODO: Improve the algorithm by doing the count for the next digit while moving elements into the bin that contains the k-th smallest element.
// TODO: Implement parallel version of the Radix Selection algorithm by pre-allocating counts for all array chunks to keep them around after parallel histogramming.
//       These will be used to determine how many elements are in each chunk that belong to the k-th bin. All chunks that have at least one
//       element that belongs to the k-th bin can then be processed in parallel to move those elements into the k-th bin. Starting index within
//       the k-th bin for each chunk can be determined by a prefix sum over the counts for each chunk.
// TODO: See if dual-count speeds up C# Histogram (counting), which is implemented in C++.
// TODO: In-place Selection of k[] can be implemented by using a similar technique as in-place MSD Radix Sort, but with fewer bins.

using System;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HPCsharp
{
    public static partial class Algorithm
    {
        // Move elements outside the k-th bin, the bin that k is in, which belong to the k-th bin, into the k-th bin.
        // Generic implementation that work for regions to the left or to the right of the k-th bin, and for any digit size.
        private static int MoveOutsideOfKthBinIn(uint[] a, int startOfOb, int lengthOfOb, int startOfKthBin, int lengthOfKthBin, int shiftRightAmount, uint bitMask, int kthBin)
        {
            int endOfKthBin = startOfKthBin + lengthOfKthBin - 1;
            int endOfOb     = startOfOb + lengthOfOb - 1;
            int _current_ob = startOfOb, _current_ib = startOfKthBin; // _ob = outside of bin, _ib = inside of bin
            while (true)
            {
                // Look for the element that belongs in the bin that k is in, to move into that bin
                for (; _current_ob <= endOfOb; _current_ob++)
                    if (((a[_current_ob] >> shiftRightAmount) & bitMask) == kthBin) break;
                // Look for the first location in the bin that k is in, which has an element that does not belong in that bin
                if (_current_ob <= endOfOb)
                    for (; _current_ib <= endOfKthBin; _current_ib++)
                        if (((a[_current_ib] >> shiftRightAmount) & bitMask) != kthBin) break;

                if (_current_ob > endOfOb || _current_ib > endOfKthBin) break; // All the element outside the bin have been exhausted or the bin that k is in is full or 
                a[_current_ib++] = a[_current_ob++];    // Move the element that belongs in the bin into the bin
            }
            return _current_ib;
        }

        // Move elements to the left of the k-th bin, the bin that k is in, which belong to one of the k[] bins into their respective bins, ignoring the elements that do not belong to any of the k[] bins.
        // This function is not-in-place since the move step can overwrite elements that need to move to their destination k[] bin.
        // Lob = left of bin
        private static void MoveElementsLeftOfKthBinIntoBins(uint[] a, uint[] aOut, int startOfLob, int lengthOfLob, int[] startOfBin, HashSet<uint> kthBinsHashSet, int shiftRightAmount, uint bitMask)
        {
            int endOfLob = startOfLob + lengthOfLob - 1;
            // Search for the element that belongs in one of the bins that k[] are in, to move into the bin where it belongs
            for (int _currentLob = startOfLob; _currentLob <= endOfLob; _currentLob++)
            {
                uint digit = (a[_currentLob] >> shiftRightAmount) & bitMask;
                if (kthBinsHashSet.Contains(digit)) aOut[startOfBin[digit]++] = a[_currentLob];    // Move the Lob element that belongs in the bin into its bin
            }
        }

        // The rest of the elements within the current bin need to be examined to see if they belong to any of the k[] bins (including the current bin).
        // If an element does, then if it belongs to the current bin, then it's left in place, and if it belongs to one of the other k[] bins, then it needs to be moved into that bin.
        // At first don't worry about making it in-place, just move the elements that belong to the other k[] bins into their respective bins. Later in-place can be accomplished in a similar method as in-place MSD Radix Sort, but with fewer bins.
        private static void MoveElementsInsideKthBinIntoBins(uint[] a, uint[] aOut, int[] kthBins, int currentBin, int[] startOfBin, int lengthOfKthBin, HashSet<uint> kthBinsHashSet, int shiftRightAmount, uint bitMask)
        {
            for (int _currentIob = startOfBin[currentBin]; _currentIob < startOfBin[currentBin + 1]; _currentIob++) // Iob = inside of bin
            {
                uint digit = (a[_currentIob] >> shiftRightAmount) & bitMask;
                if (kthBinsHashSet.Contains(digit)) aOut[startOfBin[digit]++] = a[_currentIob]; // element belongs in one of the other k[] bins, so move it into that bin
            }
        }

        // Move elements outside the k-th bin, the bin that k is in, which belong to the k-th bin, into the k-th bin.
        // Generic implementation that work for regions to the left or to the right of the k-th bin, and for any digit size.
        private static int MoveOutsideOfKthBinInAndCount(uint[] a, int startOfOb, int lengthOfOb, int startOfKthBin, int lengthOfKthBin, int shiftRightAmount, uint bitMask, int kthBin, int[] count)
        {
            int endOfKthBin = startOfKthBin + lengthOfKthBin - 1;
            int endOfOb = startOfOb + lengthOfOb - 1;
            int _current_ob = startOfOb, _current_ib = startOfKthBin; // _ob = outside of bin, _ib = inside of bin
            int shiftRightAmountNextDigit = shiftRightAmount - Log2ofPowerOfTwoRadix;
            while (true)
            {
                // Look for the element that belongs in the bin that k is in, to move into that bin
                for (; _current_ob <= endOfOb; _current_ob++)
                    if (((a[_current_ob] >> shiftRightAmount) & bitMask) == kthBin) break;
                // Look for the first location in the bin that k is in, which has an element that does not belong in that bin
                if (_current_ob <= endOfOb)
                    for (; _current_ib <= endOfKthBin; _current_ib++)
                        if (((a[_current_ib] >> shiftRightAmount) & bitMask) != kthBin) break;
                        else count[(byte)(a[_current_ib] >> shiftRightAmountNextDigit)]++;

                if (_current_ob > endOfOb || _current_ib > endOfKthBin) break; // All the element outside the bin have been exhausted or the bin that k is in is full or 
                count[(byte)(a[_current_ob] >> shiftRightAmountNextDigit)]++;
                a[_current_ib++] = a[_current_ob++];    // Move the element that belongs in the bin into the bin
            }
            return _current_ib;
        }

        private static void RadixSelectiontInner(uint[] a, int first, int length, int shiftRightAmount, int k)
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
            _current_ib = MoveOutsideOfKthBinIn(a, first,                  startOfBin[kthBin] - first,        startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], shiftRightAmount, bitMask, kthBin);
            _current_ib = MoveOutsideOfKthBinIn(a, startOfBin[kthBin + 1], last - startOfBin[kthBin + 1] + 1, _current_ib,        startOfBin[kthBin + 1] - _current_ib,        shiftRightAmount, bitMask, kthBin);

            if (shiftRightAmount > 0)          // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else shiftRightAmount = 0;
                // Only recurse into the bin that contains the k-th smallest element and if more than one element is in that bin
                if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) > 1)
                    RadixSelectiontInner(a, startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], shiftRightAmount, k);
                else if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) == 1) return; // Only one element in the bin that k is in, so it must be the k-th smallest element
                else throw new Exception("RadixSelectiontMsdInner: No elements in the bin that k is in, which should never happen");
            }
        }

        private static void RadixSelectiontNonRecursiveInner(uint[] a, int first, int length, int shiftRightAmount, int k)
        {
            int last = first + length - 1;
            const uint bitMask = PowerOfTwoRadix - 1;

            while (shiftRightAmount >= 0)
            {
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
                _current_ib = MoveOutsideOfKthBinIn(a, first,                  startOfBin[kthBin] - first,        startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], shiftRightAmount, bitMask, kthBin);
                _current_ib = MoveOutsideOfKthBinIn(a, startOfBin[kthBin + 1], last - startOfBin[kthBin + 1] + 1, _current_ib,        startOfBin[kthBin + 1] - _current_ib,        shiftRightAmount, bitMask, kthBin);

                if (shiftRightAmount == 0) break;
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else shiftRightAmount = 0;
                // Only recurse into the bin that contains the k-th smallest element and if more than one element is in that bin
                if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) > 1)
                {
                    first  = startOfBin[kthBin];
                    length = startOfBin[kthBin + 1] - startOfBin[kthBin];
                    last   = first + length - 1;
                }
                else if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) == 1) return; // Only one element in the bin that k is in, so it must be the k-th smallest element
                else throw new Exception("RadixSelectiontMsdInner: No elements in the bin that k is in, which should never happen");
            }
        }

        private static void RadixSelectiontNonRecursiveInner2(uint[] a, int first, int length, int shiftRightAmount, int k)
        {
            int last = first + length - 1;
            const uint bitMask = PowerOfTwoRadix - 1;

            var count = HPCsharp.Algorithm.HistogramOneByteComponent(a, first, last, shiftRightAmount);

            while (shiftRightAmount >= 0)
            {
                var startOfBin = new int[PowerOfTwoRadix + 1];
                startOfBin[0] = first; startOfBin[PowerOfTwoRadix] = last + 1;
                for (int i = 1; i < PowerOfTwoRadix; i++)
                    startOfBin[i] = startOfBin[i - 1] + count[i - 1];
                for (int i = 0; i < PowerOfTwoRadix; i++)
                    count[i] = 0;

                // Determine which bin contains the k-th smallest element. kthBin will hold the bin number.
                int kthBin = 0, _current_ib;
                for (; kthBin < PowerOfTwoRadix; kthBin++)
                {
                    int binLength = startOfBin[kthBin + 1] - startOfBin[kthBin];
                    if (binLength == 0) continue; // skip empty bins
                    if (k >= startOfBin[kthBin] && k <= (startOfBin[kthBin + 1] - 1)) break;
                }
                _current_ib = MoveOutsideOfKthBinInAndCount(a, first, startOfBin[kthBin] - first, startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], shiftRightAmount, bitMask, kthBin, count);
                _current_ib = MoveOutsideOfKthBinInAndCount(a, startOfBin[kthBin + 1], last - startOfBin[kthBin + 1] + 1, _current_ib, startOfBin[kthBin + 1] - _current_ib, shiftRightAmount, bitMask, kthBin, count);

                int shiftRightAmountNextDigit = shiftRightAmount - Log2ofPowerOfTwoRadix;
                for (; _current_ib < startOfBin[kthBin + 1]; _current_ib++)
                    count[(byte)(a[_current_ib] >> shiftRightAmountNextDigit)]++;

                if (shiftRightAmount == 0) break;
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else shiftRightAmount = 0;
                // Only recurse into the bin that contains the k-th smallest element and if more than one element is in that bin
                if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) > 1)
                {
                    first = startOfBin[kthBin];
                    length = startOfBin[kthBin + 1] - startOfBin[kthBin];
                    last = first + length - 1;
                }
                else if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) == 1) return; // Only one element in the bin that k is in, so it must be the k-th smallest element
                else throw new Exception("RadixSelectiontMsdInner: No elements in the bin that k is in, which should never happen");
            }
        }

        /// <summary>
        /// In-place Radix Selection of the k-th element in an array. Processes one byte-digits at a time.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be selected from in place</param>
        /// <param name="start">starting index of the subarray</param>
        /// <param name="length">length of the subarray</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static uint SelectRadix(this uint[] arrayToBeSelected, Int32 start, Int32 length, Int32 k)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (start < 0 || length <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < start || k > (start + arrayToBeSelected.Length))
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            RadixSelectiontNonRecursiveInner2(arrayToBeSelected, start, length, shiftRightAmount, k);
            return arrayToBeSelected[k];
        }
        /// <summary>
        /// In-place Radix Selection of the k-th element in an array. Processes one byte-digits at a time.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be sorted in place</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static uint SelectRadix(this uint[] arrayToBeSelected, Int32 k)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (arrayToBeSelected.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBeSelected.Length), "array length is invalid");
            if (k < 0 || k > arrayToBeSelected.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            RadixSelectiontNonRecursiveInner2(arrayToBeSelected, 0, arrayToBeSelected.Length, shiftRightAmount, k);
            return arrayToBeSelected[k];
        }

        // Process 16-bit digits at a time, since the count array fits in modern CPU cache.
        private static void RadixSelectionWordInner(uint[] a, int first, int length, int shiftRightAmount, int k)
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
            int kthBin, _current_ib;
            for (kthBin = 0; kthBin < PowerOfTwoRadix_loc; kthBin++)
            {
                int binLength = startOfBin[kthBin + 1] - startOfBin[kthBin];
                if (binLength == 0) continue; // skip empty bins
                if (k >= startOfBin[kthBin] && k <= (startOfBin[kthBin + 1] - 1)) break;
            }
            _current_ib = MoveOutsideOfKthBinIn(a, first,                  startOfBin[kthBin] - first,        startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], shiftRightAmount, bitMask, kthBin);
            _current_ib = MoveOutsideOfKthBinIn(a, startOfBin[kthBin + 1], last - startOfBin[kthBin + 1] + 1, _current_ib,        startOfBin[kthBin + 1] - _current_ib,        shiftRightAmount, bitMask, kthBin);

            if (shiftRightAmount > 0)          // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix_loc) shiftRightAmount -= Log2ofPowerOfTwoRadix_loc;
                else shiftRightAmount = 0;
                // Only recurse into the bin that contains the k-th smallest element and if more than one element is in that bin
                if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) > 1)
                    RadixSelectionWordInner(a, startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], shiftRightAmount, k);
                else if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) == 1) return; // Only one element in the bin that k is in, so it must be the k-th smallest element
                else throw new Exception("RadixSelectiontMsdInner: No elements in the bin that k is in, which should never happen");
            }
        }
        /// <summary>
        /// In-place Radix Selection of the k-th element in an array. Processes one word-digit (16-bits) at a time.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be selected from in place</param>
        /// <param name="start">starting index of the subarray</param>
        /// <param name="length">length of the subarray</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static uint SelectRadixWord(this uint[] arrayToBeSelected, Int32 start, Int32 length, Int32 k)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (start < 0 || length <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < start || k > (start + arrayToBeSelected.Length))
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            const int Log2ofPowerOfTwoRadix_loc = 16;
            int shiftRightAmount = (sizeof(uint) / 2 * 16) - Log2ofPowerOfTwoRadix_loc;
            RadixSelectionWordInner(arrayToBeSelected, start, length, shiftRightAmount, k);
            return arrayToBeSelected[k];
        }
        /// <summary>
        /// In-place Radix Selection of the k-th element in an array. Processes one word-digit (16-bits) at a time.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be sorted in place</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static uint SelectRadixWord(this uint[] arrayToBeSelected, Int32 k)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (arrayToBeSelected.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBeSelected.Length), "array length is invalid");
            if (k < 0 || k > arrayToBeSelected.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            const int Log2ofPowerOfTwoRadix_loc = 16;
            int shiftRightAmount = (sizeof(uint) / 2 * 16) - Log2ofPowerOfTwoRadix_loc;
            RadixSelectionWordInner(arrayToBeSelected, 0, arrayToBeSelected.Length, shiftRightAmount, k);
            return arrayToBeSelected[k];
        }

        // Process 16-bit digits at a time, since the count array fits in modern CPU cache.
        // TODO: This version needs to determine whether the current element being processed belongs to one of the k[] bins or not. If it doesn't, then it can be ignored/skipped. If it does, then it needs to be moved into the correct k[] bin. This can be done by doing a binary search of the k[] bins for the current element's digit. This should speed up the algorithm when there are many bins and only a few of them contain the k[] elements.
        //       There also may be a way to use a hash set to determine whether the current element being processed belongs to one of the k[] bins or not, which may be faster than doing a binary search when there are many k[] bins.
        //       C# has a HashSet class which is high performance for checking membership, but definitely need to compare performance versus binary search, since k[] should be in sorted order. A threshold can be used to switch between using a hash set and doing a binary search, where if the number of k[] bins is above the threshold, then use a hash set, otherwise do a binary search.
        //       A potential change will be needed to how the MoveOusideOfKthBinIn function works, since it currently assumes only one k[] bin, while now it may need to handle multiple k[] bins. One way to handle this is to have the MoveOusideOfKthBinIn function take in the k[] bins that are relevant for the current digit being processed, and then check whether the current element being processed belongs to one of those k[] bins or not, and if it does, then move it into the correct k[] bin.
        //       C# has an Array.BinarySearch method which can be used to do a binary search on the k[] bins, since they should be in sorted order (returns negative index if not found). This can be used to determine whether the current element being processed belongs to one of the k[] bins or not, and if it does, then which k[] bin it belongs to.
        //       A method for processing inside each k[] bin will also be needed, which will be similar to the current MoveOusideOfKthBinIn function, but it will need to handle multiple k[] bins, and move elements into the correct k[] bin based on their digit value for the current digit being processed.
        //       The way this k[] method will work is to process the region to the left of k[0], then process k[0] bin, then process the region between k[0] and k[1], then process k[1] bin, then process the region between k[1] and k[2], and so on, until all k[] bins have been processed. This way, all elements that belong to the k[] bins will be moved into the correct k[] bins, and all elements that do not belong to any of the k[] bins will be ignored/skipped.
        //       Followed by processing the region to the right of the last k[] bin. This way, all elements that belong to the k[] bins will be moved into the correct k[] bins, and all elements that do not belong to any of the k[] bins will be ignored/skipped.
        //       It may be possible to have a single method to move elements either outside of each bin or inside each bin, with the only difference is the need to skip the elements inside each bin which were moved there already when processing the elements to the left of the bin.
        // Each k[] value is the index of the desired element to be selected within the input array.
        private static void RadixSelectionWordInner(uint[] a, uint[] a_out, int first, int length, int shiftRightAmount, int[] k)
        {
            int last = first + length - 1;
            const int PowerOfTwoRadix_loc = 256 * 256;
            const int Log2ofPowerOfTwoRadix_loc = 16;
            const uint bitMask = PowerOfTwoRadix_loc - 1;

            var count = HPCsharp.Algorithm.HistogramOneWordComponent(a, first, last, shiftRightAmount);

            var startOfBin = new int[PowerOfTwoRadix_loc + 1];      // one more to hold the start of beyond the last bin, which is needed to determine the length of the last bin
            startOfBin[0] = first; startOfBin[PowerOfTwoRadix_loc] = last + 1;
            for (int i = 1; i <= PowerOfTwoRadix_loc; i++)          // one more to hold the start of beyond the last bin, which is needed to determine the length of the last bin
                startOfBin[i] = startOfBin[i - 1] + count[i - 1];

            // kthBins will hold the bin number for each of the k[] full values (indexes) belongs to. These bins are the ones to drill down further into.
            var kthBins = new int[k.Length];
            int kthBin = 0;
            for (int i = 0; kthBin < PowerOfTwoRadix_loc; kthBin++)
            {
                // TODO: Could the following two statements be eliminated by doing the two if statements below better - to cover the case of an empty bin? Also, copilot is suggesting using binary search here.
                int binLength = startOfBin[kthBin + 1] - startOfBin[kthBin];
                if (binLength == 0) continue; // skip empty bins
                while (k[i] >= startOfBin[kthBin] && k[i] <= (startOfBin[kthBin + 1] - 1))  // TODO: This while loop can be simplified
                {
                    kthBins[i++] = kthBin;
                    if (i == k.Length) break;
                }
            }
            // Move all elements to the left of each k[] bin which belong in one of the k[] bins into the bin it belongs to
            HashSet<uint> kthBinsHashSet = new HashSet<uint>();
            for (int bin = 0; bin < kthBins.Length; bin++)  // collect the current digits of each of the k[] values
                kthBinsHashSet.Add((uint)kthBins[bin]);
            // Look for the first location inside each of the k[] bins for the element which does not belong to that bin - i.e. the first element to be replaced in that bin
            for (int i = 0; i < kthBins.Length; i++)
                for (int bin = kthBins[i]; startOfBin[bin] < startOfBin[bin + 1];)
                {
                    if (((a[startOfBin[bin]] >> shiftRightAmount) & bitMask) != bin) break;
                    else a_out[startOfBin[bin]++] = a[startOfBin[bin]];  // Move the element that belongs in the bin into the bin of the output buffer
                }
            int _current_lob = first;  // _lob = left of bin
            for (int bin = 0; bin < kthBins.Length; bin++)
            {
                MoveElementsLeftOfKthBinIntoBins(a, a_out, _current_lob, startOfBin[kthBins[bin]] - _current_lob, startOfBin, kthBinsHashSet, shiftRightAmount, bitMask);
                MoveElementsInsideKthBinIntoBins(a, a_out, kthBins, bin, startOfBin, startOfBin[kthBins[bin]] - _current_lob, kthBinsHashSet, shiftRightAmount, bitMask);
                _current_lob = startOfBin[kthBins[bin] + 1];  // advance to the start of the next bin
            }

            if (shiftRightAmount > 0)          // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix_loc) shiftRightAmount -= Log2ofPowerOfTwoRadix_loc;
                else shiftRightAmount = 0;
                // Only recurse into the bins that contains each of the k[] elements and if more than one element is in that bin
// TODO: Recurse into a bin once for all k[] elements that belong to that bin.
                if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) > 1)
                    RadixSelectionWordInner(a_out, a, startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], shiftRightAmount, k);
                else if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) == 1) return; // Only one element in the bin that k is in, so it must be the k-th smallest element
                else throw new Exception("RadixSelectionWordInner: No elements in the bin that k is in, which should never happen");
            }
        }
        /// <summary>
        /// Not-in-place Radix Selection Partition of the k[] elements in an array.
        /// Processes one word-digit (16-bits) of the arrayToBeSelected elements at a time.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be selected from, and in which the result is stored</param>
        /// <param name="start">starting index of the subarray</param>
        /// <param name="length">length of the subarray</param>
        /// <param name="k">indexes of the desired element to be selected and partitioned by in low to high order</param>
        public static void SelectRadixWord(this uint[] arrayToBeSelected, Int32 start, Int32 length, Int32[] k)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (start < 0 || length <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            for(int i = 0; i < k.Length; i++)
                if (k[i] < start || k[i] > (start + arrayToBeSelected.Length))
                    throw new ArgumentOutOfRangeException(nameof(k), "k[" + i + "] must be between start and (start + length)");
            const int Log2ofPowerOfTwoRadix_loc = 16;
            int shiftRightAmount = (sizeof(uint) / 2 * 16) - Log2ofPowerOfTwoRadix_loc;
            uint[] a_tmp = new uint[arrayToBeSelected.Length];

            RadixSelectionWordInner(arrayToBeSelected, a_tmp, start, length, shiftRightAmount, k);
        }
        /// <summary>
        /// Not-in-place Radix Selection Partition of the k[] elements in an array.
        /// Processes one word-digit (16-bits) of the arrayToBeSelected elements at a time.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be selected from, and in which the result is stored</param>
        /// <param name="k">indexes of the desired elements to be selected and partitioned by in low to high order</param>
        public static void SelectRadixWord(this uint[] arrayToBeSelected, Int32[] k)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (arrayToBeSelected.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBeSelected.Length), "array length is invalid");
            for (int i = 0; i < k.Length; i++)
                if (k[i] < 0 || k[i] >= arrayToBeSelected.Length)
                    throw new ArgumentOutOfRangeException(nameof(k), "k[" + i + "] must be between 0 and (arrayToBeSelected.Length-1)");
            const int Log2ofPowerOfTwoRadix_loc = 16;
            int shiftRightAmount = (sizeof(uint) / 2 * 16) - Log2ofPowerOfTwoRadix_loc;
            uint[] a_tmp = new uint[arrayToBeSelected.Length];

            RadixSelectionWordInner(arrayToBeSelected, a_tmp, 0, arrayToBeSelected.Length, shiftRightAmount, k);
        }
    }

    public static partial class ParallelAlgorithm
    {
        // Move elements outside the k-th bin, the bin that k is in, into the k-th bin
        // Generic implementation that work for regions to the left or to the right of the k-th bin, and for any digit size.
        private static int MoveOutsideOfKthBinIn(uint[] a, int startOfOb, int lengthOfOb, int startOfKthBin, int lengthOfKthBin,
                                                 int shiftRightAmount, uint bitMask, int kthBin)
        {
            int endOfKthBin = startOfKthBin + lengthOfKthBin - 1;
            int endOfOb = startOfOb + lengthOfOb - 1;
            int _current_ob = startOfOb, _current_ib = startOfKthBin, found_ob; // _ob = outside of bin, _ib = inside of bin
            while (true)
            {
                // Look for the element that belongs in the bin that k is in, to move into that bin
                for (found_ob = 0; _current_ob <= endOfOb; _current_ob++)
                    if (((a[_current_ob] >> shiftRightAmount) & bitMask) == kthBin) { found_ob = 1; break; }
                // Look for the first location in the bin that k is in, which has an element that does not belong in that bin
                if (found_ob == 1)
                    for (; _current_ib <= endOfKthBin; _current_ib++)
                        if (((a[_current_ib] >> shiftRightAmount) & bitMask) != kthBin) break;

                if (_current_ob > endOfOb || _current_ib > endOfKthBin) break; // All the element outside the bin have been exhausted or the bin that k is in is full or 
                a[_current_ib++] = a[_current_ob++];    // Move the element that belongs in the bin into the bin
            }
            return _current_ib;
        }

        private static void RadixSelectionParInner(uint[] a, int first, int length, int shiftRightAmount, int k, int parallelThreshold = 16384)
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
            int kthBin = 0, _current_ib;
            for (; kthBin < PowerOfTwoRadix; kthBin++)
            {
                int binLength = startOfBin[kthBin + 1] - startOfBin[kthBin];
                if (binLength == 0) continue; // skip empty bins
                if (k >= startOfBin[kthBin] && k <= (startOfBin[kthBin + 1] - 1)) break;
            }
            _current_ib = MoveOutsideOfKthBinIn(a, first,                  startOfBin[kthBin] - first,        startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], shiftRightAmount, bitMask, kthBin);
            _current_ib = MoveOutsideOfKthBinIn(a, startOfBin[kthBin + 1], last - startOfBin[kthBin + 1] + 1, _current_ib,        startOfBin[kthBin + 1] - _current_ib,        shiftRightAmount, bitMask, kthBin);

            if (shiftRightAmount > 0)          // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else shiftRightAmount = 0;
                // Only recurse into the bin that contains the k-th smallest element and if more than one element is in that bin
                if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) > 1)
                    RadixSelectionParInner(a, startOfBin[kthBin], startOfBin[kthBin + 1] - startOfBin[kthBin], shiftRightAmount, k);
                else if ((startOfBin[kthBin + 1] - startOfBin[kthBin]) == 1) return; // Only one element in the bin that k is in, so it must be the k-th smallest element
                else throw new Exception("RadixSelectiontMsdInner: No elements in the bin that k is in, which should never happen");
            }
        }
        /// <summary>
        /// In-place Radix Selection.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be selected from in place</param>
        /// <param name="start">starting index of the subarray</param>
        /// <param name="length">length of the subarray</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static uint SelectRadix(this uint[] arrayToBeSelected, Int32 start, Int32 length, Int32 k, int parallelThreshold = 100000)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (start < 0 || length <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < start || k > (start + arrayToBeSelected.Length))
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            RadixSelectionParInner(arrayToBeSelected, start, length, shiftRightAmount, k, parallelThreshold);
            return arrayToBeSelected[k];
        }
        /// <summary>
        /// In-place Radix Selection.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be sorted in place</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static uint SelectRadix(this uint[] arrayToBeSelected, Int32 k, int parallelThreshold = 100000)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (arrayToBeSelected.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBeSelected.Length), "array length is invalid");
            if (k < 0 || k > arrayToBeSelected.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            RadixSelectionParInner(arrayToBeSelected, 0, arrayToBeSelected.Length, shiftRightAmount, k, parallelThreshold);
            return arrayToBeSelected[k];
        }
    }
}
