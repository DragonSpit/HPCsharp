using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        /// <summary>
        /// Parallel Merge Sort that is not-in-place. Also, not stable, since Array.Sort is not stable, and is used as the recursion base-case.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="b">left/beginning  index of the source array, inclusive</param>
        /// <param name="e">right/end index of the source array, non-inclusive</param>
        /// <param name="comparer">method to compare array elements</param>
        public static void Quicksort<T>(this T[] src, Int32 b, Int32 e, IComparer<T> comparer = null)
        {
            if (b >= e) return;

            var equalityComparer = comparer ?? Comparer<T>.Default;
            T tmp;

            // do shuffle
            T pivot_value = src[b];
            Int32 i = b;
            Int32 j = e - 1;
            while (i != j)
            {
                while (i != j && (equalityComparer.Compare(pivot_value, src[j]) <  0)) --j;
                while (i != j && (equalityComparer.Compare(src[i], pivot_value) <= 0)) ++i;
                tmp    = src[i];
                src[i] = src[j];
                src[j] = tmp;
            }
            tmp    = src[i];
            src[i] = src[b];
            src[b] = tmp;

            Quicksort(src, b,     i, comparer);
            Quicksort(src, i + 1, e, comparer);
        }

        public static void Quicksort(this uint[] src, Int32 b, Int32 e)
        {
            if (b >= e) return;

            uint tmp;

            // do shuffle
            uint pivot_value = src[b];
            Int32 i = b;
            Int32 j = e - 1;
            while (i != j)
            {
                while (i != j && pivot_value < src[j] ) --j;
                while (i != j && src[i] <= pivot_value) ++i;
                tmp    = src[i];
                src[i] = src[j];
                src[j] = tmp;
            }
            tmp    = src[i];
            src[i] = src[b];
            src[b] = tmp;

            Quicksort(src, b,     i);
            Quicksort(src, i + 1, e);
        }
        // ADAPTED FROM:https://stackoverflow.com/questions/53722004/generic-quicksort-implemented-with-vector-and-iterators-c
        // 2X faster than the generic version
        public static void QuicksortHoare(this uint[] src, Int32 b, Int32 e)
        {
            if ((e - b) < 2)
                return;

            uint tmp;

            Int32 i = b;
            Int32 j = e - 1;
            uint pivot_value = src[i + (j - i) / 2];
            if (src[i] < pivot_value)
                while (src[++i] < pivot_value) ;
            if (src[j] > pivot_value)
                while (src[--j] > pivot_value) ;
            while (i < j)
            {
                tmp    = src[i];
                src[i] = src[j];
                src[j] = tmp;
                while (src[++i] < pivot_value) ;
                while (src[--j] > pivot_value) ;
            }
            j++;

            QuicksortHoare(src, b, j);
            QuicksortHoare(src, j, e);
        }

        // Generic version of Quicksort
        // 2X slower for built-in data types
        public static void QuicksortHoare<T>(this T[] src, Int32 b, Int32 e, IComparer<T> comparer = null)
        {
            if ((e - b) < 2)
                return;

            var equalityComparer = comparer ?? Comparer<T>.Default;
            T tmp;

            Int32 i = b;
            Int32 j = e - 1;
            T pivot_value = src[i + (j - i) / 2];
            if (equalityComparer.Compare(src[i], pivot_value) < 0)
                while (equalityComparer.Compare(src[++i], pivot_value) < 0 ) ;
            if (equalityComparer.Compare(src[j], pivot_value) > 0)
                while (equalityComparer.Compare(src[--j], pivot_value) > 0) ;
            while (i < j)
            {
                tmp = src[i];
                src[i] = src[j];
                src[j] = tmp;
                while (equalityComparer.Compare(src[++i], pivot_value) < 0) ;
                while (equalityComparer.Compare(src[--j], pivot_value) > 0) ;
            }
            j++;

            QuicksortHoare(src, b, j, comparer);
            QuicksortHoare(src, j, e, comparer);
        }

        // From Sedgewick "Algorithms in C++" 3rd edition p. 338
        public static void QuicksortThreeWayPartition(this uint[] src, Int32 l, Int32 r)
        {
            int k;
            uint v = src[r];

            if (r <= l)
                return;

            int i = l - 1;
            int j = r;
            int p = l - 1;
            int q = r;
            uint tmp;

            while(true)
            {
                while (src[++i] < v) ;
                while (v < src[--j])
                    if (j == l) break;
                if (i >= j) break;
                tmp    = src[i];
                src[i] = src[j];
                src[j] = tmp;
                if (src[i] == v)
                {
                    p++;
                    tmp    = src[i];
                    src[i] = src[p];
                    src[p] = tmp;
                }
                if (v == src[j])
                {
                    q--;
                    tmp    = src[q];
                    src[q] = src[j];
                    src[j] = tmp;
                }
            }
            tmp    = src[r];
            src[r] = src[i];
            src[i] = tmp;
            j = i - 1;
            i++;
            for (k = l; k <= p; k++, j--)
            {
                tmp    = src[k];
                src[k] = src[j];
                src[j] = tmp;
            }
            for (k = r - 1; k >= q; k--, i++)
            {
                tmp    = src[k];
                src[k] = src[i];
                src[i] = tmp;
            }

            QuicksortThreeWayPartition(src, l, j);
            QuicksortThreeWayPartition(src, i, r);
        }

    }
}
