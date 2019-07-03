using HPCsharp.Algorithms;
using HPCsharp.ParallelAlgorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HPCsharpExamples
{
    partial class Program
    {
        public static void SumMeasureArraySpeedup()
        {
            Random randNum = new Random(2);
            int arraySize = 64 * 1024 * 1024;
            int[] benchArrayOne = new int[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                benchArrayOne[i] = randNum.Next(0, 64);    // limit the range to not cause overflow exception for standard C# .Sum()
            }

            Stopwatch stopwatch = new Stopwatch();
            long frequency = Stopwatch.Frequency;
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            stopwatch.Restart();
            int sum = benchArrayOne.Sum();
            stopwatch.Stop();
            double timeForSum = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;

            stopwatch.Restart();
            int sumAsParallel = benchArrayOne.AsParallel().Sum();
            stopwatch.Stop();
            double timeForSumAsParallel = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;

            stopwatch.Restart();
            long sumHpc = benchArrayOne.SumHpc();
            stopwatch.Stop();
            double timeForSumHpc = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;

            stopwatch.Restart();
            long sumSse = benchArrayOne.SumSse();
            stopwatch.Stop();
            double timeForSumSse = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;


            stopwatch.Restart();
            long sumSsePar = benchArrayOne.SumSsePar();
            stopwatch.Stop();
            double timeForSumSsePar = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;

            //Console.WriteLine("C# array of size {0}: Sum   {1:0.000} sec", arraySize, timeForSum);
            Console.WriteLine("C# array of size {0}: Sum              {1:0.000} sec, HPC#.SumHpc    {2:0.000} sec, speedup {3:0.00}", arraySize,
                               timeForSum, timeForSumHpc, timeForSum / timeForSumHpc);
            Console.WriteLine("C# array of size {0}: Sum              {1:0.000} sec, HPC#.SumSse    {2:0.000} sec, speedup {3:0.00}", arraySize,
                               timeForSum, timeForSumSse, timeForSum / timeForSumSse);
            Console.WriteLine("C# array of size {0}: Sum.AsParallel   {1:0.000} sec, HPC#.SumSse    {2:0.000} sec, speedup {3:0.00}", arraySize,
                               timeForSumAsParallel, timeForSumSse, timeForSumAsParallel / timeForSumSse);
            Console.WriteLine("C# array of size {0}: Sum.AsParallel   {1:0.000} sec, HPC#.SumSsePar {2:0.000} sec, speedup {3:0.00}", arraySize,
                               timeForSumAsParallel, timeForSumSsePar, timeForSumAsParallel / timeForSumSsePar);
        }
    }
}
