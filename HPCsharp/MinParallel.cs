// TODO: Implement Min SSE versions as generic and use the data type run-time detection, as other algorithms do and a switch statement to route to the proper SSE implementation
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using System;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
        public static int MinSse(this int[] arrayToMin)
        {
            if (arrayToMin == null)
                throw new ArgumentNullException("Min cannot be determined for a null array");
            if (arrayToMin.Length == 0)
                throw new ArgumentException("Min cannot be determined for an empty array");
            return arrayToMin.MinSseInner(0, arrayToMin.Length - 1);
        }

        public static int MinSse(this int[] arrayToMin, int start, int length)
        {
            if (arrayToMin == null)
                throw new ArgumentNullException("Min cannot be determined for a null array");
            if (arrayToMin.Length == 0 || length == 0)
                throw new ArgumentException("Min cannot be determined for an empty array or length argument of zero");
            return arrayToMin.MinSseInner(start, start + length - 1);
        }

        // Assumes that at least one element in an array to be processed and l <= r to also ensure that at least one element is being processed, to ensure a Min result is always possible
        private static int MinSseInner(this int[] arrayToMin, int l, int r)
        {
            var minVector = new Vector<int>();
            int sseIndexEnd = l + ((r - l + 1) / Vector<int>.Count) * Vector<int>.Count;
            int i = l;
            if (i < sseIndexEnd)
            {
                minVector = new Vector<int>(arrayToMin, i);
                i += Vector<int>.Count;
            }
            for (; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVector = new Vector<int>(arrayToMin, i);
                minVector = Vector.Min(minVector, inVector);
            }
            int overallMin = minVector[0];
            for (int j = 1; j < Vector<int>.Count; j++)
                overallMin = Math.Min(overallMin, minVector[j]);
            for (; i <= r; i++)
                overallMin = Math.Min(overallMin, arrayToMin[i]);
            return overallMin;
        }

        public static int ThresholdParallelMin { get; set; } = 16 * 1024;

        private static int MinSseParInner(this int[] arrayToMin, int l, int r)
        {
            if ((r - l + 1) <= ThresholdParallelMin || (r - l + 1) <= 2)    // protect against either half of divide-and-conquer from having zero elements
                return MinSseInner(arrayToMin, l, r - l + 1);

            int m = (r + l) / 2;

            int minLeft  = 0;
            int minRight = 0;

            Parallel.Invoke(
                () => { minLeft  = MinSseParInner(arrayToMin, l,     m); },
                () => { minRight = MinSseParInner(arrayToMin, m + 1, r); }
            );
            // Combine left and right results
            return Math.Min(minLeft, minRight);
        }

        public static int MinSsePar(this int[] arrayToMin)
        {
            if (arrayToMin == null)
                throw new ArgumentNullException("Min cannot be determined for a null array");
            if (arrayToMin.Length == 0)
                throw new ArgumentException("Min cannot be determined for an empty array");
            return MinSseParInner(arrayToMin, 0, arrayToMin.Length - 1);
        }

        public static int MinSsePar(this int[] arrayToMin, int start, int length)
        {
            if (arrayToMin == null)
                throw new ArgumentNullException("Min cannot be determined for a null array");
            if (arrayToMin.Length == 0 || length == 0)
                throw new ArgumentException("Min cannot be determined for an empty array or length argument of zero");
            return arrayToMin.MinSseParInner(start, start + length - 1);
        }
    }
}