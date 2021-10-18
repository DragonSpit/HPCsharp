// TODO: Add histogramming of arrays of other data types, with byte and ushort counts
// TODO: Add histogramming of 2-D and jagged arrays of variety of data types, with byte and ushort counts
// TODO: This may pay off big, and us being able to find the optimal number of bits for Radix Sort to minimize the number of passes or recursion levels.
//       Counting array should fit into L1-cache and possibly as large as fitting into L2-cache, since these are separate for each core in Intel CPUs, whereas L3-cache is
//       shared between all cores.
// TODO: Pull out the Histogram/Counting algorithm from LSD Radix Sort where multiple components are being counted in one pass, generalize it and parallelize it.
// TODO: Simplify example benchmarks by passing in the two sorting functions to be compared. This will reduce complexity a lot! Couldn't do it since Linq Sort are not
//       really functions, but are extension methods. How do you pass those in
// TODO: Consider for 64-bit Histogram and 9-bits/component implementing 9/10-bit components, where most are 9-bit, but the last one is 10-bit, to save one pass.
// TODO: Figure out which way is faster for byte component Histogram (one byte version): // ?? Which way is faster. Need to look at assembly language listing too
// TODO: Switch from mask-shift to shift-mask which makes the mask be the same for all bytes, which may make it faster than union. Try also casting to a byte instead of masking. Look at assembly to see
//       which is better. Time to see which is faster.
// TODO: To support small histograms, support the count arrays to be passed in, so that the caller can allocate them potentially on the stack or re-use previously allocated ones.
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

        public static int[] Histogram(this uint[] inArray, int numberOfBits)
        {
            if (numberOfBits > 31)
                throw new ArgumentOutOfRangeException("numberOfBits must be <= 31");

            int numberOfBins = 1 << numberOfBits;
            int[] counts = new int[numberOfBins];
            uint mask = (uint)(numberOfBins - 1);

            for (uint currIndex = 0; currIndex < inArray.Length; currIndex++)
                counts[mask & inArray[currIndex]]++;

            return counts;
        }

        public static uint[][] HistogramByteComponents(uint[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint[][] count = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];

            uint[] count0 = count[0];
            uint[] count1 = count[1];
            uint[] count2 = count[2];
            uint[] count3 = count[3];
#if false
            var union  = new UInt32ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = inArray[current];
                count0[union.byte0]++;
                count1[union.byte1]++;
                count2[union.byte2]++;
                count3[union.byte3]++;
            }
#else
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                uint value = inArray[current];
                count0[ value        & 0xff]++;
                count1[(value >>  8) & 0xff]++;
                count2[(value >> 16) & 0xff]++;
                count3[(value >> 24) & 0xff]++;
            }
#endif
            return count;
        }

        public static uint[][][] HistogramByteComponentsAcrossWorkQuantas(uint[] inArray, uint workQuanta)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint numberOfQuantas = (inArray.Length % workQuanta) == 0 ? (uint)(inArray.Length / workQuanta) : (uint)(inArray.Length / workQuanta + 1);
            //Console.WriteLine("Histogram: inArray.Length = {0}, workQuanta = {1}, numberOfQuantas = {2}", inArray.Length, workQuanta, numberOfQuantas);

            uint[][][] count = new uint[numberOfQuantas][][];          // count for each parallel work item
            for (int i = 0; i < numberOfQuantas; i++)
            {
                count[i] = new uint[numberOfDigits][];
                for (int d = 0; d < numberOfDigits; d++)
                    count[i][d] = new uint[numberOfBins];
            }

            uint numberOfFullQuantas = (uint)(inArray.Length / workQuanta);
            int currIndex = 0;
            var union = new UInt32ByteUnion();
            uint q = 0;
            for (; q < numberOfFullQuantas; q++)
            {
                for (uint j = 0; j < workQuanta; j++)
                {
                    union.integer = inArray[currIndex++];
                    count[q][0][union.byte0]++;
                    count[q][1][union.byte1]++;
                    count[q][2][union.byte2]++;
                    count[q][3][union.byte3]++;
                }
            }
            // Last work quanta may be a partial one, whenever array length doesn't divide evenly by work quanta
            for (; currIndex < inArray.Length;)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = inArray[currIndex++];
                count[q][0][union.byte0]++;
                count[q][1][union.byte1]++;
                count[q][2][union.byte2]++;
                count[q][3][union.byte3]++;
            }

            return count;
        }

        // Produces counts for each bin per work quanta, with the left-most dimention being the work-quanta and the right-most dimention being the counts
        public static uint[][] HistogramByteComponentsAcrossWorkQuantasQC(uint[] inArray, int workQuanta, uint numberOfQuantas, uint whichByte)
        {
            const int numberOfBins = 256;
            //Console.WriteLine("Histogram: inArray.Length = {0}, workQuanta = {1}, numberOfQuantas = {2}, whichByte = {3}", inArray.Length, workQuanta, numberOfQuantas, whichByte);

            uint[][] count = new uint[numberOfQuantas][];          // count for each parallel work item
            for (int i = 0; i < numberOfQuantas; i++)
                count[i] = new uint[numberOfBins];

            int numberOfFullQuantas = inArray.Length / workQuanta;
            int currIndex = 0;
            var union = new UInt32ByteUnion();
            uint q = 0;
            if (whichByte == 0)
            {
                for (; q < numberOfFullQuantas; q++)
                {
                    for (uint j = 0; j < workQuanta; j++)
                    {
                        union.integer = inArray[currIndex++];
                        count[q][union.byte0]++;
                    }
                }
                // Last work quanta may be a partial one, whenever array length doesn't divide evenly by work quanta
                for (; currIndex < inArray.Length;)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[currIndex++];
                    count[q][union.byte0]++;
                }
            }
            else if (whichByte == 1)
            {
                for (; q < numberOfFullQuantas; q++)
                {
                    for (uint j = 0; j < workQuanta; j++)
                    {
                        union.integer = inArray[currIndex++];
                        count[q][union.byte1]++;
                    }
                }
                // Last work quanta may be a partial one, whenever array length doesn't divide evenly by work quanta
                for (; currIndex < inArray.Length;)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[currIndex++];
                    count[q][union.byte1]++;
                }
            }
            else if (whichByte == 2)
            {
                for (; q < numberOfFullQuantas; q++)
                {
                    for (uint j = 0; j < workQuanta; j++)
                    {
                        union.integer = inArray[currIndex++];
                        count[q][union.byte2]++;
                    }
                }
                // Last work quanta may be a partial one, whenever array length doesn't divide evenly by work quanta
                for (; currIndex < inArray.Length;)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[currIndex++];
                    count[q][union.byte2]++;
                }
            }
            else
            {
                for (; q < numberOfFullQuantas; q++)
                {
                    for (uint j = 0; j < workQuanta; j++)
                    {
                        union.integer = inArray[currIndex++];
                        count[q][union.byte3]++;
                    }
                }
                // Last work quanta may be a partial one, whenever array length doesn't divide evenly by work quanta
                for (; currIndex < inArray.Length;)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[currIndex++];
                    count[q][union.byte3]++;
                }
            }

            //for (q = 0; q < numberOfQuantas; q++)
            //{
            //    Console.WriteLine("h: q = {0}", q);
            //    for (uint b = 0; b < numberOfBins; b++)
            //        Console.Write("{0} ", count[q][b]);
            //    Console.WriteLine();
            //}

            return count;
        }

        // Produces counts for each bin per work quanta, with the left-most dimention being the work-quanta and the right-most dimention being the counts
        // l is the  left-most index, inclusive
        // r is the right-most index, inclusive
        public static uint[][] HistogramByteComponentsAcrossWorkQuantasQC(uint[] inArray, Int32 l, Int32 r, int workQuanta, uint numberOfQuantas, uint whichByte)
        {
            const int numberOfBins = 256;
            const uint mask = 0xff;
            int shiftRightAmount = (int)(8 * whichByte);
            //Console.WriteLine("Histogram: inArray.Length = {0}, workQuanta = {1}, numberOfQuantas = {2}, whichByte = {3}", inArray.Length, workQuanta, numberOfQuantas, whichByte);

            uint[][] count = new uint[numberOfQuantas][];          // count for each parallel work item
            for (int i = 0; i < numberOfQuantas; i++)
                count[i] = new uint[numberOfBins];

            if (l > r)
                return count;

            long startQuanta = l / workQuanta;
            long endQuanta   = r / workQuanta;
            if (startQuanta == endQuanta)       // counting within a single workQuanta, either partial or full
            {
                int q = (int)startQuanta;
                for (int currIndex = l; currIndex <= r; currIndex++)
                {
                    uint inByte = (inArray[currIndex] >> shiftRightAmount) & mask;
                    count[q][inByte]++;
                }
            }
            else
            {
                int q;
                int currIndex, endIndex;

                // process startQuanta, which is either partial or full
                q = (int)startQuanta;
                endIndex = (int)(startQuanta * workQuanta + (workQuanta - 1));
                for (currIndex = l; currIndex <= endIndex; currIndex++)
                {
                    uint inByte = (inArray[currIndex] >> shiftRightAmount) & mask;
                    count[q][inByte]++;
                }

                // process endQuanta, which is either partial or full
                q = (int)endQuanta;
                for (currIndex = (int)(endQuanta * workQuanta); currIndex <= r; currIndex++)
                {
                    uint inByte = (inArray[currIndex] >> shiftRightAmount) & mask;
                    count[q][inByte]++;
                }

                // process full workQuantas > startQuanta and < endQuanta
                currIndex = (int)((startQuanta + 1) * workQuanta);
                endQuanta--;
                for(q = (int)(startQuanta + 1); q <= endQuanta; q++)
                {
                    for (uint j = 0; j < workQuanta; j++)
                    {
                        uint inByte = (inArray[currIndex++] >> shiftRightAmount) & mask;
                        count[q][inByte]++;
                    }
                }
            }
            return count;
        }

        // Different index order
        public static uint[][][] HistogramByteComponentsAcrossWorkQuantasDQC(uint[] inArray, uint workQuanta)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint numberOfQuantas = (inArray.Length % workQuanta) == 0 ? (uint)(inArray.Length / workQuanta) : (uint)(inArray.Length / workQuanta + 1);
            uint q;
            //Console.WriteLine("Histogram: inArray.Length = {0}, workQuanta = {1}, numberOfQuantas = {2}", inArray.Length, workQuanta, numberOfQuantas);

            uint[][][] count = new uint[numberOfDigits][][];          // count for each parallel work item
            for (int d = 0; d < numberOfDigits; d++)
            {
                count[d] = new uint[numberOfQuantas][];
                for (q = 0; q < numberOfQuantas; q++)
                    count[d][q] = new uint[numberOfBins];
            }

            uint numberOfFullQuantas = (uint)(inArray.Length / workQuanta);
            int currIndex = 0;
            var union = new UInt32ByteUnion();
            for (q = 0; q < numberOfFullQuantas; q++)
            {
                for (uint j = 0; j < workQuanta; j++)
                {
                    union.integer = inArray[currIndex++];
                    count[0][q][union.byte0]++;
                    count[1][q][union.byte1]++;
                    count[2][q][union.byte2]++;
                    count[3][q][union.byte3]++;
                }
            }
            // Last work quanta may be a partial one, whenever array length doesn't divide evenly by work quanta
            for (; currIndex < inArray.Length;)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = inArray[currIndex++];
                count[0][q][union.byte0]++;
                count[1][q][union.byte1]++;
                count[2][q][union.byte2]++;
                count[3][q][union.byte3]++;
            }

            //Console.WriteLine("Comparing Counts");
            //for (uint d = 0; d < numberOfDigits; d++)
            //{
            //    uint[][] count1 = Algorithm.HistogramByteComponentsAcrossWorkQuantas1(inArray, workQuanta, d);

            //    for (q = 0; q < numberOfQuantas; q++)
            //        for (uint b = 0; b < numberOfBins; b++)
            //        {
            //            if (count[d][q][b] != count1[q][b])
            //                Console.WriteLine("count's are not equal at digit {0}  q = {1}  b = {2}: {3}  {4}", d, q, b, count[d][q][b], count1[q][b]);
            //        }
            //}

            return count;
        }

        public static uint[][] HistogramByteComponents<T>(T[] inArray, Int32 l, Int32 r, Func<T, UInt32> getKey)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(UInt32);
            uint[][] count = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];
#if true
            uint[] count0 = count[0];
            uint[] count1 = count[1];
            uint[] count2 = count[2];
            uint[] count3 = count[3];

            var union = new UInt32ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = getKey(inArray[current]);
                count0[union.byte0]++;
                count1[union.byte1]++;
                count2[union.byte2]++;
                count3[union.byte3]++;
            }
#else
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                uint value = getKey(inArray[current]);
                count[0][(byte)value        ]++;
                count[1][(byte)(value >>  8)]++;
                count[2][(byte)(value >> 16)]++;
                count[3][(byte)(value >> 24)]++;
            }
#endif
            return count;
        }

        public static uint[][] HistogramByteComponents<T>(T[] inArray, Int32 l, Int32 r, Func<T, UInt64> getKey)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(UInt64);
            uint[][] count = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];
#if true
            uint[] count0 = count[0];
            uint[] count1 = count[1];
            uint[] count2 = count[2];
            uint[] count3 = count[3];
            uint[] count4 = count[4];
            uint[] count5 = count[5];
            uint[] count6 = count[6];
            uint[] count7 = count[7];

            var union = new UInt64ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = getKey(inArray[current]);
                count0[union.byte0]++;
                count1[union.byte1]++;
                count2[union.byte2]++;
                count3[union.byte3]++;
                count4[union.byte4]++;
                count5[union.byte5]++;
                count6[union.byte6]++;
                count7[union.byte7]++;
            }
#else
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                uint value = inArray[current];
                count[0][ value &       0xff       ]++;
                count[1][(value &     0xff00) >>  8]++;
                count[2][(value &   0xff0000) >> 16]++;
                count[3][(value & 0xff000000) >> 24]++;
            }
#endif
            return count;
        }

        public static Tuple<uint[][], UInt32[]> HistogramByteComponentsAndKeyArray<T>(T[] inArray, Int32 l, Int32 r, Func<T, UInt32> getKey)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(UInt32);
            var inKeys = new UInt32[inArray.Length];
            var count  = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];
#if true
            uint[] count0 = count[0];
            uint[] count1 = count[1];
            uint[] count2 = count[2];
            uint[] count3 = count[3];

            var union = new UInt32ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = getKey(inArray[current]);
                inKeys[current] = union.integer;
                count0[union.byte0]++;
                count1[union.byte1]++;
                count2[union.byte2]++;
                count3[union.byte3]++;
            }
#else
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                uint value = inArray[current];
                count[0][ value &       0xff       ]++;
                count[1][(value &     0xff00) >>  8]++;
                count[2][(value &   0xff0000) >> 16]++;
                count[3][(value & 0xff000000) >> 24]++;
            }
#endif
            return new Tuple<uint[][], UInt32[]>(count, inKeys);
        }

        public static Tuple<uint[][], UInt64[]> HistogramByteComponentsAndKeyArray<T>(T[] inArray, Int32 l, Int32 r, Func<T, UInt64> getKey)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(UInt32);
            var inKeys = new UInt64[inArray.Length];
            var count  = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];
#if true
            uint[] count0 = count[0];
            uint[] count1 = count[1];
            uint[] count2 = count[2];
            uint[] count3 = count[3];
            uint[] count4 = count[4];
            uint[] count5 = count[5];
            uint[] count6 = count[6];
            uint[] count7 = count[7];

            var union = new UInt64ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = getKey(inArray[current]);
                inKeys[current] = union.integer;
                count0[union.byte0]++;
                count1[union.byte1]++;
                count2[union.byte2]++;
                count3[union.byte3]++;
                count4[union.byte4]++;
                count5[union.byte5]++;
                count6[union.byte6]++;
                count7[union.byte7]++;
            }
#else
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                uint value = inArray[current];
                count[0][ value &       0xff       ]++;
                count[1][(value &     0xff00) >>  8]++;
                count[2][(value &   0xff0000) >> 16]++;
                count[3][(value & 0xff000000) >> 24]++;
            }
#endif
            return new Tuple<uint[][], UInt64[]>(count, inKeys);
        }
        // The idea of 1-D array is that the individual digit counts (256 per digit in case of 8-bit digits) don't interfere with each other in L1 cache
        // whereas with jagged array they may depending on how each row happens to be allocated on the heap
        public static uint[] HistogramByteComponents1D(uint[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(uint);
            uint[] count = new uint[numberOfDigits * numberOfBins];

            var union = new UInt32ByteUnion();
            for (int current = l; current <= r; current++)
            {
                union.integer = inArray[current];
                count[      union.byte0]++;
                count[256 + union.byte1]++;
                count[512 + union.byte2]++;
                count[768 + union.byte3]++;
            }
            return count;
        }

        public static uint[][] HistogramByteComponents(ulong[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(ulong);
            uint[][] count = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];

            uint[] count0 = count[0];
            uint[] count1 = count[1];
            uint[] count2 = count[2];
            uint[] count3 = count[3];
            uint[] count4 = count[4];
            uint[] count5 = count[5];
            uint[] count6 = count[6];
            uint[] count7 = count[7];

            var union  = new UInt64ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = inArray[current];
                count0[union.byte0]++;
                count1[union.byte1]++;
                count2[union.byte2]++;
                count3[union.byte3]++;
                count4[union.byte4]++;
                count5[union.byte5]++;
                count6[union.byte6]++;
                count7[union.byte7]++;
            }
            return count;
        }

        public static uint[][] HistogramByteComponents(int[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(ulong);
            uint[][] count = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];

            uint[] count0 = count[0];
            uint[] count1 = count[1];
            uint[] count2 = count[2];
            uint[] count3 = count[3];

            var union = new Int32ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = inArray[current];
                count0[union.byte0]++;
                count1[union.byte1]++;
                count2[union.byte2]++;
                count3[((uint)inArray[current] >> 24) ^ 128]++;
            }
            return count;
        }

        public static uint[][] HistogramByteComponents(long[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(ulong);
            uint[][] count = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];

            uint[] count0 = count[0];
            uint[] count1 = count[1];
            uint[] count2 = count[2];
            uint[] count3 = count[3];
            uint[] count4 = count[4];
            uint[] count5 = count[5];
            uint[] count6 = count[6];
            uint[] count7 = count[7];

            var union = new Int64ByteUnion();
            for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = inArray[current];
                count0[union.byte0]++;
                count1[union.byte1]++;
                count2[union.byte2]++;
                count3[union.byte3]++;
                count4[union.byte4]++;
                count5[union.byte5]++;
                count6[union.byte6]++;
                count7[((ulong)inArray[current] >> 56) ^ 128]++;
            }
            return count;
        }

        public static Tuple<uint[][], int> HistogramByteComponentsAndStatistics(long[] inArray, Int32 l, Int32 r)
        {
            const int numberOfBins = 256;
            const int numberOfDigits = sizeof(ulong);
            uint[][] count = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                count[i] = new uint[numberOfBins];
            int numElementsPreSorted = 0;

            uint[] count0 = count[0];
            uint[] count1 = count[1];
            uint[] count2 = count[2];
            uint[] count3 = count[3];
            uint[] count4 = count[4];
            uint[] count5 = count[5];
            uint[] count6 = count[6];
            uint[] count7 = count[7];

            var union = new Int64ByteUnion();

            int current = l;
            if (current <= r)
            {
                union.integer = inArray[current];
                count0[union.byte0]++;
                count1[union.byte1]++;
                count2[union.byte2]++;
                count3[union.byte3]++;
                count4[union.byte4]++;
                count5[union.byte5]++;
                count6[union.byte6]++;
                count7[((ulong)inArray[current] >> 56) ^ 128]++;
                current++;

                numElementsPreSorted++;     // initial single array element is considered sorted, since there is only a single element
            }

            for (; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
            {
                union.integer = inArray[current];
                count0[union.byte0]++;
                count1[union.byte1]++;
                count2[union.byte2]++;
                count3[union.byte3]++;
                count4[union.byte4]++;
                count5[union.byte5]++;
                count6[union.byte6]++;
                count7[((ulong)inArray[current] >> 56) ^ 128]++;

                // TODO: It should be possible to take the if/branch out, possibly by using SIMD/SSE or by separating the > from the ==, where ? can be done with a subtraction
                //       and equal with XOR followed by a subtraction from all 1's
                if (inArray[current] >= inArray[current - 1])
                    numElementsPreSorted++;
            }
            return new Tuple<uint[][], int>(count, numElementsPreSorted);
        }

        public static int[] HistogramOneByteComponent(ulong[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            //const ulong byteMask = numberOfBins - 1;
            int[] count = new int[numberOfBins];

            for (int current = l; current <= r; current++)
            {
                //count[(inArray[current] >> shiftRightAmount) & byteMask]++;
                count[(byte)(inArray[current] >> shiftRightAmount)]++;          // ?? Which way is faster. Need to look at assembly language listing too
            }

            return count;
        }

        public static int[] HistogramOneByteComponent(long[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            int[] count = new int[numberOfBins];

            if (shiftRightAmount != 56)
            {
                for (int current = l; current <= r; current++)
                {
                    count[(byte)(inArray[current] >> shiftRightAmount)]++;
                }
            }
            else
            {
                for (int current = l; current <= r; current++)
                {
                    count[(byte)(inArray[current] >> shiftRightAmount) ^ 128]++;
                }
            }

            return count;
        }

        public static int[] HistogramOneByteComponent(int[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            int[] count = new int[numberOfBins];

            if (shiftRightAmount != 24)
            {
                for (int current = l; current <= r; current++)
                {
                    count[(byte)(inArray[current] >> shiftRightAmount)]++;
                }
            }
            else
            {
                for (int current = l; current <= r; current++)
                {
                    count[(byte)(inArray[current] >> shiftRightAmount) ^ 128]++;
                }
            }

            return count;
        }
        public static int[] HistogramOneByteComponent(float[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            int[] count = new int[numberOfBins];
            var f2i = default(FloatUInt32Union);

            if (shiftRightAmount != 24)
            {
                for (int current = l; current <= r; current++)
                {
                    uint digit;
                    f2i.floatValue = inArray[current];
                    if ((f2i.uinteger & 0x80000000U) == 0)
                        digit = f2i.uinteger >> shiftRightAmount;                   // positive values => don't flip anything
                    else
                        digit = (f2i.uinteger ^ 0xFFFFFFFFU) >> shiftRightAmount;   // negative values => flip the whole value

                    count[(byte)digit]++;
                }
            }
            else
            {
                for (int current = l; current <= r; current++)
                {
                    uint digit;
                    f2i.floatValue = inArray[current];
                    if ((f2i.uinteger & 0x80000000U) == 0)
                        digit = (f2i.uinteger >> shiftRightAmount) ^ 128;               // positive values => flip just the sign bit
                    else
                        digit = (f2i.uinteger ^ 0xFFFFFFFFU) >> shiftRightAmount;       // negative values => flip the whole value including the sign bit

                    count[(byte)digit]++;
                }
            }

            return count;
        }

        public static int[] HistogramOneByteComponent(double[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            int[] count = new int[numberOfBins];
            var d2i = default(DoubleUInt64Union);

            if (shiftRightAmount != 56)
            {
                for (int current = l; current <= r; current++)
                {
                    ulong digit;
                    d2i.doubleValue = inArray[current];
                    if ((d2i.ulongInteger & 0x8000000000000000) == 0)
                        digit = d2i.ulongInteger >> shiftRightAmount;                           // positive values => don't flip anything
                    else
                        digit = (d2i.ulongInteger ^ 0xFFFFFFFFFFFFFFFF) >> shiftRightAmount;    // negative values => flip the whole value

                    count[(byte)digit]++;
                }
            }
            else
            {
                for (int current = l; current <= r; current++)
                {
                    ulong digit;
                    d2i.doubleValue = inArray[current];
                    if ((d2i.ulongInteger & 0x8000000000000000) == 0)
                        digit = (d2i.ulongInteger >> shiftRightAmount) ^ 128;                       // positive values => flip just the sign bit
                    else
                        digit = (d2i.ulongInteger ^ 0xFFFFFFFFFFFFFFFF) >> shiftRightAmount;        // negative values => flip the whole value including the sign bit

                    count[(byte)digit]++;
                }
            }

            return count;
        }

        public static int[] HistogramNbitComponents(long[] inArray, Int32 l, Int32 r, int shiftRightAmount, int numberOfBitPerComponent)
        {
            const int NumBitsInLong = sizeof(long) * 8;
            ulong numberOfBins      = 1UL << numberOfBitPerComponent;
            ulong halfOfNumBins     = numberOfBins / 2;
            ulong bitMask           = numberOfBins - 1;
            int[] count = new int[numberOfBins];

            if (shiftRightAmount != (NumBitsInLong - numberOfBitPerComponent))
            {
                for (int current = l; current <= r; current++)
                {
                    count[((ulong)inArray[current] >> shiftRightAmount) & bitMask]++;
                }
            }
            else
            {
                for (int current = l; current <= r; current++)
                {
                    count[((ulong)inArray[current] >> shiftRightAmount) ^ halfOfNumBins]++;
                }
            }

            return count;
        }


        public static int[] HistogramByteComponentsUsingUnion(ulong[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            int[] count = new int[numberOfBins];
            int whichByte = shiftRightAmount / 8;

            var union = new UInt64ByteUnion();

            switch (whichByte)
            {
                case 0:
                    for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    {
                        union.integer = inArray[current];
                        count[union.byte0]++;
                    }
                    break;
                case 1:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte1]++;
                    }
                    break;
                case 2:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte2]++;
                    }
                    break;
                case 3:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte3]++;
                    }
                    break;
                case 4:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte4]++;
                    }
                    break;
                case 5:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte5]++;
                    }
                    break;
                case 6:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte6]++;
                    }
                    break;
                case 7:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte7]++;
                    }
                    break;
            }
            return count;
        }

        public static int[] HistogramByteComponentsUsingUnion(long[] inArray, Int32 l, Int32 r, int shiftRightAmount)
        {
            const int numberOfBins = 256;
            int[] count = new int[numberOfBins];
            int whichByte = shiftRightAmount / 8;

            var union = new Int64ByteUnion();

            switch (whichByte)
            {
                case 0:
                    for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    {
                        union.integer = inArray[current];
                        count[union.byte0]++;
                    }
                    break;
                case 1:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte1]++;
                    }
                    break;
                case 2:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte2]++;
                    }
                    break;
                case 3:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte3]++;
                    }
                    break;
                case 4:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte4]++;
                    }
                    break;
                case 5:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte5]++;
                    }
                    break;
                case 6:
                    for (int current = l; current <= r; current++)
                    {
                        union.integer = inArray[current];
                        count[union.byte6]++;
                    }
                    break;
                case 7:
                    for (int current = l; current <= r; current++)
                    {
                        count[((ulong)inArray[current] >> shiftRightAmount) ^ 128]++;
                    }
                    break;
            }
            return count;
        }

        public static int[] Histogram9bitComponents(float[] inArray, Int32 l, Int32 r, uint bitMask, int shiftRightAmount)
        {
            const int numberOfBins = 512;
            //const uint bitMask = numberOfBins - 1;
            int[] count = new int[numberOfBins];

            if (shiftRightAmount != 23)
            {
                for (int current = l; current <= r; current++)
                    count[((uint)inArray[current] & bitMask) >> shiftRightAmount]++;
            }
            else
            {
                for (int current = l; current <= r; current++)
                    count[((uint)inArray[current] >> shiftRightAmount) ^ 256]++;
            }

            return count;
        }

        public static int[] Histogram12bitComponents(double[] inArray, Int32 l, Int32 r, ulong bitMask, int shiftRightAmount)
        {
            const int numberOfBins = 4096;
            int[] count = new int[numberOfBins];

            if (shiftRightAmount != 52)
            {
                for (int current = l; current <= r; current++)
                {
                    var currValue = BitConverter.ToUInt64(BitConverter.GetBytes(inArray[current]), 0);
                    count[(currValue & bitMask) >> shiftRightAmount]++;
                }
            }
            else
            {
                for (int current = l; current <= r; current++)
                {
                    var currValue = BitConverter.ToUInt64(BitConverter.GetBytes(inArray[current]), 0);
                    count[(currValue >> shiftRightAmount) ^ 2048]++;
                }
            }

            return count;
        }

        public static uint[][] HistogramNBitsPerComponents(uint[] inArray, Int32 l, Int32 r, int bitsPerComponent)
        {
            int numberOfBins = 1 << bitsPerComponent;
            int numberOfDigits = (sizeof(uint) * 8 + bitsPerComponent - 1) / bitsPerComponent;  // round up
            //Console.WriteLine("HistogramNBitsPerComponents: NumberOfDigits = {0}", numberOfDigits);
            uint[][] countLeft = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countLeft[i] = new uint[numberOfBins];
            if (bitsPerComponent == 8)
            {
                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];

                var union = new UInt32ByteUnion();
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    union.integer = inArray[current];
                    countLeft0[union.byte0]++;
                    countLeft1[union.byte1]++;
                    countLeft2[union.byte2]++;
                    countLeft3[union.byte3]++;
                }
            }
            else if (bitsPerComponent == 9)
            {
                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];

                for (int current = l; current <= r; current++)
                {
                    uint value = inArray[current];
                    countLeft0[value  &      0x1ff       ]++;
                    countLeft1[(value &    0x3fe00) >>  9]++;
                    countLeft2[(value &  0x7fc0000) >> 18]++;
                    countLeft3[(value & 0xf8000000) >> 27]++;
                }
            }
            else if (bitsPerComponent == 10)
            {
                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];

                for (int current = l; current <= r; current++)
                {
                    uint value = inArray[current];
                    countLeft0[ value &      0x3ff       ]++;
                    countLeft1[(value &    0xffc00) >> 10]++;
                    countLeft2[(value & 0x3ff00000) >> 20]++;
                    countLeft3[(value & 0xc0000000) >> 30]++;
                }
            }
            else if (bitsPerComponent == 11)
            {
                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];

                for (int current = l; current <= r; current++)
                {
                    uint value = inArray[current];
                    countLeft0[ value &      0x7ff       ]++;
                    countLeft1[(value &   0x3ff800) >> 11]++;
                    countLeft2[(value & 0xffc00000) >> 22]++;
                }
            }
            else if (bitsPerComponent == 12)
            {
                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];

                for (int current = l; current <= r; current++)
                {
                    uint value = inArray[current];
                    countLeft0[ value &      0xfff       ]++;
                    countLeft1[(value &   0xfff000) >> 12]++;
                    countLeft2[(value & 0xff000000) >> 24]++;
                }
            }
            else if (bitsPerComponent == 13)
            {
                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];

                for (int current = l; current <= r; current++)
                {
                    uint value = inArray[current];
                    countLeft0[ value &     0x1fff       ]++;
                    countLeft1[(value &  0x3ffe000) >> 13]++;
                    countLeft2[(value & 0xfc000000) >> 26]++;
                }
            }
            else if (bitsPerComponent == 16)
            {
                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];

                var union = new UInt32UShortUnion();
                for (int current = l; current <= r; current++)
                {
                    union.integer = inArray[current];
                    countLeft0[union.ushort0]++;
                    countLeft1[union.ushort1]++;
                }
            }
            else
            {
                uint componentMask = (uint)numberOfBins - 1;
                for (int current = l; current <= r; current++)
                {
                    uint value = inArray[current];
                    for (int i = 0; i < numberOfDigits; i++)
                    {
                        countLeft[i][value & componentMask]++;
                        componentMask <<= bitsPerComponent;
                    }
                }
            }
            return countLeft;
        }

        public static uint[][] HistogramNBitsPerComponents(ulong[] inArray, Int32 l, Int32 r, int bitsPerComponent)
        {
            int numberOfBins = 1 << bitsPerComponent;
            int numberOfDigits = (sizeof(uint) * 8 + bitsPerComponent - 1) / bitsPerComponent;  // round up
            uint[][] countLeft = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                countLeft[i] = new uint[numberOfBins];
            if (bitsPerComponent == 8)
            {
                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];
                uint[] countLeft4 = countLeft[4];
                uint[] countLeft5 = countLeft[5];
                uint[] countLeft6 = countLeft[6];
                uint[] countLeft7 = countLeft[7];

                var union = new UInt64ByteUnion();
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
            }
            else if (bitsPerComponent == 9)
            {
                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];
                uint[] countLeft4 = countLeft[4];
                uint[] countLeft5 = countLeft[5];
                uint[] countLeft6 = countLeft[6];
                uint[] countLeft7 = countLeft[7];

                for (int current = l; current <= r; current++)
                {
                    ulong value = inArray[current];
                    countLeft0[ value &              0x1ff       ]++;
                    countLeft1[(value &            0x3fe00) >>  9]++;
                    countLeft2[(value &          0x7fc0000) >> 18]++;
                    countLeft3[(value &        0xff8000000) >> 27]++;
                    countLeft4[(value &     0x1ff000000000) >> 36]++;
                    countLeft5[(value &   0x3fe00000000000) >> 45]++;
                    countLeft6[(value & 0x7fc0000000000000) >> 54]++;
                    countLeft7[(value & 0x8000000000000000) >> 63]++;
                }
            }
            else if (bitsPerComponent == 10)    // useful for 64-bit
            {
                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];
                uint[] countLeft4 = countLeft[4];
                uint[] countLeft5 = countLeft[5];
                uint[] countLeft6 = countLeft[6];

                for (int current = l; current <= r; current++)
                {
                    ulong value = inArray[current];
                    countLeft0[ value &              0x3ff       ]++;
                    countLeft1[(value &            0xffc00) >> 10]++;
                    countLeft2[(value &         0x3ff00000) >> 20]++;
                    countLeft3[(value &       0xffc0000000) >> 30]++;
                    countLeft4[(value &    0x3ff0000000000) >> 40]++;
                    countLeft5[(value &  0xffc000000000000) >> 50]++;
                    countLeft6[(value & 0xf000000000000000) >> 60]++;
                }
            }
            else if (bitsPerComponent == 11)
            {
                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];
                uint[] countLeft4 = countLeft[4];
                uint[] countLeft5 = countLeft[5];

                for (int current = l; current <= r; current++)
                {
                    ulong value = inArray[current];
                    countLeft0[ value &              0x7ff       ]++;
                    countLeft1[(value &           0x3ff800) >> 11]++;
                    countLeft2[(value &        0x1ffc00000) >> 22]++;
                    countLeft3[(value &      0xffe00000000) >> 33]++;
                    countLeft4[(value &   0x7ff00000000000) >> 44]++;
                    countLeft5[(value & 0xff80000000000000) >> 55]++;
                }
            }
            else if (bitsPerComponent == 12)
            {
                for (int current = l; current <= r; current++)
                {
                    uint[] countLeft0 = countLeft[0];
                    uint[] countLeft1 = countLeft[1];
                    uint[] countLeft2 = countLeft[2];
                    uint[] countLeft3 = countLeft[3];
                    uint[] countLeft4 = countLeft[4];
                    uint[] countLeft5 = countLeft[5];

                    ulong value = inArray[current];
                    countLeft0[ value &              0xfff       ]++;
                    countLeft1[(value &           0xfff000) >> 12]++;
                    countLeft2[(value &        0xfff000000) >> 24]++;
                    countLeft3[(value &     0xfff000000000) >> 36]++;
                    countLeft4[(value &  0xfff000000000000) >> 48]++;
                    countLeft5[(value & 0xf000000000000000) >> 60]++;
                }
            }
            else if (bitsPerComponent == 13)
            {
                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];
                uint[] countLeft4 = countLeft[4];

                for (int current = l; current <= r; current++)
                {
                    ulong value = inArray[current];
                    countLeft0[ value &             0x1fff       ]++;
                    countLeft1[(value &          0x3ffe000) >> 13]++;
                    countLeft2[(value &       0x7ffc000000) >> 26]++;
                    countLeft3[(value &    0xfff8000000000) >> 39]++;
                    countLeft4[(value & 0xfff0000000000000) >> 52]++;
                }
            }
            else if (bitsPerComponent == 16)
            {
                uint[] countLeft0 = countLeft[0];
                uint[] countLeft1 = countLeft[1];
                uint[] countLeft2 = countLeft[2];
                uint[] countLeft3 = countLeft[3];

                var union = new UInt64UShortUnion();
                for (int current = l; current <= r; current++)
                {
                    union.integer = inArray[current];
                    countLeft0[union.ushort0]++;
                    countLeft1[union.ushort1]++;
                    countLeft2[union.ushort0]++;
                    countLeft3[union.ushort1]++;
                }
            }
            else
            {
                uint componentMask = (uint)numberOfBins - 1;
                for (int current = l; current <= r; current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                {
                    ulong value = inArray[current];
                    for (int i = 0; i < numberOfDigits; i++)
                    {
                        countLeft[i][value & componentMask]++;
                        componentMask <<= bitsPerComponent;
                    }
                }
            }
            return countLeft;
        }
    }
}
