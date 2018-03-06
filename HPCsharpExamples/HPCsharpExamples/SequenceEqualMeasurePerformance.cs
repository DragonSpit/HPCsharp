using HPCsharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HPCsharpExamples
{
    partial class Program
    {
        public static void EqualMeasureArraySpeedup()
        {
            Random randNum = new Random(2);
            int arraySize = 16 * 1024 * 1024;
            int[] benchArrayOne = new int[arraySize];
            int[] benchArrayTwo = new int[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                benchArrayOne[i] = randNum.Next(Int32.MinValue, Int32.MaxValue);    // fill array with random value between min and max
                benchArrayTwo[i] = benchArrayOne[i];
            }

            Stopwatch stopwatch = new Stopwatch();
            long frequency = Stopwatch.Frequency;
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            stopwatch.Restart();
            bool equalLinq = benchArrayOne.SequenceEqual(benchArrayTwo);
            stopwatch.Stop();
            double timeLinqEqual = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            stopwatch.Restart();
            bool equalSequencial = benchArrayOne.SequenceEqualHpc(benchArrayTwo);
            stopwatch.Stop();
            double timeForSequentialEqual = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            Console.WriteLine("C# array of size {0}: Linq.Equal {1:0.000} sec, HPC.Equal {2:0.000} sec, speedup {3:0.00}", arraySize,
                               timeLinqEqual, timeForSequentialEqual, timeLinqEqual / timeForSequentialEqual);
        }

        public static void EqualMeasureListSpeedup()
        {
            Stopwatch stopwatch = new Stopwatch();
            Random randNum = new Random(2);
            int ListSize = 16 * 1024 * 1024;
            List<int> benchListOne = new List<int>(ListSize);
            List<int> benchListTwo = new List<int>(ListSize);

            for (int i = 0; i < ListSize; i++)
            {
                benchListOne.Add(randNum.Next(Int32.MinValue, Int32.MaxValue));    // fill lists with random value between min and max
                benchListTwo.Add(benchListOne[i]);
            }

            long frequency = Stopwatch.Frequency;
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            stopwatch.Restart();
            bool equalLinq = benchListOne.SequenceEqual(benchListTwo);
            stopwatch.Stop();
            double timeForLinqEqual = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            stopwatch.Restart();
            bool equalSequencial = benchListOne.SequenceEqualHpc(benchListTwo);
            stopwatch.Stop();
            double timeForSequentialEqual = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            Console.WriteLine("C# List  of size {0}: Linq.Equal {1:0.000} sec, HPC.Equal {2:0.000} sec, speedup {3:0.00}", ListSize,
                                timeForLinqEqual, timeForSequentialEqual, timeForLinqEqual / timeForSequentialEqual);
        }
    }
}
