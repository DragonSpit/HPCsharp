// TODO: Add parallel versions of singed 8-bit and 16-bit Radix Sort/Counting Sort
// TODO: Implement paralle versions of RadixSort MSD implementations for ulong, slong and double array, but only spawn new tasks for non-empty bins instead of
//       all the bins indiscrimintently.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
        public static void SortRadixMsdPar(this byte[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlacePar();
        }

        public static void SortRadixMsdPar(this ushort[] arrayToBeSorted)
        {
            arrayToBeSorted.SortCountingInPlacePar();
        }
    }
}
