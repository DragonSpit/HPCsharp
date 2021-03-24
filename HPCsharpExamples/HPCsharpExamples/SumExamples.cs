using HPCsharp.Algorithms;
using HPCsharp.ParallelAlgorithms;
using System;
using System.Linq;
using System.Numerics;


namespace HPCsharpExamples
{
    class SumExamples
    {
        public static void SumProblems()
        {
            int[] arr = new int[] { 5, 7, 16, 3 };

            int sum = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                sum += arr[i];
            }
            Console.WriteLine("First loop sum = " + sum);

            Console.WriteLine("First .Sum() = " + arr.Sum());

            arr = new int[] { Int32.MaxValue, 1 };

            //sum = 0;
            //for (int i = 0; i < arr.Length; i++)
            //{
            //    sum += arr[i];
            //}
            //Console.WriteLine("Second loop sum = " + sum);

            //Console.WriteLine("Second .Sum() = " + arr.Sum(x => (long)x));
            //Console.WriteLine("Second .Sum() = " + arr.Sum());

            //long longSum = 0;
            //for (int i = 0; i < arr.Length; i++)
            //{
            //    longSum += arr[i];
            //}
            //Console.WriteLine("Second loop sum = " + longSum);
        }

        public static void SumHPCsharpExamples()
        {
            int[] arrInt = new int[] { 5, 7, 16, 3, Int32.MaxValue, 1 };
            int sumInt;
            long sumLong;

            //sumInt = arrInt.Sum();                  // standard C# usage, single-core, will throw overflow exception
            //sumInt = arrInt.AsParallel().Sum();     // standard C# usage, multi-core,  will throw overflow exception

            // No overflow exception thrown by HPCsharp .Sum(), producing a perfectly accurate sum
            sumLong = arrInt.SumToLong();        // serial
            sumLong = arrInt.SumToLongSse();     // data-parallel, single-core (many times faster)
            sumLong = arrInt.SumToLongSsePar();  // data-parallel,  multi-core (even faster)


            long[] arrLong = new long[] { 5, 7, 16, 3, Int64.MaxValue, 1 };
            decimal sumDecimal;
            BigInteger sumBigInteger = 0;

            //sumLong = arrLong.Sum();                  // standard C# usage,   single-core, throws an overflow exception
            //sumLong = arrLong.AsParallel().Sum();     // standard C# usage,    multi-core, throws an overflow exception
            //sumLong = arrLong.SumCheckedSse();        // data parallel (SSE), single-core, throws an overflow exception

            // No overflow exception thrown by HPCsharp .Sum(), producing a perfectly accurate sum
            sumDecimal = arrLong.SumToDecimal();               // scalar, single-core
            sumDecimal = arrLong.SumToDecimalPar();            // scalar,  multi-core

            sumDecimal = arrLong.SumToDecimalFast();           // scalar, single-core, faster when a few overflow exceptions occur
            sumDecimal = arrLong.SumToDecimalFaster();         // scalar, single-core, much faster when lots of overflows can occur
            sumDecimal = arrLong.SumToDecimalSseFaster();      // data parallel (SSE), single-core, much faster when lots of overflows can occur
            sumDecimal = arrLong.SumToDecimalFasterPar();      // scalar,               multi-core, much faster when lots of overflows can occur
            sumDecimal = arrLong.SumToDecimalSseFasterPar();   // data parallel (SSE),  multi-core, much faster when lots of overflows can occur

            // No overflow exception thrown by HPCsharp .Sum(), producing a perfectly accurate sum
            sumBigInteger = arrLong.SumToBigIntegerFast();           // scalar, single-core
            sumBigInteger = arrLong.SumToBigIntegerFaster();         // scalar, single-core (much faster when lots of overflows can occur)
            sumBigInteger = arrLong.SumToBigIntegerSseFaster();      // data parallel (SSE), single-core, much faster when lots of overflows can occur
            //sumBigInteger = arrLong.SumToBigIntegerFasterPar();      // scalar,               multi-core, much faster when lots of overflows can occur
            //sumBigInteger = arrLong.SumToBigIntegerSseFasterPar();   // data parallel (SSE),  multi-core, much faster when lots of overflows can occur


            float[] arrFloat = new float[] { 5.0f, 7.3f, 16.3f, 3.1f };
            float sumFloat;
            double sumDouble;

            sumFloat = arrFloat.Sum();         // standard C# usage

            sumDouble = arrFloat.SumHpc();     // serial
            sumDouble = arrFloat.SumSse();     // data-parallel (SSE), single-core
            sumDouble = arrFloat.SumSsePar();  // data-parallel (SSE),  multi-core (Par)

            // More accurate floati-point .Sum()
            double[] arrDouble = new double[] { 1, 10.0e100, 1, -10e100 };
            
            sumDouble                = arrDouble.Sum();                     // standard C#
            var sumDoublePar         = arrDouble.AsParallel().Sum();        // standard C#, multi-core
            var sumDoubleKahan       = arrDouble.SumMostAccurate();         // HPCsharp more accurate, serial
            var sumDoubleKahanSse    = arrDouble.SumSseMostAccurate();      // HPCsharp more accurate, data-parallel (SSE), single-core
            var sumDoubleKahanSsePar = arrDouble.SumSseParMostAccurate();   // HPCsharp more accurate, data-parallel (SSE),  multi-core

            Console.WriteLine("Sum Naive (for loop)         = {0}, correct answer is 2.0", sumDouble);
            Console.WriteLine("Sum Kahan                    = {0}, correct answer is 2.0", sumDoubleKahan);
            Console.WriteLine("Sum Kahan (SSE)              = {0}, correct answer is 2.0", sumDoubleKahanSse);
            Console.WriteLine("Sum Kahan (SSE & multi-core) = {0}, correct answer is 2.0", sumDoubleKahanSse);


            ulong[] arrUlong = new ulong[] { 5, 7, 16, 3, UInt64.MaxValue, 1 };
            long sumUlong;

            //sumULong = arrUlong.Sum();                  // standard C# usage,   single-core, throws an overflow exception
            //sumUlong = arrUlong.AsParallel().Sum();     // standard C# usage,    multi-core, throws an overflow exception
            //sumUlong = arrLong.SumCheckedSse();         // data parallel (SSE), single-core, throws an overflow exception

            // No overflow exception thrown by HPCsharp .Sum(), producing a perfectly accurate sum
            sumDecimal = arrUlong.SumToDecimal();       // scalar, single-core
            sumDecimal = arrUlong.SumToDecimalPar();    // scalar,  multi-core

            // No overflow exception thrown by HPCsharp .Sum(), producing a perfectly accurate sum
            sumDecimal = arrUlong.SumToDecimalFast();           // scalar, single-core, faster when a few overflow exceptions occur
            sumDecimal = arrUlong.SumToDecimalFaster();         // scalar, single-core, much faster when many overflows can occur
            sumDecimal = arrUlong.SumToDecimalSseFaster();      // data parallel (SSE), single-core, much faster when many overflows can occur
            sumDecimal = arrUlong.SumToDecimalFasterPar();      // scalar,               multi-core, much faster when many overflows can occur
            sumDecimal = arrUlong.SumToDecimalSseFasterPar();   // data parallel (SSE),  multi-core, much faster when many overflows can occur

            // No overflow exception thrown by HPCsharp .Sum(), producing a perfectly accurate sum
            sumBigInteger = arrUlong.SumToBigIntegerFast();           // scalar, single-core
            sumBigInteger = arrUlong.SumToBigIntegerFaster();         // scalar, single-core (much faster when many overflows can occur)
            sumBigInteger = arrUlong.SumToBigIntegerSseFaster();      // data parallel (SSE), single-core, much faster when many overflows can occur
            //sumBigInteger = arrUlong.SumToBigIntegerFasterPar();      // scalar,               multi-core, much faster when many overflows can occur
            //sumBigInteger = arrUlong.SumToBigIntegerSseFasterPar();   // data parallel (SSE),  multi-core, much faster when many overflows can occur

            BigInteger[] arrBigInteger = new BigInteger[] { 5, 7, 16, 3, UInt64.MaxValue, 1 };

            // Linq does not have BigInteger array .Sum()
            sumBigInteger = arrBigInteger.Aggregate(sumBigInteger, (current, i) => current + i);  // another way to accomplish .Sum() using Linq
            sumBigInteger = arrBigInteger.SumHpc();         // HPCsharp implements scalar, single-core .Sum()
            sumBigInteger = arrBigInteger.SumPar();         // HPCsharp implements scalar,  multi-core .Sum()
        }
    }
}
