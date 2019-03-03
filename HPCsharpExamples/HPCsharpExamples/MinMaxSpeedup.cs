using HPCsharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HPCsharpExamples
{
    partial class Program
    {
        public static void MinMeasureArraySpeedup()
        {
            Random randNum = new Random(2);
            int arraySize = 16 * 1024 * 1024;
            int[] benchArrayOne = new int[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                benchArrayOne[i] = randNum.Next(Int32.MinValue, Int32.MaxValue);    // fill array with random value between min and max
            }

            Stopwatch stopwatch = new Stopwatch();
            long frequency = Stopwatch.Frequency;
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            stopwatch.Restart();
            int minLinq = benchArrayOne.Min();
            stopwatch.Stop();
            double timeForLinqMin = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            stopwatch.Restart();
            int minHpc = benchArrayOne.MinHpc();
            stopwatch.Stop();
            double timeForSequentialMin = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            Console.WriteLine("C# array of size {0}: Linq.Min   {1:0.000} sec, HPC.Min   {2:0.000} sec, speedup {3:0.00}", arraySize,
                               timeForLinqMin, timeForSequentialMin, timeForLinqMin / timeForSequentialMin);
        }

        public static void MinMeasureListSpeedup()
        {
            Stopwatch stopwatch = new Stopwatch();
            Random randNum = new Random(2);
            int ListSize = 16 * 1024 * 1024;
            List<int> benchListOne = new List<int>(ListSize);

            for (int i = 0; i < ListSize; i++)
            {
                benchListOne.Add(randNum.Next(Int32.MinValue, Int32.MaxValue));    // fill lists with random value between min and max
            }

            long frequency = Stopwatch.Frequency;
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            stopwatch.Restart();
            int minLinq = benchListOne.Min();
            stopwatch.Stop();
            double timeForLinqMin = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            stopwatch.Restart();
            int minHpc = benchListOne.MinHpc();
            stopwatch.Stop();
            double timeForHpcMin = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            Console.WriteLine("C# List  of size {0}: Linq.Min   {1:0.000} sec, HPC.Min   {2:0.000} sec, speedup {3:0.00}", ListSize,
                                timeForLinqMin, timeForHpcMin, timeForLinqMin / timeForHpcMin);
        }

        public static void MaxMeasureArraySpeedup()
        {
            Random randNum = new Random(2);
            int arraySize = 16 * 1024 * 1024;
            int[] benchArrayOne = new int[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                benchArrayOne[i] = randNum.Next(Int32.MinValue, Int32.MaxValue);    // fill array with random value between min and max
            }

            Stopwatch stopwatch = new Stopwatch();
            long frequency = Stopwatch.Frequency;
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            stopwatch.Restart();
            int maxLinq = benchArrayOne.Max();
            stopwatch.Stop();
            double timeForLinqMax = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            stopwatch.Restart();
            int maxHpc = benchArrayOne.MaxHpc();
            stopwatch.Stop();
            double timeForSequentialMax = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            Console.WriteLine("C# array of size {0}: Linq.Max   {1:0.000} sec, HPC.Max   {2:0.000} sec, speedup {3:0.00}", arraySize,
                               timeForLinqMax, timeForSequentialMax, timeForLinqMax / timeForSequentialMax);
        }

        public static void MaxMeasureListSpeedup()
        {
            Stopwatch stopwatch = new Stopwatch();
            Random randNum = new Random(2);
            int ListSize = 16 * 1024 * 1024;
            List<int> benchListOne = new List<int>(ListSize);

            for (int i = 0; i < ListSize; i++)
            {
                benchListOne.Add(randNum.Next(Int32.MinValue, Int32.MaxValue));    // fill lists with random value between min and max
            }

            long frequency = Stopwatch.Frequency;
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            stopwatch.Restart();
            int maxLinq = benchListOne.Max();
            stopwatch.Stop();
            double timeForLinqMax = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            stopwatch.Restart();
            int maxHpc = benchListOne.MaxHpc();
            stopwatch.Stop();
            double timeForHpcMax = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            Console.WriteLine("C# List  of size {0}: Linq.Max   {1:0.000} sec, HPC.Max   {2:0.000} sec, speedup {3:0.00}", ListSize,
                                timeForLinqMax, timeForHpcMax, timeForLinqMax / timeForHpcMax);
        }
    }
}
