// TODO: Add histogramming of arrays of other data types, with byte and ushort counts
// TODO: Add histogramming of 2-D and jagged arrays of variety of data types, with byte and ushort counts
// TODO: Develop more general histograms (serial and parallel) that allow components to not just be bytes, but also any number of bits within a word. For example, 10-bit or 14-bit components.
//       and possibly even different samplings between color components.
//       This may pay off big, and us being able to find the optimal number of bits for Radix Sort to minimize the number of passes or recursion levels.
//       Counting array should fit into L1-cache and possibly as large as fitting into L2-cache, since these are separate for each core in Intel CPUs, whereas L3-cache is
//       shared between all cores.
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
            const int numberOfBins = 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[inArray[currIndex]]++;

            return counts;
        }

        public static int[] Histogram(this sbyte[] inArray)
        {
            const int numberOfBins = 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[(int)inArray[currIndex] + 128]++;

            return counts;
        }

        public static int[] Histogram(this ushort[] inArray)
        {
            const int numberOfBins = 256 * 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[inArray[currIndex]]++;

            return counts;
        }

        public static int[] Histogram(this short[] inArray)
        {
            const int numberOfBins = 256 * 256;
            int[] counts = new int[numberOfBins];

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[(int)inArray[currIndex] + 32768]++;

            return counts;
        }

    }
}
