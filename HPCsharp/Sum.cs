// TODO: Implement nullable versions of Sum, only faster than the standard C# ones. One idea is to turn all null values into zeroes, or skip them.
//       From research on the web, Linq.Sum() skips null values by default - i.e. treats them as zeroes. This is inconsistent with adding a null value to a number.
// TODO: Add the ability to handle de-normal floating-point numbers and flush them to zero to get higher performance when accuracy of small numbers is not as important
//       From what I read online, using SSE may be a better way, since it supports flush to zero for denormals and we may have control then.
// TODO: Add support for List
// TODO: Add support for User Defined Types (Objects), where the user would define a Lambda function to pull out a field within that object.
// TODO: Implement one pass sweeping tree sum for Neumaier .Sum() algorithm, which sweeps once from left to right and sums pairs, then sum of pairs and sum of pair-pairs and so on, creating/growing
//       a List (or preallocate array big enough logN size), or maybe sweep from left and right of a full binary tree size and then do the rest. It would be cool to do it all in one pass, instead of
//       log passes. This will produce a more accurate .Sum() without needing Neumaier algorithm overhead.
// TODO: Since C# has support for BigInteger data type in System.Numerics, then provide accurate .Sum() all the way to these for decimal[], float[] and double[]. Basically, provide a consistent story for .Sum() where every type can be
//       summed with perfect accuracy when needed. Make sure naming of functions is consistent for all of this and across all data types, multi-core and SSE implementations, to make it simple, logical and consistent to use.
//       Sadly, this idea won't work, since we need a BigDecimal or BigFloatingPoint to capture perfect accumulation for both of these non-integer types.
// TODO: Improve pair-wise .Sum() with O(e*lgN) so that it works for any size array, even small ones, whereas the current implementation which is suggested on Wikipedia favors large arrays, and does a naive summation for small arrays.
//       A possible way to do it is by using a stack structure to emulate recursion, pushing the currect "level-sum" onto this stack. This would work for SSE as well by pushing SSE-size data type onto the stack. There may be other even
//       more efficient methods.
// TODO: Blog/write about float .Sum() that uses double and Kahan/Nuemaier for increased accuracy, perfect for 500M array elements and then Neumaier takes it the rest of the way.
// TODO: Does Kahan algorithm also make sense for decimal?
// TODO: To speedup summing up of long to decimal accumulation, Josh suggested using a long accumulator and catching the overflow exception and then adding to decimal - i.e. most of the time accumulate to long and once in
// TODO: Answer this question on stack overflow https://stackoverflow.com/questions/53075546/array-sum-results-in-an-overflow
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System;
using System.Threading.Tasks;

namespace HPCsharp.Algorithms
{
    static public partial class Sum
    {
        /// <summary>
        /// Faster, perfectly accurate summation of long[] array, which uses a decimal accumulator for perfect accuracy,
        /// and integer summations for higher performance, handling overflow exceptions internally.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumDecimalFast(this long[] arrayToSum)
        {
            decimal overallSum = 0;
            long tempSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
            {
                try
                {
                    tempSum += arrayToSum[i];
                }
                catch (OverflowException)
                {
                    overallSum += tempSum;
                    tempSum = 0;
                }
            }
            return overallSum + tempSum;
        }

        /// <summary>
        /// Faster, perfectly accurate summation of ulong[] array, which uses a decimal accumulator for perfect accuracy,
        /// and integer summations for higher performance, handling overflow exceptions internally.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumDecimalFast(this ulong[] arrayToSum)
        {
            decimal overallSum = 0;
            ulong tempSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
            {
                try
                {
                    tempSum += arrayToSum[i];
                }
                catch (OverflowException)
                {
                    overallSum += tempSum;
                    tempSum = 0;
                }
            }
            return overallSum + tempSum;
        }

        /// <summary>
        /// Slower, perfectly accurate summation of long[] array, which uses a decimal accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumDecimalHpc(this long[] arrayToSum)
        {
            return arrayToSum.SumDecimalHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Slower, perfectly accurate summation of long[] array, which uses a decimal accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumDecimalHpc(this long[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            decimal overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Slower, perfectly accurate summation of long[] array, which uses a decimal accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal summation value</returns>
        public static decimal SumDecimalHpc(this long?[] arrayToSum)
        {
            return arrayToSum.SumDecimalHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Slower, perfectly accurate summation of long[] nullable array, which uses a decimal accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumDecimalHpc(this long?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            decimal overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (long)arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of long[] array.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than Int32.MaxValue</exception>
        public static long SumHpc(this long[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of long[] array.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than Int64.MaxValue</exception>
        public static long SumHpc(this long[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            long overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of long[] nullable array. Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than Int64.MaxValue</exception>
        public static long SumHpc(this long?[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of long[] nullable array. Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than Int64.MaxValue</exception>
        public static long SumHpc(this long?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            long overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (long)arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of int[] array, which uses a long accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static long SumHpc(this int[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of int[] array, which uses a long accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumHpc(this int[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            long overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of int[] nullable array, which uses a long accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static long SumHpc(this int?[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of int[] nullable array, which uses a long accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumHpc(this int?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            long overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (int)arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of short[] array, which uses a long accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static long SumHpc(this short[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of short[] array, which uses a long accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumHpc(this short[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            long overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of short[] nullable array, which uses a long accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static long SumHpc(this short?[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of short[] nullable array, which uses a long accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumHpc(this short?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            long overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (short)arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of sbyte[] array, which uses a long accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static long SumHpc(this sbyte[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of sbyte[] array, which uses a long accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumHpc(this sbyte[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            long overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of sbyte[] nullable array, which uses a long accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static long SumHpc(this sbyte?[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of sbyte[] nullable array, which uses a long accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumHpc(this sbyte?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            long overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (sbyte)arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of ulong[] array, which uses a decimal accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static decimal SumDecimalHpc(this ulong[] arrayToSum)
        {
            return arrayToSum.SumDecimalHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of ulong[] array, which uses a decimal accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumDecimalHpc(this ulong[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            decimal overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of ulong[] nullable array, which uses a decimal accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumDecimalHpc(this ulong?[] arrayToSum)
        {
            return arrayToSum.SumDecimalHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of ulong[] nullable array, which uses a decimal accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumDecimalHpc(this ulong?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            decimal overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (ulong)arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of ulong[] array.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than UInt64.MaxValue</exception>
        public static ulong SumHpc(this ulong[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of ulong[] array.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than UInt64.MaxValue</exception>
        public static ulong SumHpc(this ulong[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            ulong overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of ulong[] array. Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than UInt64.MaxValue</exception>
        public static ulong SumHpc(this ulong?[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of ulong[] array. Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        /// <exception>TSource:System.OverflowException: when the sum value is greater than UInt64.MaxValue</exception>
        public static ulong SumHpc(this ulong?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            ulong overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (ulong)arrayToSum[i];
            return overallSum;
        }

        public static ulong SumHpc(this uint[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        public static ulong SumHpc(this uint[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            ulong overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static ulong SumHpc(this uint?[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        public static ulong SumHpc(this uint?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            ulong overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (uint)arrayToSum[i];
            return overallSum;
        }

        public static ulong SumHpc(this ushort[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        public static ulong SumHpc(this ushort[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            ulong overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static ulong SumHpc(this ushort?[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        public static ulong SumHpc(this ushort?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            ulong overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (ushort)arrayToSum[i];
            return overallSum;
        }

        public static ulong SumHpc(this byte[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        public static ulong SumHpc(this byte[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            ulong overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static ulong SumHpc(this byte?[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        public static ulong SumHpc(this byte?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            ulong overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (byte)arrayToSum[i];
            return overallSum;
        }

        public static float SumHpc(this float[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        public static float SumHpc(this float[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            float overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static float SumHpc(this float?[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        public static float SumHpc(this float?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            float overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (float)arrayToSum[i];
            return overallSum;
        }

        public static double SumDblHpc(this float[] arrayToSum)
        {
            return arrayToSum.SumDblHpc(0, arrayToSum.Length);
        }

        public static double SumDblHpc(this float[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            double overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static double SumDblHpc(this float?[] arrayToSum)
        {
            return arrayToSum.SumDblHpc(0, arrayToSum.Length);
        }

        public static double SumDblHpc(this float?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            double overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (float)arrayToSum[i];
            return overallSum;
        }

        internal static float SumLR(this float[] arrayToSum, int l, int r)
        {
            float overallSum = 0;
            for (int i = l; i <= r; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        internal static double SumDblLR(this float[] arrayToSum, int l, int r)
        {
            double overallSum = 0;
            for (int i = l; i <= r; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static double SumHpc(this double[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        public static double SumHpc(this double[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            double overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static double SumHpc(this double?[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        public static double SumHpc(this double?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            double overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (double)arrayToSum[i];
            return overallSum;
        }

        internal static double SumLR(this double[] arrayToSum, int l, int r)
        {
            double overallSum = 0;
            for (int i = l; i <= r; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static decimal SumHpc(this decimal[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        internal static decimal SumHpc(this decimal[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            decimal overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static decimal SumHpc(this decimal?[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        internal static decimal SumHpc(this decimal?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            decimal overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (decimal)arrayToSum[i];
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

        public static float SumNeumaier(float firstValue, float secondValue)
        {
            float sum = 0.0f;
            float c   = 0.0f;                                 // A running compensation for lost low-order bits  

            float t = sum + firstValue;
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

        public static double SumNeumaierDbl(float firstValue, float secondValue)
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
        public static float SumNeumaier(this float[] arrayToSum)
        {
            return arrayToSum.SumNeumaier(0, arrayToSum.Length);
        }

        public static float SumNeumaier(this float[] arrayToSum, int startIndex, int length)
        {
            float sum = 0.0f;
            float c = 0.0f;                               // A running compensation for lost low-order bits  
            int endIndex = startIndex + length;

            for (int i = startIndex; i < endIndex; i++)
            {
                float t = sum + arrayToSum[i];
                if (Math.Abs(sum) >= Math.Abs(arrayToSum[i]))
                    c += (sum - t) + arrayToSum[i];         // If sum is bigger, low-order digits of input[i] are lost.
                else
                    c += (arrayToSum[i] - t) + sum;         // Else low-order digits of sum are lost
                sum = t;
            }
            return sum + c;                                 // Correction only applied once in the very end
        }

        public static float SumNeumaier(this float?[] arrayToSum)
        {
            return arrayToSum.SumNeumaier(0, arrayToSum.Length);
        }

        public static float SumNeumaier(this float?[] arrayToSum, int startIndex, int length)
        {
            float sum = 0.0f;
            float c   = 0.0f;                               // A running compensation for lost low-order bits  
            int endIndex = startIndex + length;

            for (int i = startIndex; i < endIndex; i++)
            {
                if (arrayToSum[i] != null)
                {
                    float arrayElement = (float)arrayToSum[i];
                    float t = sum + arrayElement;
                    if (Math.Abs(sum) >= Math.Abs(arrayElement))
                        c += (sum - t) + arrayElement;         // If sum is bigger, low-order digits of input[i] are lost.
                    else
                        c += (arrayElement - t) + sum;         // Else low-order digits of sum are lost
                    sum = t;
                }
            }
            return sum + c;                                 // Correction only applied once in the very end
        }

        public static double SumNeumaierDbl(this float[] arrayToSum)
        {
            return arrayToSum.SumNeumaier(0, arrayToSum.Length);
        }

        public static double SumNeumaierDbl(this float[] arrayToSum, int startIndex, int length)
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

        public static double SumNeumaierDbl(this float?[] arrayToSum)
        {
            return arrayToSum.SumNeumaier(0, arrayToSum.Length);
        }

        public static double SumNeumaierDbl(this float?[] arrayToSum, int startIndex, int length)
        {
            double sum = 0.0;
            double c   = 0.0;                               // A running compensation for lost low-order bits  
            int endIndex = startIndex + length;

            for (int i = startIndex; i < endIndex; i++)
            {
                if (arrayToSum[i] != null)
                {
                    double arrayElement = (double)arrayToSum[i];
                    double t = sum + arrayElement;
                    if (Math.Abs(sum) >= Math.Abs(arrayElement))
                        c += (sum - t) + arrayElement;         // If sum is bigger, low-order digits of input[i] are lost.
                    else
                        c += (arrayElement - t) + sum;         // Else low-order digits of sum are lost
                    sum = t;
                }
            }
            return sum + c;                                 // Correction only applied once in the very end
        }

        public static double SumNeumaier(double firstValue, double secondValue)
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
            return arrayToSum.SumNeumaier(0, arrayToSum.Length);
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

        public static double SumNeumaier(this double?[] arrayToSum)
        {
            return arrayToSum.SumNeumaier(0, arrayToSum.Length);
        }

        public static double SumNeumaier(this double?[] arrayToSum, int startIndex, int length)
        {
            double sum = 0.0;
            double c = 0.0;                                 // A running compensation for lost low-order bits  
            int endIndex = startIndex + length;

            for (int i = startIndex; i < endIndex; i++)
            {
                if (arrayToSum[i] != null)
                {
                    double arrayElement = (double)arrayToSum[i];
                    double t = sum + arrayElement;
                    if (Math.Abs(sum) >= Math.Abs(arrayElement))
                        c += (sum - t) + arrayElement;         // If sum is bigger, low-order digits of input[i] are lost.
                    else
                        c += (arrayElement - t) + sum;         // Else low-order digits of sum are lost
                    sum = t;
                }
            }
            return sum + c;                                 // Correction only applied once in the very end
        }

        private static float SumPairwiseInner(this float[] arrayToSum, int l, int r, Func<float[], int, int, float> baseCase, Func<float, float, float> reduce, int thresholdDivideAndConquerSum = 16 * 1024)
        {
            float sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= thresholdDivideAndConquerSum)
                return baseCase(arrayToSum, l, r);

            int m = (r + l) / 2;

            float sumRight = 0;

            sumLeft  = SumPairwiseInner(arrayToSum, l,     m, baseCase, reduce);
            sumRight = SumPairwiseInner(arrayToSum, m + 1, r, baseCase, reduce);

            return reduce(sumLeft, sumRight);
        }

        private static double SumPairwiseDblInner(this float[] arrayToSum, int l, int r, Func<float[], int, int, double> baseCase, Func<double, double, double> reduce, int thresholdDivideAndConquerSum = 16 * 1024)
        {
            double sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= thresholdDivideAndConquerSum)
                return baseCase(arrayToSum, l, r);

            int m = (r + l) / 2;

            double sumRight = 0;

            sumLeft  = SumPairwiseDblInner(arrayToSum, l,     m, baseCase, reduce);
            sumRight = SumPairwiseDblInner(arrayToSum, m + 1, r, baseCase, reduce);

            return reduce(sumLeft, sumRight);
        }

        private static double SumPairwiseInner(this double[] arrayToSum, int l, int r, Func<double[], int, int, double> baseCase, Func<double, double, double> reduce, int thresholdDivideAndConquerSum = 16 * 1024)
        {
            double sumLeft = 0;

            if (l > r)
                return sumLeft;
            if ((r - l + 1) <= thresholdDivideAndConquerSum)
                return baseCase(arrayToSum, l, r);

            int m = (r + l) / 2;

            double sumRight = 0;

            sumLeft  = SumPairwiseInner(arrayToSum, l,     m, baseCase, reduce);
            sumRight = SumPairwiseInner(arrayToSum, m + 1, r, baseCase, reduce);

            return reduce(sumLeft, sumRight);
        }

        public static float SumPairwise(this float[] arrayToSum, int thresholdDivideAndConquerSum = 16 * 1024)
        {
            return arrayToSum.SumPairwiseInner(0, arrayToSum.Length - 1, Algorithms.Sum.SumLR, (x, y) => x + y);
        }

        public static float SumPairwise(this float[] arrayToSum, int start, int length, int thresholdDivideAndConquerSum = 16 * 1024)
        {
            return arrayToSum.SumPairwiseInner(start, start + length - 1, Algorithms.Sum.SumLR, (x, y) => x + y);
        }

        public static double SumPairwiseDbl(this float[] arrayToSum, int thresholdDivideAndConquerSum = 16 * 1024)
        {
            return arrayToSum.SumPairwiseDblInner(0, arrayToSum.Length - 1, Algorithms.Sum.SumDblLR, (x, y) => x + y);
        }

        public static double SumPairwiseDbl(this float[] arrayToSum, int start, int length, int thresholdDivideAndConquerSum = 16 * 1024)
        {
            return arrayToSum.SumPairwiseDblInner(start, start + length - 1, Algorithms.Sum.SumDblLR, (x, y) => x + y);
        }

        public static double SumPairwise(this double[] arrayToSum, int thresholdDivideAndConquerSum = 16 * 1024)
        {
            return arrayToSum.SumPairwiseInner(0, arrayToSum.Length - 1, Algorithms.Sum.SumLR, (x, y) => x + y);
        }

        public static double SumPairwise(this double[] arrayToSum, int start, int length, int thresholdDivideAndConquerSum = 16 * 1024)
        {
            return arrayToSum.SumPairwiseInner(start, start + length - 1, Algorithms.Sum.SumLR, (x, y) => x + y);
        }
    }
}
