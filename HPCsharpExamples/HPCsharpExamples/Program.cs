using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FooState;

namespace HPCsharpExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check equality
            int[] arrayOne = { 21, 43, 16, 5, 4, 0 };
            int[] arrayTwo = { 21, 43, 16, 5, 4, 0 };

            bool equalLinq = arrayOne.SequenceEqual(     arrayTwo);    // System.Linq.SequenceEqual()
            bool equalSeq  = arrayOne.FsSeqSequenceEqual(arrayTwo);    // FooState non-parallel/sequential SequenceEqual
            bool equalPar  = arrayOne.FsParSequenceEqual(arrayTwo);    // FooState parallel                SequenceEqual

            Console.WriteLine("Check equality: {0} {1} {2}", equalLinq, equalSeq, equalPar);

            // Check inequality
            int[] arrayThree = { 21, 43, 16, 3, 4, 0 };     // one element is different

            equalLinq = arrayOne.SequenceEqual(     arrayThree);    // System.Linq.SequenceEqual()
            equalSeq  = arrayOne.FsSeqSequenceEqual(arrayThree);    // FooState non-parallel/sequential SequenceEqual
            equalPar  = arrayOne.FsParSequenceEqual(arrayThree);    // FooState parallel                SequenceEqual

            Console.WriteLine("Check inequality: {0} {1} {2}", equalLinq, equalSeq, equalPar);
        }
    }
}
