// TODO: Implement multi-core versions of the two Add algorithms, to see if we can go even faster.
// TODO: Try loop unrolling for the SSE implementation to see if it gains in performance.
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System;

namespace HPCsharp.ParallelAlgorithms
{
    static public partial class Addition
    {
        public static int[] AddSse(this int[] arrayA, int[] arrayB)
        {
            return arrayA.AddSseInner(arrayB, 0, arrayA.Length - 1);
        }

        public static int[] AddSse(this int[] arrayA, int[] arrayB, int start, int length)
        {
            return arrayA.AddSseInner(arrayB, start, start + length - 1);
        }

        private static int[] AddSseInner(this int[] arrayA, int[] arrayB, int l, int r)
        {
            var addArray = new int[arrayA.Length];
            int sseIndexEnd = l + ((r - l + 1) / Vector<int>.Count) * Vector<int>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVectorA = new Vector<int>(arrayA, i);
                var inVectorB = new Vector<int>(arrayB, i);
                (inVectorA + inVectorB).CopyTo(addArray, i);
            }
            for (; i <= r; i++)
                addArray[i] = arrayA[i] + arrayB[i];
            return addArray;
        }

        public static void AddToSse(this int[] arrayA, int[] arrayB)
        {
            arrayA.AddToSseInner(arrayB, 0, arrayA.Length - 1);
        }

        public static void AddToSse(this int[] arrayA, int[] arrayB, int start, int length)
        {
            arrayA.AddToSseInner(arrayB, start, start + length - 1);
        }

        private static void AddToSseInner(this int[] arrayA, int[] arrayB, int l, int r)
        {
            int sseIndexEnd = l + ((r - l + 1) / Vector<int>.Count) * Vector<int>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVectorA = new Vector<int>(arrayA, i);
                var inVectorB = new Vector<int>(arrayB, i);
                inVectorA += inVectorB;
                inVectorA.CopyTo(arrayA, i);
            }
            for (; i <= r; i++)
                arrayA[i] += arrayB[i];
        }
    }
}
