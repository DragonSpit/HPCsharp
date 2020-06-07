using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using HPCsharp.ParallelAlgorithms;

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

        // Does not seem to be faster than the scaler version, probably because it's not limited by memory bandwidth
        public static int[] HistogramOneByteComponentSse(long[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            //const ulong byteMask = numberOfBins - 1;
            int[] count = new int[numberOfBins];
            int[] byteIndex = new int[Vector<long>.Count];
            int sseIndexEnd = l + ((r - l + 1) / Vector<long>.Count) * Vector<long>.Count;
            int byteOffset = shiftRightAmount / sizeof(long);
            int i;

            for (int j = 0; j < Vector<long>.Count; j++)
                byteIndex[j] = j * sizeof(long) + byteOffset;

            if (shiftRightAmount != 56)
            {
                for (i = l; i < sseIndexEnd; i += Vector<long>.Count)
                {
                    var inVector   = new Vector<long>(inArray, i);
                    var byteVector = Vector.AsVectorByte(inVector);
                    for (int j = 0; j < Vector<long>.Count; j++)
                        count[byteVector[byteIndex[j]]]++;
                }
                for (; i <= r; i++)
                    count[(byte)(inArray[i] >> shiftRightAmount)]++;
            }
            else
            {
                for (i = l; i < sseIndexEnd; i += Vector<long>.Count)
                {
                    var inVector = new Vector<long>(inArray, i);
                    var byteVector = Vector.AsVectorByte(inVector);
                    for (int j = 0; j < Vector<long>.Count; j++)
                        count[byteVector[byteIndex[j]] ^ 128]++;
                }
                for (; i <= r; i++)
                    count[(byte)(inArray[i] >> shiftRightAmount) ^ 128]++;
            }

            return count;
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

        public static Int32 ThresholdByteCount { get; set; } = 16 * 1024;

        public static uint[][] HistogramByteComponentsPar(uint[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint[][] countLeft = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countLeft[i] = new uint[numberOfBins];

            if (l > r)      // zero elements to compare
                return countLeft;
            if ((r - l + 1) <= ThresholdByteCount)
            {
                var union = new Algorithm.UInt32ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft[0][union.byte0]++;
                    countLeft[1][union.byte1]++;
                    countLeft[2][union.byte2]++;
                    countLeft[3][union.byte3]++;
                }
                return countLeft;
            }

            int m = (r + l) / 2;

            uint[][] countRight = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countRight[i] = new uint[numberOfBins];

            Parallel.Invoke(
                () => { countLeft  = HistogramByteComponentsPar(inArray, l,     m); },
                () => { countRight = HistogramByteComponentsPar(inArray, m + 1, r); }
            );
            // Combine left and right results
            for (int i = 0; i < numberOfDigits; i++)
                for (int j = 0; j < numberOfBins; j++)
                    countLeft[i][j] += countRight[i][j];

            return countLeft;
        }

        public static uint[][] HistogramByteComponentsSsePar(uint[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint[][] countLeft = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countLeft[i] = new uint[numberOfBins];

            if (l > r)      // zero elements to compare
                return countLeft;
            if ((r - l + 1) <= ThresholdByteCount)
            {
                var union = new Algorithm.UInt32ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft[0][union.byte0]++;
                    countLeft[1][union.byte1]++;
                    countLeft[2][union.byte2]++;
                    countLeft[3][union.byte3]++;
                }
                return countLeft;
            }

            int m = (r + l) / 2;

            uint[][] countRight = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countRight[i] = new uint[numberOfBins];

            Parallel.Invoke(
                () => { countLeft  = HistogramByteComponentsSsePar(inArray, l, m); },
                () => { countRight = HistogramByteComponentsSsePar(inArray, m + 1, r); }
            );
            // Combine left and right results
            for (int i = 0; i < numberOfDigits; i++)
                Addition.AddToSse(countLeft[i], countRight[i]);

            return countLeft;
        }

        public static uint[][] HistogramByteComponentsPar(int[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint[][] countLeft = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countLeft[i] = new uint[numberOfBins];

            if (l > r)      // zero elements to compare
                return countLeft;
            if ((r - l + 1) <= ThresholdByteCount)
            {
                var union = new Algorithm.Int32ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft[0][union.byte0]++;
                    countLeft[1][union.byte1]++;
                    countLeft[2][union.byte2]++;
                    countLeft[3][((uint)inArray[current] >> 24) ^ 128]++;
                }
                return countLeft;
            }

            int m = (r + l) / 2;

            uint[][] countRight = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countRight[i] = new uint[numberOfBins];

            Parallel.Invoke(
                () => { countLeft  = HistogramByteComponentsPar(inArray, l,     m); },
                () => { countRight = HistogramByteComponentsPar(inArray, m + 1, r); }
            );
            // Combine left and right results
            for (int i = 0; i < numberOfDigits; i++)
                for (int j = 0; j < numberOfBins; j++)
                    countLeft[i][j] += countRight[i][j];

            return countLeft;
        }

        public static uint[][] HistogramByteComponentsSsePar(int[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint[][] countLeft = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countLeft[i] = new uint[numberOfBins];

            if (l > r)      // zero elements to compare
                return countLeft;
            if ((r - l + 1) <= ThresholdByteCount)
            {
                var union = new Algorithm.Int32ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft[0][union.byte0]++;
                    countLeft[1][union.byte1]++;
                    countLeft[2][union.byte2]++;
                    countLeft[3][((uint)inArray[current] >> 24) ^ 128]++;
                }
                return countLeft;
            }

            int m = (r + l) / 2;

            uint[][] countRight = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countRight[i] = new uint[numberOfBins];

            Parallel.Invoke(
                () => { countLeft  = HistogramByteComponentsSsePar(inArray, l,     m); },
                () => { countRight = HistogramByteComponentsSsePar(inArray, m + 1, r); }
            );
            // Combine left and right results
            for (int i = 0; i < numberOfDigits; i++)
                Addition.AddToSse(countLeft[i], countRight[i]);

            return countLeft;
        }

        public static uint[][] HistogramByteComponentsPar(ulong[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(ulong);
            uint[][] countLeft = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countLeft[i] = new uint[numberOfBins];

            if (l > r)      // zero elements to compare
                return countLeft;
            if ((r - l + 1) <= ThresholdByteCount)
            {
                var union = new Algorithm.UInt64ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft[0][union.byte0]++;
                    countLeft[1][union.byte1]++;
                    countLeft[2][union.byte2]++;
                    countLeft[3][union.byte3]++;
                    countLeft[4][union.byte4]++;
                    countLeft[5][union.byte5]++;
                    countLeft[6][union.byte6]++;
                    countLeft[7][union.byte7]++;
                }
                return countLeft;
            }

            int m = (r + l) / 2;

            uint[][] countRight = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countRight[i] = new uint[numberOfBins];

            Parallel.Invoke(
                () => { countLeft  = HistogramByteComponentsPar(inArray, l,     m); },
                () => { countRight = HistogramByteComponentsPar(inArray, m + 1, r); }
            );
            // Combine left and right results
            for (int i = 0; i < numberOfDigits; i++)
                for (int j = 0; j < numberOfBins; j++)
                    countLeft[i][j] += countRight[i][j];

            return countLeft;
        }

        public static uint[][] HistogramByteComponentsPar(long[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(ulong);
            uint[][] countLeft = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countLeft[i] = new uint[numberOfBins];

            if (l > r)      // zero elements to compare
                return countLeft;
            if ((r - l + 1) <= ThresholdByteCount)
            {
                var union = new Algorithm.Int64ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft[0][union.byte0]++;
                    countLeft[1][union.byte1]++;
                    countLeft[2][union.byte2]++;
                    countLeft[3][union.byte3]++;
                    countLeft[4][union.byte4]++;
                    countLeft[5][union.byte5]++;
                    countLeft[6][union.byte6]++;
                    countLeft[7][((ulong)inArray[current] >> 56) ^ 128]++;
                }
                return countLeft;
            }

            int m = (r + l) / 2;

            uint[][] countRight = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countRight[i] = new uint[numberOfBins];

            Parallel.Invoke(
                () => { countLeft  = HistogramByteComponentsPar(inArray, l,     m); },
                () => { countRight = HistogramByteComponentsPar(inArray, m + 1, r); }
            );
            // Combine left and right results
            for (int i = 0; i < numberOfDigits; i++)
                for (int j = 0; j < numberOfBins; j++)
                    countLeft[i][j] += countRight[i][j];

            return countLeft;
        }

        public static uint[][] HistogramByteComponentsSsePar(long[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(ulong);
            uint[][] countLeft = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countLeft[i] = new uint[numberOfBins];

            if (l > r)      // zero elements to compare
                return countLeft;
            if ((r - l + 1) <= ThresholdByteCount)
            {
                var union = new Algorithm.Int64ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft[0][union.byte0]++;
                    countLeft[1][union.byte1]++;
                    countLeft[2][union.byte2]++;
                    countLeft[3][union.byte3]++;
                    countLeft[4][union.byte4]++;
                    countLeft[5][union.byte5]++;
                    countLeft[6][union.byte6]++;
                    countLeft[7][((ulong)inArray[current] >> 56) ^ 128]++;
                }
                return countLeft;
            }

            int m = (r + l) / 2;

            uint[][] countRight = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countRight[i] = new uint[numberOfBins];

            Parallel.Invoke(
                () => { countLeft  = HistogramByteComponentsSsePar(inArray, l,     m); },
                () => { countRight = HistogramByteComponentsSsePar(inArray, m + 1, r); }
            );
            // Combine left and right results
            for (int i = 0; i < numberOfDigits; i++)
                Addition.AddToSse(countLeft[i], countRight[i]);

            return countLeft;
        }

        public static int[] HistogramOneByteComponentPar(long[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            int[] countLeft = new int[numberOfBins];

            if (l > r)      // zero elements to compare
                return countLeft;
            if ((r - l + 1) <= ThresholdByteCount)
                return Algorithm.HistogramOneByteComponent(inArray, l, r, shiftRightAmount);

            int m = (r + l) / 2;

            int[] countRight = new int[numberOfBins];

            Parallel.Invoke(
                () => { countLeft  = HistogramOneByteComponentPar(inArray, l,     m, shiftRightAmount); },
                () => { countRight = HistogramOneByteComponentPar(inArray, m + 1, r, shiftRightAmount); }
            );
            // Combine left and right results
            countLeft = Addition.AddSse(countLeft, countRight);

            return countLeft;
        }
    }
}
