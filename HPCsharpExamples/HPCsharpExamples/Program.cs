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

            // Test Radix Sort
            uint[] arrayFour = { 21, 43, 16, 5, 54, 3 };
            uint[] arrayFive = { 21, 43, 16, 5, 54, 3 };

            uint[] sortedArrayFour = arrayFour.RadixSortLSD();
            Array.Sort(arrayFive);

            bool equalSortedArrays = sortedArrayFour.SequenceEqual(arrayFive);

            if (equalSortedArrays)
                Console.WriteLine("Sorting results are equal");
            else
                Console.WriteLine("Sorting results did not compare!");

            // Measure Array RadixSort speedup
            SortMeasureArraySpeedup();
            // Measure List RadixSort speedup
            SortMeasureListSpeedup();
        }
    }
}
