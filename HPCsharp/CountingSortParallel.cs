// TODO: Improve speed further for sorting small arrays by allocating memory on the stack if possible to use safely, such as spans.
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
#if false
    static public partial class ParallelAlgorithm
    {
        public static byte[] SortCountingPar(this byte[] inputArray)
        {
            byte[] sortedArray = new byte[inputArray.Length];

            int[] counts = inputArray.Histogram();

            int startIndex = 0;
            for (uint countIndex = 0; countIndex < counts.Length; countIndex++)
            {
                sortedArray.FillPar((byte)countIndex, startIndex, counts[countIndex]);
                startIndex += counts[countIndex];
            }

            return sortedArray;
        }

        public static void SortCountingInPlacePar(this byte[] arrayToSort)
        {
            int[] counts = arrayToSort.Histogram();

            int startIndex = 0;
            for (uint countIndex = 0; countIndex < counts.Length; countIndex++)
            {
                arrayToSort.FillPar((byte)countIndex, startIndex, counts[countIndex]);
                startIndex += counts[countIndex];
            }
        }
    }
#endif
}
