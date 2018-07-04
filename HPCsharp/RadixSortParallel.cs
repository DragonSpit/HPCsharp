using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
        /// <summary>
        /// Minimal amount of work to be performed in parallel
        /// </summary>
        public static UInt32 SortRadixParallelWorkQuanta { get; set; } = 8 * 1024;
        /// <summary>
        /// Sort an array of unsigned integers using Parallel Radix Sorting algorithm (least significant digit variation)
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns>array of unsigned integers</returns>
        public static uint[] SortRadixPar(this uint[] inputArray)
        {
            uint numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            uint[] outputArray = new uint[inputArray.Length];
            bool outputArrayHasResult = false;

            uint numWorkItems;
            if (inputArray.Length % SortRadixParallelWorkQuanta == 0)
                numWorkItems = (uint)inputArray.Length / SortRadixParallelWorkQuanta;
            else
                numWorkItems = (uint)inputArray.Length / SortRadixParallelWorkQuanta + 1;

            uint[,] count      = new uint[numWorkItems, numberOfBins];  // count        for each parallel work item
            uint[,] startOfBin = new uint[numWorkItems, numberOfBins];  // start of bit for each parallel work item

            // Use TPL ideas from https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming

            uint bitMask = 255;
            int shiftRightAmount = 0;

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                for (uint r = 0; r < numWorkItems; r++)
                    for (uint c = 0; c < numberOfBins; c++)
                        count[r, c] = 0;
                for (uint current = 0; current < inputArray.Length; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    uint r = current / SortRadixParallelWorkQuanta;
                    count[r, ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++;
                }
                // There is probably a faster and possibly parallel way of accomplishing this
                for (uint c = 0; c < numberOfBins; c++)     // for each column, which is a bin, create startOfBin for each work item, but relative to zero
                {
                    startOfBin[0, c] = 0;
                    for (uint r = 1; r < numWorkItems; r++) // Victor J. Duvanenko https://github.com/DragonSpit/HPCsharp
                        startOfBin[r, c] = (uint)(startOfBin[r - 1, c] + count[r - 1, c]);
                }
                for (uint c = 1; c < numberOfBins; c++)     // adjust each item within each bin by the offset of previous bin and that bins size
                {
                    uint sizeOfPreviouBin = startOfBin[numWorkItems - 1, c - 1] + count[numWorkItems - 1, c - 1];
                    for (uint r = 0; r < numWorkItems; r++)
                        startOfBin[r, c] += sizeOfPreviouBin;
                }

#if false
                for (uint current = 0; current < inputArray.Length; current++)
                {
                    uint r = current / SortRadixParallelWorkQuanta;
                    outputArray[startOfBin[r, ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++] = inputArray[current];
                }
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
                // The last iteration, which may not have the full parallelWorkQuanta of items to process
                uint currentLast = (numWorkItems - 1) * SortRadixParallelWorkQuanta;
                uint numItems = (uint)inputArray.Length % SortRadixParallelWorkQuanta;
                for (uint i = 0; i < numItems; i++)
                {
                    outputArray[startOfBin[(numWorkItems - 1), ExtractDigit(inputArray[currentLast], bitMask, shiftRightAmount)]++] = inputArray[currentLast];
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
