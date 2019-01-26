using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
        public static Int32 ThresholdHistogramBytePar { get; set; } = 64 * 1024;

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

        public static int[] HistogramInnerPar(byte[] inArray, Int32 l, Int32 r)
        {
            int numberOfBins = 256;
            int[] countLeft = new int[numberOfBins];

            if (l > r)      // zero elements to compare
                return countLeft;
            if ((r - l + 1) <= ThresholdHistogramBytePar)
            {
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    countLeft[inArray[current]]++;
                return countLeft;
            }

            int m = (r + l) / 2;

            int[] countRight = new int[numberOfBins];

            Parallel.Invoke(
                () => { countLeft  = HistogramInnerPar(inArray, l,     m); },
                () => { countRight = HistogramInnerPar(inArray, m + 1, r); }
            );
            
            for (int j = 0; j < numberOfBins; j++)      // Combine left and right results into a single count/histogram
                countLeft[j] += countRight[j];

            return countLeft;
        }

        public static int[] HistogramPar(this byte[] inArray)
        {
            return HistogramInnerPar(inArray, 0, inArray.Length - 1);
        }

        public static Int32 ThresholdHistogamShortPar { get; set; } = 64 * 1024;

        public static int[] HistogramInnerPar(ushort[] inArray, Int32 l, Int32 r)
        {
            int numberOfBins = 256 * 256;
            int[] countLeft = new int[numberOfBins];

            if (l > r)      // zero elements to compare
                return countLeft;
            if ((r - l + 1) <= ThresholdHistogamShortPar)
            {
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    countLeft[inArray[current]]++;
                return countLeft;
            }

            int m = (r + l) / 2;

            int[] countRight = new int[numberOfBins];

            Parallel.Invoke(
                () => { countLeft  = HistogramInnerPar(inArray, l,     m); },
                () => { countRight = HistogramInnerPar(inArray, m + 1, r); }
            );

            for (int j = 0; j < numberOfBins; j++)      // Combine left and right results into a single count/histogram
                countLeft[j] += countRight[j];

            return countLeft;
        }

        public static int[] HistogramPar(this ushort[] inArray)
        {
            return HistogramInnerPar(inArray, 0, inArray.Length - 1);
        }
    }
}
