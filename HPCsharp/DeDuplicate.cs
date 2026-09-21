// TODO: Implement a generic version of the Unique algorithm that works with any type that implements IComparable<T> or IEquatable<T> since this algorithm uses comparisons.
using System;

namespace HPCsharp
{
    public static partial class Algorithm
    {
        /// <summary>
        /// In-place removal of duplicates algorithm. Stable algorithm.
        /// Unique elements are returned in the first part of the array. The return value is the number of unique elements.
        /// </summary>
        /// <param name="arrayToDeDup">array that is to be sorted in place</param>
        /// <returns>The number of unique elements in the array</returns>
        public static int Unique(this uint[] arrayToDeDup)
        {
            if (arrayToDeDup == null)
                throw new ArgumentNullException(nameof(arrayToDeDup));
            int rIndex = 0, wIndex = 0;
            for (; (rIndex + 1) < arrayToDeDup.Length; rIndex++)
            {
                arrayToDeDup[wIndex] = arrayToDeDup[rIndex];
                if (arrayToDeDup[rIndex] != arrayToDeDup[rIndex + 1])
                    wIndex++;
            }
            arrayToDeDup[wIndex] = arrayToDeDup[rIndex];  // copy the last array element to its proper index
            return wIndex + 1;
        }

        private static int DeDuplicatePriv(this uint[] arrayToDeDup)
        {
            HPCsharp.Algorithm.SortRadixMsd(arrayToDeDup);
            return Unique(arrayToDeDup);
        }

        /// <summary>
        /// In-place DeDuplicate algorithm, which removes duplicate array elements. Not a stable algorithm.
        /// The array is sorted in place and the unique elements are returned in the first part of the array. The return value is the number of unique elements.
        /// </summary>
        /// <param name="arrayToDeDup">array that is to be deduplicated in place</param>
        /// <returns>The number of unique elements in the array</returns>
        public static int DeDuplicate(this uint[] arrayToDeDup)
        {
            if (arrayToDeDup == null)
                throw new ArgumentNullException(nameof(arrayToDeDup));
            return DeDuplicatePriv(arrayToDeDup);
        }
    }
}
