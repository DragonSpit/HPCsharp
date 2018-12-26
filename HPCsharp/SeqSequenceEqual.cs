// TODO: Implement SequenceCompare, where the user provides a non-default comparison, so that sequences can be compared for less than, equal to and greater than using a custom comparison method
//       This should be easy to parallelize and for the user to provide their own comparison method for their own custom user classes. An easy and consistent way would be to expose the 
//       Comparer in the same way we did for Merge Sort.
// TODO: Add the ability to pass an equality comparison, similar to C# standard routines to support equality comparison for user defined classes.
using System;
using System.Collections;
using System.Collections.Generic;

// IEnumerable is 4X slower than List or Array
// IList       is 2X slower than List

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
        ///   TSource:
        ///     The type of the elements of the input sequences.
        ///
        /// Returns:
        ///     true if the two source sequences, each within l and r bounds, and their corresponding
        ///     elements are equal according to the default equality comparer for their type;
        ///     otherwise, false.
        ///
        /// Exceptions:
        ///   TSource:System.ArgumentNullException: first or second is null.
        ///   TSource:System.ArgumentOutOfRangeException: if l or r is not inside the array bounds
        /// </summary>
        public static bool SequenceEqualHpc<TSource>(this TSource[] first, TSource[] second, Int32 l, Int32 r)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (l > r)      // zero elements to compare
                return true;
            if (!(l >= 0 && r < first.Length && r >= 0 && r < second.Length))
                throw new System.ArgumentOutOfRangeException();

            var equalityComparer = Comparer<TSource>.Default;
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
        ///   TSource:
        ///     The type of the elements of the input sequences.
        ///
        /// Returns:
        ///     true if the two source sequences are of equal length and their corresponding
        ///     elements are equal according to the default equality comparer for their type;
        ///     otherwise, false.
        ///
        /// Exceptions:
        ///   TSource:System.ArgumentNullException: first or second is null.
        /// </summary>
        public static bool SequenceEqualHpc<TSource>(this TSource[] first, TSource[] second)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Length != second.Length)
                return false;

            var equalityComparer = Comparer<TSource>.Default;
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
        ///   TSource:
        ///     The type of the elements of the input sequences.
        ///
        /// Returns:
        ///     true if the two source sequences, each within l and r bounds, and their corresponding
        ///     elements are equal according to the default equality comparer for their type;
        ///     otherwise, false.
        ///
        /// Exceptions:
        ///   TSource:System.ArgumentNullException: first or second is null.
        ///   TSource:System.ArgumentOutOfRangeException: if l or r is not inside the array bounds
        /// </summary>
        public static bool SequenceEqualHpc<TSource>(this List<TSource> first, List<TSource> second, Int32 l, Int32 r)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (!(l >= 0 && r < first.Count && r >= 0 && r < second.Count))
                throw new System.ArgumentOutOfRangeException();

            var equalityComparer = Comparer<TSource>.Default;
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
        ///   TSource:
        ///     The type of the elements of the input sequences.
        ///
        /// Returns:
        ///     true if the two source sequences are of equal length and their corresponding
        ///     elements are equal according to the default equality comparer for their type;
        ///     otherwise, false.
        ///
        /// Exceptions:
        ///   TSource:System.ArgumentNullException: first or second is null.
        /// </summary>
        public static bool SequenceEqualHpc<TSource>(this List<TSource> first, List<TSource> second)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Count != second.Count)
                return false;

            var equalityComparer = Comparer<TSource>.Default;
            for (Int32 i = 0; i < first.Count; i++)
            {
                if (equalityComparer.Compare(first[i], second[i]) != 0)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Test equality of two ArrayLists
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool SequenceEqualHpc(this ArrayList first, ArrayList second)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Count != second.Count)
                return false;

            var equalityComparer = Comparer.Default;
            for (Int32 i = 0; i < first.Count; i++)
            {
                if (equalityComparer.Compare(first[i], second[i]) != 0)
                    return false;
            }
            return true;
        }
    }
}
