// TODO: Write a technical paper on RadixSortFaster, with it's new method to improve memory access pattern of Radix Sort, which is especially affective when sorting
//       arrays or user defined classes, which use references and thus can be scattered all over the heap. Measurements are showing 10X performance improvement.
// TODO: To potentially improve performance of LSD Radix Sort of User Defined Types, create an array of structs that contain a reference to each UDT and key to be sorted on. This way only a single
//       array is being read/written instead of two, which may perform better than two arrays being read/written currently.
// TODO: Try Radix Sort by processing 4-bits at a time to reduce random memory access pattern. Characterize memory access patterns that that CPU can support. When does the performance fall off? 8 buffers?
// TODO: To reduce random memory access pattern develop a multi-buffer class (maybe) that you write thru, which has multiple buffers that you write to, and then automatically
//       flushes them to memory when the individual buffer size reaches its limit. This turns random memory accesses into sequential memory accesses. It also allows
//       to flush all of the buffers at any time. This is a similar strategy to what Intel used to speed up Radix Sort. Then play with the buffer size - i.e. is it cache
//       line size or bigger. This would be a class that you write thru. You would set the address of each destination for each buffer, and then you would write data thru
//       individual buffer to system memory.
// TODO: Make all multi-dimensional buffers a single array, to keep cache usage and mapping to the same cache location impossible.
// TODO: For parallel algorithm, parallel the counting portion of the algorithm, as I've done in my Dr. Dobb's papers for the parallel counting sort and MSD Radix Sort.
//       To start with don't parallel the permuting portion of the algorithm until we figure out how to do it with a performance gain.
// TODO: Figure out why for pre-sorted arrays of uint Radix Sort's 4 passes are much slower on the middle two passes than on the first and last pass. The last pass is
//       understandable since most likely all elements go into a single bin and that's why it's fast. This could be an opportunity to expose a weakness in this algorithm
//       or an opportunity to fix this weakness.
// TODO: Change the count array into a 1-D array to minimize cache contention, since 2-D array gets allocated one row at a time and may cache interfere between rows,
//       depending on how each row gets allocated. With 1-D the memory layout is guaranteed to be contigous, which should produce less cache contention.
//       Do the same with startOfBin 2-D array. It'll be a little bit more painful to use, but performance gains should make it worthwhile.
// TODO: Implement a generic Sort (in-place and not versions) for all of the data types that John listed that would select internally which algorithm to use, so that
//       the user doesn't have to. Maybe allow the user to select the algorithm.
// TODO: Extend Radix Sort to borrow some good ideas from the Radix Sort video and expand on them, such instead of returning just one number from the user defined
//       function, but also accept returning a Tuple of supported data types, and then sort based on these in-order, and throw an unsupported type exception if the user
//       provides a data type that is not supported.
// TODO: Separate the Counting Sort portion from Radix Sort and not only parallelize the counting portion, since that parallelizes counting and reading of the source array
//       but also parallelize the writing portion by splitting up the count array into either divide-and-conquer write method, or more regular chunks.
// TODO: Instead of divide-and-conquer parallel Count by splitting the input array into page size chunks on cache line boundaries (possibly by checking the array starting
//       pointer address) and then divide on page boundaried in a for loop, which should lead to even higher performance.
// TODO: Do a similar method of splitting up the Count array into page size chunks and checking the destination pointer address and splitting up by either divide-and-conquer
//       and by a for loop where each iteration is in parallel and a certain number of pages.
// TODO: Add a task to clean-up some of the sorting interfaces to not have "ref to arrays", just allocate needed arrays internally (less for the user to worry about)
//       when the additional memory is needed and just document it not being a true-inplace algorithm, but just has an in-place interface
// TODO: Document these algorithms as "not truly in-place", but providing an in-place interface. Explain how much additional memory each algorithm uses.
// TODO: Instead of masking and shifting in the inner loop of Radix Sort, use the union, once writes have been de-randomized, it may to improve performance then.
//       Tried replacing the inner loop with union and it turned out to be slower, but this was before de-randomization.
// TODO: Reduce memory footprint of partial array sort by allocating only enough memory for the partial array, instead of needing a temporary array that is a full size.
// TODO: Figure out how to end RadixSort early for those cases where the keys being sorted are within a limited range, such as for keys in a database - e.g. fewer than 16 M keys which are 0 to 16M
//       which is within 24-bits the lower bits. Bring this optimization from MSD Radix Sort, as it should help here as well. It doesn't help LSD Radix Sort as much
//       because for slong when negative and positive values are used we end up with two bins as we get to more significant digits (unless we limit it to just positives eventhough
//       the data type is an slong). However, two bins are find (and possibly even more), since if these bins are already in-order then there is permuting is not needed!
//       This will be a huge speed-up for John's use case! Is it easy to tell if the bins are in the correct order quickly? All negative and all positive? This is extra overhead.
// TODO: Wonder if paging the temporary/destination array into memory first would help performance. Otherwise, if C# does it lazily, then random access may make that slower? Maybe.
// TODO: See if Wikipedia Counting Sort ideas and concepts could possibly help improve performance https://en.wikipedia.org/wiki/Counting_sort
// TODO: Since LSD Radix Sort now does the Counting portion of the algorithm in a single pass outside of the permutation loop that processes based on digits, we could add statistics detection
//       during that pass for very low cost, or possibly almost no cost, since that pass is memory bandwidth limited. Detection of presorted, reverse sorted and constant (which is also presorted)
//       should be simple to detect, even in parallel.
// TODO: If the input array is completely pre-sorted then just output it, otherwise if a certain percentage is pre-sorted (i.e. close to pre-sorted) then use Array.Sort, otherwise use LSD Radix Sort.
// TODO: Apply the Counting/Histogram optimization from my blog to Radix Sort of user defined types (actually across all of the LSD Radix Sorts).
// TODO: Consistently switch to int[] for StartOfBin everywhere, since that improves performance for index operations, since in C# int is the native index type (or is it, something to double-check!)
//       It seems that C# supports several data types for indexes (int, uint, long and ulong). Need to experiment which data type C# prefers (guessing uint, but not sure) to generate the least IL instructions.
//       Post the best type to use on https://stackoverflow.com/questions/16486533/type-of-array-index-in-c once I figure out what that is, whichever generates the least amount of IL
// TODO: figure out why for long[] trick of checking for a single bucket, we still have to sort using the last digit, otherwise incrementing test case fails for some sizes of input array.
// TODO: Improve the interface of LSD Radix Sort function that pass in a key array and a user type array and sort both simultaneously, to return a tuple instead of using a reference, since we want to return
//       two arrays.
// TODO: Consider LSD Radix Sort version that returns sorted array (e.g. array of bytes that was passed in to be sorted) and also creates and returns an array of indexes. In this case, the developer doesn't
//       even have to provide the second input array.
// TODO: JavaScript original version of LSD Radix Sort for UDT's is way faster when the array fits in cache (10X). This is a good behavior to test and document in JavaScript and C#
// TODO: Use the same optimization as done for MSD Radix Sort, which detects if all values fell in a single bin and then doesn't do any work. This helps not only the constant array case, but also
//       the case of indexes being only as large as the array size. This optimization is worthwhile since it helps more than one special case.
// TODO: Optimize floating-point Radix Sort by minimize flipping by doing flipping once at the beginning and at the end.
// TODO: Implement LSD Radix sort for the larger memory array support of C# that I found on StackOverflow.
// TODO: Implement Rick's request on HPCsharp github repo for support of double[] keys, separate from the array being sorted, also with swapped arguments
//       to match Array.Sort(array1, array2) interface. These functions also need to be updated to the latest highest performance versions.
// TODO: Implement ascending/descending options for all LSD Radix Sort algorithms, like is done for the byte[] case, as this can be done without any performance penalties. Show that
//       Array.Sort has performance penalties due to the need to create a lambda function to reverse the comparison.
// TODO: Implement adaptive Radix Sort (serial and parallel) where if there is enough memory, LSD Radix Sort is used for faster performance, otherwise MSD Radix Sort kicks in when memory is tight.
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        /// <summary>
        /// Sort an array of unsigned integers using Radix Sorting algorithm (least significant digit variation)
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns>array of unsigned integers</returns>
        private static uint[] SortRadix3(this uint[] inputArray)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            uint[] outputArray = new uint[inputArray.Length];
            uint[] count       = new uint[numberOfBins];
            uint[] startOfBin  = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                for (uint i = 0; i < numberOfBins; i++)
                    count[i] = 0;
                for (uint current = 0; current < inputArray.Length; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    count[ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++;

                startOfBin[0] = 0;
                for (uint i = 1; i < numberOfBins; i++) // Victor J. Duvanenko
                    startOfBin[i] = (uint)(startOfBin[i - 1] + count[i - 1]);

                for (uint current = 0; current < inputArray.Length; current++)
                    outputArray[startOfBin[ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++] = inputArray[current];

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;

                uint[] tmp  = inputArray;       // swap input and output arrays
                inputArray  = outputArray;
                outputArray = tmp;
            }
            if (outputArrayHasResult)
                for (uint current = 0; current < inputArray.Length; current++)    // copy from output array into the input array
                    inputArray[current] = outputArray[current];

            return inputArray;
        }
        // For now, it's fixed at 8-bits per digit. We can generalize later.
        private static void ExtractDigits(UInt32 value, uint[] digits)
        {
            digits[0] = (value &       0xff);
            digits[1] = (value &     0xff00) >> 8;
            digits[2] = (value &   0xff0000) >> 16;
            digits[3] = (value & 0xff000000) >> 24;
        }
        /// <summary>
        /// Sort an array of unsigned integers using Radix Sorting algorithm (least significant digit variation)
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns>array of unsigned integers</returns>
        private static uint[] SortRadix2(this uint[] inputArray)
        {
            int numberOfBins = 256;
            int numberOfDigits = 4;
            int Log2ofPowerOfTwoRadix = 8;
            int d = 0;
            uint[] outputArray = new uint[inputArray.Length];

            uint[][] count = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];
            uint[][] startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            //Stopwatch stopwatch = new Stopwatch();
            //long frequency = Stopwatch.Frequency;
            ////Console.WriteLine("  Timer frequency in ticks per second = {0}", frequency);
            //long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            //stopwatch.Restart();
            var digits = new byte[4];
            for (uint current = 0; current < inputArray.Length; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                uint value = inputArray[current];
                digits[0] = (byte)(value & 0xff);
                digits[1] = (byte)((value & 0xff00) >> 8);
                digits[2] = (byte)((value & 0xff0000) >> 16);
                digits[3] = (byte)((value & 0xff000000) >> 24);

                int i = 0;
                foreach(var digit in digits)
                {
                    count[i][digit]++;
                    i++;
                }
            }
            //stopwatch.Stop();
            //double timeForCounting = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            //Console.WriteLine("Time for counting: {0}", timeForCounting);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            d = 0;
            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                //stopwatch.Restart();
                uint[] startOfBinLoc = startOfBin[d];
                for (uint current = 0; current < inputArray.Length; current++)
                {
                    //outputArray[startOfBinLoc[ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++] = inputArray[current];
                    outputArray[startOfBinLoc[(inputArray[current] & bitMask) >> shiftRightAmount]++] = inputArray[current];
                }
                //stopwatch.Stop();
                //double timeForPermuting = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
                //Console.WriteLine("Time for permuting: {0}", timeForPermuting);

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;
                d++;

                uint[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }
            if (outputArrayHasResult)
                for (uint current = 0; current < inputArray.Length; current++)    // copy from output array into the input array
                    inputArray[current] = outputArray[current];

            return inputArray;
        }

        public static byte[] SortRadix(this byte[] arrayToBeSorted)
        {
            return arrayToBeSorted.SortCountingInPlaceFunc();
        }

        public static sbyte[] SortRadix(this sbyte[] arrayToBeSorted)
        {
            return arrayToBeSorted.SortCountingInPlaceFunc();
        }

        public static ushort[] SortRadix(this ushort[] arrayToBeSorted)
        {
            return arrayToBeSorted.SortCountingInPlaceFunc();
        }

        public static short[] SortRadix(this short[] arrayToBeSorted)
        {
            return arrayToBeSorted.SortCountingInPlaceFunc();
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct Int32ByteUnion
        {
            [FieldOffset(0)]
            public byte byte0;
            [FieldOffset(1)]
            public byte byte1;
            [FieldOffset(2)]
            public byte byte2;
            [FieldOffset(3)]
            public byte byte3;

            [FieldOffset(0)]
            public Int32 integer;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct UInt32ByteUnion
        {
            [FieldOffset(0)]
            public byte byte0;
            [FieldOffset(1)]
            public byte byte1;
            [FieldOffset(2)]
            public byte byte2;
            [FieldOffset(3)]
            public byte byte3;

            [FieldOffset(0)]
            public UInt32 integer;
        }
        [StructLayout(LayoutKind.Explicit)]
        internal struct UInt64ByteUnion
        {
            [FieldOffset(0)]
            public byte byte0;
            [FieldOffset(1)]
            public byte byte1;
            [FieldOffset(2)]
            public byte byte2;
            [FieldOffset(3)]
            public byte byte3;
            [FieldOffset(4)]
            public byte byte4;
            [FieldOffset(5)]
            public byte byte5;
            [FieldOffset(6)]
            public byte byte6;
            [FieldOffset(7)]
            public byte byte7;

            [FieldOffset(0)]
            public UInt64 integer;
        }
        [StructLayout(LayoutKind.Explicit)]
        internal struct Int64ByteUnion
        {
            [FieldOffset(0)]
            public byte byte0;
            [FieldOffset(1)]
            public byte byte1;
            [FieldOffset(2)]
            public byte byte2;
            [FieldOffset(3)]
            public byte byte3;
            [FieldOffset(4)]
            public byte byte4;
            [FieldOffset(5)]
            public byte byte5;
            [FieldOffset(6)]
            public byte byte6;
            [FieldOffset(7)]
            public byte byte7;

            [FieldOffset(0)]
            public Int64 integer;
        }
        [StructLayout(LayoutKind.Explicit)]
        internal struct UInt32UShortUnion
        {
            [FieldOffset(0)]
            public ushort ushort0;
            [FieldOffset(1)]
            public ushort ushort1;

            [FieldOffset(0)]
            public UInt32 integer;
        }
        [StructLayout(LayoutKind.Explicit)]
        internal struct UInt64UShortUnion
        {
            [FieldOffset(0)]
            public ushort ushort0;
            [FieldOffset(1)]
            public ushort ushort1;
            [FieldOffset(2)]
            public ushort ushort2;
            [FieldOffset(3)]
            public ushort ushort3;

            [FieldOffset(0)]
            public UInt64 integer;
        }
        [StructLayout(LayoutKind.Explicit)]
        public struct FloatUInt32Union
        {
            [FieldOffset(0)]
            public uint uinteger;
            [FieldOffset(0)]
            public float floatValue;
        }
        [StructLayout(LayoutKind.Explicit)]
        public struct DoubleUInt64Union
        {
            [FieldOffset(0)]
            public UInt64 ulongInteger;
            [FieldOffset(0)]
            public double doubleValue;
        }

        /// <summary>
        /// Sort an array of unsigned integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// This algorithm is not in-place. This algorithm is stable.
        /// </summary>
        /// <param name="inputArray">array of unsigned integers to be sorted</param>
        /// <returns>sorted array of unsigned integers</returns>
        public static uint[] SortRadix(this uint[] inputArray)
        {
            const int bitsPerDigit = 8;
            uint numberOfBins = 1 << bitsPerDigit;
            uint numberOfDigits = (sizeof(uint) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            //Console.WriteLine("SortRadix: NumberOfDigits = {0}", numberOfDigits);
            int d = 0;
            uint[] outputArray = new uint[inputArray.Length];

            uint[][] startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = numberOfBins - 1;
            int shiftRightAmount = 0;

            //Stopwatch stopwatch = new Stopwatch();
            //long frequency = Stopwatch.Frequency;
            ////Console.WriteLine("  Timer frequency in ticks per second = {0}", frequency);
            //long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            //stopwatch.Restart();
            uint[][] count = HistogramByteComponents(inputArray, 0, inputArray.Length - 1);
            //stopwatch.Stop();
            //double timeForCounting = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            //Console.WriteLine("Time for counting: {0}", timeForCounting);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            d = 0;
            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                //stopwatch.Restart();
                uint[] startOfBinLoc = startOfBin[d];
                for (uint current = 0; current < inputArray.Length; current++)
                {
                    outputArray[startOfBinLoc[(inputArray[current] & bitMask) >> shiftRightAmount]++] = inputArray[current];
                    //Console.WriteLine("curr: {0}, index: {1}, startOfBin: {2}", current, (inputArray[current] & bitMask) >> shiftRightAmount, startOfBinLoc[(inputArray[current] & bitMask) >> shiftRightAmount]);
                }
                //stopwatch.Stop();
                //double timeForPermuting = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
                //Console.WriteLine("Time for permuting: {0}", timeForPermuting);

                bitMask <<= bitsPerDigit;
                shiftRightAmount += bitsPerDigit;
                outputArrayHasResult = !outputArrayHasResult;
                d++;

                uint[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }
            return outputArrayHasResult ? outputArray : inputArray;
        }
        /// <summary>
        /// Sort an array of unsigned integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// This algorithm is not in-place. This algorithm is stable.
        /// </summary>
        /// <param name="inOutArray">array of unsigned integers to be sorted, and where the sorted array will be returned</param>
        /// <param name="startIndex">index of the first element where sorting is to start</param>
        /// <param name="length">number of array elements to sort</param>
        /// <returns>sorted array of unsigned integers</returns>
        public static void SortRadix(this uint[] inOutArray, int startIndex, int length)
        {
            const int bitsPerDigit = 8;
            uint numberOfBins = 1 << bitsPerDigit;
            uint numberOfDigits = (sizeof(uint) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            //Console.WriteLine("SortRadix: NumberOfDigits = {0}", numberOfDigits);
            uint[] workBuffer = new uint[inOutArray.Length];
            int d;

            uint[][] startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = numberOfBins - 1;
            int shiftRightAmount = 0;

            //Stopwatch stopwatch = new Stopwatch();
            //long frequency = Stopwatch.Frequency;
            ////Console.WriteLine("  Timer frequency in ticks per second = {0}", frequency);
            //long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            //stopwatch.Restart();
            uint[][] count = HistogramByteComponents(inOutArray, startIndex, startIndex + length - 1);
            //stopwatch.Stop();
            //double timeForCounting = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            //Console.WriteLine("Time for counting: {0}", timeForCounting);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = (uint)startIndex;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            d = 0;
            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                //stopwatch.Restart();
                uint[] startOfBinLoc = startOfBin[d];
                for (int current = startIndex; current < (startIndex + length); current++)
                {
                    workBuffer[startOfBinLoc[(inOutArray[current] & bitMask) >> shiftRightAmount]++] = inOutArray[current];
                    //Console.WriteLine("curr: {0}, index: {1}, startOfBin: {2}", current, (inputArray[current] & bitMask) >> shiftRightAmount, startOfBinLoc[(inputArray[current] & bitMask) >> shiftRightAmount]);
                }
                //stopwatch.Stop();
                //double timeForPermuting = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
                //Console.WriteLine("Time for permuting: {0}", timeForPermuting);

                bitMask <<= bitsPerDigit;
                shiftRightAmount += bitsPerDigit;
                outputArrayHasResult = !outputArrayHasResult;
                d++;

                uint[] tmp = inOutArray;       // swap input and output arrays
                inOutArray = workBuffer;
                workBuffer = tmp;
            }
        }
        /// <summary>
        /// Sort an array of unsigned integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// The core algorithm is not in-place, but the interface is in-place. This algorithm is stable.
        /// </summary>
        /// <param name="inputArray">array of unsigned integers to be sorted</param>
        /// <returns>sorted array of unsigned integers</returns>
        public static void SortRadixInPlaceInterface(this uint[] inputArray)
        {
            SortRadix(inputArray, 0, inputArray.Length);
        }
        /// <summary>
        /// Sort an array of unsigned long integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// This algorithm is not in-place. This algorithm is stable.
        /// </summary>
        /// <param name="inputArray">array of unsigned long integers to be sorted</param>
        /// <returns>sorted array of unsigned long integers</returns>
        public static ulong[] SortRadix(this ulong[] inputArray)
        {
            const int bitsPerDigit = 8;
            uint numberOfBins = 1 << bitsPerDigit;
            uint numberOfDigits = (sizeof(ulong) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            int d = 0;
            var outputArray = new ulong[inputArray.Length];

            uint[][] startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            ulong bitMask = numberOfBins - 1;
            int shiftRightAmount = 0;

            uint[][] count = HistogramByteComponents(inputArray, 0, inputArray.Length - 1);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            d = 0;
            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                uint[] startOfBinLoc = startOfBin[d];
                for (uint current = 0; current < inputArray.Length; current++)
                    outputArray[startOfBinLoc[(inputArray[current] & bitMask) >> shiftRightAmount]++] = inputArray[current];

                bitMask <<= bitsPerDigit;
                shiftRightAmount += bitsPerDigit;
                outputArrayHasResult = !outputArrayHasResult;
                d++;

                ulong[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }
            return outputArrayHasResult ? outputArray : inputArray;
        }
        /// <summary>
        /// Sort an array of unsigned long integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// The core algorithm is not in-place, but the interface is in-place. This algorithm is stable.
        /// </summary>
        /// <param name="inputArray">array of unsigned long integers to be sorted</param>
        /// <returns>sorted array of unsigned long integers</returns>
        public static void SortRadixInPlaceInterface(this ulong[] inputArray)
        {
            var sortedArray = SortRadix(inputArray);
            Array.Copy(sortedArray, inputArray, inputArray.Length);
        }

        private static void PermuteArrayUsingUnion(long[] inputArray, long[] outputArray, uint[] startOfBinLoc, int shiftRightAmount)
        {
            const ulong halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;
            int whichByte = shiftRightAmount / 8;

            var union = new Int64ByteUnion();

            switch (whichByte)
            {
                case 0:
                    for (uint current = 0; current < inputArray.Length; current++)
                    {
                        union.integer = inputArray[current];
                        outputArray[startOfBinLoc[union.byte0]++] = inputArray[current];
                    }
                    break;
                case 1:
                    for (uint current = 0; current < inputArray.Length; current++)
                    {
                        union.integer = inputArray[current];
                        outputArray[startOfBinLoc[union.byte1]++] = inputArray[current];
                    }
                    break;
                case 2:
                    for (uint current = 0; current < inputArray.Length; current++)
                    {
                        union.integer = inputArray[current];
                        outputArray[startOfBinLoc[union.byte2]++] = inputArray[current];
                    }
                    break;
                case 3:
                    for (uint current = 0; current < inputArray.Length; current++)
                    {
                        union.integer = inputArray[current];
                        outputArray[startOfBinLoc[union.byte3]++] = inputArray[current];
                    }
                    break;
                case 4:
                    for (uint current = 0; current < inputArray.Length; current++)
                    {
                        union.integer = inputArray[current];
                        outputArray[startOfBinLoc[union.byte4]++] = inputArray[current];
                    }
                    break;
                case 5:
                    for (uint current = 0; current < inputArray.Length; current++)
                    {
                        union.integer = inputArray[current];
                        outputArray[startOfBinLoc[union.byte5]++] = inputArray[current];
                    }
                    break;
                case 6:
                    for (uint current = 0; current < inputArray.Length; current++)
                    {
                        union.integer = inputArray[current];
                        outputArray[startOfBinLoc[union.byte6]++] = inputArray[current];
                    }
                    break;
                case 7:
                    for (uint current = 0; current < inputArray.Length; current++)
                    {
                        union.integer = inputArray[current];
                        outputArray[startOfBinLoc[((ulong)inputArray[current] >> shiftRightAmount) ^ halfOfPowerOfTwoRadix]++] = inputArray[current];
                    }
                    break;
            }
        }
        /// <summary>
        /// Sort an array of integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// This algorithm is not in-place. This algorithm is stable.
        /// </summary>
        /// <param name="inputArray">array of integers to be sorted</param>
        /// <returns>sorted array of integers</returns>
        public static int[] SortRadix2(this int[] inputArray, bool ascending = true)
        {
            var sortedArray = inputArray.SortRadix();
            sortedArray.Reversal(0, sortedArray.Length - 1);
            return sortedArray;
        }
        /// <summary>
        /// Sort an array of integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// This algorithm is not in-place. This algorithm is stable.
        /// </summary>
        /// <param name="inputArray">array of integers to be sorted</param>
        /// <returns>sorted array of integers</returns>
        public static int[] SortRadix(this int[] inputArray)
        {
            const int bitsPerDigit = 8;
            const uint numberOfBins = 1 << bitsPerDigit;
            uint numberOfDigits = (sizeof(uint) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            int d;
            var outputArray = new int[inputArray.Length];

            int[][] startOfBin = new int[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new int[numberOfBins];
            bool outputArrayHasResult = false;

            const uint bitMask = numberOfBins - 1;
            const uint halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;
            int shiftRightAmount = 0;

            uint[][] count = HistogramByteComponents(inputArray, 0, inputArray.Length - 1);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + (int)count[d][i - 1];
            }

            d = 0;
            while (d < numberOfDigits)
            {
                int[] startOfBinLoc = startOfBin[d];

                if (d != 3)
                    for (uint current = 0; current < inputArray.Length; current++)
                        outputArray[startOfBinLoc[((uint)inputArray[current] >> shiftRightAmount) & bitMask]++] = inputArray[current];
                else
                    for (uint current = 0; current < inputArray.Length; current++)
                        outputArray[startOfBinLoc[((uint)inputArray[current] >> shiftRightAmount) ^ halfOfPowerOfTwoRadix]++] = inputArray[current];

                shiftRightAmount += bitsPerDigit;
                outputArrayHasResult = !outputArrayHasResult;
                d++;

                int[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }
            return outputArrayHasResult ? outputArray : inputArray;
        }
        /// <summary>
        /// Sort an array of signed long integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// This algorithm is not in-place. This algorithm is stable.
        /// </summary>
        /// <param name="inputArray">array of signed long integers to be sorted</param>
        /// <returns>sorted array of signed long integers</returns>
        public static long[] SortRadix(this long[] inputArray)
        {
            const int bitsPerDigit = 8;
            const uint numberOfBins = 1 << bitsPerDigit;
            uint numberOfDigits = (sizeof(ulong) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            int d;
            var outputArray = new long[inputArray.Length];

            int[][] startOfBin = new int[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new int[numberOfBins];
            bool outputArrayHasResult = false;

            const ulong bitMask = numberOfBins - 1;
            const ulong halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;
            int shiftRightAmount = 0;

            uint[][] count = HistogramByteComponents(inputArray, 0, inputArray.Length - 1);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + (int)count[d][i - 1];
            }

            d = 0;
            while (d < numberOfDigits)
            {
                int[] startOfBinLoc = startOfBin[d];

                if (d != 7)
                    for (uint current = 0; current < inputArray.Length; current++)
                        outputArray[startOfBinLoc[((ulong)inputArray[current] >> shiftRightAmount) & bitMask]++] = inputArray[current];
                else
                    for (uint current = 0; current < inputArray.Length; current++)
                        outputArray[startOfBinLoc[((ulong)inputArray[current] >> shiftRightAmount) ^ halfOfPowerOfTwoRadix]++] = inputArray[current];

                shiftRightAmount += bitsPerDigit;
                outputArrayHasResult = !outputArrayHasResult;
                d++;

                long[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }
            return outputArrayHasResult ? outputArray : inputArray;
        }
        /// <summary>
        /// Sort an array of signed long integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// This algorithm is not in-place. This algorithm is stable.
        /// Pre-sorted array is detected and sorting is avoided. Mostly pre-sorted is also detected and Array.Sort is used in that case.
        /// Threshold for what is "mostly sorted" is provided and can be set by the developer.
        /// </summary>
        /// <param name="inputArray">array of signed long integers to be sorted</param>
        /// <returns>sorted array of signed long integers</returns>
        public static long[] SortRadixWithPresortedDetection(this long[] inputArray, double fractionPresorted)
        {
            const int bitsPerDigit = 8;
            const uint numberOfBins = 1 << bitsPerDigit;
            uint numberOfDigits = (sizeof(ulong) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            int d = 0;
            var outputArray = new long[inputArray.Length];

            int[][] startOfBin = new int[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new int[numberOfBins];
            bool outputArrayHasResult = false;

            const ulong bitMask = numberOfBins - 1;
            const ulong halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;
            int shiftRightAmount = 0;

            var countsAndPresorted = HistogramByteComponentsAndStatistics(inputArray, 0, inputArray.Length - 1);

            uint[][] count = countsAndPresorted.Item1;

            if (countsAndPresorted.Item2 == inputArray.Length)  // the input array is pre-sorted
                return inputArray;
            if ((double)countsAndPresorted.Item2 / inputArray.Length >= fractionPresorted || fractionPresorted == 1.0)
            {
                Array.Sort(inputArray);
                return inputArray;
            }

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + (int)count[d][i - 1];
            }

            int[] bucketsUsed = new int[numberOfDigits];
            for (d = 0; d < numberOfDigits; d++)
                for (int i = 0; i < numberOfBins; i++)
                    if (count[d][i] > 0) bucketsUsed[d]++;

            d = 0;
            while (d < numberOfDigits)
            {
                if (bucketsUsed[d] > 1 || d == 7)   // TODO: Figure out why processing the last digit is necessary for incrementing array input test case. Yes, the most signficant digit is special, but why, even for positive only values
                {
                    int[] startOfBinLoc = startOfBin[d];

                    if (d != 7)
                        for (uint current = 0; current < inputArray.Length; current++)
                            outputArray[startOfBinLoc[((ulong)inputArray[current] >> shiftRightAmount) & bitMask]++] = inputArray[current];
                    else
                        for (uint current = 0; current < inputArray.Length; current++)
                            outputArray[startOfBinLoc[((ulong)inputArray[current] >> shiftRightAmount) ^ halfOfPowerOfTwoRadix]++] = inputArray[current];

                    outputArrayHasResult = !outputArrayHasResult;

                    long[] tmp = inputArray;       // swap input and output arrays
                    inputArray = outputArray;
                    outputArray = tmp;
                }
                shiftRightAmount += bitsPerDigit;
                d++;
            }
            return outputArrayHasResult ? outputArray : inputArray;
        }
        /// <summary>
        /// Sort an array of signed long integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// This algorithm is not in-place. This algorithm is stable.
        /// </summary>
        /// <param name="inputArray">array of signed long integers to be sorted</param>
        /// <returns>sorted array of signed long integers</returns>
        public static long[] SortRadix3(this long[] inputArray)
        {
            const int bitsPerDigit = 8;
            const int numberOfBins = 1 << bitsPerDigit;
            uint numberOfDigits = (sizeof(ulong) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            int d = 0;
            var outputArray = new long[inputArray.Length];

            int[][] startOfBin = new int[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new int[numberOfBins];
            bool outputArrayHasResult = false;

            //const ulong bitMask = numberOfBins - 1;
            const ulong halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;
            int shiftRightAmount = 0;

            uint[][] count = HistogramByteComponents(inputArray, 0, inputArray.Length - 1);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + (int)count[d][i - 1];
            }
            // Write-thru-sequencialization buffer
            const int NumberOfLongsPerWTHS = 64;
            long[] wthsBuffer = new long[numberOfBins * NumberOfLongsPerWTHS];
            int[] currIndexes = new int[numberOfBins];
            for (int i = 0; i < numberOfBins; i++)
                currIndexes[i] =  i      * NumberOfLongsPerWTHS;

            d = 0;
            while (d < numberOfDigits)
            {
                int[] startOfBinLoc = startOfBin[d];

                if (d != 7)
                {
                    for (uint current = 0; current < inputArray.Length; current++)
                    {
                        byte digit = (byte)(inputArray[current] >> shiftRightAmount);
                        if (currIndexes[digit] < ((int)digit + 1) * NumberOfLongsPerWTHS)
                        {
                            wthsBuffer[currIndexes[digit]++] = inputArray[current];
                        }
                        else
                        {
                            int outIndex = startOfBinLoc[digit];
                            int buffIndex = digit * NumberOfLongsPerWTHS;
                            int endBuffIndex = buffIndex + NumberOfLongsPerWTHS;
                            while (buffIndex < endBuffIndex)
                                outputArray[outIndex++] = wthsBuffer[buffIndex++];
                            startOfBinLoc[digit] += NumberOfLongsPerWTHS;
                            currIndexes[digit] = digit * NumberOfLongsPerWTHS;
                            wthsBuffer[currIndexes[digit]++] = inputArray[current];
                        }
                    }
                    for (int i = 0; i < numberOfBins; i++)
                    {
                        byte digit = (byte)i;
                        int outIndex = startOfBinLoc[digit];
                        int buffIndex = digit * NumberOfLongsPerWTHS;
                        int endBuffIndex = currIndexes[digit];
                        while (buffIndex < endBuffIndex)
                        {
                            outputArray[outIndex++] = wthsBuffer[buffIndex++];
                            startOfBinLoc[digit]++;
                        }
                        currIndexes[digit] = digit * NumberOfLongsPerWTHS;
                    }
                }
                else
                    for (uint current = 0; current < inputArray.Length; current++)
                        outputArray[startOfBinLoc[((ulong)inputArray[current] >> shiftRightAmount) ^ halfOfPowerOfTwoRadix]++] = inputArray[current];

                shiftRightAmount += bitsPerDigit;
                outputArrayHasResult = !outputArrayHasResult;
                d++;

                long[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }
            return outputArrayHasResult ? outputArray : inputArray;
        }
        /// <summary>
        /// Sort an array of signed long integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// The core algorithm is not in-place, but the interface is in-place. This algorithm is stable.
        /// </summary>
        /// <param name="inputArray">array of signed long integers to be sorted</param>
        /// <returns>sorted array of signed long integers</returns>
        public static void SortRadixInPlaceInterface(this long[] inputArray)
        {
            var sortedArray = SortRadix(inputArray);
            if (sortedArray != inputArray)
                Array.Copy(sortedArray, inputArray, inputArray.Length);
        }

        private static uint[] SortRadixExperimental(this uint[] inputArray)
        {
            const int bitsPerDigit = 8;
            const uint numberOfBins = 1 << bitsPerDigit;
            const uint numberOfDigits = (sizeof(uint) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            int d = 0;
            uint[] outputArray = new uint[inputArray.Length];

            uint[][] startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            //const uint bitMask = numberOfBins - 1;
            int shiftRightAmount = 0;

            uint[][] count = HistogramByteComponents(inputArray, 0, inputArray.Length - 1);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            for (d = 0; d < numberOfDigits; d++)
            {
                uint[] startOfBinLoc = startOfBin[d];
                for (uint current = 0; current < inputArray.Length; current++)
                {
                    //outputArray[startOfBinLoc[(inputArray[current] >> shiftRightAmount) & bitMask]++] = inputArray[current];
                    outputArray[startOfBinLoc[(byte)(inputArray[current] >> shiftRightAmount)]++] = inputArray[current];
                }

                shiftRightAmount += bitsPerDigit;
                outputArrayHasResult = !outputArrayHasResult;

                uint[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }
            return outputArrayHasResult ? outputArray : inputArray;
        }

        private static uint[] SortRadixExperimental2(this uint[] inputArray)
        {
            const int bitsPerDigit = 8;
            const uint numberOfBins = 1 << bitsPerDigit;
            const uint numberOfDigits = (sizeof(uint) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            //Console.WriteLine("SortRadix: NumberOfDigits = {0}", numberOfDigits);
            int d = 0;
            uint[] outputArray = new uint[inputArray.Length];

            uint[][] startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = numberOfBins - 1;
            int shiftRightAmount = 0;

            //Stopwatch stopwatch = new Stopwatch();
            //long frequency = Stopwatch.Frequency;
            ////Console.WriteLine("  Timer frequency in ticks per second = {0}", frequency);
            //long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            //stopwatch.Restart();
            uint[][] count = HistogramByteComponents(inputArray, 0, inputArray.Length - 1);
            //uint[][] count = HistogramNBitsPerComponents(inputArray, 0, inputArray.Length - 1, bitsPerDigit);

            //stopwatch.Stop();
            //double timeForCounting = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            //Console.WriteLine("Time for counting: {0}", timeForCounting);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            d = 0;
            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                //Console.WriteLine("SortRadix: NumberOfDigits = {0:X} ShiftRightAmount = {1}", bitMask, shiftRightAmount);

                //stopwatch.Restart();
                uint[] startOfBinLoc = startOfBin[d];
                for (uint current = 0; current < inputArray.Length; current++)
                {
                    outputArray[startOfBinLoc[(inputArray[current] & bitMask) >> shiftRightAmount]++] = inputArray[current];
                    //Console.WriteLine("curr: {0}, index: {1}, startOfBin: {2}", current, (inputArray[current] & bitMask) >> shiftRightAmount, startOfBinLoc[(inputArray[current] & bitMask) >> shiftRightAmount]);
                }
                //stopwatch.Stop();
                //double timeForPermuting = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
                //Console.WriteLine("Time for permuting: {0}", timeForPermuting);

                bitMask <<= bitsPerDigit;
                shiftRightAmount += bitsPerDigit;
                outputArrayHasResult = !outputArrayHasResult;
                d++;

                uint[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }
            //Console.WriteLine("SortRadix: outputArrayHasResult = {0}", outputArrayHasResult);
            return outputArrayHasResult ? outputArray : inputArray;
        }

        /// <summary>
        /// Sort an List of unsigned integers using Radix Sorting algorithm (least significant digit variation)
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns>sorted List of unsigned integers</returns>
        public static List<uint> SortRadix(this List<uint> inputArray)
        {
            var srcCopy = inputArray.ToArray();
            var sortedArray = srcCopy.SortRadix();
            var sortedList = new List<uint>(sortedArray);
            return sortedList;
        }
        /// <summary>
        /// Sort an array of unsigned integers using Radix Sorting algorithm (least significant digit variation).
        /// Derandomizes memory writes for more consistent performance across various input data distributions.
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns>sorted array of unsigned integers</returns>
        public static uint[] SortRadixDerandomizedWrites(this uint[] inputArray)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            uint cacheLineSizeInBytes = 64 * 4;
            uint cacheLineSizeInUInt32s = cacheLineSizeInBytes / 4;  // is there sizeOf() in C#
            uint[] cacheBuffers       = new uint[numberOfBins * cacheLineSizeInUInt32s];
            uint[] cacheBufferIndexes = new uint[numberOfBins];
            uint[] outputArray        = new uint[inputArray.Length];
            uint[] count              = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            uint[] startOfBin = new uint[numberOfBins];

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                for (uint i = 0; i < numberOfBins; i++)
                {
                    count[i] = 0;
                    cacheBufferIndexes[i] = 0;
                }
                for (uint current = 0; current < inputArray.Length; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    count[ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++;

                startOfBin[0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[i] = (uint)(startOfBin[i - 1] + count[i - 1]);

                for (uint current = 0; current < inputArray.Length; current++)
                {
                    uint whichBin = ExtractDigit(inputArray[current], bitMask, shiftRightAmount);
                    uint startOfBuffer = whichBin * cacheLineSizeInUInt32s;
                    if (cacheBufferIndexes[whichBin] < cacheLineSizeInUInt32s)  // place current element into its cacheBuffer
                    {
                        cacheBuffers[startOfBuffer + cacheBufferIndexes[whichBin]] = inputArray[current];
                        cacheBufferIndexes[whichBin]++;
                    }
                    else     // flush the buffer to system memory
                    {
                        uint index = startOfBuffer;
                        for (int i = 0; i < cacheLineSizeInUInt32s; i++)
                            outputArray[startOfBin[whichBin]++] = cacheBuffers[index++];
                        cacheBuffers[startOfBuffer] = inputArray[current];
                        cacheBufferIndexes[whichBin] = 1;
                    }
                    //outputArray[endOfBin[ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++] = inputArray[current];
                }
                // Flush all of the cache buffers
                for (uint whichBin = 0; whichBin < numberOfBins; whichBin++)
                {
                    uint startOfBuffer = whichBin * cacheLineSizeInUInt32s;
                    uint index = startOfBuffer;
                    uint currentIndex = cacheBufferIndexes[whichBin];
                    for (int i = 0; i < currentIndex; i++)
                        outputArray[startOfBin[whichBin]++] = cacheBuffers[index++];
                }

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;

                uint[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }
            if (outputArrayHasResult)
                for (uint current = 0; current < inputArray.Length; current++)    // copy from output array into the input array
                    inputArray[current] = outputArray[current];

            return inputArray;
        }
        public static uint[] SortRadixDerandomizedWrites3(this uint[] inputArray)
        {
            const int numberOfBins = 256;
            const int Log2ofPowerOfTwoRadix = 8;
            const int numberOfDigits = 4;
            const uint cacheLineSizeInBytes = 64 * 4;
            const uint BufferDepth = cacheLineSizeInBytes / sizeof(uint);
            uint[] cacheBuffers       = new uint[numberOfBins * BufferDepth];
            uint[] bufferIndexCurrent = new uint[numberOfBins];
            uint[] bufferIndexStart   = new uint[numberOfBins];
            uint[] bufferIndexEnd     = new uint[numberOfBins];
            uint[] outputArray = new uint[inputArray.Length];
            bool outputArrayHasResult = false;
            int d = 0;

            uint[][] startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];

            uint[][] count = HistogramByteComponents(inputArray, 0, inputArray.Length - 1);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            uint bitMask = 255;
            int shiftRightAmount = 0;
            d = 0;

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                uint[] startOfBinLoc = startOfBin[d];

                for (uint i = 0; i < numberOfBins; i++)
                {
                    bufferIndexStart[  i] = i * BufferDepth;
                    bufferIndexCurrent[i] = i * BufferDepth;
                    bufferIndexEnd[    i] = i * BufferDepth + BufferDepth - 1;
                }

                for (uint current = 0; current < inputArray.Length; current++)
                {
                    uint whichBin = (inputArray[current] & bitMask) >> shiftRightAmount;
                    if (bufferIndexCurrent[whichBin] <= bufferIndexEnd[whichBin])  // place current element into its cacheBuffer
                    {
                        cacheBuffers[bufferIndexCurrent[whichBin]++] = inputArray[current];
                    }
                    else     // flush the buffer to system memory
                    {
                        uint srcIndex = whichBin * BufferDepth;
                        uint dstIndex = startOfBinLoc[whichBin];
                        for (int i = 0; i < BufferDepth; i++)
                            outputArray[dstIndex++] = cacheBuffers[srcIndex++];
                        startOfBinLoc[whichBin] += BufferDepth;
                        bufferIndexCurrent[whichBin] = bufferIndexStart[whichBin];
                        cacheBuffers[bufferIndexCurrent[whichBin]++] = inputArray[current];
                    }
                }
                // Flush all of the cache buffers
                for (uint whichBin = 0; whichBin < numberOfBins; whichBin++)
                {
                    uint index = whichBin * BufferDepth;
                    uint dstIndex = startOfBinLoc[whichBin];
                    uint numItems = bufferIndexCurrent[whichBin] - bufferIndexStart[whichBin];
                    for (int i = 0; i < numItems; i++)
                        outputArray[dstIndex++] = cacheBuffers[index++];
                }

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;
                d++;

                uint[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }
            if (outputArrayHasResult)
                for (uint current = 0; current < inputArray.Length; current++)    // copy from output array into the input array
                    inputArray[current] = outputArray[current];

            return inputArray;
        }
        // Improved version (hopefully) with various optimization ideas developed since the time the above was written
        public static uint[] SortRadixDerandomizedWrites2(this uint[] inputArray)
        {
            const int numberOfBins = 256;
            const int Log2ofPowerOfTwoRadix = 8;
            const int bitsPerDigit = Log2ofPowerOfTwoRadix;
            const int numberOfDigits = (sizeof(uint) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            uint cacheLineSizeInBytes = 64 * 4 * 4;
            uint cacheLineSizeInUInt32s = cacheLineSizeInBytes / sizeof(uint);
            uint[] cacheBuffers       = new uint[numberOfBins * cacheLineSizeInUInt32s];
            uint[] cacheBufferIndexes = new uint[numberOfBins];
            uint[] outputArray = new uint[inputArray.Length];
            uint[][] startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];
            bool outputArrayHasResult = false;
            int d = 0;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            uint[][] count = HistogramByteComponents(inputArray, 0, inputArray.Length - 1);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            d = 0;
            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                for (uint i = 0; i < numberOfBins; i++)
                    cacheBufferIndexes[i] = i * cacheLineSizeInUInt32s;

                uint[] startOfBinLoc = startOfBin[d];

                for (uint current = 0; current < inputArray.Length; current++)
                {
                    uint whichBin = ExtractDigit(inputArray[current], bitMask, shiftRightAmount);
                    uint startOfBuffer = whichBin * cacheLineSizeInUInt32s;
                    if (cacheBufferIndexes[whichBin] < cacheLineSizeInUInt32s)  // place current element into its cacheBuffer
                    {
                        cacheBuffers[cacheBufferIndexes[whichBin]++] = inputArray[current];
                    }
                    else     // flush the buffer to system memory
                    {
                        uint srcIndex = startOfBuffer;
                        uint dstIndex = startOfBinLoc[whichBin];
#if false
                        for (int i = 0; i < cacheLineSizeInUInt32s; i++)
                            outputArray[dstIndex++] = cacheBuffers[srcIndex++];
#else
                        Array.Copy(cacheBuffers, srcIndex, outputArray, dstIndex, cacheLineSizeInUInt32s);
#endif
                        startOfBinLoc[whichBin] += cacheLineSizeInUInt32s;
                        cacheBuffers[startOfBuffer] = inputArray[current];
                        cacheBufferIndexes[whichBin] = whichBin * cacheLineSizeInUInt32s + 1;
                    }
                    //outputArray[endOfBin[ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++] = inputArray[current];
                }
                // Flush all of the cache buffers
                for (uint whichBin = 0; whichBin < numberOfBins; whichBin++)
                {
                    uint startOfBuffer = whichBin * cacheLineSizeInUInt32s;
                    uint index = startOfBuffer;
                    uint currentIndex = cacheBufferIndexes[whichBin];
                    for (int i = 0; i < currentIndex; i++)
                        outputArray[startOfBinLoc[whichBin]++] = cacheBuffers[index++];
                }

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;
                d++;

                uint[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }
            if (outputArrayHasResult)
                for (uint current = 0; current < inputArray.Length; current++)    // copy from output array into the input array
                    inputArray[current] = outputArray[current];

            return inputArray;
        }
        /// <summary>
        /// Sort an array of unsigned integers using Radix Sorting algorithm (least significant digit variation).
        /// Derandomizes memory writes for more consistent performance across various input data distributions.
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns>sorted array of unsigned integers</returns>
        public static uint[] SortRadixDerandomizeWrites(this uint[] inputArray, Int32 start, Int32 length)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            uint cacheLineSizeInBytes = 64 * 4;
            uint cacheLineSizeInUInt32s = cacheLineSizeInBytes / 4;  // is there sizeOf() in C#
            uint[] cacheBuffers = new uint[numberOfBins * cacheLineSizeInUInt32s];
            uint[] cacheBufferIndexes = new uint[numberOfBins];
            uint[] outputArray = new uint[inputArray.Length];
            uint[] count = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            uint[] startOfBin = new uint[numberOfBins];

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                for (uint i = 0; i < numberOfBins; i++)
                {
                    count[i] = 0;
                    cacheBufferIndexes[i] = 0;
                }
                for (int current = start; current < (start + length); current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    count[ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++;

                startOfBin[0] = (uint)start;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[i] = (uint)(startOfBin[i - 1] + count[i - 1]);

                for (int current = start; current < (start + length); current++)
                {
                    uint whichBin = ExtractDigit(inputArray[current], bitMask, shiftRightAmount);
                    uint startOfBuffer = whichBin * cacheLineSizeInUInt32s;
                    if (cacheBufferIndexes[whichBin] < cacheLineSizeInUInt32s)  // place current element into its cacheBuffer
                    {
                        cacheBuffers[startOfBuffer + cacheBufferIndexes[whichBin]] = inputArray[current];
                        cacheBufferIndexes[whichBin]++;
                    }
                    else     // flush the buffer to system memory
                    {
                        uint index = startOfBuffer;
                        for (int i = 0; i < cacheLineSizeInUInt32s; i++)
                            outputArray[startOfBin[whichBin]++] = cacheBuffers[index++];
                        cacheBuffers[startOfBuffer] = inputArray[current];
                        cacheBufferIndexes[whichBin] = 1;
                    }
                    //outputArray[endOfBin[ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++] = inputArray[current];
                }
                // Flush all of the cache buffers
                for (uint whichBin = 0; whichBin < numberOfBins; whichBin++)
                {
                    uint startOfBuffer = whichBin * cacheLineSizeInUInt32s;
                    uint index = startOfBuffer;
                    uint currentIndex = cacheBufferIndexes[whichBin];
                    for (int i = 0; i < currentIndex; i++)
                        outputArray[startOfBin[whichBin]++] = cacheBuffers[index++];
                }

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;

                uint[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }
            if (outputArrayHasResult)
                for (int current = start; current < (start + length); current++)    // copy from output array into the input array
                    inputArray[current] = outputArray[current];

            return inputArray;
        }
        /// <summary>
        /// Sort an array of unsigned integers using Radix Sorting algorithm (least significant digit variation).
        /// Derandomizes memory writes for more consistent performance across various input data distributions.
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns>sorted array of unsigned integers</returns>
        public static void SortRadixDerandomizedWrites<T>(this T[] inputArray, Int32 start, Int32 length, T[] dst, Func<T, UInt32> getKey)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            uint cacheLineSizeInBytes = 64 * 4;
            uint cacheLineSizeInUInt32s = cacheLineSizeInBytes / 4;  // is there sizeOf() in C#
            T[] cacheBuffers = new T[numberOfBins * cacheLineSizeInUInt32s];
            uint[] cacheBufferIndexes = new uint[numberOfBins];
            uint[] count = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            uint[] startOfBin = new uint[numberOfBins];

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                for (uint i = 0; i < numberOfBins; i++)
                {
                    count[i] = 0;
                    cacheBufferIndexes[i] = 0;
                }
                for (int current = start; current < (start + length); current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    count[ExtractDigit(getKey(inputArray[current]), bitMask, shiftRightAmount)]++;

                startOfBin[0] = (uint)start;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[i] = (uint)(startOfBin[i - 1] + count[i - 1]);

                for (int current = start; current < (start + length); current++)
                {
                    uint whichBin = ExtractDigit(getKey(inputArray[current]), bitMask, shiftRightAmount);
                    uint startOfBuffer = whichBin * cacheLineSizeInUInt32s;
                    if (cacheBufferIndexes[whichBin] < cacheLineSizeInUInt32s)  // place current element into its cacheBuffer
                    {
                        cacheBuffers[startOfBuffer + cacheBufferIndexes[whichBin]] = inputArray[current];
                        cacheBufferIndexes[whichBin]++;
                    }
                    else     // flush the buffer to system memory
                    {
                        uint index = startOfBuffer;
                        for (int i = 0; i < cacheLineSizeInUInt32s; i++)
                            dst[startOfBin[whichBin]++] = cacheBuffers[index++];
                        cacheBuffers[startOfBuffer] = inputArray[current];
                        cacheBufferIndexes[whichBin] = 1;
                    }
                    //outputArray[endOfBin[ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++] = inputArray[current];
                }
                // Flush all of the cache buffers
                for (uint whichBin = 0; whichBin < numberOfBins; whichBin++)
                {
                    uint startOfBuffer = whichBin * cacheLineSizeInUInt32s;
                    uint index = startOfBuffer;
                    uint currentIndex = cacheBufferIndexes[whichBin];
                    for (int i = 0; i < currentIndex; i++)
                        dst[startOfBin[whichBin]++] = cacheBuffers[index++];
                }

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;

                T[] tmp = inputArray;       // swap input and output arrays
                inputArray = dst;
                dst = tmp;
            }
            if (outputArrayHasResult)
                for (int current = start; current < (start + length); current++)    // copy from output array into the input array
                    inputArray[current] = dst[current];
        }
        /// <summary>
        /// Sort an array of user defined class containing an unsigned integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// Slower algorithm that allocates only 3K bytes of extra memory. This algorithm is not in-place, creating a new sorted output array.
        /// </summary>
        /// <param name="inputArray"></param>
        /// <param name="getKey">user provided function to extract the unsigned 32-bit key sorted on, from the user defined class</param>
        /// <returns>sorted array of user defined class</returns>
        public static T[] SortRadix<T>(this T[] inputArray, Func<T, UInt32> getKey)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            var outputArray = new T[inputArray.Length];
            var count = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            var startOfBin = new uint[numberOfBins];

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                for (uint i = 0; i < numberOfBins; i++)
                    count[i] = 0;
                for (uint current = 0; current < inputArray.Length; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    count[ExtractDigit(getKey(inputArray[current]), bitMask, shiftRightAmount)]++;

                startOfBin[0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[i] = startOfBin[i - 1] + count[i - 1];

                for (uint current = 0; current < inputArray.Length; current++)
                    outputArray[startOfBin[ExtractDigit(getKey(inputArray[current]), bitMask, shiftRightAmount)]++] = inputArray[current];

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;

                T[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }

            return outputArrayHasResult ? outputArray : inputArray;
        }
        public static T[] SortRadixNew<T>(this T[] inputArray, Func<T, UInt32> getKey)
        {
            uint numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            int numberOfDigits = sizeof(UInt32) * 8 / Log2ofPowerOfTwoRadix;
            var outputArray = new T[inputArray.Length];
            bool outputArrayHasResult = false;
            uint bitMask = numberOfBins - 1;
            int shiftRightAmount = 0;
            int d = 0;

            uint[][] count = HistogramByteComponents(inputArray, 0, inputArray.Length - 1, getKey);

            var startOfBin = new uint[numberOfDigits][];
            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d] = new uint[numberOfBins];
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            d = 0;
            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                uint[] startOfBinLoc = startOfBin[d];

                for (uint current = 0; current < inputArray.Length; current++)
                    outputArray[startOfBinLoc[(byte)(getKey(inputArray[current]) >> shiftRightAmount)]++] = inputArray[current];

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;
                d++;

                T[] tmp = inputArray;  inputArray = outputArray;  outputArray = tmp;  // swap input and output arrays
            }

            return outputArrayHasResult ? outputArray : inputArray;
        }
        /// <summary>
        /// Sort an array of user defined class containing an unsigned 64-bit integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// Slower algorithm that allocates only 3K bytes of extra memory. This algorithm is not in-place, creating a new sorted output array.
        /// </summary>
        /// <param name="inputArray"></param>
        /// <param name="getKey">user provided function to extract the unsigned 64-bit key sorted on, from the user defined class</param>
        /// <returns>sorted array of user defined class</returns>
        public static T[] SortRadix<T>(this T[] inputArray, Func<T, UInt64> getKey)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            var outputArray = new T[inputArray.Length];
            var count = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            var startOfBin = new uint[numberOfBins];

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                for (uint i = 0; i < numberOfBins; i++)
                    count[i] = 0;
                for (uint current = 0; current < inputArray.Length; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    count[ExtractDigit(getKey(inputArray[current]), bitMask, shiftRightAmount)]++;

                startOfBin[0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[i] = startOfBin[i - 1] + count[i - 1];

                for (uint current = 0; current < inputArray.Length; current++)
                    outputArray[startOfBin[ExtractDigit(getKey(inputArray[current]), bitMask, shiftRightAmount)]++] = inputArray[current];

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;

                T[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }

            return outputArrayHasResult ? outputArray : inputArray;
        }
        public static T[] SortRadixNew<T>(this T[] inputArray, Func<T, UInt64> getKey)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            int numberOfDigits = sizeof(UInt64) * 8 / Log2ofPowerOfTwoRadix;
            var outputArray = new T[inputArray.Length];
            bool outputArrayHasResult = false;
            UInt64 bitMask = 255;
            int shiftRightAmount = 0;

            int d = 0;

            var startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];

            uint[][] count = HistogramByteComponents(inputArray, 0, inputArray.Length - 1, getKey);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            d = 0;
            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                uint[] startOfBinLoc = startOfBin[d];

                for (uint current = 0; current < inputArray.Length; current++)
                    outputArray[startOfBinLoc[(byte)(getKey(inputArray[current]) >> shiftRightAmount)]++] = inputArray[current];

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;
                d++;

                T[] tmp = inputArray; inputArray = outputArray; outputArray = tmp;  // swap input and output arrays
            }

            return outputArrayHasResult ? outputArray : inputArray;
        }
        // The following algorithms use a method where we extract an array of keys and have a source and destination array of keys to go along with
        // array of references so that accesses to keys are more cache friendly, and accesses to references go along with they keys!
        // The keys are in a linear array. The references to objects are in a linear array. Each object is never touched, since objects are scattered
        // in memory, making access to objects slow.
        // This method should be much better for arrays of objects/classes for JavaScript, Java, C#, C++ and Python.

        /// <summary>
        /// Sort an array of user defined class containing an unsigned integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// Faster algorithm that uses 8N bytes more temporary memory than SortRadix, where N is the size of the input array.
        /// This algorithm is not in-place, creating a new sorted output array.
        /// </summary>
        /// <param name="inputArray">input array of type T</param>
        /// <param name="getKey">user provided function to extract the unsigned 32-bit key sorted on, from the user defined class</param>
        /// <returns>sorted array of user defined class</returns>
        public static T[] SortRadixFaster<T>(this T[] inputArray, Func<T, UInt32> getKey)
        {
            int numberOfBins = 256;
            int NumberOfBitsPerDigit = 8;
            var outputArray = new T[inputArray.Length];
            var inKeys = new UInt32[inputArray.Length];
            var outSortedKeys = new UInt32[inputArray.Length];
            var count = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            var startOfBin = new uint[numberOfBins];

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                for (uint i = 0; i < numberOfBins; i++)
                    count[i] = 0;
                if (bitMask == 255)     // first pass
                    for (uint current = 0; current < inputArray.Length; current++)    // Scan Key array and count the number of times each digit value appears - i.e. size of each bin
                    {
                        inKeys[current] = getKey(inputArray[current]);
                        count[ExtractDigit(inKeys[current], bitMask, shiftRightAmount)]++;
                    }
                else
                    for (uint current = 0; current < inputArray.Length; current++)    // Scan Key array and count the number of times each digit value appears - i.e. size of each bin
                        count[ExtractDigit(inKeys[current], bitMask, shiftRightAmount)]++;

                startOfBin[0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[i] = startOfBin[i - 1] + count[i - 1];

                for (uint current = 0; current < inputArray.Length; current++)
                {
                    uint endOfBinIndex = ExtractDigit(inKeys[current], bitMask, shiftRightAmount);
                    uint index = startOfBin[endOfBinIndex];
                    outputArray[index] = inputArray[current];
                    outSortedKeys[index] = inKeys[current];
                    startOfBin[endOfBinIndex]++;
                }

                bitMask <<= NumberOfBitsPerDigit;
                shiftRightAmount += NumberOfBitsPerDigit;
                outputArrayHasResult = !outputArrayHasResult;

                T[] tmp = inputArray; inputArray = outputArray; outputArray = tmp;          // swap input and output arrays
                UInt32[] tmpKeys = inKeys; inKeys = outSortedKeys; outSortedKeys = tmpKeys; // swap input and output key arrays
            }

            return outputArrayHasResult ? outputArray : inputArray;
        }
        public static T[] SortRadixFasterNew<T>(this T[] inputArray, Func<T, UInt32> getKey)
        {
            const int NumberOfBitsPerDigit = 8;
            const int NumberOfBins = 1 << NumberOfBitsPerDigit;
            const int NumberOfDigits = sizeof(UInt32) * 8 / NumberOfBitsPerDigit;
            var outputArray = new T[inputArray.Length];
            var outSortedKeys = new UInt32[inputArray.Length];
            bool outputArrayHasResult = false;
            uint bitMask = NumberOfBins - 1;
            int shiftRightAmount = 0;
            int d;

            var countAndKeyArray = HistogramByteComponentsAndKeyArray(inputArray, 0, inputArray.Length - 1, getKey);
            uint[][] count  = countAndKeyArray.Item1;
            UInt32[] inKeys = countAndKeyArray.Item2;

            var startOfBin = new uint[NumberOfDigits][];
            for (d = 0; d < NumberOfDigits; d++)
            {
                startOfBin[d] = new uint[NumberOfBins];
                startOfBin[d][0] = 0;
                for (uint i = 1; i < NumberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            d = 0;
            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                uint[] startOfBinLoc = startOfBin[d];

                for (uint current = 0; current < inputArray.Length; current++)
                {
                    byte endOfBinIndex = (byte)(inKeys[current] >> shiftRightAmount);
                    uint index = startOfBinLoc[endOfBinIndex];
                    outputArray[  index] = inputArray[current];
                    outSortedKeys[index] = inKeys[    current];
                    startOfBinLoc[endOfBinIndex]++;
                }

                bitMask <<= NumberOfBitsPerDigit;
                shiftRightAmount += NumberOfBitsPerDigit;
                outputArrayHasResult = !outputArrayHasResult;
                d++;

                T[] tmp = inputArray; inputArray = outputArray; outputArray = tmp;     // swap input and output arrays
                UInt32[] tmpKeys = inKeys; inKeys = outSortedKeys; outSortedKeys = tmpKeys; // swap input and output key arrays
            }

            return outputArrayHasResult ? outputArray : inputArray;
        }
        /// <summary>
        /// Sort an array of user defined class containing an unsigned integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// Faster algorithm that uses 16N bytes more temporary memory than SortRadix, where N is the size of the input array.
        /// </summary>
        /// <param name="inputArray">input array of type T</param>
        /// <param name="getKey">user provided function to extract the unsigned 64-bit key sorted on, from the user defined class</param>
        /// <returns>sorted array of user defined class</returns>
        public static T[] SortRadixFaster<T>(this T[] inputArray, Func<T, UInt64> getKey)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            var outputArray = new T[inputArray.Length];
            var inKeys = new UInt64[inputArray.Length];
            var outSortedKeys = new UInt64[inputArray.Length];
            var count = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            var startOfBin = new uint[numberOfBins];

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                for (uint i = 0; i < numberOfBins; i++)
                    count[i] = 0;
                if (bitMask == 255)
                    for (uint current = 0; current < inputArray.Length; current++)    // Scan Key array and count the number of times each digit value appears - i.e. size of each bin
                    {
                        inKeys[current] = getKey(inputArray[current]);
                        count[ExtractDigit(inKeys[current], bitMask, shiftRightAmount)]++;
                    }
                else
                    for (uint current = 0; current < inputArray.Length; current++)    // Scan Key array and count the number of times each digit value appears - i.e. size of each bin
                        count[ExtractDigit(inKeys[current], bitMask, shiftRightAmount)]++;

                startOfBin[0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[i] = startOfBin[i - 1] + count[i - 1];

                for (uint current = 0; current < inputArray.Length; current++)
                {
                    uint endOfBinIndex = ExtractDigit(inKeys[current], bitMask, shiftRightAmount);
                    uint index = startOfBin[endOfBinIndex];
                    outputArray[index] = inputArray[current];
                    outSortedKeys[index] = inKeys[current];
                    startOfBin[endOfBinIndex]++;
                }

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;

                T[] tmp = inputArray; inputArray = outputArray; outputArray = tmp;     // swap input and output arrays
                UInt64[] tmpKeys = inKeys; inKeys = outSortedKeys; outSortedKeys = tmpKeys; // swap input and output key arrays
            }

            return outputArrayHasResult ? outputArray : inputArray;
        }

        public static T[] SortRadixFasterNew<T>(this T[] inputArray, Func<T, UInt64> getKey)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            int numberOfDigits = 4;
            var outputArray = new T[inputArray.Length];
            var outSortedKeys = new UInt64[inputArray.Length];
            bool outputArrayHasResult = false;
            uint bitMask = 255;
            int shiftRightAmount = 0;
            int d;

            var startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];

            var countAndKeyArray = HistogramByteComponentsAndKeyArray(inputArray, 0, inputArray.Length - 1, getKey);
            uint[][] count  = countAndKeyArray.Item1;
            UInt64[] inKeys = countAndKeyArray.Item2;

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            d = 0;
            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                uint[] startOfBinLoc = startOfBin[d];

                for (uint current = 0; current < inputArray.Length; current++)
                {
                    uint endOfBinIndex = (byte)(inKeys[current] >> shiftRightAmount);
                    uint index = startOfBinLoc[endOfBinIndex];
                    outputArray[index] = inputArray[current];
                    outSortedKeys[index] = inKeys[current];
                    startOfBinLoc[endOfBinIndex]++;
                }

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;
                d++;

                T[] tmp = inputArray; inputArray = outputArray; outputArray = tmp;     // swap input and output arrays
                UInt64[] tmpKeys = inKeys; inKeys = outSortedKeys; outSortedKeys = tmpKeys; // swap input and output key arrays
            }

            return outputArrayHasResult ? outputArray : inputArray;
        }
        /// <summary>
        /// Sort a List of user defined class containing an unsigned integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// Faster algorithm that uses 8N bytes more temporary memory than SortRadix, where N is the size of the input array.
        /// </summary>
        /// <param name="inputList">input List of type T</param>
        /// <param name="getKey">user provided function to extract the unsigned 32-bit key sorted on, from the user defined class</param>
        /// <returns>sorted List of user defined class</returns>
        public static List<T> SortRadixFaster<T>(this List<T> inputList, Func<T, UInt32> getKey)
        {
            var srcCopy = inputList.ToArray();
            var sortedArray = srcCopy.SortRadixFaster(getKey);
            var sortedList = new List<T>(sortedArray);
            return sortedList;
        }
        /// <summary>
        /// Sort a List of user defined class containing an unsigned integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// Faster algorithm that uses 16N bytes more temporary memory than SortRadix, where N is the size of the input array.
        /// </summary>
        /// <param name="inputList">input List of type T</param>
        /// <param name="getKey">user provided function to extract the unsigned 64-bit key sorted on, from the user defined class</param>
        /// <returns>sorted List of user defined class</returns>
        public static List<T> SortRadixFaster<T>(this List<T> inputList, Func<T, UInt64> getKey)
        {
            var srcCopy = inputList.ToArray();
            var sortedArray = srcCopy.SortRadixFaster(getKey);
            var sortedList = new List<T>(sortedArray);
            return sortedList;
        }
        /// <summary>
        /// Sort an array of user defined class containing an unsigned 64-bit integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// Slower algorithm that allocates only 3K bytes of extra memory.
        /// </summary>
        /// <param name="inputArray"></param>
        /// <param name="getKey">user provided function to extract the unsigned 64-bit key sorted on, from the user defined class</param>
        /// <returns>sorted array of user defined class</returns>
        public static void SortRadix<T>(this T[] inputArray, Int32 start, Int32 length, T[] outputArray, Func<T, UInt32> getKey)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            uint[] count = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            uint[] startOfBin = new uint[numberOfBins];

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                // TODO: This can be optimized by changing to the two phase strategy of counting in a single-pass for all the digits
                for (uint i = 0; i < numberOfBins; i++)
                    count[i] = 0;
                for (int current = start; current < (start + length); current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    count[ExtractDigit(getKey(inputArray[current]), bitMask, shiftRightAmount)]++;

                startOfBin[0] = (uint)start;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[i] = startOfBin[i - 1] + count[i - 1];

                for (int current = start; current < (start + length); current++)
                    outputArray[startOfBin[ExtractDigit(getKey(inputArray[current]), bitMask, shiftRightAmount)]++] = inputArray[current];

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;

                T[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }
            if (!outputArrayHasResult)
                for (int current = start; current < (start + length); current++)
                    outputArray[current] = inputArray[current];
        }
        private static UInt32 ExtractDigit(UInt32 value, UInt32 bitMask, int shiftRightAmount)
        {
            return (value & bitMask) >> shiftRightAmount;	// extract the digit we are sorting based on
        }
        private static UInt32 ExtractDigit(UInt64 value, UInt64 bitMask, int shiftRightAmount)
        {
            return (UInt32)((value & bitMask) >> shiftRightAmount);	// extract the digit we are sorting based on
        }
        /// <summary>
        /// Sort a List of user defined class containing an unsigned integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// Slower algorithm that allocates only 3K bytes of extra memory.
        /// </summary>
        /// <param name="inputList"></param>
        /// <param name="getKey">user provided function to extract the unsigned 32-bit key sorted on</param>
        /// <returns>sorted List of user defined class</returns>
        public static List<T> SortRadix<T>(this List<T> inputList, Func<T, UInt32> getKey)
        {
            var srcCopy = inputList.ToArray();
            var sortedArray = srcCopy.SortRadix(getKey);
            var sortedList = new List<T>(sortedArray);
            return sortedList;
        }
        /// <summary>
        /// Sort a List of user defined class containing an unsigned 64-bit integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// Slower algorithm that allocates only 3K bytes of extra memory.
        /// </summary>
        /// <param name="inputList"></param>
        /// <param name="getKey">user provided function to extract the unsigned 32-bit key sorted on</param>
        /// <returns>sorted List of user defined class</returns>
        public static List<T> SortRadix<T>(this List<T> inputList, Func<T, UInt64> getKey)
        {
            var srcCopy = inputList.ToArray();
            var sortedArray = srcCopy.SortRadix(getKey);
            var sortedList = new List<T>(sortedArray);
            return sortedList;
        }
        /// <summary>
        /// Sort an array of user defined class based of a separate array of unsigned integer Keys, using Radix Sorting algorithm.
        /// Linear time sort algorithm.
        /// </summary>
        /// <param name="inputArray">input array of type T</param>
        /// <param name="inKeys">input array of keys (unsigned integers) to be sorted on</param>
        /// <returns>Tuple consisting of a sorted array of user defined class and a sorted array of unsigned integer keys (UInt32)</returns>
        public static Tuple<T[], UInt32[]> SortRadix<T>(this T[] inputArray, UInt32[] inKeys)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            UInt32[] outSortedKeys = new UInt32[inputArray.Length];
            T[] outputArray = new T[inputArray.Length];
            uint[] count = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            uint[] startOfBin = new uint[numberOfBins];
            uint[] endOfBin = new uint[numberOfBins];

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                for (uint i = 0; i < numberOfBins; i++)
                    count[i] = 0;
                for (uint current = 0; current < inputArray.Length; current++)    // Scan Key array and count the number of times each digit value appears - i.e. size of each bin
                    count[ExtractDigit(inKeys[current], bitMask, shiftRightAmount)]++;

                startOfBin[0] = endOfBin[0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[i] = endOfBin[i] = (uint)(startOfBin[i - 1] + count[i - 1]);

                for (uint current = 0; current < inputArray.Length; current++)
                {
                    uint endOfBinIndex = ExtractDigit(inKeys[current], bitMask, shiftRightAmount);
                    uint index = endOfBin[endOfBinIndex];
                    outputArray[index] = inputArray[current];
                    outSortedKeys[index] = inKeys[current];
                    endOfBin[endOfBinIndex]++;
                }

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;

                T[] tmp = inputArray;        // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
                UInt32[] tmpKeys = inKeys;   // swap input and output key arrays
                inKeys = outSortedKeys;
                outSortedKeys = tmpKeys;
            }
            if (outputArrayHasResult)
                for (uint current = 0; current < inputArray.Length; current++)    // copy from output array into the input array
                {
                    inputArray[current] = outputArray[current];
                    inKeys[current] = outSortedKeys[current];
                }

            return new Tuple<T[], UInt32[]>(inputArray, outSortedKeys);
        }
        /// <summary>
        /// Sort an array of user defined class based of a separate array of unsigned long integer Keys, using Radix Sorting algorithm.
        /// Linear time sort algorithm.
        /// </summary>
        /// <param name="inputArray">input array of type T</param>
        /// <param name="inKeys">input array of keys (unsigned integers) to be sorted on</param>
        /// <returns>Tuple consisting of a sorted array of user defined class and a sorted array of unsigned long keys (UInt64)</returns>
        public static Tuple<T[], UInt64[]> SortRadix<T>(this T[] inputArray, UInt64[] inKeys)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            UInt64[] outSortedKeys = new UInt64[inputArray.Length];
            T[] outputArray = new T[inputArray.Length];
            uint[] count = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            uint[] startOfBin = new uint[numberOfBins];
            uint[] endOfBin = new uint[numberOfBins];

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                for (uint i = 0; i < numberOfBins; i++)
                    count[i] = 0;
                for (uint current = 0; current < inputArray.Length; current++)    // Scan Key array and count the number of times each digit value appears - i.e. size of each bin
                    count[ExtractDigit(inKeys[current], bitMask, shiftRightAmount)]++;

                startOfBin[0] = endOfBin[0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[i] = endOfBin[i] = (uint)(startOfBin[i - 1] + count[i - 1]);

                for (uint current = 0; current < inputArray.Length; current++)
                {
                    uint endOfBinIndex = ExtractDigit(inKeys[current], bitMask, shiftRightAmount);
                    uint index = endOfBin[endOfBinIndex];
                    outputArray[index] = inputArray[current];
                    outSortedKeys[index] = inKeys[current];
                    endOfBin[endOfBinIndex]++;
                }

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;

                T[] tmp = inputArray;        // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
                UInt64[] tmpKeys = inKeys;   // swap input and output key arrays
                inKeys = outSortedKeys;
                outSortedKeys = tmpKeys;
            }
            if (outputArrayHasResult)
                for (uint current = 0; current < inputArray.Length; current++)    // copy from output array into the input array
                {
                    inputArray[current] = outputArray[current];
                    inKeys[current] = outSortedKeys[current];
                }

            return new Tuple<T[], UInt64[]>(inputArray, outSortedKeys);
        }
        /// <summary>
        /// Sort a List of user defined class based of a separate array of unsigned integer Keys, using Radix Sorting algorithm.
        /// Linear time sort algorithm.
        /// </summary>
        /// <param name="inputList">input List of type T</param>
        /// <param name="inKeys">input array of keys (unsigned integers) to be sorted on</param>
        /// <returns>Tuple consisting of a sorted List of user defined class and a sorted array of unsigned integer keys (UInt32)</returns>
        public static Tuple<List<T>, UInt32[]> SortRadix<T>(this List<T> inputList, UInt32[] inKeys)
        {
            var srcCopy = inputList.ToArray();
            var sortedArrayAndKeys = srcCopy.SortRadix(inKeys);
            var sortedList = new List<T>(sortedArrayAndKeys.Item1);
            return new Tuple<List<T>, UInt32[]>(sortedList, sortedArrayAndKeys.Item2);
        }
        /// <summary>
        /// Sort a List of user defined class based of a separate array of unsigned long integer Keys, using Radix Sorting algorithm.
        /// Linear time sort algorithm.
        /// </summary>
        /// <param name="inputList">input List of type T</param>
        /// <param name="inKeys">input array of keys (unsigned integers) to be sorted on</param>
        /// <returns>sorted List of user defined class</returns>
        public static Tuple<List<T>, UInt64[]> SortRadix<T>(this List<T> inputList, UInt64[] inKeys)
        {
            var srcCopy = inputList.ToArray();
            var sortedArrayAndKeys = srcCopy.SortRadix(inKeys);
            var sortedList = new List<T>(sortedArrayAndKeys.Item1);
            return new Tuple<List<T>, UInt64[]>(sortedList, sortedArrayAndKeys.Item2);
        }

        public enum SortOrder
        {
            Ascending,
            Descending
        }
        /// <summary>
        /// Sort an array of unsigned bytes, returning only sorted indexes. Input array is unaltered. Linear time sort algorithm.
        /// </summary>
        /// <param name="inputArray">input array of unsigned bytes</param>
        /// <returns>array of sorted indexes</returns>
        public static Int32[] SortRadixReturnIndexes(this byte[] inputArray, SortOrder sortOrder = SortOrder.Ascending)
        {
            int numberOfBins = 256;
            var outputIndexArray = new Int32[inputArray.Length];
            int[] count          = new int[numberOfBins];
            int[] startOfBin     = new int[numberOfBins];

            for (int i = 0; i < numberOfBins; i++)
                count[i] = 0;
            for (int current = 0; current < inputArray.Length; current++)    // Scan Key array and count the number of times each digit value appears - i.e. size of each bin
                count[inputArray[current]]++;

            if (sortOrder == SortOrder.Ascending)
            {
                startOfBin[0] = 0;
                for (int i = 1; i < numberOfBins; i++)
                startOfBin[i] = startOfBin[i - 1] + count[i - 1];
            }
            else
            {
                startOfBin[numberOfBins - 1] = 0;
                for (int i = numberOfBins - 2; i >= 0; i--)
                    startOfBin[i] = startOfBin[i + 1] + count[i + 1];
            }

            for (int current = 0; current < inputArray.Length; current++)
            {
                byte digit = inputArray[current];
                int index  = startOfBin[digit]++;
                outputIndexArray[index] = current;          // current location is the current index within the input array
            }

            return outputIndexArray;
        }
    }
}
