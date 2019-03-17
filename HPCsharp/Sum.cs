// TODO: Implement nullable versions of Sum, only faster than the standard C# ones. One idea is to turn all null values into zeroes.
// TODO: Once improved Sum for floating-point are available, update https://stackoverflow.com/questions/2419343/how-to-sum-up-an-array-of-integers-in-c-sharp/54794753#54794753
// TODO: Consider improved .Sum for decimals using the same idea: serial and multi-core. Can't do SSE, since there is no decimal support.
// TODO: Add the ability to handle de-normal floating-point numbers and flush them to zero to get higher performance when accuracy of small numbers is not as important
//       From what I read online, using SSE may be a better way, since it supports flush to zero for denormals and we may have control then.
// TODO: Try measuring performance of .Sum() that is scalar and SSE when the array fits into cache (L1 or L2) since in these cases performance will not be limited by system
//       memory bandwidth, but will be limited by the cache memory bandwidth which is much higher. Run over the same array using .Sum() many times to measure average and min time.
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        public static long SumHpc(this long[] arrayToSum)
        {
            long overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static long SumHpc(this int[] arrayToSum)
        {
            long overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static long SumHpc(this short[] arrayToSum)
        {
            long overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static long SumHpc(this sbyte[] arrayToSum)
        {
            long overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static ulong SumHpc(this uint[] arrayToSum)
        {
            ulong overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static ulong SumHpc(this ushort[] arrayToSum)
        {
            ulong overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static ulong SumHpc(this byte[] arrayToSum)
        {
            ulong overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static double SumHpc(this float[] arrayToSum)
        {
            double overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        // Implementation https://en.wikipedia.org/wiki/Kahan_summation_algorithm
        public static double SumKahan(this float[] arrayToSum)
        {
            double sum = 0.0;
            double c = 0.0;                                 // A running compensation for lost low-order bits    
            for (int i = 0; i < arrayToSum.Length; i++)
            {
                double y = arrayToSum[i] - c;               // So far, so good: c is zero
                double t = sum + y;                         // Alas, sum is big, y small, so low-order bits of y are lost
                c = (t - sum) - y;                          // (t - sum) cancels the high-order par of y; subtracting y recovers negativ (low part of y)
                sum = t;                                    // Algebraically, c should always be zero. Beware overly-aggressive optimizing compilers!
                                                            // Next time around, the lost low part will be added to y in a fresh attempt.
            }
            return sum;
        }

        // Implementation https://en.wikipedia.org/wiki/Kahan_summation_algorithm
        public static double SumNeumaier(this float[] arrayToSum)
        {
            double sum = 0.0;
            double c = 0.0;                                 // A running compensation for lost low-order bits    
            for (int i = 0; i < arrayToSum.Length; i++)
            {
                double t = sum + arrayToSum[i];
                if (Math.Abs(sum) >= Math.Abs(arrayToSum[i]))
                    c += (sum - t) + arrayToSum[i];         // If sum is bigger, low-order digits of input[i] are lost.
                else
                    c += (arrayToSum[i] - t) + sum;         // Else low-order digits of sum are lost
                sum = t;
            }
            return sum + c;                                 // Correction only applied once in the very end
        }

        // Implementation https://en.wikipedia.org/wiki/Kahan_summation_algorithm
        public static double SumKahan(this double[] arrayToSum)
        {
            double sum = 0.0;
            double c = 0.0;                                 // A running compensation for lost low-order bits    
            for (int i = 0; i < arrayToSum.Length; i++)
            {
                double y = arrayToSum[i] - c;               // So far, so good: c is zero
                double t = sum + y;                         // Alas, sum is big, y small, so low-order bits of y are lost
                c = (t - sum) - y;                          // (t - sum) cancels the high-order par of y; subtracting y recovers negativ (low part of y)
                sum = t;                                    // Algebraically, c should always be zero. Beware overly-aggressive optimizing compilers!
                                                            // Next time around, the lost low part will be added to y in a fresh attempt.
            }
            return sum;
        }

        // Implementation https://en.wikipedia.org/wiki/Kahan_summation_algorithm
        public static double SumNeumaier(this double[] arrayToSum)
        {
            double sum = 0.0;
            double c = 0.0;                                 // A running compensation for lost low-order bits    
            for (int i = 0; i < arrayToSum.Length; i++)
            {
                double t = sum + arrayToSum[i];
                if (Math.Abs(sum) >= Math.Abs(arrayToSum[i]))
                    c += (sum - t) + arrayToSum[i];         // If sum is bigger, low-order digits of input[i] are lost.
                else
                    c += (arrayToSum[i] - t) + sum;         // Else low-order digits of sum are lost
                sum = t;
            }
            return sum + c;                                 // Correction only applied once in the very end
        }
    }
}
