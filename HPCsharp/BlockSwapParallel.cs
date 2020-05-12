// TODO: Parallelize block-swap algorithm to speedup in-place merge, possibly using SSE with instruction to reverse order within an SSE Vector
//       Or, maybe SSE rotation/reverse order can be avoided, knowing that we'll be rotating it back in the following pass.
// TODO: Parallelize block-swap algorithms to see if there is a benefit, now that unit testing and benchmarking in C# is in place
//       Try simple things first like scalar parallelism of reversal algorithms middle stage of reversal by running the two portions reversal
//       in parallel to see if it speeds up at all.
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HPCsharp.ParallelAlgorithms;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
        // Swaps two sequential subarrays ranges a[ l .. m ] and a[ m + 1 .. r ]
        public static void BlockSwapReversalPar<T>(T[] array, int l, int m, int r)
        {
            Parallel.Invoke(
                () => { array.Reversal(l,     m); },
                () => { array.Reversal(m + 1, r); }
            );
            array.Reversal(l, r);
        }
    }
}
