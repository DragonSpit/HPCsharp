// TODO: Add histogramming of arrays of other data types, with byte and ushort counts
// TODO: Add histogramming of 2-D and jagged arrays of variety of data types, with byte and ushort counts
// TODO: Develop more general histograms (serial and parallel) that allow components to not just be bytes, but also any number of bits within a word. For example, 10-bit or 14-bit components.
//       and possibly even different samplings between color components.
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

        // So far, not any faster, but works correctly
        private static int[] HistogramSse(this byte[] inArray)
        {
            int numberOfBins = 256;
            int[] counts = new int[numberOfBins];
            int vectorLength = Vector<byte>.Count;
            int currIndex;

            for (currIndex = 0; currIndex <= (inArray.Length - vectorLength); currIndex += vectorLength)
            {
                var readVector = new Vector<byte>(inArray, currIndex);
                for (int i = 0; i < vectorLength; i++)
                    counts[readVector[i]]++;
            }
            for (; currIndex < inArray.Length; currIndex++)
                counts[inArray[currIndex]]++;

            return counts;
        }
    }
}
