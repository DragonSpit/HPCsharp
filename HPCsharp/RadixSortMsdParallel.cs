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
