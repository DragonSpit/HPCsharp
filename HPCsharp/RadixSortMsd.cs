// TODO: Create a generic version that can sort multiple data types, possibly like was done with Fill(), where we check which data type it is and call the appropriate
//       function underneath. This seems to be really hard in C#
// TODO: Use Array.Sort as the base algorithm (recursion termination case) for MSD Radix Sort, since it's in-place and uses Introspective Sort in the latest version
//       of .NET. Find the optimal threshold, which could be pretty large.
// TODO: One way to experiment to small % performance enhancements is to create two versions and compare their performance against each other. Plus find your statistical
//       analysis stuff and apply it as well. We need to be able to capture many small performance improvements.
// TODO: Implement Malte's discovery of taking advantage of ILP of the CPU, by "unrolling" series of swaps by reducing dependencies across several swaps.
//       It may be possible to create a function to generalize this method and make it available to developers, to extract more performance out of cascaded swaps.
//       And, make this a part of the swapping suite of functions.
// TODO: Consider accelerating the special case of all array elements going into one or a few bins. The case of one bin is common due to small arrays that use
//       large indexes. In the case of a single bin, there is nothing to do. In the case of a small number of bins, can we do it faster? Let's think about this case.
// TODO: Unrolling cascaded swaps may require an additional array to go thru, maybe, if that makes it simpler to place elements there and then to take them out of there.
//       Walk thru the cascaded flow by hand and see what kind of structure is needed and would work. Start by hardcoding to 2 level unroll to get it working and see if there is a benefit.
// TODO: Create a check of each bin against the size of the CPU cache (cache aware) and sort it using Array.Sort if the bin fits within L2 cache entirely (minus a bit for overhead)
// TODO: Parallelize Histogram of components of 64-bit elements (ulong, long, and double).
// TODO: Parallelize lower levels of MSD Radix Sort dynamically, where only if the bin is large enough is the new task created to sort than bin further using Radix Sort, otherwise use Array.Sort
// TODO: Create an inner loop function that uses union to pull out the desired byte/digit, since we know this is faster than shifting and masking
// TODO: Create an adaptive MSD Radix Sort implementation, where if the array/bin is > 64K elements then use 32-bit entries in the count array, but if the array/bin is 64K elements or fewer
//       then use ushort count entries, which will fit more entries into L1 and L2 cache allowing for processing of more bits of each element per iteration - e.g. instead of 8-bits/digit we
//       could go up to 9-bits per digit, or from 10-bits/digit to 11-bits/digit, and the reduce the number of passes.
// TODO: Defnitely implement the special case of all array elements ending up in a single bin. It's exactly how John is testing, as he generates randoms with values up to the size of the
//       array - e.g. if array size is 1M elements then each array element has a value 0 to 1M-1. Covering this special case by checking if all elements are in a single bin, will accelerate
//       this special case and the case of a constant array, and ramps that go up to the array size. Even with 100M elements of longs, each value is only 27-bits out of 64-bits, with the upper
//       bits always at zero.
// TODO: I figured out Malte's technique for more CPU ILP - it's simply handling multiple array items at a time. Instead of handling a single array item and then cascading swaps from there until
//       it loops back, Malte's realized that he could process two or more array items at a time, probably with some inner checking to make sure the loop isn't just two items. This will need to be
//       tested very carefully, especially using two values thru the entire array (with both possible phases). Yeah, you process two or more array elements at a time, which will add more complexity,
//       at a significant gain of performance. Once we do this, it would be worth contacting Brad's son to discuss with him how to add this to compilers in general to recogmize this kind of a pattern
//       and unroll it to gain performance by more ILP.
// TODO: It may be possible to generalize most significant digit detection (instead of hardcoding 56 right shift for 8-bit digit) by detecting if the MSD includes the most significant bit in it.
//       Could possibly codify into a function that gets rightShiftAmount and digit size. This leads to generalization of most significant digit detection to support digits of any size.
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HPCsharp
{
    static public partial class Algorithm
    {
#if false
        // This is a possible eventual goal of generic RadixSort implementation which will support more data types over time
        private static void SortRadixMsd<T>(this T[] arrayToBeSorted) where T : struct
        {
            int numBytesInItem = 0;
            if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
                SortCountingInPlace(arrayToBeSorted);
            else if (typeof(T) == typeof(ushort) || typeof(T) != typeof(short))
                numBytesInItem = 2;
            else if (typeof(T) == typeof(uint) || typeof(T) != typeof(int))
                numBytesInItem = 4;
            else if (typeof(T) == typeof(ulong) || typeof(T) != typeof(long))
                numBytesInItem = 8;
            else
                throw new ArgumentException(string.Format("Type '{0}' is unsupported.", typeof(T).ToString()));
        }
#endif

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixMsd(this byte[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static byte[] SortRadixMsdInPlaceFunc(this byte[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
            return arrayToBeSorted;
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixMsd(this sbyte[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static sbyte[] SortRadixMsdInPlaceFunc(this sbyte[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
            return arrayToBeSorted;
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixMsd(this ushort[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static ushort[] SortRadixMsdInPlaceFunc(this ushort[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
            return arrayToBeSorted;
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixMsd(this short[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static short[] SortRadixMsdInPlaceFunc(this short[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
            return arrayToBeSorted;
        }

        private static void SortRadixMsd(this int[] arrayToBeSorted)
        {
            // TODO: Implement me
        }

        private static void SortRadixMsd(this uint[] arrayToBeSorted)
        {
            // TODO: Implement me
        }

        public static Int32 SortRadixMsdShortThreshold { get; set; } = 1024;
        public static Int32 SortRadixMsdUShortThreshold { get; set; } = 1024;
        public static Int32 SortRadixMsdULongThreshold { get; set; } = 1024;
        public static Int32 SortRadixMsdLongThreshold { get; set; } = 64;
        public static Int32 SortRadixMsdDoubleThreshold { get; set; } = 1024;


        // Port of Victor's articles in Dr. Dobb's Journal January 14, 2011
        // Plain function In-place MSD Radix Sort implementation (simplified).
        private const int PowerOfTwoRadix       = 256;
        private const int Log2ofPowerOfTwoRadix =   8;
        private const int PowerOfTwoRadixDouble       = 4096;
        private const int Log2ofPowerOfTwoRadixDouble =   12;

        private static void RadixSortMsdULongInner(ulong[] a, int first, int length, int shiftRightAmount, Action<ulong[], int, int> baseCaseInPlaceSort)
        {
            if (length < SortRadixMsdULongThreshold)
            {
                baseCaseInPlaceSort(a, first, length);
                return;
            }
            int last = first + length - 1;
            const ulong bitMask = PowerOfTwoRadix - 1;

            var count = HistogramByteComponents(a, first, last, shiftRightAmount);

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin   = new int[PowerOfTwoRadix];
            int nextBin = 1;
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadix] = -1;         // sentinal
            for (int i = 1; i < PowerOfTwoRadix; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];

            for (int _current = first; _current <= last;)
            {
                ulong digit;
                ulong current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                while (endOfBin[digit = (current_element >> shiftRightAmount) & bitMask] != _current)
                    Swap(ref current_element, a, endOfBin[digit]++);
                a[_current] = current_element;

                endOfBin[digit]++;
                while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                _current = endOfBin[nextBin - 1];
            }
            if (shiftRightAmount > 0)          // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix ) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else                                            shiftRightAmount  = 0;

                for (int i = 0; i < PowerOfTwoRadix; i++ )
                    RadixSortMsdULongInner( a, startOfBin[i], endOfBin[i] - startOfBin[i], shiftRightAmount, baseCaseInPlaceSort );
            }
        }

        private static void devFunction(ulong[] a, int first, int length, int shiftRightAmount)
        {
            var endOfBin = new int[PowerOfTwoRadix];
            int last = first + length - 1;
            const ulong bitMask = PowerOfTwoRadix - 1;

            for (int _current = first; _current <= last;)
            {
                ulong digit;
                ulong current_element = a[_current];
                while (true)
                {
                    digit = (current_element >> shiftRightAmount) & bitMask;
                    if (endOfBin[digit] == _current) break;
                    Swap(ref current_element, a, endOfBin[digit]++);
                }
                a[_current] = current_element;
            }
        }
        private static void devFunctionUnrolled(ulong[] a, int first, int length, int shiftRightAmount)
        {
            var endOfBin = new int[PowerOfTwoRadix];
            int last = first + length - 1;
            const ulong bitMask = PowerOfTwoRadix - 1;

            for (int _current = first; _current <= last;)
            {
                ulong digit;
                var elementBuffer = new ulong[4];
                int index = 0;
                elementBuffer[index] = a[_current];
                while (true)
                {
                    digit = (elementBuffer[index] >> shiftRightAmount) & bitMask;
                    if (endOfBin[digit] == _current) break;
                    elementBuffer[++index] = a[endOfBin[digit]];
                    digit = (elementBuffer[index] >> shiftRightAmount) & bitMask;
                    if (endOfBin[digit] == _current)
                    {
                        a[endOfBin[digit]++] = elementBuffer[index];
                        break;
                    }
                    elementBuffer[++index] = a[endOfBin[digit]++];
                }
                a[_current] = elementBuffer[index];
            }
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixMsd(this ulong[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(ulong) * 8 - Log2ofPowerOfTwoRadix;
            // InsertionSort could be passed in as another base case since it's in-place
            RadixSortMsdULongInner(arrayToBeSorted, 0, arrayToBeSorted.Length, shiftRightAmount, Array.Sort);
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static ulong[] SortRadixMsdInPlaceFunc(this ulong[] arrayToBeSorted)
        {
            arrayToBeSorted.SortRadixMsd();
            return arrayToBeSorted;
        }

        private static void RadixSortMsdLongInner(long[] a, int first, int length, int shiftRightAmount, Action<long[], int, int> baseCaseInPlaceSort)
        {
            if (length < SortRadixMsdLongThreshold)
            {
                baseCaseInPlaceSort(a, first, length);
                //InsertionSort(a, first, length);
                return;
            }
            int last = first + length - 1;
            const ulong bitMask = PowerOfTwoRadix - 1;
            const ulong halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;

            var count = HistogramByteComponents(a, first, last, shiftRightAmount);

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin   = new int[PowerOfTwoRadix];
            int nextBin = 1;
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadix] = -1;         // sentinal
            for (int i = 1; i < PowerOfTwoRadix; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];
            int bucketsUsed = 0;
            for (int i = 0; i < count.Length; i++)
                if (count[i] > 0) bucketsUsed++;

            if (bucketsUsed > 1)
            {
                if (shiftRightAmount == 56)     // Most significant digit
                {
                    for (int _current = first; _current <= last;)
                    {
                        ulong digit;
                        long current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                        while (endOfBin[digit = ((ulong)current_element >> shiftRightAmount) ^ halfOfPowerOfTwoRadix] != _current)
                            Swap(ref current_element, a, endOfBin[digit]++);
                        a[_current] = current_element;

                        endOfBin[digit]++;
                        while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                        _current = endOfBin[nextBin - 1];
                    }
                }
                else
                {
                    for (int _current = first; _current <= last;)
                    {
                        ulong digit;
                        long current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                        while (endOfBin[digit = ((ulong)current_element >> shiftRightAmount) & bitMask] != _current)
                            Swap(ref current_element, a, endOfBin[digit]++);
                        a[_current] = current_element;

                        endOfBin[digit]++;
                        while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                        _current = endOfBin[nextBin - 1];
                    }
                }
                if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
                {
                    shiftRightAmount = shiftRightAmount >= Log2ofPowerOfTwoRadix ? shiftRightAmount -= Log2ofPowerOfTwoRadix : 0;

                    for (int i = 0; i < PowerOfTwoRadix; i++)
                        RadixSortMsdLongInner(a, startOfBin[i], endOfBin[i] - startOfBin[i], shiftRightAmount, baseCaseInPlaceSort);
                }
            }
            else
            {
                if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
                {
                    shiftRightAmount = shiftRightAmount >= Log2ofPowerOfTwoRadix ? shiftRightAmount -= Log2ofPowerOfTwoRadix : 0;
                    RadixSortMsdLongInner(a, first, length, shiftRightAmount, baseCaseInPlaceSort);
                }
            }
        }
        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixMsd(this long[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(ulong) * 8 - Log2ofPowerOfTwoRadix;
            // InsertionSort could be passed in as another base case since it's in-place
            RadixSortMsdLongInner(arrayToBeSorted, 0, arrayToBeSorted.Length, shiftRightAmount, Array.Sort);
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static long[] SortRadixMsdInPlaceFunc(this long[] arrayToBeSorted)
        {
            arrayToBeSorted.SortRadixMsd();
            return arrayToBeSorted;
        }

        private static void RadixSortDoubleInner(double[] a, int first, int length, int shiftRightAmount, Action<double[], int, int> baseCaseInPlaceSort)
        {
            if (length < SortRadixMsdDoubleThreshold)
            {
                baseCaseInPlaceSort(a, first, length);
                return;
            }
            int last = first + length - 1;
            const ulong bitMask = 0xfff;        // 12-bits for the exponent and process mantissa 12-bits at a time

            var count = Histogram12bitComponents(a, first, last, shiftRightAmount);

            var startOfBin = new int[PowerOfTwoRadixDouble + 1];
            var endOfBin   = new int[PowerOfTwoRadixDouble];
            int nextBin = 1;
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadixDouble] = -1;         // sentinal
            for (int i = 1; i < PowerOfTwoRadixDouble; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];

            if (shiftRightAmount == 52)     // Exponent
            {
                for (int _current = first; _current <= last;)
                {
                    ulong digit;
                    double current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                    while (endOfBin[digit = ((ulong)current_element >> shiftRightAmount) ^ 2048] != _current)
                        Swap(ref current_element, a, endOfBin[digit]++);
                    a[_current] = current_element;

                    endOfBin[digit]++;
                    while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                    _current = endOfBin[nextBin - 1];
                }
            }
            else     // Mantissa
            {
                for (int _current = first; _current <= last;)
                {
                    ulong digit;
                    double current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                    while (endOfBin[digit = ((ulong)current_element >> shiftRightAmount) & bitMask] != _current)
                        Swap(ref current_element, a, endOfBin[digit]++);
                    a[_current] = current_element;

                    endOfBin[digit]++;
                    while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                    _current = endOfBin[nextBin - 1];
                }
            }
            if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadixDouble) shiftRightAmount -= Log2ofPowerOfTwoRadixDouble;
                else shiftRightAmount = 0;

                for (int i = 0; i < PowerOfTwoRadixDouble; i++)
                    RadixSortDoubleInner(a, startOfBin[i], endOfBin[i] - startOfBin[i], shiftRightAmount, baseCaseInPlaceSort);
            }
        }
        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixMsd(this double[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(double) * 8 - Log2ofPowerOfTwoRadixDouble;
            // InsertionSort could be passed in as another base case since it's in-place
            RadixSortDoubleInner(arrayToBeSorted, 0, arrayToBeSorted.Length, shiftRightAmount, Array.Sort);
        }

        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static double[] SortRadixMsdInPlaceFunc(this double[] arrayToBeSorted)
        {
            arrayToBeSorted.SortRadixMsd();
            return arrayToBeSorted;
        }

#if false
        private static void RadixSortUnsignedPowerOf2RadixSimple1(ulong[] a, int first, int length, int currentDigit, int Threshold)
        {
            if (length < Threshold)
            {
                //InsertionSort(a, first, length);
                Array.Sort(a, first, length);
                return;
            }
            int last = first + length - 1;

            var count = HistogramByteComponents(a, first, length, currentDigit);

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin = new int[PowerOfTwoRadix];
            int nextBin = 1;
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadix] = -1;         // sentinal
            for (int i = 1; i < PowerOfTwoRadix; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];

            var union = new UInt64ByteUnion();
            for (int _current = first; _current <= last;)
            {
                ulong digit;
                ulong current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                while (true)
                {
                    union.integer = current_element;
                    if (endOfBin[digit = (current_element & bitMask) >> shiftRightAmount] != _current)
                        Swap(ref current_element, a, endOfBin[digit]++);
                }
                a[_current] = current_element;

                endOfBin[digit]++;
                while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                _current = endOfBin[nextBin - 1];
            }
            currentDigit--;
            if (currentDigit >= 0)                     // end recursion when all the bits have been processes
            {
                for (int i = 0; i < PowerOfTwoRadix; i++)
                    RadixSortUnsignedPowerOf2RadixSimple1(a, startOfBin[i], endOfBin[i] - startOfBin[i], currentDigit, Threshold);
            }
        }
        public static ulong[] RadixSortMsd1(this ulong[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(ulong) * 8 - Log2ofPowerOfTwoRadix;
            ulong bitMask = ((ulong)(PowerOfTwoRadix - 1)) << shiftRightAmount;  // bitMask controls/selects how many and which bits we process at a time
            const int Threshold = 1000;
            int currentDigit = 7;
            RadixSortUnsignedPowerOf2RadixSimple1(arrayToBeSorted, 0, arrayToBeSorted.Length, currentDigit, Threshold);
            return arrayToBeSorted;
        }
#endif
        private static void RadixSortMsdUShortInner(ushort[] a, int first, int length, ushort bitMask, int shiftRightAmount, Action<ushort[], int, int> baseCaseInPlaceSort)
        {
            //Console.WriteLine("Lower: first = {0} length = {1} bitMask = {2:X} shiftRightAmount = {3} ", first, length, bitMask, shiftRightAmount);
            if (length < SortRadixMsdUShortThreshold)
            {
                baseCaseInPlaceSort(a, first, length);
                return;
            }
            int last = first + length - 1;

            var count = new int[PowerOfTwoRadix];
            for (int i = 0; i < PowerOfTwoRadix; i++)
                count[i] = 0;
            //Console.WriteLine("inArray: ");
            for (int _current = first; _current <= last; _current++)
            {
                //Console.Write("{0:X} ", a[_current]);
                count[(a[_current] & bitMask) >> shiftRightAmount]++;
            }
            //Console.WriteLine();

            //Console.WriteLine("count: ");
            //for (int i = 0; i < PowerOfTwoRadix; i++)
            //    Console.Write(count[i] + " ");
            //Console.WriteLine();

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin   = new int[PowerOfTwoRadix];
            int nextBin = 1;
            //Console.WriteLine("EndOfBin: ");
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadix] = -1;         // sentinal
            //Console.Write(endOfBin[0] + " ");
            for (int i = 1; i < PowerOfTwoRadix; i++)
            {
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];
                //Console.Write(endOfBin[i] + " ");
            }
            //Console.WriteLine();

            for (int _current = first; _current <= last;)
            {
                ushort digit;
                ushort current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                while (endOfBin[digit = (ushort)((current_element & bitMask) >> shiftRightAmount)] != _current)
                    Swap(ref current_element, a, endOfBin[digit]++);
                a[_current] = current_element;

                endOfBin[digit]++;
                while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                _current = endOfBin[nextBin - 1];
            }
            bitMask >>= Log2ofPowerOfTwoRadix;
            if (bitMask != 0)                     // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else shiftRightAmount = 0;

                for (int i = 0; i < PowerOfTwoRadix; i++)
                {
                    if (endOfBin[i] - startOfBin[i] > 0)    // TODO: This should not be needed and is only there to ease porting to C#
                        RadixSortMsdUShortInner(a, startOfBin[i], endOfBin[i] - startOfBin[i], bitMask, shiftRightAmount, baseCaseInPlaceSort);
                }
            }
        }
        private static ushort[] SortRadixMsdInPlaceFunc2(this ushort[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(ushort) * 8 - Log2ofPowerOfTwoRadix;
            ushort bitMask = (ushort)(((ushort)(PowerOfTwoRadix - 1)) << shiftRightAmount);  // bitMask controls/selects how many and which bits we process at a time
            // InsertionSort could be passed in as another base case since it's in-place
            RadixSortMsdUShortInner(arrayToBeSorted, 0, arrayToBeSorted.Length, bitMask, shiftRightAmount, Array.Sort);
            return arrayToBeSorted;
        }
    }
}
