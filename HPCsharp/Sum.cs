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
//       Sadly, this idea won't work, since we need a BigDecimal or BigFloatingPoint to capture perfect accumulation for both of these non-integer types. There are some discussions on stack overflow about BigDecimal and we may be able
//       to bring the best of these into HPCsharp, possibly as experimental at first until tested thoroughly. IEEE also has a decimal s/w implementation, as part of the floating-point standard, as a library.
// TODO: Using BigInteger will work for all integer data types: signed and unsigned. This may be faster and an interesting
//       alternative to decimal, and may possibly be faster, which would be fun to verify and provide performance data for.
//       Performance can also be improved by using the same idea of using ulong/long until it overflows. Microsoft also suggested
//       this idea on their BigInteger web page.
// TODO: Improve pair-wise .Sum() with O(e*lgN) so that it works for any size array, even small ones, whereas the current implementation which is suggested on Wikipedia favors large arrays, and does a naive summation for small arrays.
//       A possible way to do it is by using a stack structure to emulate recursion, pushing the currect "level-sum" onto this stack. This would work for SSE as well by pushing SSE-size data type onto the stack. There may be other even
//       more efficient methods.
// TODO: Blog/write about float .Sum() that uses double and Kahan/Nuemaier for increased accuracy, perfect for 500M array elements and then Neumaier takes it the rest of the way.
// TODO: Similar to above, but implement a version of Kahan/Neumaier sum for float arrays with the same exponent, which would be divide-and-conquer split up and when the chunks are smaller than 500M those array spans would
//       be summed up using double for possibly higher speed and perfect precision, until we needed to accumulate results of these array spans, when we would use Kahan/Neumaier.
// TODO: Does Kahan algorithm also make sense for decimal?
// TODO: Answer this question on stack overflow https://stackoverflow.com/questions/53075546/array-sum-results-in-an-overflow
// TODO: Implement scalar pair-wise floating-point summation for nullable arrays of floats and doubles.
// TODO: Implement nullable versions of BigIntegerFast and DecimalFast sums of long?[] and ulong?[]
// TODO: Rename all Hpc extensions to Hpcs - high performance c-sharp
// TODO: Faster implementation of decimal and BigInteger summation of long[] (singed) by checking the two conditions
//       where overflow and underflow are possible: two positives and two negatives. Only in these two conditions is it
//       possible to overflow or underflow. If sum is positive, then check input for positive, else check input for negative.
// TODO: It seems that Linq doesn't include a BigInteger.Sum() implementation. Add this to HPCsharp and improve the answer of https://stackoverflow.com/questions/41515299/how-to-calculate-the-sum-of-an-int-array-whose-result-exceeds-int32-max-value/41515465
// TODO: It may be possible to consolidate some of the .Sum() functions by adding a generic abstraction on top of groups of .Sum()'s using "where T : struct" construct followed by checking if the type is certain ones that are supported, otherwise
//       throw an error
// TODO: Contribute to https://stackoverflow.com/questions/891217/how-expensive-are-exceptions-in-c as I measured exception
//       at 8.4 microseconds each or about 120 Thousand/second
// TODO: It may be possible to create larger arrays in C#, but it's unusual https://stackoverflow.com/questions/30895549/cant-create-huge-arrays
//       It would be cool to write a blog about it and to benchmark it to compare performance.
// TODO: One way to speed-up the worst case of ulong[] and long[] summation is to create a custom 128-bit data type, which
//       would be used during all of the summation, handling overflow detection and extending summation to 128-bit integer
//       This may possibly be faster than using Decimal. It seems simple to implement an increment of the upper 64-bits of
//       the 128-bit value any time overflow of the lower 64-bit occurs. Then at the end convert the 128-bit value into Decimal or BigInteger.
//       The beauty of this algorithm is that it will perform at the same high speed for Decimal or BigInteger result!
// TODO: Continue discussion in https://github.com/dotnet/corefx/issues/17147 for support of 128-bit native integers in C#
// TODO: Figure out how https://www.geeksforgeeks.org/program-for-average-of-an-array-without-running-into-overflow/ works to compute average/mean without overflow
//       https://www.heikohoffmann.de/htmlthesis/node134.html also https://stackoverflow.com/questions/1930454/what-is-a-good-solution-for-calculating-an-average-where-the-sum-of-all-values-e
//       Do careful numerical analysis of these algorithms, as at first they seem possibly problematic and may produce results that are quite different. Plus, floating-point does not use
//       safer Kahan summation
// TODO: Implement Average computing using integer division and modulo method, as this method does not require use of BigInteger, and does not overflow, and thus does not need
//       extended precision support, and may turn out to be faster and simpler. However, this may be only good for computing average, whereas sum is useful for many other things.
//       There are no SEE instructions to accelerate it - i.e. no integer division and no integer modulo. Are there SSE double-precision floating-point division instructions?
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
        /// Summation of BigInteger array into a BigInteger sum.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumHpc(this BigInteger[] arrayToSum)
        {
            BigInteger overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of BigInteger array into a BigInteger sum.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumHpc(this BigInteger[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            BigInteger overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Faster, perfectly accurate summation of long[] array, which uses a BigInteger accumulator for perfect accuracy,
        /// and long integer summations for higher performance, handling overflow exceptions internally.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerFast(this long[] arrayToSum)
        {
            BigInteger overallSum = 0;
            long longSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
            {
                try
                {
                    longSum = checked(longSum + arrayToSum[i]);
                }
                catch (OverflowException)
                {
                    overallSum += longSum;
                    overallSum += arrayToSum[i];
                    longSum = 0;
                }
            }
            return overallSum + longSum;
        }
        /// <summary>
        /// Faster, perfectly accurate summation of long[] nullable array, which uses a BigInteger accumulator for perfect accuracy,
        /// and long integer summations for higher performance, handling overflow exceptions internally.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerFast(this long?[] arrayToSum)
        {
            BigInteger overallSum = 0;
            long longSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
            {
                try
                {
                    if (arrayToSum[i] != null)
                        longSum = checked(longSum + (long)arrayToSum[i]);
                }
                catch (OverflowException)
                {
                    overallSum += longSum;
                    overallSum += (long)arrayToSum[i];
                    longSum = 0;
                }
            }
            return overallSum + longSum;
        }

        /// <summary>
        /// Faster, perfectly accurate summation of ulong[] array, which uses a BigInteger accumulator for perfect accuracy,
        /// and long integer summations for higher performance, handling overflow exceptions internally.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerFast(this ulong[] arrayToSum)
        {
            BigInteger overallSum = 0;
            ulong ulongSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
            {
                try
                {
                    ulongSum = checked(ulongSum + arrayToSum[i]);
                }
                catch (OverflowException)
                {
                    overallSum += ulongSum;
                    overallSum += arrayToSum[i];
                    ulongSum = 0;
                }
            }
            return overallSum + ulongSum;
        }

        /// <summary>
        /// Even faster, perfectly accurate summation of ulong[] array, which uses a BigInteger accumulator for perfect accuracy,
        /// and integer summations for higher performance, detecting overflow condition without exceptions.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalEvenFaster(this ulong[] arrayToSum)
        {
            decimal overallSum = 0;
            ulong ulongSum = 0;
            uint uintUpperSum = 0;      // together uintUpperSum and ulongSum represent a 96-bit uint
            for (int i = 0; i < arrayToSum.Length; i++)
            {
                ulong newUlongSum = ulongSum + arrayToSum[i];
                if (newUlongSum < ulongSum)
                    uintUpperSum++;            // carry-out of lower 64-bit into carry-in of upper 32-bits
                ulongSum = newUlongSum;
            }
            decimal multiplier = 0x8000_0000_0000_0000;
            overallSum = multiplier * (Decimal)2 * (Decimal)uintUpperSum;   // uintUpperSum << 64
            overallSum += ulongSum;
            
            return overallSum;
        }

        /// <summary>
        /// Faster, perfectly accurate summation of ulong[] array, which uses a Decimal accumulator for perfect accuracy,
        /// and integer summations for higher performance, detecting overflow condition without exceptions.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalFaster(this ulong[] arrayToSum)
        {
            decimal overallSum = 0;
            ulong ulongSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
            {
                ulong newUlongSum = ulongSum + arrayToSum[i];
                if (newUlongSum >= ulongSum)
                    ulongSum = newUlongSum;     // no numeric overflow, as the new unsigned sum increased
                else
                {
                    overallSum += ulongSum;
                    overallSum += arrayToSum[i];
                    ulongSum = 0;
                }
            }
            return overallSum + ulongSum;
        }

        /// <summary>
        /// Faster, perfectly accurate summation of ulong[] array, which uses a Decimal accumulator for perfect accuracy,
        /// and integer summations for higher performance, detecting overflow condition without exceptions.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalFaster(this ulong[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            decimal overallSum = 0;
            ulong ulongSum = 0;
            for (int i = startIndex; i < endIndex; i++)
            {
                ulong newUlongSum = ulongSum + arrayToSum[i];
                if (newUlongSum >= ulongSum)
                    ulongSum = newUlongSum;     // no numeric overflow, as the new unsigned sum increased
                else
                {
                    overallSum += ulongSum;
                    overallSum += arrayToSum[i];
                    ulongSum = 0;
                }
            }
            return overallSum + ulongSum;
        }

        /// <summary>
        /// Faster, perfectly accurate summation of ulong[] array, which uses a BigInteger accumulator for perfect accuracy,
        /// and integer summations for higher performance, detecting overflow condition without exceptions.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerFaster(this ulong[] arrayToSum)
        {
            BigInteger overallSum = 0;
            ulong ulongSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
            {
                ulong newUlongSum = ulongSum + arrayToSum[i];
                if (newUlongSum >= ulongSum)
                    ulongSum = newUlongSum;     // no numeric overflow, as the new unsigned sum increased
                else
                {
                    overallSum += ulongSum;
                    overallSum += arrayToSum[i];
                    ulongSum = 0;
                }
            }
            return overallSum + ulongSum;
        }

        /// <summary>
        /// Faster, perfectly accurate summation of ulong[] array, which uses a BigInteger accumulator for perfect accuracy,
        /// and integer summations for higher performance, detecting overflow condition without exceptions.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerFaster(this ulong[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            BigInteger overallSum = 0;
            ulong ulongSum = 0;
            for (int i = startIndex; i < endIndex; i++)
            {
                ulong newUlongSum = ulongSum + arrayToSum[i];
                if (newUlongSum >= ulongSum)
                    ulongSum = newUlongSum;     // no numeric overflow, as the new unsigned sum increased
                else
                {
                    overallSum += ulongSum;
                    overallSum += arrayToSum[i];
                    ulongSum = 0;
                }
            }
            return overallSum + ulongSum;
        }

        /// <summary>
        /// Faster, perfectly accurate summation of ulong?[] nullable array, which uses a BigInteger accumulator for perfect accuracy,
        /// and long integer summations for higher performance, handling overflow exceptions internally.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerFast(this ulong?[] arrayToSum)
        {
            BigInteger overallSum = 0;
            ulong ulongSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
            {
                try
                {
                    if (arrayToSum[i] != null)
                        ulongSum = checked(ulongSum + (ulong)arrayToSum[i]);
                }
                catch (OverflowException)
                {
                    overallSum += ulongSum;
                    overallSum += (ulong)arrayToSum[i];
                    ulongSum = 0;
                }
            }
            return overallSum + ulongSum;
        }

        /// <summary>
        /// Faster, perfectly accurate summation of long[] array, which uses a BigInteger accumulator for perfect accuracy,
        /// and integer summations for higher performance, detecting overflow condition without exceptions.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerFaster(this long[] arrayToSum)
        {
            return arrayToSum.SumToBigIntegerFaster(0, arrayToSum.Length);
        }

        /// <summary>
        /// Faster, perfectly accurate summation of long[] array, which uses a BigInteger accumulator for perfect accuracy,
        /// and integer summations for higher performance, detecting overflow condition without exceptions.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigIntegerFaster(this long[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            BigInteger overallSum = 0;
            long longSum = 0;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (longSum >= 0)
                {
                    if (arrayToSum[i] >= 0)
                    {
                        long newLongSum = longSum + arrayToSum[i];
                        if (newLongSum >= longSum)
                            longSum = newLongSum;     // no numeric overflow, as the new positive sum increased
                        else
                        {
                            overallSum += longSum;
                            overallSum += arrayToSum[i];
                            longSum = 0;
                        }
                    }
                    else
                    {
                        longSum += arrayToSum[i];
                    }
                }
                else if (arrayToSum[i] < 0)
                {
                    long newLongSum = longSum + arrayToSum[i];
                    if (newLongSum <= longSum)
                        longSum = newLongSum;     // no numeric underflow, as the new negative sum decreased
                    else
                    {
                        overallSum += longSum;
                        overallSum += arrayToSum[i];
                        longSum = 0;
                    }
                }
                else
                {
                    longSum += arrayToSum[i];
                }
            }
            return overallSum + longSum;
        }

        /// <summary>
        /// Summation of long[] array, which uses a BigInteger accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigInteger(this long[] arrayToSum)
        {
            return arrayToSum.SumToBigInteger(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of long[] array, which uses a BigInteger accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigInteger(this long[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            BigInteger overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Faster, perfectly accurate summation of long[] array, which uses a decimal accumulator for perfect accuracy,
        /// and integer summations for higher performance, handling overflow exceptions internally.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalFast(this long[] arrayToSum)
        {
            decimal overallSum = 0;
            long longSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
            {
                try
                {
                    longSum = checked(longSum + arrayToSum[i]);
                }
                catch (OverflowException)
                {
                    overallSum += longSum;
                    overallSum += arrayToSum[i];
                    longSum = 0;
                }
            }
            return overallSum + longSum;
        }

        /// <summary>
        /// Faster, perfectly accurate summation of long[] array, which uses a BigInteger accumulator for perfect accuracy,
        /// and integer summations for higher performance, detecting overflow condition without exceptions.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalFaster(this long[] arrayToSum)
        {
            return arrayToSum.SumToDecimalFaster(0, arrayToSum.Length);
        }

        /// <summary>
        /// Faster, perfectly accurate summation of long[] array, which uses a BigInteger accumulator for perfect accuracy,
        /// and integer summations for higher performance, detecting overflow condition without exceptions.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalFaster(this long[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            decimal overallSum = 0;
            long longSum = 0;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (longSum >= 0)
                {
                    if (arrayToSum[i] >= 0)
                    {
                        long newLongSum = longSum + arrayToSum[i];
                        if (newLongSum >= longSum)
                            longSum = newLongSum;     // no numeric overflow, as the new positive sum increased
                        else
                        {
                            overallSum += longSum;
                            overallSum += arrayToSum[i];
                            longSum = 0;
                        }
                    }
                    else
                    {
                        longSum += arrayToSum[i];
                    }
                }
                else if (arrayToSum[i] < 0)
                {
                    long newLongSum = longSum + arrayToSum[i];
                    if (newLongSum <= longSum)
                        longSum = newLongSum;     // no numeric underflow, as the new negative sum decreased
                    else
                    {
                        overallSum += longSum;
                        overallSum += arrayToSum[i];
                        longSum = 0;
                    }
                }
                else
                {
                    longSum += arrayToSum[i];
                }
            }
            return overallSum + longSum;
        }

        /// <summary>
        /// Faster, perfectly accurate summation of ulong[] array, which uses a decimal accumulator for perfect accuracy,
        /// and integer summations for higher performance, handling overflow exceptions internally.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimalFast(this ulong[] arrayToSum)
        {
            decimal overallSum = 0;
            ulong ulongSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
            {
                try
                {
                    ulongSum = checked(ulongSum + arrayToSum[i]);
                }
                catch (OverflowException)
                {
                    overallSum += ulongSum;
                    overallSum += arrayToSum[i];
                    ulongSum = 0;
                }
            }
            return overallSum + ulongSum;
        }

        /// <summary>
        /// Slower, perfectly accurate summation of long[] array, which uses a decimal accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimal(this long[] arrayToSum)
        {
            return arrayToSum.SumToDecimal(0, arrayToSum.Length);
        }

        /// <summary>
        /// Slower, perfectly accurate summation of long[] array, which uses a decimal accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimal(this long[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            decimal overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Slower, perfectly accurate summation of long[] array, which uses a decimal accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal summation value</returns>
        public static decimal SumToDecimal(this long?[] arrayToSum)
        {
            return arrayToSum.SumToDecimal(0, arrayToSum.Length);
        }

        /// <summary>
        /// Slower, perfectly accurate summation of long[] nullable array, which uses a decimal accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimal(this long?[] arrayToSum, int startIndex, int length)
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
        /// <exception>TSource:System.OverflowException: when the sum value is greater than Int64.MaxValue</exception>
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
            {
                overallSum = checked(overallSum + arrayToSum[i]);
            }
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
                {
                    overallSum = checked(overallSum + (long)arrayToSum[i]);
                }
            return overallSum;
        }

        /// <summary>
        /// Summation of int[] array, which uses a long accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLong(this int[] arrayToSum)
        {
            return arrayToSum.SumToLong(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of int[] array, which uses a long accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLong(this int[] arrayToSum, int startIndex, int length)
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
        public static long SumToLong(this int?[] arrayToSum)
        {
            return arrayToSum.SumToLong(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of int[] nullable array, which uses a long accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLong(this int?[] arrayToSum, int startIndex, int length)
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
        public static long SumToLong(this short[] arrayToSum)
        {
            return arrayToSum.SumToLong(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of short[] array, which uses a long accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLong(this short[] arrayToSum, int startIndex, int length)
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
        public static long SumToLong(this short?[] arrayToSum)
        {
            return arrayToSum.SumToLong(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of short[] nullable array, which uses a long accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLong(this short?[] arrayToSum, int startIndex, int length)
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
        public static long SumToLong(this sbyte[] arrayToSum)
        {
            return arrayToSum.SumToLong(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of sbyte[] array, which uses a long accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLong(this sbyte[] arrayToSum, int startIndex, int length)
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
        public static long SumToLong(this sbyte?[] arrayToSum)
        {
            return arrayToSum.SumToLong(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of sbyte[] nullable array, which uses a long accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static long SumToLong(this sbyte?[] arrayToSum, int startIndex, int length)
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
        public static decimal SumToDecimal(this ulong[] arrayToSum)
        {
            return arrayToSum.SumToDecimal(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of ulong[] array, which uses a decimal accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimal(this ulong[] arrayToSum, int startIndex, int length)
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
        public static decimal SumToDecimal(this ulong?[] arrayToSum)
        {
            return arrayToSum.SumToDecimal(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of ulong[] nullable array, which uses a decimal accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumToDecimal(this ulong?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            decimal overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (ulong)arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of ulong[] array, which uses a BigInteger accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigInteger(this ulong[] arrayToSum)
        {
            return arrayToSum.SumToBigInteger(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of ulong[] array, which uses a BigInteger accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigInteger(this ulong[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            BigInteger overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of ulong[] nullable array, which uses a BigInteger accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static BigInteger SumToBigInteger(this ulong?[] arrayToSum)
        {
            return arrayToSum.SumToBigInteger(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of ulong[] nullable array, which uses a BigInteger accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>BigInteger sum</returns>
        public static BigInteger SumToBigInteger(this ulong?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            BigInteger overallSum = 0;
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
            {
                checked
                {
                    overallSum += arrayToSum[i];
                }
            }
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
                {
                    checked
                    {
                        overallSum += (ulong)arrayToSum[i];
                    }
                }
            return overallSum;
        }

        /// <summary>
        /// Summation of uint[] array, which uses a ulong accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlong(this uint[] arrayToSum)
        {
            return arrayToSum.SumUlong(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of uint[] array, which uses a ulong accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumUlong(this uint[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            ulong overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of uint[] nullable array, which uses a ulong accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumUlong(this uint?[] arrayToSum)
        {
            return arrayToSum.SumToUlong(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of uint[] nullable array, which uses a ulong accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlong(this uint?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            ulong overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (uint)arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of ushort[] array, which uses a ulong accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static ulong SumToUlong(this ushort[] arrayToSum)
        {
            return arrayToSum.SumToUlong(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of ushort[] nullable array, which uses a ulong accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlong(this ushort[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            ulong overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of ushort[] nullable array, which uses a ulong accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlong(this ushort?[] arrayToSum)
        {
            return arrayToSum.SumToUlong(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of ushort[] nullable array, which uses a ulong accumulator for perfect accuracy.
        /// Null values are skipped. Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlong(this ushort?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            ulong overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (ushort)arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of byte[] array, which uses a ulong accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static ulong SumToUlong(this byte[] arrayToSum)
        {
            return arrayToSum.SumToUlong(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of byte[] array, which uses a ulong accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlong(this byte[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            ulong overallSum = 0;
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
        public static ulong SumToUlong(this byte?[] arrayToSum)
        {
            return arrayToSum.SumToUlong(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of byte[] nullable array, which uses a ulong accumulator for perfect accuracy.
        /// Will not throw overflow exception.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>ulong sum</returns>
        public static ulong SumToUlong(this byte?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            ulong overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (byte)arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of float[] array.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>float sum</returns>
        public static float SumHpc(this float[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of float[] array.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>float sum</returns>
        public static float SumHpc(this float[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            float overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of float[] nullable array. Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
       /// <returns>float sum</returns>
        public static float SumHpc(this float?[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of float[] nullable array. Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>float sum</returns>
        public static float SumHpc(this float?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            float overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (float)arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of float[] array, which uses a double accumulator for higher accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDouble(this float[] arrayToSum)
        {
            return arrayToSum.SumToDouble(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of float[] array, which uses a double accumulator for higher accuracy.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDouble(this float[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            double overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of float[] nullable array, which uses a double accumulator for higher accuracy.
        /// Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDouble(this float?[] arrayToSum)
        {
            return arrayToSum.SumToDouble(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of float[] nullable array, which uses a double accumulator for higher accuracy.
        /// Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        public static double SumToDouble(this float?[] arrayToSum, int startIndex, int length)
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

        internal static double SumToDoubleLR(this float[] arrayToSum, int l, int r)
        {
            double overallSum = 0;
            for (int i = l; i <= r; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of double[] array.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumHpc(this double[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of double[] array.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumHpc(this double[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            double overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of double[] nullable array. Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>long sum</returns>
        public static double SumHpc(this double?[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of double[] nullable array. Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
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

        /// <summary>
        /// Summation of decimal[] array.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumHpc(this decimal[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of decimal[] array.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>decimal sum</returns>
        internal static decimal SumHpc(this decimal[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            decimal overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Summation of decimal[] nullable array. Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>decimal sum</returns>
        public static decimal SumHpc(this decimal?[] arrayToSum)
        {
            return arrayToSum.SumHpc(0, arrayToSum.Length);
        }

        /// <summary>
        /// Summation of decimal[] nullable array. Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>long sum</returns>
        internal static decimal SumHpc(this decimal?[] arrayToSum, int startIndex, int length)
        {
            int endIndex = startIndex + length;
            decimal overallSum = 0;
            for (int i = startIndex; i < endIndex; i++)
                if (arrayToSum[i] != null)
                    overallSum += (decimal)arrayToSum[i];
            return overallSum;
        }

        /// <summary>
        /// Implementation https://en.wikipedia.org/wiki/Kahan_summation_algorithm
        /// Summation of float[] array, using a more accurate Kahan summation algorithm.
        /// Converts input values into double and uses double for accumulation and compensation.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        private static double SumToDoubleKahanInner(this float[] arrayToSum)
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

        /// <summary>
        /// Implementation https://en.wikipedia.org/wiki/Kahan_summation_algorithm
        /// Summation of double[] array, using a more accurate Kahan summation algorithm.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        private static double SumKahan1(this double[] arrayToSum)
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

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan more accurate floating-point summation, for two values provided.
        /// </summary>
        /// <param name="firstValue">first value to sum up</param>
        /// <param name="secondValue">second value to sum up</param>
        /// <returns>float sum</returns>
        public static float SumMostAccurate(float firstValue, float secondValue)
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

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan more accurate floating-point summation, for two values provided.
        /// Both arguments are first converted to double for higher precision result.
        /// </summary>
        /// <param name="firstValue">first value to sum up</param>
        /// <param name="secondValue">second value to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleMostAccurate(float firstValue, float secondValue)
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

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation.
        /// Summation of float[] array, using a more accurate Kahan summation algorithm.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>float sum</returns>
        public static float SumMostAccurate(this float[] arrayToSum)
        {
            return arrayToSum.SumMostAccurate(0, arrayToSum.Length);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation.
        /// Summation of float[] array, using a more accurate Kahan summation algorithm.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>float sum</returns>
        public static float SumMostAccurate(this float[] arrayToSum, int startIndex, int length)
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

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation.
        /// Summation of float[] nullable array, using a more accurate Kahan summation algorithm.
        /// Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>float sum</returns>
        public static float SumMostAccurate(this float?[] arrayToSum)
        {
            return arrayToSum.SumMostAccurate(0, arrayToSum.Length);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation.
        /// Summation of float[] nullable array, using a more accurate Kahan summation algorithm.
        /// Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>float sum</returns>
        public static float SumMostAccurate(this float?[] arrayToSum, int startIndex, int length)
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

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation.
        /// Summation of float[] array, using a more accurate Kahan summation algorithm.
        /// Input array elements are converted to double for additional accuracy for all internal computations.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleMostAccurate(this float[] arrayToSum)
        {
            return arrayToSum.SumToDoubleMostAccurate(0, arrayToSum.Length);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation.
        /// Summation of float[] array, using a more accurate Kahan summation algorithm.
        /// Input array elements are converted to double for additional accuracy for all internal computations.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleMostAccurate(this float[] arrayToSum, int startIndex, int length)
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

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation.
        /// Summation of float[] nullable array, using a more accurate Kahan summation algorithm.
        /// Input array elements are converted to double for additional accuracy for all internal computations.
        /// Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleMostAccurate(this float?[] arrayToSum)
        {
            return arrayToSum.SumMostAccurate(0, arrayToSum.Length);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation.
        /// Summation of float[] nullable array, using a more accurate Kahan summation algorithm.
        /// Input array elements are converted to double for additional accuracy for all internal computations.
        /// Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleMostAccurate(this float?[] arrayToSum, int startIndex, int length)
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

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan more accurate floating-point summation, for two values provided.
        /// </summary>
        /// <param name="firstValue">first value to sum up</param>
        /// <param name="secondValue">second value to sum up</param>
        /// <returns>double sum</returns>
        public static double SumMostAccurate(double firstValue, double secondValue)
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
        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation.
        /// Summation of double[] array, using a more accurate Kahan summation algorithm.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumMostAccurate(this double[] arrayToSum)
        {
            return arrayToSum.SumMostAccurate(0, arrayToSum.Length);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation.
        /// Summation of double[] array, using a more accurate Kahan summation algorithm.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumMostAccurate(this double[] arrayToSum, int startIndex, int length)
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

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation.
        /// Summation of double[] nullable array, using a more accurate Kahan summation algorithm.
        /// Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumMostAccurate(this double?[] arrayToSum)
        {
            return arrayToSum.SumMostAccurate(0, arrayToSum.Length);
        }

        /// <summary>
        /// Implementation of the Neumaier variation of Kahan floating-point summation.
        /// Summation of double[] nullable array, using a more accurate Kahan summation algorithm.
        /// Null values are skipped.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumMostAccurate(this double?[] arrayToSum, int startIndex, int length)
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

        /// <summary>
        /// Summation of float[] array, using a more accurate pair-wise summation algorithm.
        /// Performs less work than Kahan summation, while providing more accuracy than a for loop summation.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>float sum</returns>
        public static float SumMoreAccurate(this float[] arrayToSum, int thresholdDivideAndConquerSum = 16 * 1024)
        {
            return arrayToSum.SumPairwiseInner(0, arrayToSum.Length - 1, Algorithms.Sum.SumLR, (x, y) => x + y);
        }

        /// <summary>
        /// Summation of float[] array, using a more accurate pair-wise summation algorithm.
        /// Performs less work than Kahan summation, while providing more accuracy than a for loop summation.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>float sum</returns>
        public static float SumMoreAccurate(this float[] arrayToSum, int startIndex, int length, int thresholdDivideAndConquerSum = 16 * 1024)
        {
            return arrayToSum.SumPairwiseInner(startIndex, startIndex + length - 1, Algorithms.Sum.SumLR, (x, y) => x + y);
        }

        /// <summary>
        /// Summation of float[] array, using a more accurate pair-wise summation algorithm.
        /// Performs less work than Kahan summation, while providing more accuracy than a for loop summation.
        /// Input array elements are converted to double for additional accuracy for all internal computations.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleMoreAccurate(this float[] arrayToSum, int thresholdDivideAndConquerSum = 16 * 1024)
        {
            return arrayToSum.SumPairwiseDblInner(0, arrayToSum.Length - 1, Algorithms.Sum.SumToDoubleLR, (x, y) => x + y);
        }

        /// <summary>
        /// Summation of float[] array, using a more accurate pair-wise summation algorithm.
        /// Performs less work than Kahan summation, while providing more accuracy than a for loop summation.
        /// Input array elements are converted to double for additional accuracy for all internal computations.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumToDoubleMoreAccurate(this float[] arrayToSum, int startIndex, int length, int thresholdDivideAndConquerSum = 16 * 1024)
        {
            return arrayToSum.SumPairwiseDblInner(startIndex, startIndex + length - 1, Algorithms.Sum.SumToDoubleLR, (x, y) => x + y);
        }

        /// <summary>
        /// Summation of double[] array, using a more accurate pair-wise summation algorithm.
        /// Performs less work than Kahan summation, while providing more accuracy than a for loop summation.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <returns>double sum</returns>
        public static double SumMoreAccurate(this double[] arrayToSum, int thresholdDivideAndConquerSum = 16 * 1024)
        {
            return arrayToSum.SumPairwiseInner(0, arrayToSum.Length - 1, Algorithms.Sum.SumLR, (x, y) => x + y);
        }

        /// <summary>
        /// Summation of double[] array, using a more accurate pair-wise summation algorithm.
        /// Performs less work than Kahan summation, while providing more accuracy than a for loop summation.
        /// </summary>
        /// <param name="arrayToSum">An array to sum up</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        /// <returns>double sum</returns>
        public static double SumMoreAccurate(this double[] arrayToSum, int startIndex, int length, int thresholdDivideAndConquerSum = 16 * 1024)
        {
            return arrayToSum.SumPairwiseInner(startIndex, startIndex + length - 1, Algorithms.Sum.SumLR, (x, y) => x + y);
        }
    }
}
