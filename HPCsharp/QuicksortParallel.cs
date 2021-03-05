// FROM:https://stackoverflow.com/questions/53722004/generic-quicksort-implemented-with-vector-and-iterators-c
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;
using HPCsharp.ParallelAlgorithms;

namespace HPCsharp
{
    /// <summary>
    /// Parallel Algorithms operating on variety of containers, providing trade-off between abstraction and performance
    /// </summary>
    static public partial class ParallelAlgorithm
    {
        private static void QuicksortInnerPar<T>(this T[] src, Int32 b, Int32 e, IComparer<T> comparer = null)
        {
            const Int32 cutoff = 4096;

            if (e - b <= cutoff)
            {
                Algorithm.QuicksortHoare(src, b, e, comparer);
            }
            else
            {
                var equalityComparer = comparer ?? Comparer<T>.Default;
                T tmp;

                Int32 i = b;
                Int32 j = e - 1;
                T pivot_value = src[i + (j - i) / 2];
                if (equalityComparer.Compare(src[i], pivot_value) < 0)
                    while (equalityComparer.Compare(src[++i], pivot_value) < 0) ;
                if (equalityComparer.Compare(src[j], pivot_value) > 0)
                    while (equalityComparer.Compare(src[--j], pivot_value) > 0) ;
                while (i < j)
                {
                    tmp    = src[i];
                    src[i] = src[j];
                    src[j] = tmp;
                    while (equalityComparer.Compare(src[++i], pivot_value) < 0) ;
                    while (equalityComparer.Compare(src[--j], pivot_value) > 0) ;
                }
                j++;

                Parallel.Invoke(
                    () => { QuicksortInnerPar<T>(src, b, j, comparer); },
                    () => { QuicksortInnerPar<T>(src, j, e, comparer); }
                );
            }
        }
        /// <summary>
        /// Sorts an array of any data type that supports a Comparer in-place using the Parallel Quicksort algorithm.
        /// </summary>
        /// <typeparam name="T">data type of each array element</typeparam>
        /// <param name="src">source array</param>
        /// <param name="aStart">starting index, inclusive</param>
        /// <param name="aLength">number of array elements</param>
        /// <param name="comparer">method to compare array elements</param>
        public static void QuicksortPar<T>(this T[] src, Int32 aStart, Int32 aLength, Comparer<T> comparer = null)
        {
            QuicksortInnerPar(src, aStart, aStart + aLength, comparer);
        }

        public static void QuicksortPar<T>(this T[] src, Comparer<T> comparer = null)
        {
            QuicksortInnerPar(src, 0, src.Length, comparer);
        }

        private static void QuicksortInnerPar(this uint[] src, Int32 b, Int32 e)
        {
            const Int32 cutoff = 4096;

            if (e - b <= cutoff)
            {
                Algorithm.QuicksortHoare(src, b, e);
            }
            else
            {
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
                    tmp = src[i];
                    src[i] = src[j];
                    src[j] = tmp;
                    while (src[++i] < pivot_value) ;
                    while (src[--j] > pivot_value) ;
                }
                j++;

                Parallel.Invoke(
                    () => { QuicksortInnerPar(src, b, j); },
                    () => { QuicksortInnerPar(src, j, e); }
                );
            }
        }
        /// <summary>
        /// Sorts an array of unsigned integers in-place using the Parallel Quicksort algorithm.
        /// </summary>
        /// <param name="src">source array of unsigned integers</param>
        /// <param name="aStart">starting index, inclusive</param>
        /// <param name="aLength">number of array elements</param>
        public static void QuicksortPar(this uint[] src, Int32 aStart, Int32 aLength)
        {
            QuicksortInnerPar(src, aStart, aStart + aLength);
        }

        public static void QuicksortPar(this uint[] src)
        {
            QuicksortInnerPar(src, 0, src.Length);
        }
    }
}
