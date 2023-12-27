﻿// TODO: Idea: Since SortRadix2 significantly improves memory access pattern of Radix Sort of User Defined Classes (i.e. array of references with are scattered within the heap)
//       use this improvement (which is about 10X speedup) to speedup the parallel version, as each of the tasks will only get in each other's way during the first pass, and
//       will use the much improved memory access pattern of subsequent passes of the LSD Radix Sort algorithm, hopefully providing parallel acceleration.
// TODO: Set parallelism for Parallel Radix Sort to the number of CPU cores by default.
// TODO: To speedup parallel Counting/Histogram, create a single dimension array instead of a jagged one, which will have an optimal layout within L1 cache with counts
//       not interfering with each other.
// TODO: Parallelize user-defined-type extraction of counts and input keys in the histogram and benchmark
// TODO: Add handling of an input array of size zero to all sorting algorithms, where an output array of zero length is returned, if not in-place
// TODO: In parallel LSD Radix Sort, optimize this division out by using nested loops, as division even integer is slow
// TODO: It seems like ComputeStartOfBinsPar needs to have the parallel threshold to be passed in and optimized
// TODO: Optimize digit extraction in LSD and MSD Radix algorithms by doing shift right first and then masking, to keep the mask a constant value, and not needing to shift the mask.

#pragma warning disable CA1510
#pragma warning disable CA1303

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
        /// <summary>
        /// Parallel Sort an array of bytes using Radix Sorting algorithm (least significant digit variation - LSD)
        /// This algorithm is stable, but is not in-place.
        /// </summary>
        /// <param name="arrayToBeSorted">array of unsigned integers to be sorted</param>
        /// <returns>sorted array of bytes</returns>
        public static byte[] SortRadixPar(this byte[] arrayToBeSorted)
        {
            return arrayToBeSorted.SortCountingInPlaceFuncPar();
        }

        /// <summary>
        /// Parallel Sort an array of unsigned short integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// This algorithm is stable, but is not in-place.
        /// </summary>
        /// <param name="arrayToBeSorted">array of unsigned integers to be sorted</param>
        /// <returns>sorted array of unsigned short integers</returns>
        public static ushort[] SortRadixPar(this ushort[] arrayToBeSorted)
        {
            return arrayToBeSorted.SortCountingInPlaceFuncPar();
        }

        public static uint[][] ComputeStartOfBinsPar(this uint[] inputArray, int workQuanta, uint numberOfQuantas, uint digit)
        {
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
            uint numberOfBins = 256;

            //uint[][] count = Algorithm.HistogramByteComponentsAcrossWorkQuantasQC(inputArray, workQuanta, digit);
            //uint[][] count = Algorithm.HistogramByteComponentsAcrossWorkQuantasQC(inputArray, 0, inputArray.Length - 1, workQuanta, digit);
            uint[][] count = ParallelAlgorithm.HistogramByteComponentsQCPar(inputArray, 0, inputArray.Length - 1, workQuanta, numberOfQuantas, digit);

            uint[][] startOfBin = new uint[numberOfQuantas][];     // start of bin for each parallel work item
            for (int q = 0; q < numberOfQuantas; q++)
                startOfBin[q] = new uint[numberOfBins];

            uint[] sizeOfBin = new uint[numberOfBins];

            // Determine the overall size of each bin, across all work quanta
            for (uint b = 0; b < numberOfBins; b++)
            {
                sizeOfBin[b] = 0;
                for (int q = 0; q < numberOfQuantas; q++)
                    sizeOfBin[b] += count[q][b];
                //Console.WriteLine("ComputeStartOfBins: d = {0}  sizeOfBin[{1}] = {2}", digit, b, sizeOfBin[b]);
            }

            // Determine starting of bins for work quanta 0
            startOfBin[0][0] = 0;
            for (uint b = 1; b < numberOfBins; b++)
            {
                startOfBin[0][b] = startOfBin[0][b - 1] + sizeOfBin[b - 1];
                //startOfBin[0][b] = sizeOfBin[b - 1];
                //if (digit == 1)
                //    Console.WriteLine("ComputeStartOfBins: d = {0}  startOfBin[0][{1}] = {2}", digit, b, startOfBin[0][b]);
            }

            // Determine starting of bins for work quanta 1 thru Q
            for (uint q = 1; q < numberOfQuantas; q++)
                for (uint b = 0; b < numberOfBins; b++)
                {
                    startOfBin[q][b] = startOfBin[q - 1][b] + count[q - 1][b];
                    //if (digit == 1)
                    //    Console.WriteLine("ComputeStartOfBins: d = {0}  sizeOfBin[{1}][{2}] = {3}", digit, q, b, startOfBin[q][b]);
                }

            return startOfBin;
        }
        // Permute function with de-randomized write memory accesses
        private static void SortRadixDeRandomizedInnerFunction( uint[] inputArray, uint[] outputArray, uint[][] startOfBin, uint[][] bufferIndex, uint[][] bufferDerandomize, uint[] bufferIndexEnd,
                                                                CustomData data, uint endIndex, uint BufferDepth, int NumberOfBins)
        {
            if (data == null)
                return;
            uint qLoc = data.q;
            uint[] startOfBinLoc        = startOfBin[       qLoc];
            uint[] bufferIndexLoc       = bufferIndex[      qLoc];
            uint[] bufferDerandomizeLoc = bufferDerandomize[qLoc];
            //Console.WriteLine("current = {0}, q = {1}, bitMask = {2}, shiftRightAmount = {3}", currIndex, qLoc, data.bitMask, data.shiftRightAmount);
            for (uint currIndex = data.current; currIndex < endIndex; currIndex++)
            {
                uint currDigit = (inputArray[currIndex] & data.bitMask) >> data.shiftRightAmount;
                if (bufferIndexLoc[currDigit] < bufferIndexEnd[currDigit])
                {
                    bufferDerandomizeLoc[bufferIndexLoc[currDigit]++] = inputArray[currIndex];
                }
                else
                {
                    uint outIndex  = startOfBinLoc[currDigit];
                    uint buffIndex = currDigit * BufferDepth;
                    //for (uint i = 0; i < BufferDepth; i++)
                    //    outputArray[outIndex++] = bufferDerandomizeLoc[buffIndex++];   // TODO: use SSE
                    Array.Copy(bufferDerandomizeLoc, buffIndex, outputArray, outIndex, BufferDepth);
                    startOfBinLoc[currDigit] += BufferDepth;
                    bufferDerandomizeLoc[currDigit * BufferDepth] = inputArray[currIndex];
                    bufferIndexLoc[currDigit] = currDigit * BufferDepth + 1;
                }
            }
            // Flush all the derandomization buffers
            for (uint whichBuff = 0; whichBuff < NumberOfBins; whichBuff++)
            {
                uint buffStartIndex = whichBuff * BufferDepth;
                uint buffEndIndex = bufferIndexLoc[whichBuff];
                //Console.WriteLine("q = {0}, numOfElementsInBuff[{1}] = {2}", qLoc, whichBuff, numOfElementsInBuff);
                while (buffStartIndex < buffEndIndex)
                    outputArray[startOfBinLoc[whichBuff]++] = bufferDerandomizeLoc[buffStartIndex++];
                bufferIndexLoc[whichBuff] = whichBuff * BufferDepth;
            }
        }

        /// <summary>
        /// Fully Parallel Sort an array of unsigned integers using LSD Radix Sorting algorithm (least significant digit - LSD)
        /// This algorithm is stable, and is not in-place. Counting portion and permutation are both parallel.
        /// This method is referenced in the Parallel LSD Radix Sort section of Practical Parallel Algorithms Book
        /// </summary>
        /// <param name="inputArray">array of unsigned integers to be sorted</param>
        /// <param name="ParallelWorkQuantum">number of array elements each core will process</param>
        /// <returns>sorted array of unsigned integers</returns>
        public static uint[] SortRadixPar(this uint[] inputArray, int ParallelWorkQuantum = 64 * 1024)
        {
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
            const int NumberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            uint[] outputArray = new uint[inputArray.Length];
            bool outputArrayHasResult = false;
            uint quanta = (inputArray.Length % ParallelWorkQuantum) == 0 ? (uint)(inputArray.Length / ParallelWorkQuantum)
                                                                         : (uint)(inputArray.Length / ParallelWorkQuantum + 1);
            // Setup de-randomization buffers for writes during the permutation phase
            const uint BufferDepth = 64;
            uint[][] bufferDerandomize = new uint[quanta][];
            for (int q = 0; q < quanta; q++)
                bufferDerandomize[q] = new uint[NumberOfBins * BufferDepth];

            uint[][] bufferIndex = new uint[quanta][];
            for (int q = 0; q < quanta; q++)
            {
                bufferIndex[q] = new uint[NumberOfBins];
                bufferIndex[q][0] = 0;
                for (int b = 1; b < NumberOfBins; b++)
                    bufferIndex[q][b] = bufferIndex[q][b - 1] + BufferDepth;
            }
            uint[] bufferIndexEnd = new uint[NumberOfBins];
            bufferIndexEnd[0] = BufferDepth;                            // non-inclusive
            for (int b = 1; b < NumberOfBins; b++)
                bufferIndexEnd[b] = bufferIndexEnd[b - 1] + BufferDepth;
            // End of de-randomization buffers setup

            if (inputArray.Length == 0)
                return outputArray;

            // Use TPL ideas from https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming

            uint bitMask = 255;
            int shiftRightAmount = 0;
            uint digit = 0;

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                uint[][] startOfBin = ComputeStartOfBinsPar(inputArray, ParallelWorkQuantum, quanta, digit);

                // The last work item may not have the full parallelWorkQuanta of items to process
                uint numberOfFullQuantas = (uint)(inputArray.Length / ParallelWorkQuantum);
                Task[] taskArray = new Task[quanta];
                uint q;
                for (q = 0; q < numberOfFullQuantas; q++)
                {
                    uint current = (uint)(q * ParallelWorkQuantum);
                    taskArray[q] = Task.Factory.StartNew((Object obj) => {
                        CustomData data = obj as CustomData;
                        uint endIndex = (uint)(data.current + ParallelWorkQuantum);

                        SortRadixDeRandomizedInnerFunction(inputArray, outputArray, startOfBin, bufferIndex, bufferDerandomize, bufferIndexEnd, data, endIndex, BufferDepth, NumberOfBins);
                    },
                    new CustomData() { current = current, q = q, bitMask = bitMask, shiftRightAmount = shiftRightAmount }
                    );
                }
                if (quanta > numberOfFullQuantas)      // last partially filled workQuantum
                {
                    uint current = (uint)(q * ParallelWorkQuantum);
                    taskArray[q] = Task.Factory.StartNew((Object obj) => {
                        CustomData data = obj as CustomData;
                        uint endIndex = (uint)inputArray.Length;

                        SortRadixDeRandomizedInnerFunction(inputArray, outputArray, startOfBin, bufferIndex, bufferDerandomize, bufferIndexEnd, data, endIndex, BufferDepth, NumberOfBins);
                    },
                    new CustomData() { current = current, q = q, bitMask = bitMask, shiftRightAmount = shiftRightAmount }
                    );
                }
                Task.WaitAll(taskArray);    // wait for all work quanta to finish before starting the next pass, as passes have to be done in sequence

                bitMask <<= Log2ofPowerOfTwoRadix;
                digit++;
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

        /// <summary>
        /// Parallel Sort an array of unsigned integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// The core algorithm is not in-place, but the interface is in-place. This algorithm is stable.
        /// </summary>
        /// <param name="inputArray">array of unsigned integers to be sorted</param>
        /// <returns>sorted array of unsigned integers</returns>
        public static void SortRadixInPlaceInterfacePar(this uint[] inputArray)
        {
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
            var sortedArray = SortRadixPar(inputArray);
            Array.Copy(sortedArray, inputArray, inputArray.Length);
        }

        /// <summary>
        /// Partially Parallel Sort an array of unsigned integers using LSD Radix Sorting algorithm (least significant digit - LSD)
        /// This algorithm is stable, but is not in-place.
        /// Only the Histogram portion of the algorithm is parallel.
        /// </summary>
        /// <param name="inputArray">array of unsigned integers to be sorted</param>
        /// <returns>sorted array of unsigned integers</returns>
        public static uint[] SortRadixPartialPar(this uint[] inputArray, int parallelThresholdHistogram = 16 * 1024)
        {
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
            int numberOfBins = 256;
            int numberOfDigits = 4;
            int Log2ofPowerOfTwoRadix = 8;
            int d = 0;
            uint[] outputArray = new uint[inputArray.Length];

            uint[][] startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            if ((parallelThresholdHistogram * Environment.ProcessorCount) <= inputArray.Length)
                parallelThresholdHistogram = inputArray.Length / Environment.ProcessorCount;

            uint[][] count = HistogramByteComponentsPar(inputArray, 0, inputArray.Length - 1, parallelThresholdHistogram);

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
        /// Sort an array of unsigned integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// This algorithm is not in-place. This algorithm is stable. Two-phase implementation.
        /// </summary>
        /// <param name="inOutArray">array of unsigned integers to be sorted, and where the sorted array will be returned</param>
        /// <param name="startIndex">index of the first element where sorting is to start</param>
        /// <param name="length">number of array elements to sort</param>
        /// <returns>sorted array of unsigned integers</returns>
        public static void SortRadixPartialPar(this uint[] inOutArray, int startIndex, int length, int parallelThresholdHistogram = 16 * 1024)
        {
            if (inOutArray == null)
                throw new ArgumentNullException(nameof(inOutArray));
            const int bitsPerDigit = 8;
            uint numberOfBins = 1 << bitsPerDigit;
            uint numberOfDigits = (sizeof(uint) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            //Console.WriteLine("SortRadix: NumberOfDigits = {0}", numberOfDigits);
            uint[] workBuffer = new uint[inOutArray.Length];    // TODO: Reduce to length instead of inOutArray.Length
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
            if ((parallelThresholdHistogram * Environment.ProcessorCount) <= length)
                parallelThresholdHistogram = inOutArray.Length / Environment.ProcessorCount;

            uint[][] count = HistogramByteComponentsPar(inOutArray, startIndex, startIndex + length - 1, parallelThresholdHistogram);

            //uint[][] count = HistogramByteComponents(inOutArray, startIndex, startIndex + length - 1);
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
        /// Parallel Sort an array of unsigned integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// This algorithm uses SIMD/SSE instructions for higher performance on each core, as well as multiple cores.
        /// This algorithm is stable, but is not in-place.
        /// Only the Histogram portion of the algorithm is parallel.
        /// </summary>
        /// <param name="inputArray">array of unsigned integers to be sorted</param>
        /// <returns>sorted array of unsigned integers</returns>
        public static uint[] SortRadixSsePar(this uint[] inputArray)
        {
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
            int numberOfBins = 256;
            int numberOfDigits = 4;
            int Log2ofPowerOfTwoRadix = 8;
            int d = 0;
            uint[] outputArray = new uint[inputArray.Length];

            uint[][] startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            uint[][] count = HistogramByteComponentsSsePar(inputArray, 0, inputArray.Length - 1);

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
        /// Partially Parallel Sort an array of signed integers using LSD Radix Sorting algorithm (least significant digit - LSD)
        /// This algorithm is stable, but not in-place.
        /// Only the Histogram portion of the algorithm is parallel.
        /// </summary>
        /// <param name="inputArray">array of signed long integers to be sorted</param>
        /// <returns>sorted array of signed long integers</returns>
        public static int[] SortRadixPar(this int[] inputArray)
        {
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
            const int bitsPerDigit = 8;
            const uint numberOfBins = 1 << bitsPerDigit;
            const uint numberOfDigits = (sizeof(uint) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            int d = 0;
            var outputArray = new int[inputArray.Length];

            uint[][] startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            const uint bitMask = numberOfBins - 1;
            const uint halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;
            int shiftRightAmount = 0;

            uint[][] count = HistogramByteComponentsPar(inputArray, 0, inputArray.Length - 1);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            d = 0;
            while (d < numberOfDigits)
            {
                uint[] startOfBinLoc = startOfBin[d];

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
        /// Partially Parallel Sort an array of signed integers using LSD Radix Sorting algorithm (least significant digit - LSD)
        /// This algorithm is stable, but not in-place.
        /// Only the Histogram portion of the algorithm is parallel.
        /// </summary>
        /// <param name="inputArray">array of signed long integers to be sorted</param>
        /// <returns>sorted array of signed long integers</returns>
        public static int[] SortRadixSsePar(this int[] inputArray)
        {
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
            const int bitsPerDigit = 8;
            const uint numberOfBins = 1 << bitsPerDigit;
            const uint numberOfDigits = (sizeof(uint) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            int d = 0;
            var outputArray = new int[inputArray.Length];

            uint[][] startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            const uint bitMask = numberOfBins - 1;
            const uint halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;
            int shiftRightAmount = 0;

            uint[][] count = HistogramByteComponentsSsePar(inputArray, 0, inputArray.Length - 1);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            d = 0;
            while (d < numberOfDigits)
            {
                uint[] startOfBinLoc = startOfBin[d];

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
        /// Partially Parallel Sort an array of signed long integers using LSD Radix Sorting algorithm (least significant digit - LSD)
        /// This algorithm is stable, but not in-place.
        /// Only the Histogram portion of the algorithm is parallel.
        /// </summary>
        /// <param name="inputArray">array of signed long integers to be sorted</param>
        /// <returns>sorted array of signed long integers</returns>
        public static long[] SortRadixPar(this long[] inputArray)
        {
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
            const int bitsPerDigit = 8;
            const uint numberOfBins = 1 << bitsPerDigit;
            const uint numberOfDigits = (sizeof(ulong) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            int d = 0;
            var outputArray = new long[inputArray.Length];

            uint[][] startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            const ulong bitMask = numberOfBins - 1;
            const ulong halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;
            int shiftRightAmount = 0;

            uint[][] count = HistogramByteComponentsPar(inputArray, 0, inputArray.Length - 1);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            d = 0;
            while (d < numberOfDigits)
            {
                uint[] startOfBinLoc = startOfBin[d];

                if (d != 7)
                    for (uint current = 0; current < inputArray.Length; current++)
                        outputArray[startOfBinLoc[((ulong)inputArray[current]  >> shiftRightAmount) & bitMask]++] = inputArray[current];
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
        /// Partial Parallel Sort an array of signed long integers using LSD Radix Sorting algorithm (least significant digit - LSD)
        /// This algorithm uses SIMD/SSE instructions for higher performance on each core, as well as multiple cores.
        /// This algorithm is stable, but not in-place.
        /// Only the Histogram portion of the algorithm is parallel.
        /// </summary>
        /// <param name="inputArray">array of signed long integers to be sorted</param>
        /// <returns>sorted array of signed long integers</returns>
        public static long[] SortRadixSsePar(this long[] inputArray)
        {
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
            const int bitsPerDigit = 8;
            const uint numberOfBins = 1 << bitsPerDigit;
            const uint numberOfDigits = (sizeof(ulong) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            int d = 0;
            var outputArray = new long[inputArray.Length];

            uint[][] startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            const ulong bitMask = numberOfBins - 1;
            const ulong halfOfPowerOfTwoRadix = PowerOfTwoRadix / 2;
            int shiftRightAmount = 0;

            uint[][] count = HistogramByteComponentsSsePar(inputArray, 0, inputArray.Length - 1);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            d = 0;
            while (d < numberOfDigits)
            {
                uint[] startOfBinLoc = startOfBin[d];

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
        /// Partially Parallel Sort an array of signed long integers using LSD Radix Sorting algorithm (least significant digit - LSD)
        /// The core algorithm is not in-place, but the interface is in-place. This algorithm is stable.
        /// Only the Histogram portion of the algorithm is parallel.
        /// </summary>
        /// <param name="inputArray">array of signed long integers to be sorted</param>
        /// <returns>sorted array of signed long integers</returns>
        public static void SortRadixInPlaceInterfacePar(this long[] inputArray)
        {
            var sortedArray = SortRadixPar(inputArray);
            Array.Copy(sortedArray, inputArray, inputArray.Length);
        }

        /// <summary>
        /// Partially Parallel Sort an array of unsigned long integers using LSD Radix Sorting algorithm (least significant digit - LSD)
        /// This algorithm is stable, but is not in-place.
        /// Only the Histogram portion of the algorithm is parallel.
        /// </summary>
        /// <param name="inputArray">array of unsigned long integers to be sorted</param>
        /// <returns>sorted array of unsigned long integers</returns>
        public static ulong[] SortRadixPar(this ulong[] inputArray)
        {
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
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

            uint[][] count = HistogramByteComponentsPar(inputArray, 0, inputArray.Length - 1);

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
        /// Partially Parallel Sort an array of unsigned integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// The core algorithm is not in-place, but the interface is in-place. This algorithm is stable.
        /// Only the Histogram portion of the algorithm is parallel.
        /// </summary>
        /// <param name="inputArray">array of unsigned integers to be sorted</param>
        /// <returns>sorted array of unsigned integers</returns>
        public static void SortRadixInPlaceInterfacePar(this ulong[] inputArray)
        {
            var sortedArray = SortRadixPar(inputArray);
            Array.Copy(sortedArray, inputArray, inputArray.Length);
        }
    }

    sealed class CustomData
    {
        public uint current;
        public uint q;
        public uint bitMask;
        public int shiftRightAmount;
    }

    static public partial class ParallelAlgorithmExperimental
    {
        /// <summary>
        /// Minimal amount of work to be performed in parallel
        /// </summary>
        public static UInt32 SortRadixParallelWorkQuanta { get; set; } = 64 * 1024;
        ///// <summary>
        ///// Number of tasks that will run in parallel within the Parallel Radix Sort algorithm
        ///// </summary>
        //public static Int32 SortRadixParallelAmountOfParallelism { get; set; } = Environment.ProcessorCount;
        /// <summary>
        /// Sort an array of unsigned integers using Parallel Radix Sorting algorithm (least significant digit variation)
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns>array of unsigned integers</returns>
        public static uint[] SortRadixPar1(this uint[] inputArray)
        {
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
            uint numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            uint[] outputArray = new uint[inputArray.Length];
            bool outputArrayHasResult = false;

            uint numWorkItems = (uint)inputArray.Length / SortRadixParallelWorkQuanta + 1;

            uint[][] count = new uint[numWorkItems][];          // count        for each parallel work item
            for (int i = 0; i < numWorkItems; i++)
                count[i] = new uint[numberOfBins];
            uint[][] startOfBin = new uint[numWorkItems][];     // start of bin for each parallel work item
            for (int i = 0; i < numWorkItems; i++)
                startOfBin[i] = new uint[numberOfBins];

            // Use TPL ideas from https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming

            uint bitMask = 255;
            int shiftRightAmount = 0;

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                for (uint r = 0; r < numWorkItems; r++)
                    for (uint b = 0; b < numberOfBins; b++)
                        count[r][b] = 0;
                for (uint current = 0; current < inputArray.Length; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    uint r = current / SortRadixParallelWorkQuanta;             // TODO: Optimize this division out by using nested loops, as division even integer is slow
                    count[r][ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++;
                }
                for (uint b = 0; b < numberOfBins; b++)     // for each bin, create startOfBin for each work item (work quanta), but relative to zero
                {
                    startOfBin[0][b] = 0;
                    for (uint r = 1; r < numWorkItems; r++)
                        startOfBin[r][b] = (uint)(startOfBin[r - 1][b] + count[r - 1][b]);
                }
                for (uint b = 1; b < numberOfBins; b++)     // adjust each item within each bin by the offset of previous bin and that bins size
                {
                    uint sizeOfPrevBin = startOfBin[numWorkItems - 1][b - 1] + count[numWorkItems - 1][b - 1];
                    for (uint r = 0; r < numWorkItems; r++)
                        startOfBin[r][b] += sizeOfPrevBin;
                }
#if false
                for (uint current = 0; current < inputArray.Length; current++)
                {
                    uint r = current / SortRadixParallelWorkQuanta;
                    outputArray[startOfBin[r, ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++] = inputArray[current];
                }
#else
#if true
                // The last work item may not have the full parallelWorkQuanta of items to process
                Task[] taskArray = new Task[numWorkItems - 1];
                for (uint q = 0; q < numWorkItems - 1; q++)
                {
                    uint current = q * SortRadixParallelWorkQuanta;
                    taskArray[q] = Task.Factory.StartNew((Object obj) => {
                            CustomData data = obj as CustomData;
                            if (data == null)
                                return;
                            uint currIndex = data.current;
                            uint qLoc = data.q;
                            uint[] startOfBinLoc = startOfBin[qLoc];
                            //Console.WriteLine("current = {0}, r = {1}, bitMask = {2}, shiftRightAmount = {3}", currIndex, rLoc, data.bitMask, data.shiftRightAmount);
                            for (uint i = 0; i < SortRadixParallelWorkQuanta; i++)
                            {
                                outputArray[startOfBinLoc[(inputArray[currIndex] & data.bitMask) >> data.shiftRightAmount]++] = inputArray[currIndex];
                                currIndex++;
                            }
                        },
                        new CustomData() { current = current, q = q, bitMask = bitMask, shiftRightAmount = shiftRightAmount }
                    );
                }
                Task.WaitAll(taskArray);
#else
                // The last work item may not have the full parallelWorkQuanta of items to process
                for (uint r = 0; r < numWorkItems - 1; r++)
                {
                    uint current = r * SortRadixParallelWorkQuanta;
                    for (uint i = 0; i < SortRadixParallelWorkQuanta; i++)
                    {
                        outputArray[startOfBin[r, ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++] = inputArray[current];
                        current++;
                    }
                }
#endif
                // The last iteration, which may not have the full parallelWorkQuanta of items to process
                uint currentLast = (numWorkItems - 1) * SortRadixParallelWorkQuanta;
                uint numItems = (uint)inputArray.Length % SortRadixParallelWorkQuanta;
                for (uint i = 0; i < numItems; i++)
                {
                    outputArray[startOfBin[(numWorkItems - 1)][ExtractDigit(inputArray[currentLast], bitMask, shiftRightAmount)]++] = inputArray[currentLast];
                    currentLast++;
                }
#endif

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

        // Brought Histogram outside of the main loop, to separate into two phases: Histogram/counting and permutation
        public static uint[] SortRadixPar2(this uint[] inputArray)
        {
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
            uint numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            int numDigits = 4;
            uint[] outputArray = new uint[inputArray.Length];
            bool outputArrayHasResult = false;

            if (inputArray.Length == 0)
                return outputArray;

            uint numberOfQuantas = (inputArray.Length % SortRadixParallelWorkQuanta) == 0 ? (uint)(inputArray.Length / SortRadixParallelWorkQuanta)
                                                                                          : (uint)(inputArray.Length / SortRadixParallelWorkQuanta + 1);
            uint numberOfFullQuantas = (uint)(inputArray.Length / SortRadixParallelWorkQuanta);

            Console.WriteLine("Before Histogram");
            // TODO: This histogram operation can be done in parallel to speed it up
            uint[][][] count = Algorithm.HistogramByteComponentsAcrossWorkQuantas(inputArray, SortRadixParallelWorkQuanta);
            Console.WriteLine("After Histogram");

            uint[][][] startOfBin = new uint[numberOfQuantas][][];     // start of bin for each parallel work item
            for (int q = 0; q < numberOfQuantas; q++)
            {
                startOfBin[q] = new uint[numDigits][];
                for (int d = 0; d < numDigits; d++)
                    startOfBin[q][d] = new uint[numberOfBins];
            }

            for (int d = 0; d < numDigits; d++)             // for each bin, create startOfBin for each work quanta, but relative to zero for quanta[0] & bin[0]
                for (uint b = 0; b < numberOfBins; b++)     // because all bin[0]'s will come before all bin[1]'s and so on... and each bin is split into pieces associated with each work quanta
                {
                    startOfBin[0][d][b] = 0;
                    for (int q = 1; q < numberOfQuantas; q++)
                        startOfBin[q][d][b] = startOfBin[q - 1][d][b] + count[q - 1][d][b];
                }

            for (int d = 0; d < numDigits; d++)
                for (uint b = 1; b < numberOfBins; b++)     // adjust each item within each bin by the offset of previous bin and that bin's size
                {
                    uint startOfThisBin = startOfBin[numberOfQuantas - 1][d][b - 1] + count[numberOfQuantas - 1][d][b - 1];
                    for (uint q = 0; q < numberOfQuantas; q++)
                        startOfBin[q][d][b] += startOfThisBin;
                }
                //for (int d = 0; d < numDigits; d++)
                //    for (uint q = 0; q < quanta; q++)
                //    {
                //        Console.WriteLine("s: q = {0}   d = {1}", q, d);
                //        for (uint b = 0; b < numberOfBins; b++)
                //            Console.Write("{0}, ", startOfBin[q][d][b]);
                //        Console.WriteLine();
                //    }

            // Use TPL ideas from https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming

            uint bitMask = 255;
            int shiftRightAmount = 0;
            uint digit = 0;

            Console.WriteLine("Before main permutation");
            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
#if false
                for (uint current = 0; current < inputArray.Length; current++)
                {
                    uint r = current / SortRadixParallelWorkQuanta;
                    outputArray[startOfBin[r, ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++] = inputArray[current];
                }
#else
#if false
                // The last work item may not have the full parallelWorkQuanta of items to process
                Task[] taskArray = new Task[numberOfFullQuantas];
                for (uint q = 0; q < numberOfFullQuantas; q++)
                {
                    uint current = q * SortRadixParallelWorkQuanta;
                    taskArray[q] = Task.Factory.StartNew((Object obj) => {
                        CustomData data = obj as CustomData;
                        if (data == null)
                            return;
                        uint currIndex = data.current;
                        uint qLoc = data.q;
                        uint[] startOfBinLoc = startOfBin[qLoc][digit];
                        //Console.WriteLine("current = {0}, q = {1}, bitMask = {2}, shiftRightAmount = {3}", currIndex, qLoc, data.bitMask, data.shiftRightAmount);
                        for (uint i = 0; i < SortRadixParallelWorkQuanta; i++)
                        {
                            outputArray[startOfBinLoc[(inputArray[currIndex] & data.bitMask) >> data.shiftRightAmount]++] = inputArray[currIndex];
                            currIndex++;
                        }
                    },
                        new CustomData() { current = current, q = q, bitMask = bitMask, shiftRightAmount = shiftRightAmount }
                    );
                }
                Task.WaitAll(taskArray);
#else
                for (uint q = 0; q < numberOfFullQuantas; q++)
                {
                    uint[] startOfBinLoc = startOfBin[q][digit];
                    uint current = q * SortRadixParallelWorkQuanta;
                    for (uint i = 0; i < SortRadixParallelWorkQuanta; i++)
                    {
                        outputArray[startOfBinLoc[ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++] = inputArray[current];
                        current++;
                    }
                }
#endif
                // The last work item may not have the full parallelWorkQuanta of items to process
                Console.WriteLine("Before last permutation");
                if (numberOfQuantas > numberOfFullQuantas)
                {
                    // The last iteration, which may not have the full parallelWorkQuanta of items to process
                    uint currentLast = numberOfFullQuantas * SortRadixParallelWorkQuanta;
                    uint numItems = (uint)inputArray.Length % SortRadixParallelWorkQuanta;
                    for (uint i = 0; i < numItems; i++)
                    {
                        outputArray[startOfBin[numberOfFullQuantas][digit][ExtractDigit(inputArray[currentLast], bitMask, shiftRightAmount)]++] = inputArray[currentLast];
                        currentLast++;
                    }
                }
#endif

                bitMask <<= Log2ofPowerOfTwoRadix;
                digit++;
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

        private static void DoRadixSortLsdParallel(uint current, uint[] outputArray, uint[,] startOfBin, uint r, uint[] inputArray, uint bitMask, int shiftRightAmount)
        {
            for (uint i = 0; i < SortRadixParallelWorkQuanta; i++)
            {
                outputArray[startOfBin[r, ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++] = inputArray[current];
                current++;
            }
        }
        private static UInt32 ExtractDigit(UInt32 value, UInt32 bitMask, int shiftRightAmount)
        {
            return (value & bitMask) >> shiftRightAmount;	// extract the digit we are sorting based on
        }
        private static UInt32 ExtractDigit(UInt64 value, UInt64 bitMask, int shiftRightAmount)
        {
            return (UInt32)((value & bitMask) >> shiftRightAmount);	// extract the digit we are sorting based on
        }
    }
}
