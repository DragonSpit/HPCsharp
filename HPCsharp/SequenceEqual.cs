// TODO: Implement SequenceCompare, where the user provides a non-default comparison, so that sequences can be compared for less than, equal to and greater than using a custom comparison method
//       This should be easy to parallelize and for the user to provide their own comparison method for their own custom user classes. An easy and consistent way would be to expose the 
//       Comparer in the same way we did for Merge Sort.
// TODO: Add the ability to pass an equality lambda function.
// TODO: Add the ability to determine equality of a List of sequences, serial and parallel.
// TODO: Create a more general SequenceOperate() where a function is passed in that returns a bool for example which operates across the arrays, and then another function that works
//       along the array to combine the results of each from the previous step. MapReduce - the user would pass in a Map function and a Reduce function. Linq has some functionality
//       but may not work across multiple arrays (need to verify).
// TODO: Create a sequence equals that understands "sorting stability" and does an equals on stable or unstable sorted arrays.
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
        /// Determines whether two sequences are equal by comparing the elements by using
        /// the default equality comparer for their type.
        /// </summary>
        /// <typeparam name="T">array of type TSource</typeparam>
        /// <param name="first">An array to compare to second</param>
        /// <param name="second">An array to compare to the first sequence</param>
        /// <param name="l">Index of the left array element: the starting array element for the comparison (inclusive)</param>
        /// <param name="r">Index of the right array element: the ending array element for the comparison (inclusive)</param>
        /// <returns>true is the two arrays are of equal length and corresponding elements are all equal, false if they are not equal</returns>
        /// <exception>TSource:System.ArgumentNullException: first or second is null.</exception>
        /// <exception>TSource:System.ArgumentOutOfRangeException: if l or r is not inside the array bounds.</exception>
        public static bool SequenceEqualHpc<T>(this T[] first, T[] second, Int32 l, Int32 r)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (l > r)      // zero elements to compare
                return true;
            if (!(l >= 0 && r < first.Length && r >= 0 && r < second.Length))
                throw new System.ArgumentOutOfRangeException();

            var equalityComparer = Comparer<T>.Default;
            for (Int32 i = l; i <= r; i++)     // inclusive of l and r
            {
                if (equalityComparer.Compare(first[i], second[i]) != 0)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements by using
        /// the passed in equality comparer.
        /// </summary>
        /// <typeparam name="T">array of type TSource</typeparam>
        /// <param name="first">An array to compare to second</param>
        /// <param name="second">An array to compare to the first sequence</param>
        /// <param name="l">Index of the left array element: the starting array element for the comparison (inclusive)</param>
        /// <param name="r">Index of the right array element: the ending array element for the comparison (inclusive)</param>
        /// <param name="equalityComparer">equality comparer used to compare two array elements of type TSource</param>
        /// <returns>true is the two arrays are of equal length and corresponding elements are all equal, false if they are not equal</returns>
        /// <exception>TSource:System.ArgumentNullException: first or second is null.</exception>
        /// <exception>TSource:System.ArgumentOutOfRangeException: if l or r is not inside the array bounds.</exception>
        public static bool SequenceEqualHpc<T>(this T[] first, T[] second, Int32 l, Int32 r, IEqualityComparer<T> equalityComparer)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (l > r)      // zero elements to compare
                return true;
            if (!(l >= 0 && r < first.Length && r >= 0 && r < second.Length))
                throw new System.ArgumentOutOfRangeException();

            for (Int32 i = l; i <= r; i++)     // inclusive of l and r
            {
                if (!equalityComparer.Equals(first[i], second[i]))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements by using
        /// the default equality comparer for their type.
        /// </summary>
        /// <typeparam name="T">array of type TSource</typeparam>
        /// <param name="first">An array to compare to second</param>
        /// <param name="second">An array to compare to the first sequence</param>
        /// <returns>true is the two arrays are of equal length and corresponding elements are all equal, false if they are not equal</returns>
        /// <exception>TSource:System.ArgumentNullException: first or second is null.</exception>
        public static bool SequenceEqualHpc<T>(this T[] first, T[] second)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Length != second.Length)
                return false;

            var equalityComparer = Comparer<T>.Default;
            for (Int32 i = 0; i < first.Length; i++)
            {
                if (equalityComparer.Compare(first[i], second[i]) != 0)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements by using
        /// the passed in equality comparer.
        /// </summary>
        /// <typeparam name="T">array of type TSource</typeparam>
        /// <param name="first">An array to compare to second</param>
        /// <param name="second">An array to compare to the first sequence</param>
        /// <param name="equalityComparer">equality comparer used to compare two array elements of type TSource</param>
        /// <returns>true is the two arrays are of equal length and corresponding elements are all equal, false if they are not equal</returns>
        /// <exception>TSource:System.ArgumentNullException: first or second is null.</exception>
        public static bool SequenceEqualHpc<T>(this T[] first, T[] second, IEqualityComparer<T> equalityComparer)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Length != second.Length)
                return false;

            for (Int32 i = 0; i < first.Length; i++)
            {
                if (!equalityComparer.Equals(first[i], second[i]))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements by using
        /// the default equality comparer for their type.
        /// </summary>
        /// <typeparam name="T">List of type TSource</typeparam>
        /// <param name="first">A List to compare to second sequence</param>
        /// <param name="second">A List to compare to the first sequence</param>
        /// <param name="l">Index of the left List element: the starting List element for the comparison (inclusive)</param>
        /// <param name="r">Index of the right List element: the ending List element for the comparison (inclusive)</param>
        /// <returns>true is the two Lists are of equal length and corresponding elements are all equal, false if they are not equal</returns>
        /// <exception>TSource:System.ArgumentNullException: first or second is null.</exception>
        /// <exception>TSource:System.ArgumentOutOfRangeException: if l or r is not inside the List bounds.</exception>
        public static bool SequenceEqualHpc<T>(this List<T> first, List<T> second, Int32 l, Int32 r)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (!(l >= 0 && r < first.Count && r >= 0 && r < second.Count))
                throw new System.ArgumentOutOfRangeException();

            var equalityComparer = Comparer<T>.Default;
            for (Int32 i = l; i <= r; i++)     // inclusive of l and r
            {
                if (equalityComparer.Compare(first[i], second[i]) != 0)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements by using
        /// the passed in equality comparer.
        /// </summary>
        /// <typeparam name="T">List of type TSource</typeparam>
        /// <param name="first">A List to compare to second</param>
        /// <param name="second">A List to compare to the first sequence</param>
        /// <param name="l">Index of the left List element: the starting List element for the comparison (inclusive)</param>
        /// <param name="r">Index of the right List element: the ending List element for the comparison (inclusive)</param>
        /// <param name="equalityComparer">equality comparer used to compare two List elements of type TSource</param>
        /// <returns>true is the two Lists are of equal length and corresponding elements are all equal, false if they are not equal</returns>
        /// <exception>TSource:System.ArgumentNullException: first or second is null.</exception>
        /// <exception>TSource:System.ArgumentOutOfRangeException: if l or r is not inside the List bounds.</exception>
        public static bool SequenceEqualHpc<T>(this List<T> first, List<T> second, Int32 l, Int32 r, IEqualityComparer<T> equalityComparer)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (!(l >= 0 && r < first.Count && r >= 0 && r < second.Count))
                throw new System.ArgumentOutOfRangeException();

            for (Int32 i = l; i <= r; i++)     // inclusive of l and r
            {
                if (!equalityComparer.Equals(first[i], second[i]))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements by using
        /// the default equality comparer for their type.
        /// </summary>
        /// <typeparam name="T">List of type TSource</typeparam>
        /// <param name="first">A List to compare to second sequence</param>
        /// <param name="second">A List to compare to the first sequence</param>
        /// <returns>true is the two Lists are of equal length and corresponding elements are all equal, false if they are not equal</returns>
        /// <exception>TSource:System.ArgumentNullException: first or second is null.</exception>
        /// <exception>TSource:System.ArgumentOutOfRangeException: if l or r is not inside the List bounds.</exception>
        public static bool SequenceEqualHpc<T>(this List<T> first, List<T> second)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Count != second.Count)
                return false;

            var equalityComparer = Comparer<T>.Default;
            for (Int32 i = 0; i < first.Count; i++)
            {
                if (equalityComparer.Compare(first[i], second[i]) != 0)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements by using
        /// the passed in equality comparer.
        /// </summary>
        /// <typeparam name="T">List of type TSource</typeparam>
        /// <param name="first">A List to compare to second</param>
        /// <param name="second">A List to compare to the first sequence</param>
        /// <param name="equalityComparer">equality comparer used to compare two List elements of type TSource</param>
        /// <returns>true is the two Lists are of equal length and corresponding elements are all equal, false if they are not equal</returns>
        /// <exception>TSource:System.ArgumentNullException: first or second is null.</exception>
        /// <exception>TSource:System.ArgumentOutOfRangeException: if l or r is not inside the List bounds.</exception>
        public static bool SequenceEqualHpc<T>(this List<T> first, List<T> second, IEqualityComparer<T> equalityComparer)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Count != second.Count)
                return false;

            for (Int32 i = 0; i < first.Count; i++)
            {
                if (!equalityComparer.Equals(first[i], second[i]))
                    return false;
            }
            return true;
        }
    }
}
