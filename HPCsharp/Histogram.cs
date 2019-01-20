// TODO: Add histogramming of arrays of other data types, with byte and ushort counts
// TODO: Add histogramming of 2-D and jagged arrays of variety of data types, with byte and ushort counts
// TODO: Implement parallel histogram to support parallel Counting Sort
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        public static int[] Histogram(this byte[] inArray)
        {
            int numberOfBins = 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[inArray[currIndex]]++;

            return counts;
        }

        public static int[] Histogram(this ushort[] inArray)
        {
            int numberOfBins = 256 * 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[inArray[currIndex]]++;

            return counts;
        }
    }
}
