// TODO: Add these algorithms to the Readme table/list of algorithms. Now, that we have SSE that's way faster than scalar.
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System;
using System.Threading.Tasks;

namespace HPCsharp.Algorithms
{
    static public partial class Addition
    {
        public static int[] Add(this int[] arrayA, int[] arrayB)
        {
            if (arrayA == null)
                throw new ArgumentNullException(nameof(arrayA));
            if (arrayB == null)
                throw new ArgumentNullException(nameof(arrayB));
            return arrayA.AddInner(arrayB, 0, arrayA.Length - 1);
        }

        public static int[] Add(this int[] arrayA, int[] arrayB, int start, int length)
        {
            if (arrayA == null)
                throw new ArgumentNullException(nameof(arrayA));
            if (arrayB == null)
                throw new ArgumentNullException(nameof(arrayB));
            return arrayA.AddInner(arrayB, start, start + length - 1);
        }

        private static int[] AddInner(this int[] arrayA, int[] arrayB, int l, int r)
        {
            var addArray = new int[arrayA.Length];

            for (int i = l; i <= r; i++)
                addArray[i] = arrayA[i] + arrayB[i];

            return addArray;
        }


        public static void AddTo(this int[] arrayA, int[] arrayB)
        {
            if (arrayA == null)
                throw new ArgumentNullException(nameof(arrayA));
            if (arrayB == null)
                throw new ArgumentNullException(nameof(arrayB));
            arrayA.AddToInner(arrayB, 0, arrayA.Length - 1);
        }

        public static void AddTo(this int[] arrayA, int[] arrayB, int start, int length)
        {
            if (arrayA == null)
                throw new ArgumentNullException(nameof(arrayA));
            if (arrayB == null)
                throw new ArgumentNullException(nameof(arrayB));
            arrayA.AddToInner(arrayB, start, start + length - 1);
        }

        private static void AddToInner(this int[] arrayA, int[] arrayB, int l, int r)
        {
            for (int i = l; i <= r; i++)
                arrayA[i] += arrayB[i];
        }
    }
}
