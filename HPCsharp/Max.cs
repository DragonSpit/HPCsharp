// TODO: Add SIMD/SSE acceleration for Max and Min for built-in types
// TODO: Implement SIMD/SSE acceleration for MinMax (bounds) for built-in types
// TODO: Add multi-core versions of these
// TODO: Add the ability for the user to pass in their own comparer function
// TODO: Add SIMD/SSE accelerated version where the user passes in a function to extract a key from their user defined class to perform a Max operation. Multi-core will still be possible, but not SSE.
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
        ///   TSource:
        ///     The type of the elements of the input sequences.
        ///
        /// Returns:
        ///     the minimum element within the sequence, within l and r bounds, and
        ///     according to the default equality comparer for their type;
        ///
        /// Exceptions:
        ///   TSource:System.ArgumentNullException: if array is null.
        ///   TSource:System.ArgumentOutOfRangeException: if l or r is not inside the array bounds
        /// </summary>
        public static TSource MaxHpc<TSource>(this TSource[] a, Int32 l, Int32 r)
        {
            if (a == null)
                throw new System.ArgumentNullException();
            if (l > r)      // zero elements to compare
                throw new System.ArgumentOutOfRangeException();
            if (!(l >= 0 && r < a.Length))
                throw new System.ArgumentOutOfRangeException();

            var equalityComparer = Comparer<TSource>.Default;
            TSource currMax = a[l];
            for (Int32 i = l + 1; i <= r; i++)     // inclusive of l and r
            {
                if (equalityComparer.Compare(currMax, a[i]) < 0)
                    currMax = a[i];
            }
            return currMax;
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
        ///   TSource:
        ///     The type of the elements of the input sequences.
        ///
        /// Returns:
        ///     the minimum element within the sequence, within l and r bounds, and
        ///     according to the default equality comparer for their type;
        ///
        /// Exceptions:
        ///   TSource:System.ArgumentNullException: if array is null.
        /// </summary>
        public static TSource MaxHpc<TSource>(this TSource[] a)
        {
            if (a == null)
                throw new System.ArgumentNullException();

            var equalityComparer = Comparer<TSource>.Default;
            TSource currMax = a[0];
            for (Int32 i = 1; i < a.Length; i++)
            {
                if (equalityComparer.Compare(currMax, a[i]) < 0)
                    currMax = a[i];
            }
            return currMax;
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
        ///   TSource:
        ///     The type of the elements of the input sequences.
        ///
        /// Returns:
        ///     the minimum element within the sequence, within l and r bounds, and
        ///     according to the default equality comparer for their type;
        ///
        /// Exceptions:
        ///   TSource:System.ArgumentNullException: if List is null.
        ///   TSource:System.ArgumentOutOfRangeException: if l or r is not inside the array bounds
        /// </summary>
        public static TSource MaxHpc<TSource>(this List<TSource> a, Int32 l, Int32 r)
        {
            if (a == null)
                throw new System.ArgumentNullException();
            if (!(l >= 0 && r < a.Count))
                throw new System.ArgumentOutOfRangeException();

            var equalityComparer = Comparer<TSource>.Default;
            TSource currMax = a[l];
            for (Int32 i = l + 1; i <= r; i++)     // inclusive of l and r
            {
                if (equalityComparer.Compare(currMax, a[i]) < 0)
                    currMax = a[i];
            }
            return currMax;
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
        ///   TSource:
        ///     The type of the elements of the input sequences.
        ///
        /// Returns:
        ///     the minimum element within the sequence, within l and r bounds, and
        ///     according to the default equality comparer for their type;
        ///
        /// Exceptions:
        ///   TSource:System.ArgumentNullException: if List is null.
        /// </summary>
        public static TSource MaxHpc<TSource>(this List<TSource> a)
        {
            if (a == null)
                throw new System.ArgumentNullException();

            var equalityComparer = Comparer<TSource>.Default;
            TSource currMax = a[0];
            for (Int32 i = 1; i < a.Count; i++)
            {
                if (equalityComparer.Compare(currMax, a[i]) < 0)
                    currMax = a[i];
            }
            return currMax;
        }
    }
}