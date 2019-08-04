using System;
using System.Linq;
using System.Numerics;
using HPCsharp.Algorithms;
using HPCsharp.ParallelAlgorithms;
using NUnit.Framework;

namespace HPCSharp.UnitTests
{
    [TestFixture]
    public sealed class SumTests
    {
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void ShouldThrowOverflowExceptionLong(int whichTestCase)
        {
            long[] arrLong  = new long[] { 5, 7, 16, 3, Int64.MaxValue, 1 };

            if (whichTestCase == 0)
            {
                Assert.Throws<OverflowException>(() => arrLong.SumHpc());
            }
            else if (whichTestCase == 1)
            {
                Assert.Throws<OverflowException>(() => arrLong.SumSse());
            }
            else if (whichTestCase == 2)
            {
                ulong[] arrLong1 = new ulong[] { 5, 7, 16, 4, 2, 0, UInt64.MaxValue, 3, 1 };
                Assert.Throws<OverflowException>(() => arrLong1.SumCheckedSse());
            }
        }
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void ShouldNotThrowOverflowExceptionLong(int whichTestCase)
        {
            if (whichTestCase == 1)
            {
                // This test demonstrates that SSE doesn't throw overflow exception
                long[] arrLong1 = new long[] { 5, 7, 16, 4, 2, 0, Int64.MaxValue, 3, 1 };
                Assert.DoesNotThrow(() => arrLong1.SumSse());
            }
            else if (whichTestCase == 2)
            {
                ulong[] arrLong1 = new ulong[] { 5, 7, 16, 4, 2, 0, UInt64.MaxValue, 3, 1 };
                Assert.Throws<OverflowException>(() => arrLong1.SumCheckedSse());
            }
        }
        [Test]
        [TestCase(Int64.MaxValue, 1)]
        [TestCase(Int64.MinValue, -1L)]
        [TestCase(Int64.MaxValue, 0)]
        [TestCase(Int64.MinValue, 0)]
        [TestCase(Int64.MinValue, Int64.MaxValue)]
        [TestCase(Int64.MaxValue, Int64.MaxValue)]
        [TestCase(Int64.MinValue, Int64.MinValue)]
        public void CorrectnessOfSumOfLongArrayToBigIntegerAndDecimalFaster(long input0, long input1)
        {
            long[] arrLong = new long[] { input0, input1 };

            BigInteger resultBigInteger = new BigInteger(input0) + new BigInteger(input1);
            Assert.AreEqual(resultBigInteger, arrLong.SumToBigIntegerFaster());

            Decimal resultDecimal = new Decimal(input0) + new Decimal(input1);
            Assert.AreEqual(resultDecimal, arrLong.SumToDecimalFaster());
        }
        [Test]
        [TestCase(UInt64.MaxValue, 1UL)]
        [TestCase(UInt64.MaxValue, 0UL)]
        public void CorrectnessOfSumOfULongArrayToBigIntegerAndDecimalFaster(ulong input0, ulong input1)
        {
            ulong[] arrLong = new ulong[] { input0, input1 };

            BigInteger resultBigInteger = new BigInteger(input0) + new BigInteger(input1);
            Assert.AreEqual(resultBigInteger, arrLong.SumToBigIntegerFaster());

            Decimal resultDecimal = new Decimal(input0) + new Decimal(input1);
            Assert.AreEqual(resultDecimal, arrLong.SumToDecimalFaster());
        }
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void ShouldThrowOverflowExceptionLongSse(int whichTestCase)
        {
            // Throw an exception in SIMD/SSE code
            if (whichTestCase == 0)
            {
                // Throw an exception in SIMD/SSE code
                long[] arrLong = new long[] { Int64.MaxValue, 5, 7, 16, 4, 2, 8, 3 };
                Assert.Throws<OverflowException>(() => arrLong.SumCheckedSse());
                arrLong = new long[] { 5, Int64.MaxValue, 7, 16, 4, 2, 8, 3 };
                Assert.Throws<OverflowException>(() => arrLong.SumCheckedSse());
                arrLong = new long[] { 5, 7, Int64.MaxValue, 16, 4, 2, 8, 3};
                Assert.Throws<OverflowException>(() => arrLong.SumCheckedSse());
                arrLong = new long[] { 5, 7, 16, Int64.MaxValue, 4, 2, 8, 3 };
                Assert.Throws<OverflowException>(() => arrLong.SumCheckedSse());
            }
            else if (whichTestCase == 1)
            {
                // Throw exception in scalar code following SIMD/SSE code
                long[] arrLong = new long[] { 5, 7, 16, 4, 2, 8, 3, 1, Int64.MaxValue, 1 };
                Assert.Throws<OverflowException>(() => arrLong.SumCheckedSse());
                arrLong = new long[] { 5, 7, 16, 0, 2, 8, 3, Int64.MaxValue, 1 };
                Assert.Throws<OverflowException>(() => arrLong.SumCheckedSse());
            }
        }
        [Test]
        [TestCase(0)]
        public void CorrectnessOfLongCheckedSseSum(int whichTestCase)
        {
            if (whichTestCase == 0)
            {
                var arrLong = new long[] { 5, 7, 16, 2, 4, 2, 3, 0, 3 };
                Assert.DoesNotThrow(() => arrLong.SumCheckedSse());
                Assert.AreEqual(42, arrLong.SumCheckedSse());
            }
        }
        [Test]
        [TestCase(0)]
        public void CorrectnessOfBigIntegerSum(int whichTestCase)
        {
            if (whichTestCase == 0)
            {
                var arrLong = new long[] { 5, 7, 16, Int64.MaxValue, 4, 2, 3, 0, 3 };
                Assert.DoesNotThrow(() => arrLong.SumToBigIntegerFaster());
                var result = new BigInteger();
                result = arrLong.Aggregate(result, (current, i) => current + i);
                Assert.AreEqual(arrLong.SumToBigIntegerFaster(), result);
            }
        }
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public void OverflowOfAddition(int whichTestCase)
        {
            if (whichTestCase == 0)
            {
                uint valueUint = 4294967295;   // UInt32.MaxValue, 0xFFFFFFFF
                Assert.DoesNotThrow(() => valueUint += 1);
                Assert.AreEqual(valueUint, 0);

                ulong valueUlong = 18446744073709551615;   // UInt64.MaxValue, 0xFFFFFFFFFFFFFFFF
                Assert.DoesNotThrow(() => valueUlong += 1);
                Assert.AreEqual(valueUlong, 0);

                int valueInt = 2147483647;   // Int32.MaxValue, 0x7FFFFFFF
                Assert.DoesNotThrow(() => valueInt += 1);
                Assert.AreEqual(valueInt, Int32.MinValue);

                long valueLong = 9223372036854775807;   // Int64.MaxValue, 0x7FFFFFFFFFFFFFFF
                Assert.DoesNotThrow(() => valueLong += 1);
                Assert.AreEqual(valueLong, Int64.MinValue);

                valueInt = -2147483648;   // Int32.MinValue, 0xFFFFFFFF
                Assert.DoesNotThrow(() => valueInt -= 1);
                Assert.AreEqual(valueInt, Int32.MaxValue);

                valueLong = -9223372036854775808;   // Int64.MinValue, 0xFFFFFFFFFFFFFFFF
                Assert.DoesNotThrow(() => valueLong -= 1);
                Assert.AreEqual(valueLong, Int64.MaxValue);
            }
            else if (whichTestCase == 1)
            {
                uint valueUint = 4294967295;   // UInt32.MaxValue, 0xFFFFFFFF
                Assert.Throws<OverflowException>(() => valueUint = checked(valueUint + 1));

                ulong valueUlong = 18446744073709551615;   // UInt64.MaxValue, 0xFFFFFFFFFFFFFFFF
                Assert.Throws<OverflowException>(() => valueUlong = checked(valueUlong + 1));

                int valueInt = 2147483647;   // Int32.MaxValue, 0x7FFFFFFF
                Assert.Throws<OverflowException>(() => valueInt = checked(valueInt + 1));

                long valueLong = 9223372036854775807;   // Int64.MaxValue, 0x7FFFFFFFFFFFFFFF
                Assert.Throws<OverflowException>(() => valueLong = checked(valueLong + 1));

                valueInt = -2147483648;   // Int32.MinValue, 0xFFFFFFFF
                Assert.Throws<OverflowException>(() => valueInt = checked(valueInt - 1));

                valueLong = -9223372036854775808;   // Int64.MinValue, 0xFFFFFFFFFFFFFFFF
                Assert.Throws<OverflowException>(() => valueLong = checked(valueLong - 1));
            }
        }
    }
}
