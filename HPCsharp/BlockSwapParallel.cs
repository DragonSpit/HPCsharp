// TODO: Parallelize block-swap algorithm to speedup in-place merge, possibly using SSE with instruction to reverse order within an SSE Vector
//       Or, maybe SSE rotation/reverse order can be avoided, knowing that we'll be rotating it back in the following pass.
// TODO: Parallelize block-swap algorithms to see if there is a benefit, now that unit testing and benchmarking in C# is in place
//       Try simple things first like scalar parallelism of reversal algorithms middle stage of reversal by running the two portions reversal
//       in parallel to see if it speeds up at all.
// TODO: Write a blog on parallelizing BlockSwap Reversal algorithm
// TODO: Implement a C# .Reverse() as a parallel function version
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using HPCsharp.ParallelAlgorithms;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
        // Swaps two sequential subarrays ranges a[ l .. m ] and a[ m + 1 .. r ]
        public static void BlockSwapReversalPar<T>(T[] array, int l, int m, int r, int threshold = 16 * 1024)
        {
            int length = r - l + 1;
            if (length < threshold)
            {
                array.Reversal(l,     m);
                array.Reversal(m + 1, r);
                array.Reversal(l,     r);
            }
            else
            {
                Parallel.Invoke(
                    () => { array.Reversal(l,     m); },
                    () => { array.Reversal(m + 1, r); }
                );
                array.Reversal(l, r);     // serial version of the rest of the code
                //int firstLength = ((length + 2) / 4) * 2;  // how much first  core will swap. Guaranteed to be an even value
                //int firstSwapLength = firstLength  / 2;
                //Parallel.Invoke(
                //    () => { array.Swap(l, r - firstSwapLength + 1, firstSwapLength, true); },   // all but the last part need to be in Swap(startA, startB, length) form, since these are outer rings
                //                                                                                // to parallelize more, add more of the above parts (more outer onion rings/layers)
                //    () => { array.Reversal(l + firstSwapLength, r - firstSwapLength); }         // the last/innermost part needs to be in Reversal(l, r) form, since its the inner core of the onion
                //);
            }
        }

        public static void BlockSwapReversalPar2<T>(T[] array, int l, int m, int r, int threshold = 16 * 1024)
        {
            int length = r - l + 1;
            if (length < threshold)
            {
                array.Reversal(l,     m);
                array.Reversal(m + 1, r);
                array.Reversal(l,     r);
            }
            else
            {
                int firstLength = (((m - l + 1) + 2) / 4) * 2;  // how much first  core will swap. Guaranteed to be an even value
                int firstSwapLength = firstLength / 2;
                int secondLength = (((r - (m + 1) + 1) + 2) / 4) * 2;  // how much first  core will swap. Guaranteed to be an even value
                int secondSwapLength = secondLength / 2;
                Parallel.Invoke(
                    //() => { array.Reversal(l,     m); },
                    () => { array.Swap(    l,                   m - firstSwapLength + 1, firstSwapLength, true); },
                    () => { array.Reversal(l + firstSwapLength, m - firstSwapLength); },
                    //() => { array.Reversal(m + 1,               r); }
                    () => { array.Swap(    m + 1,                    r - secondSwapLength + 1, secondSwapLength, true); },
                    () => { array.Reversal(m + 1 + secondSwapLength, r - secondSwapLength); }
                );
                //array.Reversal(l, r);     // serial version of the rest of the code
                firstLength = ((length + 2) / 4) * 2;  // how much first  core will swap. Guaranteed to be an even value
                firstSwapLength = firstLength / 2;
                Parallel.Invoke(
                    () => { array.Swap(l, r - firstSwapLength + 1, firstSwapLength, true); },   // all but the last part need to be in Swap(startA, startB, length) form, since these are outer rings
                                                                                                // to parallelize more, add more of the above parts (more outer onion rings/layers)
                    () => { array.Reversal(l + firstSwapLength, r - firstSwapLength); }         // the last/innermost part needs to be in Reversal(l, r) form, since its the inner core of the onion
                );
            }
        }
    }
}
