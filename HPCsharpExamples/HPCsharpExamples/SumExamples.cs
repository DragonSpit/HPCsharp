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
            BigInteger sumBigInteger;

            //sumLong = arrLong.Sum();                  // standard C# usage, single-core, will throw overflow exception
            //sumLong = arrLong.AsParallel().Sum();     // standard C# usage, multi-core,  will throw overflow exception

            // No overflow exception thrown by HPCsharp .Sum(), producing a perfectly accurate sum
            sumDecimal = arrLong.SumToDecimal();     // serial
            sumDecimal = arrLong.SumToDecimal();     // data-parallel, single-core (many times faster)
            sumDecimal = arrLong.SumToDecimalPar();  // data-parallel,  multi-core (even faster)

            // No overflow exception thrown by HPCsharp .Sum(), producing a perfectly accurate sum
            sumBigInteger = arrLong.SumToBigIntegerFast();     // serial
            sumBigInteger = arrLong.SumToBigIntegerFaster();   // serial single-core (much faster when lots of overflows occur)


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
        }
    }
}
