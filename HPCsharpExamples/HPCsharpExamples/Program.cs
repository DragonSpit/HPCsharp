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
            int[] arrayOne = { 21, 43, 16, 5, 4, 0 };
            int[] arrayTwo = { 21, 43, 16, 5, 4, 0 };

            bool equalLinq = arrayOne.SequenceEqual(   arrayTwo);    // Linq     SequenceEqual()
            bool equalHpc  = arrayOne.HpcSequenceEqual(arrayTwo);    // HPCsharp SequenceEqual()

            Console.WriteLine("Check array equality: {0} {1}", equalLinq, equalHpc);

            // Check array inequality
            int[] arrayThree = { 21, 43, 16, 3, 4, 0 };     // one element is different

            equalLinq = arrayOne.SequenceEqual(   arrayThree);    // Linq     SequenceEqual()
            equalHpc  = arrayOne.HpcSequenceEqual(arrayThree);    // HPCsharp SequenceEqual()

            Console.WriteLine("Check array inequality: {0} {1}", equalLinq, equalHpc);

            // Check List equality
            List<int> listOne = new List<int>() { 21, 43, 16, 5, 4, 0 };
            List<int> listTwo = new List<int>() { 21, 43, 16, 5, 4, 0 };

            equalLinq = listOne.SequenceEqual(   listTwo);    // Linq     SequenceEqual()
            equalHpc  = listOne.HpcSequenceEqual(listTwo);    // HPCsharp SequenceEqual()

            Console.WriteLine("Check list equality: {0} {1}", equalLinq, equalHpc);

            // Check List inequality
            List<int> listThree = new List<int>() { 21, 43, 16, 3, 4, 0 };     // one element is different

            equalLinq = listOne.SequenceEqual(   listThree);    // Linq     SequenceEqual()
            equalHpc  = listOne.HpcSequenceEqual(listThree);    // HPCsharp SequenceEqual()

            Console.WriteLine("Check list inequality: {0} {1}", equalLinq, equalHpc);

            // Measure array equality speedup (parallel and sequential)
            MeasureArraySpeedup();

            // Measure List equality speedup (parallel and sequential)
            MeasureListSpeedup();
        }
    }
}
