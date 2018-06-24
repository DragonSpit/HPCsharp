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
        /// <summary>
        /// Sort an array of unsigned integers using Radix Sorting algorithm (least significant digit variation)
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns>array of unsigned integers</returns>
        public static uint[] SortRadix4(this uint[] inputArray)
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
            uint[] endOfBin = new uint[numberOfBins];

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                for (uint i = 0; i < numberOfBins; i++)
                {
                    count[i] = 0;
                    cacheBufferIndexes[i] = 0;
                }
                for (uint current = 0; current < inputArray.Length; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    count[ExtractDigit(inputArray[current], bitMask, shiftRightAmount)]++;

                startOfBin[0] = endOfBin[0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[i] = endOfBin[i] = (uint)(startOfBin[i - 1] + count[i - 1]);

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
                            outputArray[endOfBin[whichBin]++] = cacheBuffers[index++];
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
                        outputArray[endOfBin[whichBin]++] = cacheBuffers[index++];
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
        /// <summary>
        /// Sort an array of user defined class containing an unsigned integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// Slower algorithm that allocates only 3K bytes of extra memory.
        /// </summary>
        /// <param name="inputArray"></param>
        /// <param name="getKey">user provided function to extract the unsigned 32-bit key sorted on, from the user defined class</param>
        /// <returns>sorted array of user defined class</returns>
        public static T[] SortRadix<T>(this T[] inputArray, Func<T, UInt32> getKey)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
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
                for (uint current = 0; current < inputArray.Length; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    count[ExtractDigit(getKey(inputArray[current]), bitMask, shiftRightAmount)]++;

                startOfBin[0] = endOfBin[0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[i] = endOfBin[i] = (uint)(startOfBin[i - 1] + count[i - 1]);

                for (uint current = 0; current < inputArray.Length; current++)
                    outputArray[endOfBin[ExtractDigit(getKey(inputArray[current]), bitMask, shiftRightAmount)]++] = inputArray[current];

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;

                T[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }
            if (outputArrayHasResult)
                for (uint current = 0; current < inputArray.Length; current++)    // copy from output array into the input array
                    inputArray[current] = outputArray[current];

            return inputArray;
        }
        /// <summary>
        /// Sort an array of user defined class containing an unsigned 64-bit integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// Slower algorithm that allocates only 3K bytes of extra memory.
        /// </summary>
        /// <param name="inputArray"></param>
        /// <param name="getKey">user provided function to extract the unsigned 64-bit key sorted on, from the user defined class</param>
        /// <returns>sorted array of user defined class</returns>
        public static T[] SortRadix<T>(this T[] inputArray, Func<T, UInt64> getKey)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
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
                for (uint current = 0; current < inputArray.Length; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    count[ExtractDigit(getKey(inputArray[current]), bitMask, shiftRightAmount)]++;

                startOfBin[0] = endOfBin[0] = 0;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[i] = endOfBin[i] = (uint)(startOfBin[i - 1] + count[i - 1]);

                for (uint current = 0; current < inputArray.Length; current++)
                    outputArray[endOfBin[ExtractDigit(getKey(inputArray[current]), bitMask, shiftRightAmount)]++] = inputArray[current];

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;

                T[] tmp = inputArray;       // swap input and output arrays
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
        /// Sort an array of user defined class containing an unsigned integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// </summary>
        /// <param name="inputArray">input array of type T</param>
        /// <param name="inKeys">input array of keys (unsigned integers) to be sorted on</param>
        /// <param name="outSortedKeys">sorted array of keys (unsigned integers)</param>
        /// <returns>sorted array of user defined class</returns>
        public static T[] SortRadix<T>(this T[] inputArray, UInt32[] inKeys, ref UInt32[] outSortedKeys)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            T[] outputArray = new T[inputArray.Length];
            if (outSortedKeys == null || outSortedKeys.Length != inputArray.Length)
                outSortedKeys = new UInt32[inputArray.Length];
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

            return inputArray;
        }
        /// <summary>
        /// Sort an array of user defined class containing an unsigned 64-bit integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// </summary>
        /// <param name="inputArray">input array of type T</param>
        /// <param name="inKeys">input array of keys (unsigned integers) to be sorted on</param>
        /// <param name="outSortedKeys">sorted array of keys (unsigned integers)</param>
        /// <returns>sorted array of user defined class</returns>
        public static T[] SortRadix<T>(this T[] inputArray, UInt64[] inKeys, ref UInt64[] outSortedKeys)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            T[] outputArray = new T[inputArray.Length];
            if (outSortedKeys == null || outSortedKeys.Length != inputArray.Length)
                outSortedKeys = new UInt64[inputArray.Length];
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

            return inputArray;
        }
        /// <summary>
        /// Sort a List of user defined class containing an unsigned integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// </summary>
        /// <param name="inputList">input List of type T</param>
        /// <param name="inKeys">input array of keys (unsigned integers) to be sorted on</param>
        /// <param name="outSortedKeys">sorted array of keys (unsigned integers)</param>
        /// <returns>sorted List of user defined class</returns>
        public static List<T> SortRadix<T>(this List<T> inputList, UInt32[] inKeys, ref UInt32[] outSortedKeys)
        {
            var srcCopy = inputList.ToArray();
            var sortedArray = srcCopy.SortRadix(inKeys, ref outSortedKeys);
            var sortedList = new List<T>(sortedArray);
            return sortedList;
        }
        /// <summary>
        /// Sort an array of user defined class containing an unsigned 64-bit integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// </summary>
        /// <param name="inputList">input List of type T</param>
        /// <param name="inKeys">input array of keys (unsigned integers) to be sorted on</param>
        /// <param name="outSortedKeys">sorted array of keys (unsigned integers)</param>
        /// <returns>sorted List of user defined class</returns>
        public static List<T> SortRadix<T>(this List<T> inputList, UInt64[] inKeys, ref UInt64[] outSortedKeys)
        {
            var srcCopy = inputList.ToArray();
            var sortedArray = srcCopy.SortRadix(inKeys, ref outSortedKeys);
            var sortedList = new List<T>(sortedArray);
            return sortedList;
        }
        // The following algorithms use a method where we extract an array of keys and have a source and destination array of keys to go along with
        // array of references so that accesses to keys are more cache friendly, and accesses to references go along with they keys!
        // The keys are in a linear array. The references to objects are in a linear array. Each object is never touched, since objects are scattered
        // in memory, making access to objects slow.
        // This method should be much better for arrays of objects/classes for JavaScript, Java, C#, C++ and Python.

        /// <summary>
        /// Sort an array of user defined class containing an unsigned integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// Faster algorithm that uses 8N bytes more temporary memory than SortRadix, where N is the size of the input array.
        /// </summary>
        /// <param name="inputArray">input array of type T</param>
        /// <param name="getKey">user provided function to extract the unsigned 32-bit key sorted on, from the user defined class</param>
        /// <returns>sorted array of user defined class</returns>
        public static T[] SortRadix2<T>(this T[] inputArray, Func<T, UInt32> getKey)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            T[] outputArray = new T[inputArray.Length];
            UInt32[] inKeys = new UInt32[inputArray.Length];
            UInt32[] outSortedKeys = new UInt32[inputArray.Length];
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
                if (bitMask == 255)     // first pass
                    for (uint current = 0; current < inputArray.Length; current++)    // Scan Key array and count the number of times each digit value appears - i.e. size of each bin
                    {
                        inKeys[current] = getKey(inputArray[current]);
                        count[ExtractDigit(inKeys[current], bitMask, shiftRightAmount)]++;
                    }
                else
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

            return inputArray;
        }
        /// <summary>
        /// Sort an array of user defined class containing an unsigned integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// Faster algorithm that uses 16N bytes more temporary memory than SortRadix, where N is the size of the input array.
        /// </summary>
        /// <param name="inputArray">input array of type T</param>
        /// <param name="getKey">user provided function to extract the unsigned 64-bit key sorted on, from the user defined class</param>
        /// <returns>sorted array of user defined class</returns>
        public static T[] SortRadix2<T>(this T[] inputArray, Func<T, UInt64> getKey)
        {
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            T[] outputArray = new T[inputArray.Length];
            UInt64[] inKeys = new UInt64[inputArray.Length];
            UInt64[] outSortedKeys = new UInt64[inputArray.Length];
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
                if (bitMask == 255)
                    for (uint current = 0; current < inputArray.Length; current++)    // Scan Key array and count the number of times each digit value appears - i.e. size of each bin
                    {
                        inKeys[current] = getKey(inputArray[current]);
                        count[ExtractDigit(inKeys[current], bitMask, shiftRightAmount)]++;
                    }
                else
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

            return inputArray;
        }
        /// <summary>
        /// Sort a List of user defined class containing an unsigned integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// Faster algorithm that uses 8N bytes more temporary memory than SortRadix, where N is the size of the input array.
        /// </summary>
        /// <param name="inputList">input List of type T</param>
        /// <param name="getKey">user provided function to extract the unsigned 32-bit key sorted on, from the user defined class</param>
        /// <returns>sorted List of user defined class</returns>
        public static List<T> SortRadix2<T>(this List<T> inputList, Func<T, UInt32> getKey)
        {
            var srcCopy = inputList.ToArray();
            var sortedArray = srcCopy.SortRadix2(getKey);
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
        public static List<T> SortRadix2<T>(this List<T> inputList, Func<T, UInt64> getKey)
        {
            var srcCopy = inputList.ToArray();
            var sortedArray = srcCopy.SortRadix2(getKey);
            var sortedList = new List<T>(sortedArray);
            return sortedList;
        }
    }
}
