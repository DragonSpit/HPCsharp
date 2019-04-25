using HPCsharp;
using System;
using System.Linq;

namespace HPCsharpExamples
{
    class SortingUsageExamples
    {
        public static void SortingBasicExamples()
        {
            long[] ArrayOne = { 21, 43, 16, 5, 54, 3 };
            long[] sortedArrayOne;

            // In-Place Sorting
            Array.Sort(ArrayOne);                                  // C# standard in-place Sort
            Algorithm.SortMergeInPlace(ArrayOne);                  // HPCsharp Merge Sort (serial). Direct function call usage
            ArrayOne.SortMergeInPlace();                           // HPCsharp Merge Sort (serial). Extension method call usage
            ArrayOne.SortRadixMsd();                               // HPCsharp Radix Sort (serial)   - uint[] has not yet been implemented, but other data types exist

            // Not In-Place Sorting
            sortedArrayOne = ArrayOne.OrderBy(i => i).ToArray();     // Linq Sort
            sortedArrayOne = ArrayOne.SortMerge();                   // HPCsharp Merge Sort (serial)
            sortedArrayOne = Algorithm.SortMerge(ArrayOne);          // HPCsharp Merge Sort (serial). Direct function call usage
            sortedArrayOne = ArrayOne.SortRadix();                   // HPCsharp Radix Sort (serial)

            // In-Place Sorting (Parallel)
            ArrayOne.SortMergeInPlacePar();                        // HPCsharp Merge Sort (parallel)
            ArrayOne.SortRadixMsdPar();                            // HPCsharp Merge Sort (parallel) - byte, short, ubyte, ushort exist

            // Not In-Place Sorting (Parallel)
            sortedArrayOne = ArrayOne.AsParallel().OrderBy(i => i).ToArray();   // Linq Sort (parallel)
            sortedArrayOne = ArrayOne.SortMergePar();              // HPCsharp Merge Sort (parallel)
            sortedArrayOne = ArrayOne.SortRadixPar();              // HPCsharp Radix Sort (parallel)

            // Other Sorts
            // Insertion Sort - O(N^2) - use only for sorting fewer than 50 elements
            // Counting  Sort - O(N)   - only for byte, short, ubyte, ushort arrays
        }
    }
}
