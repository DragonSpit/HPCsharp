using HPCsharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HPCsharpExamples
{
    partial class Program
    {
        public static void SortMeasureArraySpeedup(bool parallel, bool vsLinq, bool radixSort)
        {
            Random randNum = new Random(5);
            int arraySize = 1 * 1000 * 1000;
            uint[] benchArrayOne  = new uint[arraySize];
            uint[] benchArrayTwo  = new uint[arraySize];
            uint[] sortedArrayOne = new uint[arraySize];
            uint[] sortedArrayTwo = new uint[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                benchArrayOne[i] = (uint)randNum.Next(0, Int32.MaxValue);    // fill array with random       values between min and max
                //benchArrayOne[i] = (uint)i;                                // fill array with incrementing values
                //benchArrayOne[i] = 0;                                      // fill array with constant     values
                benchArrayTwo[i] = benchArrayOne[i];
            }

            Stopwatch stopwatch = new Stopwatch();
            long frequency = Stopwatch.Frequency;
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            stopwatch.Restart();
            if (!vsLinq)
            {
                if (!parallel)
                {
                    if (!radixSort)
                        benchArrayOne.SortMergeInPlace();
                    else
                        sortedArrayOne = benchArrayOne.SortRadix();
                }
                else
                    benchArrayOne.SortMergeInPlacePar();
            }
            else
            {
                if (!parallel)
                {
                    if (!radixSort)
                        sortedArrayOne = benchArrayOne.SortMerge();
                    else
                        sortedArrayOne = benchArrayOne.SortRadix();
                }
                else
                {
                    if (!radixSort)
                    {
                        sortedArrayOne = benchArrayOne.SortMergePar();          // Stable sorting is not necessary for array of integers
                        //sortedArrayOne = benchArrayOne.SortMergeStablePar();
                    }
                    else
                    {
                        //sortedArrayOne = benchArrayOne.SortRadix();
                        sortedArrayOne = benchArrayOne.SortRadixPar();
                    }
                }
            }
            stopwatch.Stop();
            double timeMergeSort = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            stopwatch.Restart();
            if (!vsLinq)
            {
                Array.Sort(benchArrayTwo);
            }
            else
            {
                if (parallel)
                    sortedArrayTwo = benchArrayTwo.AsParallel().OrderBy(element => element).ToArray();
                else
                    sortedArrayTwo = benchArrayTwo.OrderBy(element => element).ToArray();
            }
            stopwatch.Stop();
            double timeArraySort = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;

            if (!vsLinq)
            {
                if (!radixSort)
                {
                    bool equalSortedArrays = benchArrayOne.SequenceEqual(benchArrayTwo);
                    if (!equalSortedArrays)
                        Console.WriteLine("Sorting results using Merge Sort are not equal!");
                }
                else
                {
                    bool equalSortedArrays = benchArrayTwo.SequenceEqual(sortedArrayOne);
                    if (!equalSortedArrays)
                        Console.WriteLine("Sorting results using Radix Sort are not equal!");
                }
            }
            else
            {
                bool equalSortedArrays = sortedArrayOne.SequenceEqual(sortedArrayTwo);
                if (!equalSortedArrays)
                    Console.WriteLine("Sorting results using Merge Sort are not equal!");
            }

            if (!vsLinq)
            {
                if (!parallel)
                {
                    if (!radixSort)
                        Console.WriteLine("C# array of size {0}: Array.Sort             {1:0.000} sec, Serial   Merge Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                   timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                    else
                        Console.WriteLine("C# array of size {0}: Array.Sort             {1:0.000} sec, Serial   Radix Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                            timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                }
                else
                {
                    if (!radixSort)
                        Console.WriteLine("C# array of size {0}: Array.Sort             {1:0.000} sec, Parallel Merge Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                            timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                    else
                        Console.WriteLine("C# array of size {0}: Array.Sort             {1:0.000} sec, Serial   Radix Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                            timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                }
            }
            else
            {
                if (!parallel)
                {
                    if (!radixSort)
                        Console.WriteLine("C# array of size {0}: Linq.SortBy Serial     {1:0.000} sec, Serial   Merge Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                            timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                    else
                        Console.WriteLine("C# array of size {0}: Linq.SortBy Serial     {1:0.000} sec, Serial   Radix Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                            timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                }
                else
                {
                    if (!radixSort)
                        Console.WriteLine("C# array of size {0}: Linq.SortBy.AsParallel {1:0.000} sec, Parallel Merge Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                            timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                    else
                        Console.WriteLine("C# array of size {0}: Linq.SortBy.AsParallel {1:0.000} sec, Serial   Radix Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                            timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                }
            }
        }

        public static void SortMeasureListSpeedup()
        {
            Stopwatch stopwatch = new Stopwatch();
            Random randNum = new Random(2);
            int ListSize = 10 * 1000 * 1000;
            List<uint> benchListOne = new List<uint>(ListSize);
            List<uint> benchListTwo = new List<uint>(ListSize);

            for (int i = 0; i < ListSize; i++)
            {
                benchListOne.Add((uint)randNum.Next(0, Int32.MaxValue));    // fill lists with random       values between min and max
                //benchListOne.Add((uint)i);                                // fill lists with incrementing values
                //benchListOne.Add(0);                                      // fill lists with constant     values
                benchListTwo.Add(benchListOne[i]);
            }

            long frequency = Stopwatch.Frequency;
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            stopwatch.Restart();
            List<uint> sortedArrayOne = benchListOne.SortRadix();
            stopwatch.Stop();
            double timeRadixSort = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            stopwatch.Restart();
            benchListTwo.Sort();
            stopwatch.Stop();
            double timeListSort = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;

            bool equalSortedArrays = sortedArrayOne.SequenceEqual(benchListTwo);
            if (!equalSortedArrays)
                Console.WriteLine("Sorting List for Radix Sort are not equal!");

            Console.WriteLine("C# List of size {0}: List.Sort {1:0.000} sec, Serial Radix Sort {2:0.000} sec, speedup {3:0.00}", ListSize,
                               timeListSort, timeRadixSort, timeListSort / timeRadixSort);
        }

        public static void SortMeasureArrayOfUserDefinedClassSpeedup(bool parallel, bool vsLinq, bool radixSort)
        {
            Random randNum = new Random(5);
            int arraySize = 1 * 1000 * 1000;

            var comparer = new UserDefinedClassComparer();
            var equal = new UserDefinedClassEquality();
            var benchArrayOne  = new UserDefinedClass[arraySize];
            var benchArrayTwo  = new UserDefinedClass[arraySize];
            var sortedArrayOne = new UserDefinedClass[arraySize];
            var sortedArrayTwo = new UserDefinedClass[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                var randomValue = (uint)randNum.Next(0, Int32.MaxValue);
                benchArrayOne[i] = new UserDefinedClass(randomValue, i);
                //benchArrayOne[i] = new UserDefinedClass((uint)i, i);           // fill array with incrementing values
                //benchArrayOne[i] = new UserDefinedClass((uint)5, i);           // fill array with constant     values
            }
            for (int i = 0; i < arraySize; i++)
            {
                benchArrayTwo[i] = new UserDefinedClass(benchArrayOne[i].Key, benchArrayOne[i].Index);
            }
                Stopwatch stopwatch = new Stopwatch();
            long frequency = Stopwatch.Frequency;
            long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            stopwatch.Restart();
            if (!vsLinq)
            {
                if (!parallel)
                {
                    if (!radixSort)
                        benchArrayOne.SortMergeInPlace(comparer);
                    else
                    {
                        sortedArrayOne = benchArrayOne.SortRadix2(element => element.Key);   // faster, uses more memory
                        //sortedArrayOne = benchArrayOne.SortRadix(element => element.Key);  // slower, uses less memory
                    }
                }
                else
                    benchArrayOne.SortMergeInPlacePar(comparer);
            }
            else
            {
                if (!parallel)
                {
                    if (!radixSort)
                        sortedArrayOne = benchArrayOne.SortMerge(comparer);
                    else
                    {
                        sortedArrayOne = benchArrayOne.SortRadix2(element => element.Key);      // faster, uses more memory
                        //sortedArrayOne = benchArrayOne.SortRadix2(element => element.Key);    // slower, user less memory
                    }
                }
                else
                {
                    if (!radixSort)
                    {
                        sortedArrayOne = benchArrayOne.SortMergeStablePar(comparer);
                    }
                    else
                    {
                        sortedArrayOne = benchArrayOne.SortRadix2(element => element.Key);      // faster, uses more memory
                        //sortedArrayOne = benchArrayOne.SortRadix(element => element.Key);     // slower, uses less memory
                    }
                }
            }
            stopwatch.Stop();
            double timeMergeSort = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            stopwatch.Restart();
            if (!vsLinq)
            {
                Array.Sort(benchArrayTwo, comparer);
            }
            else
            {
                if (parallel)
                    sortedArrayTwo = benchArrayTwo.OrderBy(element => element.Key).AsParallel().ToArray();
                else
                    sortedArrayTwo = benchArrayTwo.OrderBy(element => element.Key).ToArray();
            }
            stopwatch.Stop();
            double timeArraySort = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;

            if (!vsLinq)
            {
                if (!radixSort)
                {
                    bool equalSortedArrays = benchArrayOne.SequenceEqual(benchArrayTwo, equal);
                    if (!equalSortedArrays)
                        Console.WriteLine("Sorting results using Merge Sort are not equal!");
                }
                else
                {
                    bool equalSortedArrays = benchArrayTwo.SequenceEqual(sortedArrayOne, equal);
                    if (!equalSortedArrays)
                        Console.WriteLine("Sorting results using Radix Sort are not equal!");
                }
            }
            else
            {
                bool equalSortedArrays = sortedArrayOne.SequenceEqual(sortedArrayTwo, equal);
                if (!equalSortedArrays)
                    Console.WriteLine("Sorting results using Merge Sort are not equal!");
            }

            if (!vsLinq)
            {
                if (!parallel)
                {
                    if (!radixSort)
                        Console.WriteLine("C# array of size {0}: Array.Sort             {1:0.000} sec, Serial   Merge Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                   timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                    else
                        Console.WriteLine("C# array of size {0}: Array.Sort             {1:0.000} sec, Serial   Radix Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                            timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                }
                else
                {
                    if (!radixSort)
                        Console.WriteLine("C# array of size {0}: Array.Sort             {1:0.000} sec, Parallel Merge Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                            timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                    else
                        Console.WriteLine("C# array of size {0}: Array.Sort             {1:0.000} sec, Serial   Radix Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                            timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                }
            }
            else
            {
                if (!parallel)
                {
                    if (!radixSort)
                        Console.WriteLine("C# array of size {0}: Linq.SortBy Serial     {1:0.000} sec, Serial   Merge Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                            timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                    else
                        Console.WriteLine("C# array of size {0}: Linq.SortBy Serial     {1:0.000} sec, Serial   Radix Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                            timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                }
                else
                {
                    if (!radixSort)
                        Console.WriteLine("C# array of size {0}: Linq.SortBy.AsParallel {1:0.000} sec, Parallel Merge Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                            timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                    else
                        Console.WriteLine("C# array of size {0}: Linq.SortBy.AsParallel {1:0.000} sec, Serial   Radix Sort {2:0.000} sec, speedup {3:0.00}", arraySize,
                                            timeArraySort, timeMergeSort, timeArraySort / timeMergeSort);
                }
            }
        }

    }
}

