using System;
using System.Collections.Generic;
using System.Linq;
using HPCsharp;

namespace HPCsharpExamples
{
    partial class Program
    {
        static void Main(string[] args)
        {
            // Test Radix Sort
            uint[] ArrayOne   = { 21, 43, 16, 5, 54, 3 };
            uint[] ArrayTwo   = { 21, 43, 16, 5, 54, 3 };
            uint[] ArrayThree = { 21, 43, 16, 5, 54, 3 };
            uint[] ArrayFour  = { 21, 43, 16, 5, 54, 3 };
            uint[] ArrayFive  = { 21, 43, 16, 5, 54, 3 };
            uint[] ArraySix   = { 21, 43, 16, 5, 54, 3 };

            Array.Sort(ArrayOne);                                   // C# standard in-place Sort
            ArrayTwo.SortMergeInPlace();                            // HPCsharp    in-place Merge Sort (serial)
            ArrayThree.SortMergeInPlacePar();                       // HPCsharp    in-place Merge Sort (parallel)

            uint[] sortedArrayFour = ArrayFour.SortRadix();         // HPCsharp Radix Sort (not in-place, serial)
            uint[] sortedArrayFive = ArrayFive.SortMerge();         // HPCsharp Merge Sort (not in-place, serial)
            uint[] sortedArraySix  = ArraySix.SortMergePar();       // HPCsharp Merge Sort (not in-place, parallel)

            bool equalSortedArraysOneAndTwo   = ArrayOne.SequenceEqual(ArrayTwo);
            bool equalSortedArraysOneAndThree = ArrayOne.SequenceEqual(ArrayThree);
            bool equalSortedArraysOneAndFour  = ArrayOne.SequenceEqual(ArrayFour);
            bool equalSortedArraysOneAndFive  = ArrayOne.SequenceEqual(ArrayFive);
            bool equalSortedArraysOneAndSix   = ArrayOne.SequenceEqual(ArraySix);

            if (equalSortedArraysOneAndTwo && equalSortedArraysOneAndThree && equalSortedArraysOneAndFour &&
                equalSortedArraysOneAndFive && equalSortedArraysOneAndSix)
                Console.WriteLine("Sorting for variety of Merge Sort(s) results are equal");
            else
                Console.WriteLine("Sorting for variety of Merge Sort(s) results are not equal!");

            Console.WriteLine();
            SortMeasureArraySpeedup(false, false, false);     // Measure Array Serial  Sorting speedup for Serial   Merge Sort
            SortMeasureArraySpeedup(true,  false, false);     // Measure Array Serial  Sorting speedup for Parallel Merge Sort
            SortMeasureArraySpeedup(false, true,  false);     // Measure Linq  Serial  Sorting speedup for Serial   Merge Sort
            SortMeasureArraySpeedup(true,  true,  false);     // Measure Linq Parallel Sorting speedup for Parallel Merge Sort

            Console.WriteLine();
            SortMeasureArraySpeedup(false, false, true);       // Measure Array Serial   Sorting speedup for Serial Radix Sort
            SortMeasureArraySpeedup(false, true,  true);       // Measure Linq  Serial   Sorting speedup for Serial Radix Sort
            SortMeasureArraySpeedup(true,  true,  true);       // Measure Linq  Parallel Sorting speedup for Serial Radix Sort

            Console.WriteLine();
            SortMeasureListSpeedup();       // Measure List.Sort speedup for Serial Radix Sort
            Console.WriteLine();

            // Check array equality
            int[] arrayOne = { 21, 43, 16, 5, 4, -3 };
            int[] arrayTwo = { 21, 43, 16, 5, 4, -3 };

            bool equalLinq = arrayOne.SequenceEqual(   arrayTwo);    // Linq     SequenceEqual()
            bool equalHpc  = arrayOne.SequenceEqualHpc(arrayTwo);    // HPCsharp SequenceEqual()

            Console.WriteLine("Check array   equality: {0} {1}", equalLinq, equalHpc);

            // Check array inequality
            int[] arrayThree = { 21, 43, 16, 3, 4, -3 };     // one element is different

            equalLinq = arrayOne.SequenceEqual(   arrayThree);    // Linq     SequenceEqual()
            equalHpc  = arrayOne.SequenceEqualHpc(arrayThree);    // HPCsharp SequenceEqual()

            Console.WriteLine("Check array inequality: {0} {1}", equalLinq, equalHpc);

            // Check List equality
            List<int> listOne = new List<int>() { 21, 43, 16, 5, 4, -3 };
            List<int> listTwo = new List<int>() { 21, 43, 16, 5, 4, -3 };

            equalLinq = listOne.SequenceEqual(   listTwo);    // Linq     SequenceEqual()
            equalHpc  = listOne.SequenceEqualHpc(listTwo);    // HPCsharp SequenceEqual()

            Console.WriteLine("Check list   equality: {0} {1}", equalLinq, equalHpc);

            // Check List inequality
            List<int> listThree = new List<int>() { 21, 43, 16, 3, 4, -3 };     // one element is different

            equalLinq = listOne.SequenceEqual(   listThree);    // Linq     SequenceEqual()
            equalHpc  = listOne.SequenceEqualHpc(listThree);    // HPCsharp SequenceEqual()

            Console.WriteLine("Check list inequality: {0} {1}", equalLinq, equalHpc);

            // Array Min
            int minLinq = arrayOne.Min();
            int minHpc  = arrayOne.MinHpc();
            Console.WriteLine("Check array Min: {0} {1}", minLinq, minHpc);

            // List Min
            minLinq = listOne.Min();
            minHpc  = listOne.MinHpc();
            Console.WriteLine("Check List  Min: {0} {1}", minLinq, minHpc);

            // Array Max
            int maxLinq = arrayOne.Max();
            int maxHpc  = arrayOne.MaxHpc();
            Console.WriteLine("Check array Max: {0} {1}", maxLinq, maxHpc);

            // List Min
            maxLinq = listOne.Max();
            maxHpc  = listOne.MaxHpc();
            Console.WriteLine("Check List  Max: {0} {1}", maxLinq, maxHpc);

            // Measure array.SequenceEqual speedup
            EqualMeasureArraySpeedup();
            // Measure List.SequenceEqual speedup
            EqualMeasureListSpeedup();

            // Measure array.Min speedup
            MinMeasureArraySpeedup();
            // Measure List.Min speedup
            MinMeasureListSpeedup();

            // Measure array.Max speedup
            MaxMeasureArraySpeedup();
            // Measure List.Max speedup
            MaxMeasureListSpeedup();
        }
    }
}
