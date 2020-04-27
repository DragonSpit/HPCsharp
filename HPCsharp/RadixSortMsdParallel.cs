// TODO: Add parallel versions of singed 8-bit and 16-bit Radix Sort/Counting Sort
// TODO: Implement paralle versions of RadixSort MSD implementations for ulong, slong and double array, but only spawn new tasks for non-empty bins instead of
//       all the bins indiscrimintently.
// TODO: Develop a predicate parallel for capability where the pattern is "if problem size is bigger than a threshold, then it's given to a task to execute in parallel,
//       otherwise it is executed on the current thread" since this is a common pattern that comes up.
// TODO: Implement parallel versions of HistogramByteComponents, as that seems like a potential performance limiter at the moment to parallel acceleration, since in MSD
//       Radix Sort this operation runs many more times than in LSD Radix Sort (where it runs only once for all the digits). Thus, optimizing its performance is more critical here.
// TODO: Make sure to inline Swap() in routines that John is using.
// TODO: Benchmark MSD Radix Sort parallel with and without SSE AddTo usage in the Histogram function.
// TODO: Implement MSD Radix Sort (in-place) in chunks of an array. Then use our parallel in-place Merge (port from C++ Dr. Dobbs paper), to produce a fully in-place
//       hybrid algorithm (can we then make it stable too? Also, in my Dr. Dobbs papers?)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
        public static void SortRadixMsdPar(this byte[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlacePar();
        }

        public static void SortRadixMsdPar(this ushort[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlacePar();
        }

        public static Int32 SortRadixMsdLongThreshold { get; set; } = 64;
        public static Int32 SortRadixMsdLongParallelThreshold { get; set; } = 1024;

        // Port of Victor's articles in Dr. Dobb's Journal January 14, 2011
        // Plain function In-place MSD Radix Sort implementation (simplified).
        private const int PowerOfTwoRadix = 256;
        private const int Log2ofPowerOfTwoRadix = 8;

        private static void RadixSortMsdLongParInner(long[] a, int first, int length, int shiftRightAmount, Action<long[], int, int> baseCaseInPlaceSort)
        {
            int last = first + length - 1;
            //const long bitMask = PowerOfTwoRadix - 1;
            const byte halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;
            //Stopwatch stopwatch = new Stopwatch();
            //long frequency = Stopwatch.Frequency;
            //Console.WriteLine("  Timer frequency in ticks per second = {0}", frequency);
            //long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            //stopwatch.Restart();

            var count = ParallelAlgorithm.HistogramOneByteComponentPar(a, first, last, shiftRightAmount);

            //stopwatch.Stop();
            //double timeForCounting = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            //Console.WriteLine("Time for counting: {0}", timeForCounting);

            var startOfBin = new int[PowerOfTwoRadix + 1];
            var endOfBin = new int[PowerOfTwoRadix];
            int nextBin = 1;
            startOfBin[0] = endOfBin[0] = first; startOfBin[PowerOfTwoRadix] = -1;         // sentinal
            for (int i = 1; i < PowerOfTwoRadix; i++)
                startOfBin[i] = endOfBin[i] = startOfBin[i - 1] + count[i - 1];
            int bucketsUsed = 0;
            for (int i = 0; i < count.Length; i++)
                if (count[i] > 0) bucketsUsed++;

            //stopwatch.Restart();

            if (bucketsUsed > 1)
            {
                if (shiftRightAmount == 56)     // Most significant digit
                {
                    for (int _current = first; _current <= last;)
                    {
                        byte digit;
                        byte halfptr = halfOfPowerOfTwoRadix;
                        while (endOfBin[digit = (byte)((byte)(a[_current] >> shiftRightAmount) ^ halfptr)] != _current)
                        {
                            long temp = a[_current];            // inlining Swap() increased performance about 5-10%
                            a[_current] = a[endOfBin[digit]];
                            a[endOfBin[digit]++] = temp;
                        }
                        endOfBin[digit]++;

                        while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                        _current = endOfBin[nextBin - 1];
                    }
                }
                else
                {
                    for (int _current = first; _current <= last;)
                    {
                        byte digit;
                        while (endOfBin[digit = (byte)(a[_current] >> shiftRightAmount)] != _current)
                        {
                            long temp = a[_current];            // inlining Swap() increased performance about 5-10%
                            a[_current] = a[endOfBin[digit]];
                            a[endOfBin[digit]++] = temp;
                        }
                        endOfBin[digit]++;

                        while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                        _current = endOfBin[nextBin - 1];
                    }
                }
                //stopwatch.Stop();
                //double timeForPermuting = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
                //Console.WriteLine("Size = {0}, Time for counting: {1}, Time for permuting: {2}, Ratio = {3:0.00}", length, timeForCounting, timeForPermuting, timeForCounting/timeForPermuting);

                if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
                {
                    shiftRightAmount = shiftRightAmount >= Log2ofPowerOfTwoRadix ? shiftRightAmount -= Log2ofPowerOfTwoRadix : 0;

                    for (int i = 0; i < PowerOfTwoRadix; i++)
                    {
                        int numElements = endOfBin[i] - startOfBin[i];

                        if (numElements >= SortRadixMsdLongThreshold)
                            RadixSortMsdLongParInner(a, startOfBin[i], numElements, shiftRightAmount, baseCaseInPlaceSort);
                        else if (numElements >= 2)
                            //InsertionSort(a, startOfBin[i], numElements);
                            baseCaseInPlaceSort(a, startOfBin[i], numElements);
                    }
                }
            }
            else
            {
                if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
                {
                    shiftRightAmount = shiftRightAmount >= Log2ofPowerOfTwoRadix ? shiftRightAmount -= Log2ofPowerOfTwoRadix : 0;

                    if (length >= SortRadixMsdLongThreshold)
                        RadixSortMsdLongParInner(a, first, length, shiftRightAmount, baseCaseInPlaceSort);
                    else if (length >= 2)
                        //InsertionSort(a, first, length);
                        baseCaseInPlaceSort(a, first, length);
                }
            }
        }

        private static void RadixSortMsdLongFullyParInner(long[] a, int first, int length, int shiftRightAmount, Action<long[], int, int> baseCaseInPlaceSort)
        {
            if (length < SortRadixMsdLongThreshold)
            {
                baseCaseInPlaceSort(a, first, length);
                //InsertionSort(a, first, length);
                return;
            }
            int last = first + length - 1;
            //const ulong bitMask = PowerOfTwoRadix - 1;
            const byte halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;

            var count = HistogramOneByteComponentPar(a, first, last, shiftRightAmount);

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
                        byte digit;
                        byte halfptr = halfOfPowerOfTwoRadix;
                        while (endOfBin[digit = (byte)((byte)(a[_current] >> shiftRightAmount) ^ halfptr)] != _current)
                        {
                            long temp = a[_current];            // inlining Swap() increased performance about 5-10%
                            a[_current] = a[endOfBin[digit]];
                            a[endOfBin[digit]++] = temp;
                        }
                        endOfBin[digit]++;

                        while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                        _current = endOfBin[nextBin - 1];
                    }
                }
                else
                {
                    for (int _current = first; _current <= last;)
                    {
                        byte digit;
                        while (endOfBin[digit = (byte)(a[_current] >> shiftRightAmount)] != _current)
                        {
                            long temp = a[_current];            // inlining Swap() increased performance about 5-10%
                            a[_current] = a[endOfBin[digit]];
                            a[endOfBin[digit]++] = temp;
                        }
                        endOfBin[digit]++;

                        while (endOfBin[nextBin - 1] == startOfBin[nextBin]) nextBin++;   // skip over empty and full bins, when the end of the current bin reaches the start of the next bin
                        _current = endOfBin[nextBin - 1];
                    }
                }
                if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
                {
                    shiftRightAmount = shiftRightAmount >= Log2ofPowerOfTwoRadix ? shiftRightAmount -= Log2ofPowerOfTwoRadix : 0;

                    List<Action> actions = new List<Action>();

                    for (int i = 0; i < PowerOfTwoRadix; i++)
                    {
                        if ((endOfBin[i] - startOfBin[i]) > SortRadixMsdLongParallelThreshold)
                        {
                            //Task.Factory.StartNew(() => RadixSortMsdLongParInner(a, startOfBin[i], endOfBin[i] - startOfBin[i], shiftRightAmount, baseCaseInPlaceSort),
                            //    CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
                            actions.Add(() => RadixSortMsdLongFullyParInner(a, startOfBin[i], endOfBin[i] - startOfBin[i], shiftRightAmount, baseCaseInPlaceSort));
                        }
                        else
                            RadixSortMsdLongFullyParInner(a, startOfBin[i], endOfBin[i] - startOfBin[i], shiftRightAmount, baseCaseInPlaceSort);
                    }
                    Parallel.Invoke(actions.ToArray());
                }
            }
            else
            {
                if (shiftRightAmount > 0)    // end recursion when all the bits have been processes
                {
                    shiftRightAmount = shiftRightAmount >= Log2ofPowerOfTwoRadix ? shiftRightAmount -= Log2ofPowerOfTwoRadix : 0;
                    RadixSortMsdLongFullyParInner(a, first, length, shiftRightAmount, baseCaseInPlaceSort);
                }
            }
        }
        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        public static void SortRadixMsdPar(this long[] arrayToBeSorted)
        {
            int shiftRightAmount = sizeof(ulong) * 8 - Log2ofPowerOfTwoRadix;
            // InsertionSort could be passed in as another base case since it's in-place
            RadixSortMsdLongParInner(arrayToBeSorted, 0, arrayToBeSorted.Length, shiftRightAmount, Array.Sort);
        }
        /// <summary>
        /// In-place Radix Sort (Most Significant Digit), not stable. Functional style interface, which returns the input array, but sorted.
        /// </summary>
        /// <param name="arrayToBeSorted">array that is to be sorted in place</param>
        /// <returns>returns the input array itself, but sorted</returns>
        public static long[] SortRadixMsdInPlaceFuncPar(this long[] arrayToBeSorted)
        {
            arrayToBeSorted.SortRadixMsd();
            return arrayToBeSorted;
        }
    }
}
