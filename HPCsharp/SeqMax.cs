using System;
using System.Collections.Generic;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        /// <summary>
        /// Summary:
        ///     Finds the minimum element of a sequences by comparing the elements using
        ///     the default equality comparer for their type if a comparator is not provided.
        ///
        /// Parameters:
        ///   a:
        ///     An array to find the minimum element of.
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
        ///     the minimum element within the sequence, within l and r bounds, and
        ///     according to the default equality comparer for their type;
        ///
        /// Exceptions:
        ///   T:System.ArgumentNullException: if array is null.
        ///   T:System.ArgumentOutOfRangeException: if l or r is not inside the array bounds
        /// </summary>
        public static T HpcMax<T>(this T[] a, Int32 l, Int32 r, Comparer<T> comparer = null)
        {
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
                if (equalityComparer.Compare(currMin, a[i]) < 0)
                    currMin = a[i];
            }
            return currMin;
        }
        /// <summary>
        /// Summary:
        ///     Finds the minimum element of a sequences by comparing the elements using
        ///     the default equality comparer for their type if a comparator is not provided.
        ///
        /// Parameters:
        ///   a:
        ///     An array to find the minimum element of.
        ///
        /// Type parameters:
        ///   T:
        ///     The type of the elements of the input sequences.
        ///
        /// Returns:
        ///     the minimum element within the sequence, within l and r bounds, and
        ///     according to the default equality comparer for their type;
        ///
        /// Exceptions:
        ///   T:System.ArgumentNullException: if array is null.
        /// </summary>
        public static T HpcMax<T>(this T[] a, Comparer<T> comparer = null)
        {
            if (a == null)
                throw new System.ArgumentNullException();

            var equalityComparer = comparer ?? Comparer<T>.Default;
            T currMin = a[0];
            for (Int32 i = 1; i < a.Length; i++)
            {
                if (equalityComparer.Compare(currMin, a[i]) < 0)
                    currMin = a[i];
            }
            return currMin;
        }
        /// <summary>
        /// Summary:
        ///     Finds the minimum element of a sequences by comparing the elements using
        ///     the default equality comparer for their type if a comparator is not provided.
        ///
        /// Parameters:
        ///   a:
        ///     A List to find the minimum element of.
        ///
        ///   l:
        ///     Index of the left List element: the starting List element for the comparison (inclusive)
        ///
        ///   r:
        ///     Index of the right List element: the ending List element for the comparison (inclusive)
        ///
        ///   comparer:
        ///     optional comparer, which returns an integer (-1, 0, 1) to indicate less than, equal to, or greater than
        ///
        /// Type parameters:
        ///   T:
        ///     The type of the elements of the input sequences.
        ///
        /// Returns:
        ///     the minimum element within the sequence, within l and r bounds, and
        ///     according to the default equality comparer for their type;
        ///
        /// Exceptions:
        ///   T:System.ArgumentNullException: if List is null.
        ///   T:System.ArgumentOutOfRangeException: if l or r is not inside the array bounds
        /// </summary>
        public static T HpcMax<T>(this List<T> a, Int32 l, Int32 r, Comparer<T> comparer = null)
        {
            if (a == null)
                throw new System.ArgumentNullException();
            if (!(l >= 0 && r < a.Count))
                throw new System.ArgumentOutOfRangeException();

            var equalityComparer = comparer ?? Comparer<T>.Default;
            T currMin = a[l];
            for (Int32 i = l + 1; i <= r; i++)     // inclusive of l and r
            {
                if (equalityComparer.Compare(currMin, a[i]) < 0)
                    currMin = a[i];
            }
            return currMin;
        }
        /// <summary>
        /// Summary:
        ///     Finds the minimum element of a sequences by comparing the elements using
        ///     the default equality comparer for their type if a comparator is not provided.
        ///
        /// Parameters:
        ///   a:
        ///     A List to find the minimum element of.
        ///
        /// Type parameters:
        ///   T:
        ///     The type of the elements of the input sequences.
        ///
        /// Returns:
        ///     the minimum element within the sequence, within l and r bounds, and
        ///     according to the default equality comparer for their type;
        ///
        /// Exceptions:
        ///   T:System.ArgumentNullException: if List is null.
        /// </summary>
        public static T HpcMax<T>(this List<T> a, Comparer<T> comparer = null)
        {
            if (a == null)
                throw new System.ArgumentNullException();

            var equalityComparer = comparer ?? Comparer<T>.Default;
            T currMin = a[0];
            for (Int32 i = 1; i < a.Count; i++)
            {
                if (equalityComparer.Compare(currMin, a[i]) < 0)
                    currMin = a[i];
            }
            return currMin;
        }
    }
}