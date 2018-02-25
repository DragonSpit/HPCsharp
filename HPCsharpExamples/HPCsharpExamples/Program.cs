using FooState;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HPCsharpExamples
{
    partial class Program
    {
        static void Main(string[] args)
        {
            // Check array equality
            int[] arrayOne = { 21, 43, 16, 5, 4, 0 };
            int[] arrayTwo = { 21, 43, 16, 5, 4, 0 };

            bool equalLinq = arrayOne.SequenceEqual(     arrayTwo);    // System.Linq.SequenceEqual()
            bool equalSeq  = arrayOne.FsSeqSequenceEqual(arrayTwo);    // FooState non-parallel/sequential SequenceEqual
            bool equalPar  = arrayOne.FsParSequenceEqual(arrayTwo);    // FooState parallel                SequenceEqual

            Console.WriteLine("Check array equality: {0} {1} {2}", equalLinq, equalSeq, equalPar);

            // Check array inequality
            int[] arrayThree = { 21, 43, 16, 3, 4, 0 };     // one element is different

            equalLinq = arrayOne.SequenceEqual(     arrayThree);    // System.Linq.SequenceEqual()
            equalSeq  = arrayOne.FsSeqSequenceEqual(arrayThree);    // FooState non-parallel/sequential SequenceEqual
            equalPar  = arrayOne.FsParSequenceEqual(arrayThree);    // FooState parallel                SequenceEqual

            Console.WriteLine("Check array inequality: {0} {1} {2}", equalLinq, equalSeq, equalPar);

            // Check List equality
            List<int> listOne = new List<int>() { 21, 43, 16, 5, 4, 0 };
            List<int> listTwo = new List<int>() { 21, 43, 16, 5, 4, 0 };

            equalLinq = listOne.SequenceEqual(     listTwo);    // System.Linq.SequenceEqual()
            equalSeq  = listOne.FsSeqSequenceEqual(listTwo);    // FooState non-parallel/sequential SequenceEqual
            equalPar  = listOne.FsParSequenceEqual(listTwo);    // FooState parallel                SequenceEqual

            Console.WriteLine("Check list equality: {0} {1} {2}", equalLinq, equalSeq, equalPar);

            // Check List inequality
            List<int> listThree = new List<int>() { 21, 43, 16, 3, 4, 0 };     // one element is different

            equalLinq = listOne.SequenceEqual(     listThree);    // System.Linq.SequenceEqual()
            equalSeq  = listOne.FsSeqSequenceEqual(listThree);    // FooState non-parallel/sequential SequenceEqual
            equalPar  = listOne.FsParSequenceEqual(listThree);    // FooState parallel                SequenceEqual

            Console.WriteLine("Check list inequality: {0} {1} {2}", equalLinq, equalSeq, equalPar);

            // Measure array equality speedup (parallel and sequential)
            MeasureArraySpeedup();

            // Measure List equality speedup (parallel and sequential)
            MeasureListSpeedup();
        }
    }
}
