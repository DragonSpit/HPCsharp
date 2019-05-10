// TODO: Implement nullable versions of Sum, only faster than the standard C# ones. One idea is to turn all null values into zeroes.
// TODO: Consider improved .Sum for decimals using the same idea: serial and multi-core. Can't do SSE, since there is no direct CPU decimal support.
// TODO: Add the ability to handle de-normal floating-point numbers and flush them to zero to get higher performance when accuracy of small numbers is not as important
//       From what I read online, using SSE may be a better way, since it supports flush to zero for denormals and we may have control then.
// TODO: Try measuring performance of .Sum() that is scalar and SSE when the array fits into cache (L1 or L2) since in these cases performance will not be limited by system
//       memory bandwidth, but will be limited by the cache memory bandwidth which is much higher. Run over the same array using .Sum() many times to measure average and min time.
// TODO: Provide a function to sum a field within a user defined type
// TODO: Add support for List
// TODO: Add support for User Defined Types (Objects), where the user would define a Lambda function to pull out a field within that object.
// TODO: Implement one pass sweeping tree sum for Neumaier .Sum() algorithm, which sweeps once from left to right and sums pairs, then sum of pairs and sum of pair-pairs and so on, creating/growing
//       a List (or preallocate array big enough logN size), or maybe sweep from left and right of a full binary tree size and then do the rest. It would be cool to do it all in one pass, instead of
//       log passes. This will produce a more accurate .Sum() without needi9ng Neumaier algorithm overhead.
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        public static decimal SumDecimalHpc(this long[] arrayToSum)
        {
            decimal overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        internal static decimal SumDecimalHpc(this long[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            decimal overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

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

        public static decimal SumDecimalHpc(this ulong[] arrayToSum)
        {
            decimal overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        internal static decimal SumDecimalHpc(this ulong[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            decimal overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static ulong SumHpc(this ulong[] arrayToSum)
        {
            ulong overallSum = 0;
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

        public static double SumHpc(this double[] arrayToSum)
        {
            double overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static decimal SumHpc(this decimal[] arrayToSum)
        {
            decimal overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        internal static decimal SumHpc(this decimal[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            decimal overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
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

        public static double SumNeumaier(this float firstValue, float secondValue)
        {
            double sum = 0.0;
            double c = 0.0;                                 // A running compensation for lost low-order bits  

            double t = sum + firstValue;
            if (Math.Abs(sum) >= Math.Abs(firstValue))
                c += (sum - t) + firstValue;                // If sum is bigger, low-order digits of input[i] are lost.
            else
                c += (firstValue - t) + sum;                // Else low-order digits of sum are lost
            sum = t;

            t = sum + secondValue;
            if (Math.Abs(sum) >= Math.Abs(secondValue))
                c += (sum - t) + secondValue;                // If sum is bigger, low-order digits of input[i] are lost.
            else
                c += (secondValue - t) + sum;               // Else low-order digits of sum are lost
            sum = t;

            return sum + c;                                 // Correction only applied once in the very end
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

        public static double SumNeumaier(this float[] arrayToSum, int startIndex, int length)
        {
            double sum = 0.0;
            double c   = 0.0;                               // A running compensation for lost low-order bits  
            int endIndex = startIndex + length;

            for (int i = startIndex; i < endIndex; i++)
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

        public static double SumNeumaier(this double firstValue, double secondValue)
        {
            double sum = 0.0;
            double c   = 0.0;                               // A running compensation for lost low-order bits  
            
            double t = sum + firstValue;
            if (Math.Abs(sum) >= Math.Abs(firstValue))
                c += (sum - t) + firstValue;                // If sum is bigger, low-order digits of input[i] are lost.
            else
                c += (firstValue - t) + sum;                // Else low-order digits of sum are lost
            sum = t;

            t = sum + secondValue;
            if (Math.Abs(sum) >= Math.Abs(secondValue))
                c += (sum - t) + secondValue;               // If sum is bigger, low-order digits of input[i] are lost.
            else
                c += (secondValue - t) + sum;               // Else low-order digits of sum are lost
            sum = t;

            return sum + c;                                 // Correction only applied once in the very end
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

        public static double SumNeumaier(this double[] arrayToSum, int startIndex, int length)
        {
            double sum = 0.0;
            double c = 0.0;                                 // A running compensation for lost low-order bits  
            int endIndex = startIndex + length;

            for (int i = startIndex; i < endIndex; i++)
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
