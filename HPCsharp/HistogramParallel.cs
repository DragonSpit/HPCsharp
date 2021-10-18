// TODO: Implement performance improvement from my C++ code, where in the counting 2-D part, instead of using a 2-D array for the increment computation, a 1-D array is used, which gets initialized before the for loop. C++ comment is that it's faster.
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

        public static int[] HistogramInnerPar(byte[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            int numberOfBins = 256;
            int[] countLeft  = null;
            int[] countRight = null;

            if (l > r)      // zero elements to compare
            {
                countLeft = new int[numberOfBins];
                return countLeft;
            }
            if ((r - l + 1) <= parallelThreshold)
            {
                countLeft = new int[numberOfBins];
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    countLeft[inArray[current]]++;
                return countLeft;
            }

            int m = (r + l) / 2;

            Parallel.Invoke(
                () => { countLeft  = HistogramInnerPar(inArray, l,     m, parallelThreshold); },
                () => { countRight = HistogramInnerPar(inArray, m + 1, r, parallelThreshold); }
            );
            
            for (int j = 0; j < numberOfBins; j++)      // Combine left and right results into a single count/histogram
                countLeft[j] += countRight[j];

            return countLeft;
        }

        public static int[] HistogramPar(this byte[] inArray, int parallelThreshold = 16 * 1024)
        {
            return HistogramInnerPar(inArray, 0, inArray.Length - 1, parallelThreshold);
        }

        public static int[] HistogramInnerPar(ushort[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            int numberOfBins = 256 * 256;
            int[] countLeft  = null;
            int[] countRight = null;

            if (l > r)      // zero elements to compare
            {
                countLeft = new int[numberOfBins];
                return countLeft;
            }
            if ((r - l + 1) <= parallelThreshold)
            {
                countLeft = new int[numberOfBins];
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    countLeft[inArray[current]]++;
                return countLeft;
            }

            int m = (r + l) / 2;

            Parallel.Invoke(
                () => { countLeft  = HistogramInnerPar(inArray, l,     m, parallelThreshold); },
                () => { countRight = HistogramInnerPar(inArray, m + 1, r, parallelThreshold); }
            );

            for (int j = 0; j < numberOfBins; j++)      // Combine left and right results into a single count/histogram
                countLeft[j] += countRight[j];

            return countLeft;
        }

        public static int[] HistogramPar(this ushort[] inArray)
        {
            return HistogramInnerPar(inArray, 0, inArray.Length - 1);
        }

        static uint[][] HistogramByteComponentsParInner(uint[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint[][] countLeft  = null;
            uint[][] countRight = null;

            if (l > r)      // zero elements to compare
            {
                countLeft = new uint[numberOfDigits][];
                for (int i = 0; i < numberOfDigits; i++)
                    countLeft[i] = new uint[numberOfBins];
                return countLeft;
            }
            if ((r - l + 1) <= parallelThreshold)
            {
                countLeft = new uint[numberOfDigits][];
                for (int i = 0; i < numberOfDigits; i++)
                    countLeft[i] = new uint[numberOfBins];

                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];

                var union = new Algorithm.UInt32ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft0[union.byte0]++;
                    countLeft1[union.byte1]++;
                    countLeft2[union.byte2]++;
                    countLeft3[union.byte3]++;
                }
                return countLeft;
            }

            int m = (r + l) / 2;

            Parallel.Invoke(
                () => { countLeft  = HistogramByteComponentsParInner(inArray, l,     m, parallelThreshold); },
                () => { countRight = HistogramByteComponentsParInner(inArray, m + 1, r, parallelThreshold); }
            );
            // Combine left and right results (reduce step)
            for (int i = 0; i < numberOfDigits; i++)
                for (int j = 0; j < numberOfBins; j++)
                    countLeft[i][j] += countRight[i][j];

            return countLeft;
        }

        public static uint[][] HistogramByteComponentsPar(uint[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            int length = r - l + 1;
            if ((parallelThreshold * Environment.ProcessorCount) < length)
                parallelThreshold = length / Environment.ProcessorCount;
            return HistogramByteComponentsParInner(inArray, l, r, parallelThreshold);
        }

        static uint[][] HistogramByteComponentsSseParInner(uint[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint[][] countLeft  = null;
            uint[][] countRight = null;

            if (l > r)      // zero elements to compare
            {
                countLeft = new uint[numberOfDigits][];
                for (int i = 0; i < numberOfDigits; i++)
                    countLeft[i] = new uint[numberOfBins];

                return countLeft;
            }
            if ((r - l + 1) <= parallelThreshold)
            {
                countLeft = new uint[numberOfDigits][];
                for (int i = 0; i < numberOfDigits; i++)
                    countLeft[i] = new uint[numberOfBins];

                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];

                var union = new Algorithm.UInt32ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft0[union.byte0]++;
                    countLeft1[union.byte1]++;
                    countLeft2[union.byte2]++;
                    countLeft3[union.byte3]++;
                }
                return countLeft;
            }

            int m = (r + l) / 2;

            Parallel.Invoke(
                () => { countLeft  = HistogramByteComponentsSseParInner(inArray, l,     m, parallelThreshold); },
                () => { countRight = HistogramByteComponentsSseParInner(inArray, m + 1, r, parallelThreshold); }
            );
            // Combine left and right results
            for (int i = 0; i < numberOfDigits; i++)
                Addition.AddToSse(countLeft[i], countRight[i]);

            return countLeft;
        }

        public static uint[][] HistogramByteComponentsSsePar(uint[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            int length = r - l + 1;
            if ((parallelThreshold * Environment.ProcessorCount) < length)
                parallelThreshold = length / Environment.ProcessorCount;
            return HistogramByteComponentsSseParInner(inArray, l, r, parallelThreshold);
        }

        static uint[][] HistogramByteComponentsQCParInner(uint[] inArray, Int32 l, Int32 r, int workQuanta, uint numberOfQuantas, uint whichByte, int parallelThreshold = 16 * 1024)
        {
            const int numberOfBins = 256;
            uint[][] countLeft  = null;
            uint[][] countRight = null;

            if ((r - l + 1) <= parallelThreshold)
                return Algorithm.HistogramByteComponentsAcrossWorkQuantasQC(inArray, l, r, workQuanta, numberOfQuantas, whichByte);

            int m = (r + l) / 2;

            Parallel.Invoke(
                () => { countLeft  = HistogramByteComponentsQCParInner(inArray, l,     m, workQuanta, numberOfQuantas, whichByte, parallelThreshold); },
                () => { countRight = HistogramByteComponentsQCParInner(inArray, m + 1, r, workQuanta, numberOfQuantas, whichByte, parallelThreshold); }
            );
            // Combine left and right results (reduce step), only for workQuantas for which the counts were computed
            long startQuanta = l / workQuanta;
            long endQuanta   = r / workQuanta;
            for (int i = (int)startQuanta; i <= endQuanta; i++)
                for (int j = 0; j < numberOfBins; j++)
                    countLeft[i][j] += countRight[i][j];

            return countLeft;
        }

        public static uint[][] HistogramByteComponentsQCPar(uint[] inArray, Int32 l, Int32 r, int workQuanta, uint numberOfQuantas, uint whichByte, int parallelThreshold = 16 * 1024)
        {
            int length = r - l + 1;
            if ((parallelThreshold * Environment.ProcessorCount) < length)
                parallelThreshold = length / Environment.ProcessorCount;
            return HistogramByteComponentsQCParInner(inArray, l, r, workQuanta, numberOfQuantas, whichByte, parallelThreshold);
        }

        static uint[][] HistogramByteComponentsParInner(int[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint[][] countLeft  = null;
            uint[][] countRight = null;

            if (l > r)      // zero elements to compare
            {
                countLeft = new uint[numberOfDigits][];
                for (int i = 0; i < numberOfDigits; i++)
                    countLeft[i] = new uint[numberOfBins];
                return countLeft;
            }
            if ((r - l + 1) <= parallelThreshold)
            {
                countLeft = new uint[numberOfDigits][];
                for (int i = 0; i < numberOfDigits; i++)
                    countLeft[i] = new uint[numberOfBins];

                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];

                var union = new Algorithm.Int32ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft0[union.byte0]++;
                    countLeft1[union.byte1]++;
                    countLeft2[union.byte2]++;
                    countLeft3[((uint)inArray[current] >> 24) ^ 128]++;
                }
                return countLeft;
            }

            int m = (r + l) / 2;

            Parallel.Invoke(
                () => { countLeft  = HistogramByteComponentsParInner(inArray, l,     m, parallelThreshold); },
                () => { countRight = HistogramByteComponentsParInner(inArray, m + 1, r, parallelThreshold); }
            );
            // Combine left and right results
            for (int i = 0; i < numberOfDigits; i++)
                for (int j = 0; j < numberOfBins; j++)
                    countLeft[i][j] += countRight[i][j];

            return countLeft;
        }

        public static uint[][] HistogramByteComponentsPar(int[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            int length = r - l + 1;
            if ((parallelThreshold * Environment.ProcessorCount) < length)
                parallelThreshold = length / Environment.ProcessorCount;
            return HistogramByteComponentsParInner(inArray, l, r, parallelThreshold);
        }

        static uint[][] HistogramByteComponentsSseParInner(int[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint[][] countLeft = null;
            uint[][] countRight = null;

            if (l > r)      // zero elements to compare
            {
                countLeft = new uint[numberOfDigits][];
                for (int i = 0; i < numberOfDigits; i++)
                    countLeft[i] = new uint[numberOfBins];

                return countLeft;
            }
            if ((r - l + 1) <= parallelThreshold)
            {
                countLeft = new uint[numberOfDigits][];
                for (int i = 0; i < numberOfDigits; i++)
                    countLeft[i] = new uint[numberOfBins];

                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];

                var union = new Algorithm.Int32ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft0[union.byte0]++;
                    countLeft1[union.byte1]++;
                    countLeft2[union.byte2]++;
                    countLeft3[((uint)inArray[current] >> 24) ^ 128]++;
                }
                return countLeft;
            }

            int m = (r + l) / 2;

            Parallel.Invoke(
                () => { countLeft  = HistogramByteComponentsSseParInner(inArray, l,     m, parallelThreshold); },
                () => { countRight = HistogramByteComponentsSseParInner(inArray, m + 1, r, parallelThreshold); }
            );
            // Combine left and right results
            for (int i = 0; i < numberOfDigits; i++)
                Addition.AddToSse(countLeft[i], countRight[i]);

            return countLeft;
        }

        public static uint[][] HistogramByteComponentsSsePar(int[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            int length = r - l + 1;
            if ((parallelThreshold * Environment.ProcessorCount) < length)
                parallelThreshold = length / Environment.ProcessorCount;
            return HistogramByteComponentsSseParInner(inArray, l, r, parallelThreshold);
        }

        static uint[][] HistogramByteComponentsParInner(ulong[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(ulong);
            uint[][] countLeft = null;
            uint[][] countRight = null;

            if (l > r)      // zero elements to compare
            {
                countLeft = new uint[numberOfDigits][];
                for (int i = 0; i < numberOfDigits; i++)
                    countLeft[i] = new uint[numberOfBins];

                return countLeft;
            }
            if ((r - l + 1) <= parallelThreshold)
            {
                countLeft = new uint[numberOfDigits][];
                for (int i = 0; i < numberOfDigits; i++)
                    countLeft[i] = new uint[numberOfBins];

                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];
                uint[] countLeft4 = countLeft[4];
                uint[] countLeft5 = countLeft[5];
                uint[] countLeft6 = countLeft[6];
                uint[] countLeft7 = countLeft[7];

                var union = new Algorithm.UInt64ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft0[union.byte0]++;
                    countLeft1[union.byte1]++;
                    countLeft2[union.byte2]++;
                    countLeft3[union.byte3]++;
                    countLeft4[union.byte4]++;
                    countLeft5[union.byte5]++;
                    countLeft6[union.byte6]++;
                    countLeft7[union.byte7]++;
                }
                return countLeft;
            }

            int m = (r + l) / 2;

            Parallel.Invoke(
                () => { countLeft  = HistogramByteComponentsParInner(inArray, l,     m, parallelThreshold); },
                () => { countRight = HistogramByteComponentsParInner(inArray, m + 1, r, parallelThreshold); }
            );
            // Combine left and right results
            for (int i = 0; i < numberOfDigits; i++)
                for (int j = 0; j < numberOfBins; j++)
                    countLeft[i][j] += countRight[i][j];

            return countLeft;
        }

        public static uint[][] HistogramByteComponentsPar(ulong[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            int length = r - l + 1;
            if ((parallelThreshold * Environment.ProcessorCount) < length)
                parallelThreshold = length / Environment.ProcessorCount;
            return HistogramByteComponentsParInner(inArray, l, r, parallelThreshold);
        }

        static uint[][] HistogramByteComponentsParInner(long[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(ulong);
            uint[][] countLeft = null;
            uint[][] countRight = null;

            if (l > r)      // zero elements to compare
            {
                countLeft = new uint[numberOfDigits][];
                for (int i = 0; i < numberOfDigits; i++)
                    countLeft[i] = new uint[numberOfBins];

                return countLeft;
            }
            if ((r - l + 1) <= parallelThreshold)
            {
                countLeft = new uint[numberOfDigits][];
                for (int i = 0; i < numberOfDigits; i++)
                    countLeft[i] = new uint[numberOfBins];

                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];
                uint[] countLeft4 = countLeft[4];
                uint[] countLeft5 = countLeft[5];
                uint[] countLeft6 = countLeft[6];
                uint[] countLeft7 = countLeft[7];

                var union = new Algorithm.Int64ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft0[union.byte0]++;
                    countLeft1[union.byte1]++;
                    countLeft2[union.byte2]++;
                    countLeft3[union.byte3]++;
                    countLeft4[union.byte4]++;
                    countLeft5[union.byte5]++;
                    countLeft6[union.byte6]++;
                    countLeft7[((ulong)inArray[current] >> 56) ^ 128]++;
                }
                return countLeft;
            }

            int m = (r + l) / 2;

            Parallel.Invoke(
                () => { countLeft  = HistogramByteComponentsParInner(inArray, l,     m, parallelThreshold); },
                () => { countRight = HistogramByteComponentsParInner(inArray, m + 1, r, parallelThreshold); }
            );
            // Combine left and right results
            for (int i = 0; i < numberOfDigits; i++)
                for (int j = 0; j < numberOfBins; j++)
                    countLeft[i][j] += countRight[i][j];

            return countLeft;
        }

        public static uint[][] HistogramByteComponentsPar(long[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            int length = r - l + 1;
            if ((parallelThreshold * Environment.ProcessorCount) < length)
                parallelThreshold = length / Environment.ProcessorCount;
            return HistogramByteComponentsParInner(inArray, l, r, parallelThreshold);
        }

        static uint[][] HistogramByteComponentsSseParInner(long[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(ulong);
            uint[][] countLeft = null;
            uint[][] countRight = null;

            if (l > r)      // zero elements to compare
            {
                countLeft = new uint[numberOfDigits][];
                for (int i = 0; i < numberOfDigits; i++)
                    countLeft[i] = new uint[numberOfBins];

                return countLeft;
            }
            if ((r - l + 1) <= parallelThreshold)
            {
                countLeft = new uint[numberOfDigits][];
                for (int i = 0; i < numberOfDigits; i++)
                    countLeft[i] = new uint[numberOfBins];

                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];
                uint[] countLeft4 = countLeft[4];
                uint[] countLeft5 = countLeft[5];
                uint[] countLeft6 = countLeft[6];
                uint[] countLeft7 = countLeft[7];

                var union = new Algorithm.Int64ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft0[union.byte0]++;
                    countLeft1[union.byte1]++;
                    countLeft2[union.byte2]++;
                    countLeft3[union.byte3]++;
                    countLeft4[union.byte4]++;
                    countLeft5[union.byte5]++;
                    countLeft6[union.byte6]++;
                    countLeft7[((ulong)inArray[current] >> 56) ^ 128]++;
                }
                return countLeft;
            }

            int m = (r + l) / 2;

            Parallel.Invoke(
                () => { countLeft  = HistogramByteComponentsSseParInner(inArray, l,     m, parallelThreshold); },
                () => { countRight = HistogramByteComponentsSseParInner(inArray, m + 1, r, parallelThreshold); }
            );
            // Combine left and right results
            for (int i = 0; i < numberOfDigits; i++)
                Addition.AddToSse(countLeft[i], countRight[i]);

            return countLeft;
        }

        public static uint[][] HistogramByteComponentsSsePar(long[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            int length = r - l + 1;
            if ((parallelThreshold * Environment.ProcessorCount) < length)
                parallelThreshold = length / Environment.ProcessorCount;
            return HistogramByteComponentsSseParInner(inArray, l, r, parallelThreshold);
        }

        static int[] HistogramOneByteComponentParInner(long[] inArray, Int32 l, Int32 r, int shiftRightAmount, int parallelThreshold = 16 * 1024)
        {
            const int numberOfBins = 256;
            int[] countLeft = null;
            int[] countRight = null;

            if (l > r)      // zero elements to compare
            {
                countLeft = new int[numberOfBins];
                return countLeft;
            }
            if ((r - l + 1) <= parallelThreshold)
            {
                countLeft = new int[numberOfBins];
                return Algorithm.HistogramOneByteComponent(inArray, l, r, shiftRightAmount);
            }

            int m = (r + l) / 2;

            Parallel.Invoke(
                () => { countLeft  = HistogramOneByteComponentParInner(inArray, l,     m, shiftRightAmount, parallelThreshold); },
                () => { countRight = HistogramOneByteComponentParInner(inArray, m + 1, r, shiftRightAmount, parallelThreshold); }
            );
            // Combine left and right results
            countLeft = Addition.AddSse(countLeft, countRight);

            return countLeft;
        }

        public static int[] HistogramOneByteComponentPar(long[] inArray, Int32 l, Int32 r, int parallelThreshold = 16 * 1024)
        {
            int length = r - l + 1;
            if ((parallelThreshold * Environment.ProcessorCount) < length)
                parallelThreshold = length / Environment.ProcessorCount;
            return HistogramOneByteComponentParInner(inArray, l, r, parallelThreshold);
        }
    }
}
