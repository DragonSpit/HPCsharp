using System;
using System.Collections.Generic;
using System.Text;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        /// <summary>
        /// Sort an array of unsigned integers using Radix Sorting algorithm (least significant digit variation)
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns>array of unsigned integers</returns>
        public static uint[] SortRadix(this uint[] inputArray)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            uint[] outputArray = new uint[inputArray.Length];
            uint[] count       = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            uint[] startOfBin = new uint[numberOfBins];
            uint[] endOfBin   = new uint[numberOfBins];

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                for (uint i = 0; i < numberOfBins; i++)
                    count[i] = 0;
                for (uint current = 0; current < inputArray.Length; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    count[ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++;

                startOfBin[0] = endOfBin[0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[i] = endOfBin[i] = (uint)(startOfBin[i - 1] + count[i - 1]);

                for (uint current = 0; current < inputArray.Length; current++)
                    outputArray[endOfBin[ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++] = inputArray[current];

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
        private static uint ExtractDigit(uint value, uint bitMask, int shiftRightAmount)
        {
            return (value & bitMask) >> shiftRightAmount;	// extract the digit we are sorting based on
        }
        /// <summary>
        /// Sort an List of unsigned integers using Radix Sorting algorithm (least significant digit variation)
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns>array of unsigned integers</returns>
        public static List<uint> SortRadix(this List<uint> inputArray)
        {
            var srcCopy = inputArray.ToArray();
            var sortedArray = srcCopy.SortRadix();
            var sortedList = new List<uint>(sortedArray);
            return sortedList;
        }
    }
}
