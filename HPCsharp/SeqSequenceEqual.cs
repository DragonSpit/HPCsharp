using System;
using System.Collections.Generic;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        /// <summary>
        /// Summary:
        ///     Determines whether two sequences are equal by comparing the elements by using
        ///     the default equality comparer for their type.
        ///
        /// Parameters:
        ///   first:
        ///     An array to compare to second.
        ///
        ///   second:
        ///     An array to compare to the first sequence.
        ///
        ///   l:
        ///     Index of the left array element: the starting array element for the comparison (inclusive)
        ///
        ///   r:
        ///     Index of the right array element: the ending array element for the comparison (inclusive)
        ///
        ///   comparer:
        ///     optional comparer, which returns an integer (-1, 0, 1) to indicate less than, equal to, or greater than
        ///
        /// Type parameters:
        ///   T:
        ///     The type of the elements of the input sequences.
        ///
        /// Returns:
        ///     true if the two source sequences, each within l and r bounds, and their corresponding
        ///     elements are equal according to the default equality comparer for their type;
        ///     otherwise, false.
        ///
        /// Exceptions:
        ///   T:System.ArgumentNullException: first or second is null.
        ///   T:System.ArgumentOutOfRangeException: if l or r is not inside the array bounds
        /// </summary>
        public static bool HpcSequenceEqual<T>(this T[] first, T[] second, Int32 l, Int32 r, Comparer<T> comparer = null)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (l > r)      // zero elements to compare
                return true;
            if (!(l >= 0 && r < first.Length && r >= 0 && r < second.Length))
                throw new System.ArgumentOutOfRangeException();

            var equalityComparer = comparer ?? Comparer<T>.Default;
            for (Int32 i = l; i <= r; i++)     // inclusive of l and r
            {
                if (equalityComparer.Compare(first[i], second[i]) != 0)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Summary:
        ///     Determines whether two sequences are equal by comparing the elements by using
        ///     the default equality comparer for their type.
        ///
        /// Parameters:
        ///   first:
        ///     An array to compare to second.
        ///
        ///   second:
        ///     An array to compare to the first sequence.
        ///
        /// Type parameters:
        ///   T:
        ///     The type of the elements of the input sequences.
        ///
        /// Returns:
        ///     true if the two source sequences are of equal length and their corresponding
        ///     elements are equal according to the default equality comparer for their type;
        ///     otherwise, false.
        ///
        /// Exceptions:
        ///   T:System.ArgumentNullException: first or second is null.
        /// </summary>
        public static bool HpcSequenceEqual<T>(this T[] first, T[] second, Comparer<T> comparer = null)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Length != second.Length)
                return false;

            var equalityComparer = comparer ?? Comparer<T>.Default;
            for (Int32 i = 0; i < first.Length; i++)
            {
                if (equalityComparer.Compare(first[i], second[i]) != 0)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Summary:
        ///     Determines whether two sequences are equal by comparing the elements by using
        ///     the default equality comparer for their type.
        ///
        /// Parameters:
        ///   first:
        ///     A list to compare to the second sequence.
        ///
        ///   second:
        ///     A list to compare to the first sequence.
        ///
        ///   l:
        ///     Index of the left array element: the starting array element for the comparison (inclusive)
        ///
        ///   r:
        ///     Index of the right array element: the ending array element for the comparison (inclusive)
        ///
        ///   comparer:
        ///     optional comparer, which returns an integer (-1, 0, 1) to indicate less than, equal to, or greater than
        ///
        /// Type parameters:
        ///   T:
        ///     The type of the elements of the input sequences.
        ///
        /// Returns:
        ///     true if the two source sequences, each within l and r bounds, and their corresponding
        ///     elements are equal according to the default equality comparer for their type;
        ///     otherwise, false.
        ///
        /// Exceptions:
        ///   T:System.ArgumentNullException: first or second is null.
        ///   T:System.ArgumentOutOfRangeException: if l or r is not inside the array bounds
        /// </summary>
        public static bool HpcSequenceEqual<T>(this List<T> first, List<T> second, Int32 l, Int32 r, Comparer<T> comparer = null)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (!(l >= 0 && r < first.Count && r >= 0 && r < second.Count))
                throw new System.ArgumentOutOfRangeException();

            var equalityComparer = comparer ?? Comparer<T>.Default;
            for (Int32 i = l; i <= r; i++)     // inclusive of l and r
            {
                if (equalityComparer.Compare(first[i], second[i]) != 0)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Summary:
        ///     Determines whether two sequences are equal by comparing the elements by using
        ///     the default equality comparer for their type.
        ///
        /// Parameters:
        ///   first:
        ///     A list to compare to the second sequence.
        ///
        ///   second:
        ///     A list to compare to the first sequence.
        ///
        /// Type parameters:
        ///   T:
        ///     The type of the elements of the input sequences.
        ///
        /// Returns:
        ///     true if the two source sequences are of equal length and their corresponding
        ///     elements are equal according to the default equality comparer for their type;
        ///     otherwise, false.
        ///
        /// Exceptions:
        ///   T:System.ArgumentNullException: first or second is null.
        /// </summary>
        public static bool HpcSequenceEqual<T>(this List<T> first, List<T> second, Comparer<T> comparer = null)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Count != second.Count)
                return false;

            var equalityComparer = comparer ?? Comparer<T>.Default;
            for (Int32 i = 0; i < first.Count; i++)
            {
                if (equalityComparer.Compare(first[i], second[i]) != 0)
                    return false;
            }
            return true;
        }
    }
}
