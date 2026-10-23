using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPCsharp
{
    public static partial class Algorithm
    {
        private static void QuantileRadixMsdUIntInner(uint[] a, int start, int length, int shiftRightAmount, Int32[] k, int kStart, int kLength, Action<uint[], int, int> baseCaseInPlaceSort, int threshold = 1024)
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

                for (int bin = 0, kIndex = kStart; bin < PowerOfTwoRadix && kIndex < (kStart + kLength); bin++)
                {
                    // Recurse only into a bin which contains one or more of the k[] elements
                    if (startOfBin[bin] <= endOfBin[bin] && k[kIndex] >= startOfBin[bin] && k[kIndex] < endOfBin[bin])
                    {
                        int kNewStart = kIndex++;
                        int kNewLength = 1; // at least one of the k[] elements is in this bin. Determine if more k[] element are in this bin, and pass them into the recursive call for this bin.
                        for (; kIndex < (kStart + kLength); kIndex++)
                        {
                            if (k[kIndex] >= startOfBin[bin] && k[kIndex] < endOfBin[bin])
                                kNewLength++;
                            else break;
                        }
                        QuantileRadixMsdUIntInner(a, startOfBin[bin], endOfBin[bin] - startOfBin[bin], shiftRightAmount, k, kNewStart, kNewLength, baseCaseInPlaceSort);
                    }
                }
            }
        }

        private static void QuantileRadixMsdUIntInner(ulong[] a, int start, int length, int shiftRightAmount, Int32[] k, int kStart, int kLength, Action<ulong[], int, int> baseCaseInPlaceSort, int threshold = 48)
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
                ulong current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                while (endOfBin[digit = (uint)(current_element >> shiftRightAmount) & bitMask] != _current)
                {
                    ulong temp = a[endOfBin[digit]];
                    a[endOfBin[digit]++] = current_element;
                    current_element = temp;
                }
                a[_current] = current_element;

                endOfBin[digit]++;
                while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                _current = endOfBin[nextBin - 1];
            }
            if (shiftRightAmount > 0)          // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else shiftRightAmount = 0;

                // TODO: Instead of iterating over all bins, we can iterate only over the unique digits present in kDigits to optimize the recursion.
                // TODO: One way to do this is to sort kDigits and then iterate over the unique values, or use a HashSet to get unique digits.
                for (int bin = 0, kIndex = kStart; bin < PowerOfTwoRadix && kIndex < (kStart + kLength); bin++)
                {
                    // Recurse only into a bin which contains one or more of the k[] elements
                    if (startOfBin[bin] <= endOfBin[bin] && k[kIndex] >= startOfBin[bin] && k[kIndex] < endOfBin[bin])
                    {
                        int kNewStart = kIndex++;
                        int kNewLength = 1; // at least one of the k[] elements is in this bin. Determine if more k[] element are in this bin, and pass them into the recursive call for this bin.
                        for (; kIndex < (kStart + kLength); kIndex++)
                        {
                            if (k[kIndex] >= startOfBin[bin] && k[kIndex] < endOfBin[bin])
                                kNewLength++;
                            else break;
                        }
                        QuantileRadixMsdUIntInner(a, startOfBin[bin], endOfBin[bin] - startOfBin[bin], shiftRightAmount, k, kNewStart, kNewLength, baseCaseInPlaceSort);
                    }
                }
            }
        }
        /// <summary>
        /// In-place Quantile using Radix by the k-th index in an array. Not a stable algorithm.
        /// </summary>
        /// <param name="arrayToBeQuantiled">array that is to be quantiled in place</param>
        /// <param name="start">starting index of the subarray</param>
        /// <param name="length">length of the subarray</param>
        /// <param name="k">Index to be used as the quantiling element. The value of the arrayToBeQuantiled[k] will not be used for quantiling.</param>
        /// <param name="threshold">for array size smaller than threshold Array.Sort will be used instead of MSD Radix Sort like algorithm</param>
        public static void QuantileRadix(this uint[] arrayToBeQuantiled, Int32 start, Int32 length, Int32 k, Int32 threshold = 1024)
        {
            if (arrayToBeQuantiled == null)
                throw new ArgumentNullException(nameof(arrayToBeQuantiled));
            if (arrayToBeQuantiled.Length <= 0 || length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBeQuantiled.Length), "array length or length is invalid");
            int shiftRightAmount = sizeof(uint) * 8 - Log2ofPowerOfTwoRadix;
            if (k < 0 || k >= arrayToBeQuantiled.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k is out of array bounds");
            int endInclusive = start + length - 1;
            if (k < start || k > endInclusive)
                throw new ArgumentOutOfRangeException(nameof(k), "k is out of start to end bounds");
            Int32[] kArray = new Int32[1] { k };
            // Insertion Sort or Heap Sort could be passed in as another base case since they are both in-place
            QuantileRadixMsdUIntInner(arrayToBeQuantiled, start, length, shiftRightAmount, kArray, 0, kArray.Length, Array.Sort, threshold);
        }
        /// <summary>
        /// In-place Quantile using Radix by the k-th index in an array. Not a stable algorithm.
        /// </summary>
        /// <param name="arrayToBeQuantiled">array that is to be quantiled in place</param>
        /// <param name="k">Index to be used as the quantiling element. The value of the arrayToBeQuantiled[k] will not be used for quantiling.</param>
        /// <param name="threshold">for array size smaller than threshold Array.Sort will be used instead of MSD Radix Sort like algorithm</param>
        public static void QuantileRadix(this uint[] arrayToBeQuantiled, Int32 k, Int32 threshold = 1024)
        {
            if (arrayToBeQuantiled == null)
                throw new ArgumentNullException(nameof(arrayToBeQuantiled));
            if (arrayToBeQuantiled.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBeQuantiled.Length), "array length is invalid");
            if (k < 0 || k >= arrayToBeQuantiled.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k is out of array bounds");
            Int32[] kArray = new Int32[1] { k };
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            QuantileRadixMsdUIntInner(arrayToBeQuantiled, 0, arrayToBeQuantiled.Length, shiftRightAmount, kArray, 0, kArray.Length, Array.Sort, threshold);
        }
        /// <summary>
        /// In-place Quantile using Radix by the k[]-th indexes in an array. Not a stable algorithm.
        /// </summary>
        /// <param name="arrayToBeQuantiled">array that is to be quantiled in place</param>
        /// <param name="start">starting index of the subarray</param>
        /// <param name="length">length of the subarray</param>
        /// <param name="k">array of indexes of the desired elements to be quantiled by, in ascending order</param>
        /// <param name="threshold">for array size smaller than threshold Array.Sort will be used instead of MSD Radix Sort like algorithm</param>
        public static void QuantileRadix(this uint[] arrayToBeQuantiled, Int32 start, Int32 length, Int32[] k, Int32 threshold = 1024)
        {
            if (arrayToBeQuantiled == null)
                throw new ArgumentNullException(nameof(arrayToBeQuantiled));
            if (k == null)
                throw new ArgumentNullException(nameof(k));
            if (arrayToBeQuantiled.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBeQuantiled.Length), "array length or length is invalid");
            if (k.Length > arrayToBeQuantiled.Length)
                throw new ArgumentException("k array length cannot be greater than the array to be selected length");
            if (k.Length == 0 || length <= 0) return;
            int shiftRightAmount = sizeof(uint) * 8 - Log2ofPowerOfTwoRadix;
            for (int i = 0; i < k.Length; i++)
            {
                if (k[i] < 0 || k[i] >= arrayToBeQuantiled.Length)
                    throw new ArgumentOutOfRangeException(nameof(k), "k is out of array bounds");
                int endInclusive = start + length - 1;
                if (k[i] < start || k[i] > endInclusive)
                    throw new ArgumentOutOfRangeException(nameof(k), "k is out of start to end bounds");
            }
            // Insertion Sort or Heap Sort could be passed in as another base case since they are both in-place
            QuantileRadixMsdUIntInner(arrayToBeQuantiled, start, length, shiftRightAmount, k, 0, k.Length, Array.Sort, threshold);
        }
        /// <summary>
        /// In-place Quantile using Radix by the k[]-th indexes in an array. Not a stable algorithm.
        /// </summary>
        /// <param name="arrayToBeQuantiled">array that is to be quantiled in place</param>
        /// <param name="k">array of indexes of the desired elements to be quantiled by, in ascending order</param>
        /// <param name="threshold">for array size smaller than threshold Array.Sort will be used instead of MSD Radix Sort like algorithm</param>
        public static void QuantileRadix(this uint[] arrayToBeQuantiled, Int32[] k, Int32 threshold = 1024)
        {
            if (arrayToBeQuantiled == null)
                throw new ArgumentNullException(nameof(arrayToBeQuantiled));
            if (k == null)
                throw new ArgumentNullException(nameof(k));
            if (arrayToBeQuantiled.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBeQuantiled.Length), "array length is invalid");
            if (k.Length > arrayToBeQuantiled.Length)
                throw new ArgumentException("k array length cannot be greater than the array to be quantiled length");
            if (k.Length == 0) return;
            for (int i = 0; i < k.Length; i++)
            {
                if (k[i] < 0 || k[i] >= arrayToBeQuantiled.Length)
                    throw new ArgumentOutOfRangeException(nameof(k), "k is out of array bounds");
            }
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            QuantileRadixMsdUIntInner(arrayToBeQuantiled, 0, arrayToBeQuantiled.Length, shiftRightAmount, k, 0, k.Length, Array.Sort, threshold);
        }
    }
}
