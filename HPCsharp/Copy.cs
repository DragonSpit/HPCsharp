using System;
using System.Collections.Generic;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        /// <summary>
        /// Faster Array Copy, which uses C# for loop for smaller arrays and Array.Copy for larger, providing higher performance for smaller arrays.
        /// Simple usage, with the same interface as Array.Copy(). 
        /// </summary>
        /// <param name="sourceArray">first source Array to be merged</param>
        /// <param name="sourceIndex">starting index of the first sorted Array, inclusive</param>
        /// <param name="destinationArray">second source Array to be merged</param>
        /// <param name="destinationIndex">starting index of the second sorted Array, inclusive</param>
        /// <param name="length">length of the second sorted segment</param>
        static public void Copy<T>(T[] sourceArray, Int32 sourceIndex,
                                   T[] destinationArray, Int32 destinationIndex, Int32 length, Int32 threshold = 128)
        {
            if (length >= threshold)
                Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);
            else
                for (Int32 i = 0; i < length; i++)
                    destinationArray[destinationIndex++] = sourceArray[sourceIndex++];
        }
    }
}