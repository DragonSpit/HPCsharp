using System;
using System.Linq;
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
        public static T HpcMin<T>(this T[] a, Int32 l, Int32 r, Comparer<T> comparer = null)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (a == null)
                throw new System.ArgumentNullException();
            if (l > r)      // zero elements to compare
                throw new System.ArgumentOutOfRangeException();
            if (!(l >= 0 && r < a.Length))
                throw new System.ArgumentOutOfRangeException();

            var equalityComparer = comparer ?? Comparer<T>.Default;
            T currMin = a[l];
            for (Int32 i = l + 1; i <= r; i++)     // inclusive of l and r
            {
                if (equalityComparer.Compare(currMin, a[i]) > 0)    // TODO: check the correctness of < 0 for Min
                    currMin = a[i];
            }
            return currMin;
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
        public static T HpcMin<T>(this T[] a, Comparer<T> comparer = null)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (a == null)
                throw new System.ArgumentNullException();

            var equalityComparer = comparer ?? Comparer<T>.Default;
            T currMin = a[0];
            for (Int32 i = 1; i < a.Length; i++)
            {
                if (equalityComparer.Compare(currMin, a[i]) > 0)
                    currMin = a[i];
            }
            return currMin;
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
        public static T HpcMin<T>(this List<T> a, Int32 l, Int32 r, Comparer<T> comparer = null)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (a == null)
                throw new System.ArgumentNullException();
            if (!(l >= 0 && r < a.Count))
                throw new System.ArgumentOutOfRangeException();

            var equalityComparer = comparer ?? Comparer<T>.Default;
            T currMin = a[l];
            for (Int32 i = l + 1; i <= r; i++)     // inclusive of l and r
            {
                if (equalityComparer.Compare(currMin, a[i]) > 0)    // TODO: check the correctness of < 0 for Min
                    currMin = a[i];
            }
            return currMin;
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
        public static T HpcMin<T>(this List<T> a, Comparer<T> comparer = null)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (a == null)
                throw new System.ArgumentNullException();

            var equalityComparer = comparer ?? Comparer<T>.Default;
            T currMin = a[0];
            for (Int32 i = 1; i < a.Count; i++)
            {
                if (equalityComparer.Compare(currMin, a[i]) > 0)
                    currMin = a[i];
            }
            return currMin;
        }
    }
}