using HPCsharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HPCsharpExamples
{
    partial class Program
    {
        public static void SelectionMeasureDecimalArraySpeedup(bool parallel, bool vsLinq)
        {
            Random randNum = new Random(5);
            int arraySize = 10 * 1000 * 1000;
            decimal[] benchArrayOne    = new decimal[arraySize];
            decimal[] benchArrayTwo    = new decimal[arraySize];
            decimal[] benchArrayThree  = new decimal[arraySize];
            decimal[] sortedArrayTwo   = new decimal[arraySize];

            decimal minPrice = 0.0m;
            decimal maxPrice = 1000000.0m;  

            for (int i = 0; i < arraySize; i++)
            {
                // Generate double, scale it, and cast to decimal
                double scaledRandom = (randNum.NextDouble() * (double)(maxPrice - minPrice)) + (double)minPrice;

                // Round to 2 decimal places for currency 
                benchArrayOne[i] = Math.Round((decimal)scaledRandom, 2);

                //benchArrayOne[i] = (uint)i;                                  // fill array with incrementing values
                //benchArrayOne[i] = 1.23m;                                    // fill array with constant     values
                benchArrayTwo[i]   = benchArrayOne[i];
                benchArrayThree[i] = benchArrayOne[i];
            }

            int randomK = randNum.Next(0, arraySize);

            Stopwatch stopwatch = new Stopwatch();
            long frequency = Stopwatch.Frequency;
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            stopwatch.Restart();
            benchArrayOne.Select(randomK);
            stopwatch.Stop();
            double timeSelection = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;

            stopwatch.Restart();
            if (!vsLinq)
                Array.Sort(benchArrayTwo);
            else
            {
                if (parallel) sortedArrayTwo = benchArrayTwo.AsParallel().OrderBy(element => element).ToArray();
                else          sortedArrayTwo = benchArrayTwo.OrderBy(             element => element).ToArray();
            }
            stopwatch.Stop();
            double timeArraySort = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;

            if (!vsLinq)
            {
                bool equalSelectionResult = benchArrayOne[randomK] == benchArrayTwo[randomK];
                if (!equalSelectionResult)
                    Console.WriteLine("Selection result vs Non-Linq Sort is not equal!");
            }
            else
            {
                bool equalSelectionResult = benchArrayOne[randomK] == sortedArrayTwo[randomK];
                if (!equalSelectionResult)
                    Console.WriteLine("Selection result vs Linq Sort is not equal!");
            }

            if (!vsLinq)
            {
                Console.WriteLine("C# array of size {0}: Array.Sort              {1:0.000} sec, Serial Selection {2:0.000} sec, speedup {3:0.00}", arraySize,
                            timeArraySort, timeSelection, timeArraySort / timeSelection);
            }
            else
            {
                if (!parallel)
                    Console.WriteLine("C# array of size {0}: Linq.OrderBy            {1:0.000} sec, Serial Selection {2:0.000} sec, speedup {3:0.00}", arraySize,
                            timeArraySort, timeSelection, timeArraySort / timeSelection);
                else
                    Console.WriteLine("C# array of size {0}: Linq.OrderBy.Parallel   {1:0.000} sec, Serial Selection {2:0.000} sec, speedup {3:0.00}", arraySize,
                            timeArraySort, timeSelection, timeArraySort / timeSelection);
            }
        }
    }
}
