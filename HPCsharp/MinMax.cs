using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPCsharp
{
    public static partial class Algorithm
    {
        /// <summary>
        /// Determines the minimum and maximum elements of a sequence by comparing the elements using the default equality comparer for their type if a comparator is not provided.
        /// Performs a single pass over the sequence, returning both the minimum and maximum elements.
        /// </summary>
        /// <param name="a">source Array to be merged</param>
        /// <param name="comparer">optional method to compare array elements</param>
        /// <returns>Tuple(Min, Max)</returns>
        public static Tuple<T, T> MinMaxHpc<T>(this T[] a, IComparer<T> comparer = null)
        {
            ArgumentNullException.ThrowIfNull(a);

            var equalityComparer = comparer ?? Comparer<T>.Default;
            if (a.Length < 1)
                throw new ArgumentOutOfRangeException(nameof(a), "Array must contain at least one element.");
            if (a.Length == 1)
                return Tuple.Create(a[0], a[0]);

            int lengthDiv2 = a.Length / 2;

            T currMin, currMax;
            if (equalityComparer.Compare(a[0], a[1]) > 0)   // Establish initial min and max
            {
                currMin = a[1];
                currMax = a[0];
            }
            else
            {
                currMin = a[0];
                currMax = a[1];
            }

            int i = 2;
            for (int j = 1; j < lengthDiv2; j++, i += 2)
            {
                if (equalityComparer.Compare(a[i], a[i + 1]) > 0)
                {
                    if (equalityComparer.Compare(currMin, a[i + 1]) > 0)
                        currMin = a[i + 1];
                    if (equalityComparer.Compare(currMax, a[i]) < 0)
                        currMax = a[i];
                }
                else
                {
                    if (equalityComparer.Compare(currMin, a[i]) > 0)
                        currMin = a[i];
                    if (equalityComparer.Compare(currMax, a[i + 1]) < 0)
                        currMax = a[i + 1];
                }
            }
            for (; i < a.Length; i++)
            {
                if (equalityComparer.Compare(currMin, a[i]) > 0)
                    currMin = a[i];
                if (equalityComparer.Compare(currMax, a[i]) < 0)
                    currMax = a[i];
            }
            return Tuple.Create(currMin, currMax);
        }
    }
}
