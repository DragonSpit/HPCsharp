// TODO: This problem is related to https://stackoverflow.com/questions/49308115/c-sharp-vectordouble-copyto-barely-faster-than-non-simd-version?rq=1
// TODO: Could we improve performance even further by having a single array that is a structure of { A, B } elements
//       making it a single array that is brought into the cache. May not be as generally useful, but there may be cases
//       which benefit from it.
// TODO: Implement AddKernelSSE() function that can be composed with other kernels, to make more complex molecules out atomic
//       functions, and keep performance high. No loops, no if statements, no overhead.
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

        public static void AddToSse(this uint[] arrayA, uint[] arrayB)
        {
            arrayA.AddToSseInner(arrayB, 0, arrayA.Length - 1);
        }

        public static void AddToSse(this uint[] arrayA, uint[] arrayB, int start, int length)
        {
            arrayA.AddToSseInner(arrayB, start, start + length - 1);
        }

        private static void AddToSseInner(this uint[] arrayA, uint[] arrayB, int l, int r)
        {
            int sseIndexEnd = l + ((r - l + 1) / Vector<uint>.Count) * Vector<uint>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVectorA = new Vector<uint>(arrayA, i);
                var inVectorB = new Vector<uint>(arrayB, i);
                inVectorA += inVectorB;
                inVectorA.CopyTo(arrayA, i);
            }
            for (; i <= r; i++)
                arrayA[i] += arrayB[i];
        }

        // Loop unrolling, as in the below implementation did not help performance
        private static void AddToSseUnrolled(this int[] arrayA, int[] arrayB)
        {
            arrayA.AddToSseUnrolledInner(arrayB, 0, arrayA.Length - 1);
        }

        private static void AddToSseUnrolled(this int[] arrayA, int[] arrayB, int start, int length)
        {
            arrayA.AddToSseUnrolledInner(arrayB, start, start + length - 1);
        }
        private static void AddToSseUnrolledInner(this int[] arrayA, int[] arrayB, int l, int r)
        {
            int concurrentAmount = 2;
            int sseIndexEnd = l + ((r - l + 1) / (Vector<int>.Count * concurrentAmount)) * (Vector<int>.Count * concurrentAmount);
            int offset1   = Vector<int>.Count;
            int increment = Vector<int>.Count * concurrentAmount;
            int i, j;
            for (i = l, j = l + offset1; i < sseIndexEnd; i += increment, j += increment)
            {
                var inVectorA = new Vector<int>(arrayA, i);
                var inVectorB = new Vector<int>(arrayB, i);
                var inVectorC = new Vector<int>(arrayA, j);
                var inVectorD = new Vector<int>(arrayB, j);
                inVectorA += inVectorB;
                inVectorC += inVectorD;
                inVectorA.CopyTo(arrayA, i);
                inVectorC.CopyTo(arrayA, j);
            }
            for (; i <= r; i++)
                arrayA[i] += arrayB[i];
        }

        private static void AddToSseParInner(this int[] arrayA, int[] arrayB, int l, int r, int thresholdParallel = 16 * 1024)
        {
            if (l > r)
                return;
            if ((r - l + 1) <= thresholdParallel)
            {
                arrayA.AddToSseInner(arrayB, l, r);
                return;
            }

            int m = (r + l) / 2;

            Parallel.Invoke(
                () => { arrayA.AddToSseParInner(arrayB, l,     m, thresholdParallel); },
                () => { arrayA.AddToSseParInner(arrayB, m + 1, r, thresholdParallel); }
            );
        }

        /// <summary>
        /// Add two int[] arrays together, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// </summary>
        /// <param name="arrayA">Input array and the result array</param>
        /// <param name="arrayB">Second input array</param>
        public static void AddToSsePar(this int[] arrayA, int[] arrayB, int thresholdParallel = 16 * 1024)
        {
            arrayA.AddToSseParInner(arrayB, 0, arrayA.Length - 1, thresholdParallel);
        }

        /// <summary>
        /// Add two int[] arrays together, using multiple cores, and using data parallel SIMD/SSE instructions for higher performance within each core.
        /// </summary>
        /// <param name="arrayA">Input array and the result array</param>
        /// <param name="arrayB">Second input array</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        public static void AddToSsePar(this int[] arrayA, int[] arrayB, int startIndex, int length, int thresholdParallel = 16 * 1024)
        {
            arrayA.AddToSseParInner(arrayB, startIndex, startIndex + length - 1, thresholdParallel);
        }

        private static void AddToParInner(this int[] arrayA, int[] arrayB, int l, int r, int thresholdParallel = 16 * 1024)
        {
            //Console.WriteLine("AddToParInner: l = {0}, r = {1}", l, r);
            if (l > r)
                return;
            if ((r - l + 1) <= thresholdParallel)
            {
                HPCsharp.Algorithms.Addition.AddTo(arrayA, arrayB, l, r - l + 1);
                return;
            }

            int m = (r + l) / 2;

            Parallel.Invoke(
                () => { arrayA.AddToParInner(arrayB, l,     m, thresholdParallel); },
                () => { arrayA.AddToParInner(arrayB, m + 1, r, thresholdParallel); }
            );
        }

        /// <summary>
        /// Add two int[] arrays together, using multiple cores.
        /// </summary>
        /// <param name="arrayA">Input array and the result array</param>
        /// <param name="arrayB">Second input array</param>
        public static void AddToPar(this int[] arrayA, int[] arrayB, int thresholdParallel = 16 * 1024)
        {
            arrayA.AddToParInner(arrayB, 0, arrayA.Length - 1, thresholdParallel);
        }

        /// <summary>
        /// Add two int[] arrays together, using multiple cores.
        /// </summary>
        /// <param name="arrayA">Input array and the result array</param>
        /// <param name="arrayB">Second input array</param>
        /// <param name="startIndex">index of the starting element for the summation</param>
        /// <param name="length">number of array elements to sum up</param>
        public static void AddToPar(this int[] arrayA, int[] arrayB, int startIndex, int length, int thresholdParallel = 16 * 1024)
        {
            arrayA.AddToParInner(arrayB, startIndex, startIndex + length - 1, thresholdParallel);
        }
    }
}
