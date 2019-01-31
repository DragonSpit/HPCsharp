using System;
using System.Collections.Generic;
using System.Text;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        public static void Swap<T>(this T[] array, int indexA, int indexB)
        {
            T temp        = array[indexA];
            array[indexA] = array[indexB];
            array[indexB] = temp;
        }
        
        // reverse/mirror a range from l to r, inclusively, in-place
        public static void Reversal<T>(this T[] inArray, int l, int r)
        {
            while (l < r) inArray.Swap(l++, r--);
        }

        // Swaps two sequential subarrays ranges a[ l .. m ] and a[ m + 1 .. r ]
        public static void BlockExchangeReversal<T>(T[] inArray, int l, int m, int r)
        {
            inArray.Reversal(l, m);
            inArray.Reversal(m + 1, r);
            inArray.Reversal(l, r);
        }
    }
}
