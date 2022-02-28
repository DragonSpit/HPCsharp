// TODO: Parallelize block-swap algorithm to speedup in-place merge, possibly using SSE with instruction to reverse order within an SSE Vector
//       Or, maybe SSE rotation/reverse order can be avoided, knowing that we'll be rotating it back in the following pass.
// TODO: Parallelize block-swap algorithms to see if there is a benefit, now that unit testing and benchmarking in C# is in place
//       Try simple things first like scalar parallelism of reversal algorithms middle stage of reversal by running the two portions reversal
//       in parallel to see if it speeds up at all.
// TODO: Consider implementing in-place array rotation, such as possibly this (https://www.geeksforgeeks.org/block-swap-algorithm-for-array-rotation/)
//       and do a parallel version as well.
// TODO: Add another termination condition to Gries-Mills block swap algorithm of one of the array portions being a single element, and handle that case
//       with a simple array rotation (if it's worthwhile).
// TODO: Combine Reversal and Gries-Mills algorithms, to eliminate rotation of the smaller half of the array, when it pays off, since now the other
//       half has to be "fixed". There may be certain ratios between halves that work well using one algorithm versus another.
// TODO: Fix a bug with Bentley's Juggling algorithm when the starting index is non-zero.
// TODO: Use Array.Copy to copy 3X faster for those algorithms that don't reverse, which is as fast as SSE copy.
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

        public static void Swap<T>(this T[] array, int indexA, int indexB, int length, bool reverse = false)
        {
            if (!reverse)
            {
                while (length-- > 0)
                {
                    T temp          = array[indexA];                 // inlining Swap() increases performance by 25%
                    array[indexA++] = array[indexB];
                    array[indexB++] = temp;
                }
            }
            else
            {
                int currIndexB = indexB + length - 1;
                while (length-- > 0)
                {
                    T temp              = array[indexA];             // inlining Swap() increases performance by 25%
                    array[indexA++]     = array[currIndexB];
                    array[currIndexB--] = temp;
                }
            }
        }

        public static void SwapArray<T>(this T[] array, int indexA, int indexB, int length, bool reverse = false, int tempBufferSize = 1024)
        {
            T[] tempBuffer = new T[tempBufferSize];

            if (!reverse)
            {
                while ((length - tempBufferSize) > 0)
                {
                    Array.Copy(array,      indexA, tempBuffer, 0,      tempBufferSize);
                    Array.Copy(array,      indexB, array,      indexA, tempBufferSize);
                    Array.Copy(tempBuffer, 0,      array,      indexB, tempBufferSize);
                    length -= tempBufferSize;
                }
                while (length-- > 0)
                {
                    T temp          = array[indexA];                 // inlining Swap() increases performance by 25%
                    array[indexA++] = array[indexB];
                    array[indexB++] = temp;
                }
            }
            else
            {
                int currIndexB = indexB + length - 1;
                while (length-- > 0)
                {
                    T temp              = array[indexA];                 // inlining Swap() increases performance by 25%
                    array[indexA++]     = array[currIndexB];
                    array[currIndexB--] = temp;
                }
            }
        }

        public static void Swap<T>(ref T B, T[] array, int indexA)
        {
            T temp        = array[indexA];
            array[indexA] = B;
            B             = temp;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        // reverse/mirror a range from l to r, inclusively, in-place
        public static void Reversal<T>(this T[] array, int l, int r)
        {
            for(; l < r; l++, r--)
            {
                T temp   = array[l];
                array[l] = array[r];
                array[r] = temp;
            }
        }

        // Swaps two sequential subarrays ranges a[ l .. m ] and a[ m + 1 .. r ]
        public static void BlockSwapReversal<T>(T[] array, int l, int m, int r, int threshold = 1024)
        {
            if ((r - l) < threshold)
            {
                array.Reversal(l,     m);
                array.Reversal(m + 1, r);
                array.Reversal(l,     r);
            }
            else
            {
                Array.Reverse(array, l,     m - l + 1);   // 2X slower than array.Reversal when used in In-Place Merge Sort, but is 2X faster when this funciton is benchmarked by itself
                Array.Reverse(array, m + 1, r - m    );   // Theory: Array.Reverse() has large overhead => does not peform well for small arrays
                Array.Reverse(array, l,     r - l + 1);
            }
        }

        public static void BlockSwapReversalReverseOrder<T>(T[] array, int l, int m, int r)
        {
            array.Reversal(l, r);
            int mInDestination = r - (m - l + 1);
            array.Reversal(l, mInDestination);
            array.Reversal(mInDestination + 1, r);
        }

        public static void BlockSwapGriesMills<T>(T[] array, int l, int m, int r)
        {
            int rotdist = m - l + 1;
            int n       = r - l + 1;
            if (rotdist == 0 || rotdist == n) return;
            int p, i = p = rotdist;
            int j = n - p;
            p += l;
            while (i != j)
            {
                if (i > j)
                {
                    array.Swap(p - i, p, j);
                    i -= j;
                }
                else
                {
                    array.Swap(p - i, p + j - i, i);
                    j -= i;
                }
            }
            array.Swap(p - i, p, i);
        }

        // Greatest Common Divisor.  Assumes that neither input is zero
        public static int GreatestCommonDivisor(int i, int j)
        {
            if (i == 0) return j;
            if (j == 0) return i;
            while (i != j)
            {
                if (i > j) i -= j;
                else j -= i;
            }
            return i;
        }

        public static void BlockSwapJugglingBentley<T>(T[] array, int l, int m, int r)
        {
            int uLength = m - l + 1;
            int vLength = r - m;
            if (uLength <= 0 || vLength <= 0) return;
            int rotdist = m - l + 1;
            int n = r - l + 1;
            int gcdRotdistN = GreatestCommonDivisor(rotdist, n);
            for (int i = 0; i < gcdRotdistN; i++)
            {
                // move i-th values of blocks
                T t = array[i];
                int j = i;
                while (true)
                {
                    int k = j + rotdist;
                    if (k >= n)
                        k -= n;
                    if (k == i) break;
                    array[j] = array[k];
                    j = k;
                }
                array[j] = t;
            }
        }
    }
}
