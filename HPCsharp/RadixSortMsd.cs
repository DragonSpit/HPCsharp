// TODO: Create a generic version that can sort multiple data types, possibly like was done with Fill(), where we check which data type it is and call the appropriate
//       function underneath
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

        // Port of Victor's article in Dr. Dobb's Journal January 14, 2011
        // Plain function In-place MSD Radix Sort implementation (simplified).
        private const int PowerOfTwoRadix       = 256;
        private const int Log2ofPowerOfTwoRadix =   8;

        private static void RadixSortUnsignedPowerOf2RadixSimple(ulong[] a, int first, int last, ulong bitMask, int shiftRightAmount, int Threshold)
        {
            if ((last - first) < Threshold)
            {
                InsertionSort(a, first, last - first + 1);
                return;
            }

            var count = new int[PowerOfTwoRadix];
            for (int i = 0; i < PowerOfTwoRadix; i++)
                count[i] = 0;
            for (int _current = first; _current <= last; _current++)
                count[(a[_current] & bitMask) >> shiftRightAmount]++;

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin   = new int[PowerOfTwoRadix];
            int nextBin = 1;
            startOfBin[0] = endOfBin[0] = 0; startOfBin[PowerOfTwoRadix] = -1;         // sentinal
            for (int i = 1; i < PowerOfTwoRadix; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];

            for (int _current = first; _current <= last;)
            {
                ulong digit;
                ulong current_element = a[_current];  // get the compiler to recognize that a register can be used for the loop instead of a[_current] memory location
                while (endOfBin[digit = ((current_element & bitMask) >> shiftRightAmount)] != _current)
                    Swap(ref current_element, a, endOfBin[digit]++);
                a[_current] = current_element;

                endOfBin[digit]++;
                while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                _current = endOfBin[nextBin - 1];
            }
            bitMask >>= Log2ofPowerOfTwoRadix;
            if (bitMask != 0 )                     // end recursion when all the bits have been processes
            {
                if (shiftRightAmount >= Log2ofPowerOfTwoRadix ) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                else                                            shiftRightAmount  = 0;

                //Console.WriteLine("Lower: bitMask = {0:X} shiftRightAmount = {1}", bitMask, shiftRightAmount);
                for (int i = 0; i < PowerOfTwoRadix; i++ )
                    RadixSortUnsignedPowerOf2RadixSimple( a, startOfBin[i], endOfBin[i] - startOfBin[i] - 1, bitMask, shiftRightAmount, Threshold );
            }
        }
        public static ulong[] RadixSortMsd(this ulong[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(ulong) * 8 - Log2ofPowerOfTwoRadix;
            ulong bitMask = ((ulong)(PowerOfTwoRadix - 1)) << shiftRightAmount;  // bitMask controls/selects how many and which bits we process at a time
            const int Threshold = 0;
            //Console.WriteLine("Root: bitMask = {0:X} shiftRightAmount = {1}", bitMask, shiftRightAmount);
            RadixSortUnsignedPowerOf2RadixSimple(arrayToBeSorted, 0, arrayToBeSorted.Length - 1, bitMask, shiftRightAmount, Threshold);
            return arrayToBeSorted;
        }

        private static void RadixSortUnsignedPowerOf2RadixSimple(ushort[] a, int first, int length, ushort bitMask, int shiftRightAmount, int Threshold)
        {
            //Console.WriteLine("Lower: first = {0} length = {1} bitMask = {2:X} shiftRightAmount = {3} ", first, length, bitMask, shiftRightAmount);
            if (length < Threshold)
            {
                //Console.WriteLine("InsertionSort: start = {0} length = {1}", first, length);
                InsertionSort(a, first, length);
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
                        RadixSortUnsignedPowerOf2RadixSimple(a, startOfBin[i], endOfBin[i] - startOfBin[i], bitMask, shiftRightAmount, Threshold);
                }
            }
        }
        public static ushort[] RadixSortMsd(this ushort[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(ushort) * 8 - Log2ofPowerOfTwoRadix;
            ushort bitMask = (ushort)(((ushort)(PowerOfTwoRadix - 1)) << shiftRightAmount);  // bitMask controls/selects how many and which bits we process at a time
            const int Threshold = 2;
            //Console.WriteLine("Root: bitMask = {0:X} shiftRightAmount = {1}", bitMask, shiftRightAmount);
            RadixSortUnsignedPowerOf2RadixSimple(arrayToBeSorted, 0, arrayToBeSorted.Length, bitMask, shiftRightAmount, Threshold);
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
