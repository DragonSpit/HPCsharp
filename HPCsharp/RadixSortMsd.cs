// TODO: Create a generic version that can sort multiple data types, possibly like was done with Fill(), where we check which data type it is and call the appropriate
//       function underneath
// TODO: Use Array.Sort as the base algorithm (recursion termination case) for MSD Radix Sort, since it's in-place and uses Introspective Sort in the latest version
//       of .NET. Find the optimal threshold, which could be pretty large.
// TODO: One way to experiment to small % performance enhancements is to create two versions and compare their performance against each other. Plus find your statistical
//       analysis stuff and apply it as well. We need to be able to capture many small performance improvements.
// TODO: Implement Malte's discovery of taking advantage of ILP of the CPU, by "unrolling" series of swaps by reducing dependencies across several swaps.
//       It may be possible to create a function to generalize this method and make it available to developers, to extract more performance out of cascaded swaps.
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

        public static void SortRadixMsd(this byte[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
        }

        public static void SortRadixMsd(this sbyte[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
        }

        public static void SortRadixMsd(this ushort[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
        }

        public static void SortRadixMsd(this short[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlace();
        }

        private static void SortRadixMsd(this long[] arrayToBeSorted)
        {
        }

        private static void SortRadixMsd(this ulong[] arrayToBeSorted)
        {
        }

        public static Int32 SortRadixMsdShortThreshold { get; set; } = 1024;
        public static Int32 SortRadixMsdUShortThreshold { get; set; } = 1024;
        public static Int32 SortRadixMsdULongThreshold { get; set; } = 1024;
        public static Int32 SortRadixMsdLongThreshold { get; set; } = 1024;
        public static Int32 SortRadixMsdDoubleThreshold { get; set; } = 1024;


        // Port of Victor's articles in Dr. Dobb's Journal January 14, 2011
        // Plain function In-place MSD Radix Sort implementation (simplified).
        private const int PowerOfTwoRadix       = 256;
        private const int Log2ofPowerOfTwoRadix =   8;
        private const int PowerOfTwoRadixDouble       = 4096;
        private const int Log2ofPowerOfTwoRadixDouble =   12;

        private static void RadixSortMsdULongInner(ulong[] a, int first, int length, int shiftRightAmount)
        {
            if (length < SortRadixMsdULongThreshold)
            {
                //InsertionSort(a, first, length);
                Array.Sort(a, first, length);
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
                    RadixSortMsdULongInner( a, startOfBin[i], endOfBin[i] - startOfBin[i], shiftRightAmount );
            }
        }
        public static ulong[] RadixSortMsd(this ulong[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(ulong) * 8 - Log2ofPowerOfTwoRadix;
            RadixSortMsdULongInner(arrayToBeSorted, 0, arrayToBeSorted.Length, shiftRightAmount);
            return arrayToBeSorted;
        }

        private static void RadixSortMsdSignedInner(long[] a, int first, int length, int shiftRightAmount)
        {
            if (length < SortRadixMsdLongThreshold)
            {
                //InsertionSort(a, first, length);
                Array.Sort(a, first, length);
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

            if (shiftRightAmount == 56)     // Most significant digit
            {
                for (int _current = first; _current <= last;)
                {
                    ulong digit;
                    long current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                    while (endOfBin[digit = ((ulong)current_element >> shiftRightAmount) + 128] != _current)
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
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else shiftRightAmount = 0;

                for (int i = 0; i < PowerOfTwoRadix; i++)
                    RadixSortMsdSignedInner(a, startOfBin[i], endOfBin[i] - startOfBin[i], shiftRightAmount);
            }
        }
        public static long[] RadixSortMsd(this long[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(ulong) * 8 - Log2ofPowerOfTwoRadix;
            RadixSortMsdSignedInner(arrayToBeSorted, 0, arrayToBeSorted.Length, shiftRightAmount);
            return arrayToBeSorted;
        }

        private static void RadixSortDoubleInner(double[] a, int first, int length, int shiftRightAmount)
        {
            if (length < SortRadixMsdDoubleThreshold)
            {
                //InsertionSort(a, first, length);
                Array.Sort(a, first, length);
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
                    while (endOfBin[digit = ((ulong)current_element >> shiftRightAmount) + 2048] != _current)
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
                    RadixSortDoubleInner(a, startOfBin[i], endOfBin[i] - startOfBin[i], shiftRightAmount);
            }
        }
        public static double[] RadixSortMsd(this double[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(double) * 8 - Log2ofPowerOfTwoRadixDouble;
            RadixSortDoubleInner(arrayToBeSorted, 0, arrayToBeSorted.Length, shiftRightAmount);
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
        private static void RadixSortMsdUShortInner(ushort[] a, int first, int length, ushort bitMask, int shiftRightAmount)
        {
            //Console.WriteLine("Lower: first = {0} length = {1} bitMask = {2:X} shiftRightAmount = {3} ", first, length, bitMask, shiftRightAmount);
            if (length < SortRadixMsdUShortThreshold)
            {
                //Console.WriteLine("InsertionSort: start = {0} length = {1}", first, length);
                //InsertionSort(a, first, length);
                Array.Sort(a, first, length);
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
                        RadixSortMsdUShortInner(a, startOfBin[i], endOfBin[i] - startOfBin[i], bitMask, shiftRightAmount);
                }
            }
        }
        public static ushort[] RadixSortMsd(this ushort[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(ushort) * 8 - Log2ofPowerOfTwoRadix;
            ushort bitMask = (ushort)(((ushort)(PowerOfTwoRadix - 1)) << shiftRightAmount);  // bitMask controls/selects how many and which bits we process at a time
            RadixSortMsdUShortInner(arrayToBeSorted, 0, arrayToBeSorted.Length, bitMask, shiftRightAmount);
            return arrayToBeSorted;
        }

        private static long[] SortRadixMsdInplaceFunc(this long[] arrayToBeSorted)
        {
            return arrayToBeSorted;
        }

        private static void SortRadixMsd(this double[] arrayToBeSorted)
        {
        }

        private static double[] SortRadixMsdInplaceFunc(this double[] arrayToBeSorted)
        {
            return arrayToBeSorted;
        }
    }
}
