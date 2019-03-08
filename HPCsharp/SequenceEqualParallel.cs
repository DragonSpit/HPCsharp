// TODO: Implement SequenceCompare, where the user provides a non-default comparison, so that sequences can be compared for less than, equal to and greater than using a custom comparison method
//       This should be easy to parallelize and for the user to provide their own comparison method for their own custom user classes. An easy and consistent way would be to expose the 
//       Comparer in the same way we did for Merge Sort.
// TODO: See if converting List to array then doing the operation and then converting back to List, like we do for sorting, is faster.
// TODO: Add SIMD/SSE acceleration for sequence comparison for additional speedup, for all built-in data types
using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System.Numerics;

namespace HPCsharp
{
    /// <summary>
    /// Container class for parallel extension methods
    /// </summary>
    static public partial class ParallelAlgorithm
    {
        static Int32 ThresholdSequenceEqual { get; set; } = 1024;

        private static bool SequenceEqualInner<T>(T[] array1, T[] array2, Int32 l, Int32 r)
        {
            if (l > r)      // zero elements to compare
                return true;
            var equalityComparer = Comparer<T>.Default;
            if ((r - l + 1) <= ThresholdSequenceEqual)
            {
                for (Int32 i = l; i <= r; i++)     // inclusive of l and r
                {
                    if (equalityComparer.Compare(array1[i], array2[i]) != 0)
                        return false;
                }
                return true;
            }

            int m = ((r + l) / 2);
            bool leftHalfEqual = false;
            bool rightHalfEqual = false;
            Parallel.Invoke(() =>
                {
                    leftHalfEqual = SequenceEqualInner<T>(array1, array2, l, m);
                },
                () =>
                {
                    rightHalfEqual = SequenceEqualInner<T>(array1, array2, m + 1, r);
                }
            );
            return (leftHalfEqual && rightHalfEqual);   // Combine two boolean results
        }

        private static bool SequenceEqualInner<T>(T[] array1, T[] array2, Int32 l, Int32 r, IEqualityComparer<T> equalityComparer)
        {
            if (l > r)      // zero elements to compare
                return true;
            if ((r - l + 1) <= ThresholdSequenceEqual)
            {
                for (Int32 i = l; i <= r; i++)     // inclusive of l and r
                {
                    if (!equalityComparer.Equals(array1[i], array2[i]))
                        return false;
                }
                return true;
            }

            int m = ((r + l) / 2);
            bool leftHalfEqual = false;
            bool rightHalfEqual = false;
            Parallel.Invoke(() =>
                {
                    leftHalfEqual  = SequenceEqualInner<T>(array1, array2, l, m, equalityComparer);
                },
                () =>
                {
                    rightHalfEqual = SequenceEqualInner<T>(array1, array2, m + 1, r, equalityComparer);
                }
            );
            return (leftHalfEqual && rightHalfEqual);   // Combine two boolean results
        }

        private static bool SequenceEqualInner<T>(T[] array1, T[] array2, Int32 l, Int32 r, Func<T, T, bool> equalityComparer)
        {
            if (l > r)      // zero elements to compare
                return true;
            if ((r - l + 1) <= ThresholdSequenceEqual)
            {
                for (Int32 i = l; i <= r; i++)     // inclusive of l and r
                {
                    if (!equalityComparer(array1[i], array2[i]))
                        return false;
                }
                return true;
            }

            int m = ((r + l) / 2);
            bool leftHalfEqual = false;
            bool rightHalfEqual = false;
            Parallel.Invoke(() =>
                {
                    leftHalfEqual = SequenceEqualInner<T>(array1, array2, l, m, equalityComparer);
                },
                () =>
                {
                    rightHalfEqual = SequenceEqualInner<T>(array1, array2, m + 1, r, equalityComparer);
                }
            );
            return (leftHalfEqual && rightHalfEqual);   // Combine two boolean results
        }
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
        public static bool SequenceEqualHpcPar<T>(this T[] first, T[] second, Int32 l, Int32 r)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (!(l >= 0 && r < first.Length && r >= 0 && r < second.Length))
                throw new System.ArgumentOutOfRangeException();
            return SequenceEqualInner<T>(first, second, l, r);
        }
        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements by using
        /// the default equality comparer for their type.
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
        public static bool SequenceEqualHpcPar<T>(this T[] first, T[] second, Int32 l, Int32 r, IEqualityComparer<T> equalityComparer)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (!(l >= 0 && r < first.Length && r >= 0 && r < second.Length))
                throw new System.ArgumentOutOfRangeException();
            return SequenceEqualInner<T>(first, second, l, r, equalityComparer);
        }
        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements by using
        /// the default equality comparer for their type.
        /// </summary>
        /// <typeparam name="T">array of type TSource</typeparam>
        /// <param name="first">An array to compare to second</param>
        /// <param name="second">An array to compare to the first sequence</param>
        /// <param name="l">Index of the left array element: the starting array element for the comparison (inclusive)</param>
        /// <param name="r">Index of the right array element: the ending array element for the comparison (inclusive)</param>
        /// <param name="equalityComparer">Lambda equality comparer used to compare two array elements of type TSource</param>
        /// <returns>true is the two arrays are of equal length and corresponding elements are all equal, false if they are not equal</returns>
        /// <exception>TSource:System.ArgumentNullException: first or second is null.</exception>
        /// <exception>TSource:System.ArgumentOutOfRangeException: if l or r is not inside the array bounds.</exception>
        public static bool SequenceEqualHpcPar<T>(this T[] first, T[] second, Int32 l, Int32 r, Func<T, T, bool> equalityComparer)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (!(l >= 0 && r < first.Length && r >= 0 && r < second.Length))
                throw new System.ArgumentOutOfRangeException();
            return SequenceEqualInner<T>(first, second, l, r, equalityComparer);
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
        public static bool SequenceEqualHpcPar<T>(this T[] first, T[] second)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Length != second.Length)
                return false;
            return SequenceEqualInner<T>(first, second, 0, first.Length - 1);
        }
        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements by using
        /// the default equality comparer for their type.
        /// </summary>
        /// <typeparam name="T">array of type TSource</typeparam>
        /// <param name="first">An array to compare to second</param>
        /// <param name="second">An array to compare to the first sequence</param>
        /// <param name="equalityComparer">equality comparer used to compare two array elements of type TSource</param>
        /// <returns>true is the two arrays are of equal length and corresponding elements are all equal, false if they are not equal</returns>
        /// <exception>TSource:System.ArgumentNullException: first or second is null.</exception>
        public static bool SequenceEqualHpcPar<T>(this T[] first, T[] second, IEqualityComparer<T> equalityComparer)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Length != second.Length)
                return false;
            return SequenceEqualInner<T>(first, second, 0, first.Length - 1, equalityComparer);
        }

        private static bool SequenceEqualInner<T>(List<T> array1, List<T> array2, Int32 l, Int32 r)
        {
            if (l > r)      // zero elements to compare
                return true;
            var equalityComparer = Comparer<T>.Default;
            if ((r - l + 1) <= ThresholdSequenceEqual)
            {
                for (Int32 i = l; i <= r; i++)     // inclusive of l and r
                {
                    if (equalityComparer.Compare(array1[i], array2[i]) != 0)
                        return false;
                }
                return true;
            }

            int m = ((r + l) / 2);
            bool leftHalfEqual = false;
            bool rightHalfEqual = false;
            Parallel.Invoke(() =>
                {
                    leftHalfEqual = SequenceEqualInner<T>(array1, array2, l, m);
                },
                () =>
                {
                    rightHalfEqual = SequenceEqualInner<T>(array1, array2, m + 1, r);
                }
            );
            return (leftHalfEqual && rightHalfEqual);   // Combine two boolean results
        }

        private static bool SequenceEqualInner<T>(List<T> array1, List<T> array2, Int32 l, Int32 r, IEqualityComparer<T> equalityComparer)
        {
            if (l > r)      // zero elements to compare
                return true;
            if ((r - l + 1) <= ThresholdSequenceEqual)
            {
                for (Int32 i = l; i <= r; i++)     // inclusive of l and r
                {
                    if (!equalityComparer.Equals(array1[i], array2[i]))
                        return false;
                }
                return true;
            }

            int m = ((r + l) / 2);
            bool leftHalfEqual = false;
            bool rightHalfEqual = false;
            Parallel.Invoke(() =>
                {
                    leftHalfEqual  = SequenceEqualInner<T>(array1, array2, l, m, equalityComparer);
                },
                () =>
                {
                    rightHalfEqual = SequenceEqualInner<T>(array1, array2, m + 1, r, equalityComparer);
                }
            );
            return (leftHalfEqual && rightHalfEqual);   // Combine two boolean results
        }
        /// <summary>
        /// Determines whether two Lists are equal by comparing the elements by using the default equality comparer for their type.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <returns>
        ///    true if the two source sequences are of equal length and their corresponding
        ///    elements are equal according to the default equality comparer for their type;
        ///    otherwise, false.
        /// </returns>
        /// <exceptions>
        ///    T:System.ArgumentNullException: first or second is null.
        ///    T:System.ArgumentOutOfRangeException: if l or r is not inside the List bounds
        /// </exceptions>
        public static bool SequenceEqualHpcPar<T>(this List<T> first, List<T> second, Int32 l, Int32 r)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (!(l >= 0 && r < first.Count && r >= 0 && r < second.Count))
                throw new System.ArgumentOutOfRangeException();
            return SequenceEqualInner<T>(first, second, l, r);
        }
        /// <summary>
        /// Determines whether two Lists are equal by comparing the elements by using the default equality comparer for their type.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <param name="equalityComparer">equality comparer used to compare two List elements of type TSource</param>
        /// <returns>
        ///    true if the two source sequences are of equal length and their corresponding
        ///    elements are equal according to the default equality comparer for their type;
        ///    otherwise, false.
        /// </returns>
        /// <exceptions>
        ///    T:System.ArgumentNullException: first or second is null.
        ///    T:System.ArgumentOutOfRangeException: if l or r is not inside the List bounds
        /// </exceptions>
        public static bool SequenceEqualHpcPar<T>(this List<T> first, List<T> second, Int32 l, Int32 r, IEqualityComparer<T> equalityComparer)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (!(l >= 0 && r < first.Count && r >= 0 && r < second.Count))
                throw new System.ArgumentOutOfRangeException();
            return SequenceEqualInner<T>(first, second, l, r, equalityComparer);
        }
        /// <summary>
        /// Determines whether two Lists are equal by comparing the elements by using the default equality comparer for their type.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>
        ///    true if the two source sequences are of equal length and their corresponding
        ///    elements are equal according to the default equality comparer for their type;
        ///    otherwise, false.
        /// </returns>
        /// <exceptions>
        ///    T:System.ArgumentNullException: first or second is null.
        ///    T:System.ArgumentOutOfRangeException: if l or r is not inside the List bounds
        /// </exceptions>
        public static bool SequenceEqualHpcPar<T>(this List<T> first, List<T> second)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Count != second.Count)
                return false;
            return SequenceEqualInner<T>(first, second, 0, first.Count - 1);
        }
        /// <summary>
        /// Determines whether two Lists are equal by comparing the elements by using the default equality comparer for their type.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="equalityComparer">equality comparer used to compare two List elements of type TSource</param>
        /// <returns>
        ///    true if the two source sequences are of equal length and their corresponding
        ///    elements are equal according to the default equality comparer for their type;
        ///    otherwise, false.
        /// </returns>
        /// <exceptions>
        ///    T:System.ArgumentNullException: first or second is null.
        ///    T:System.ArgumentOutOfRangeException: if l or r is not inside the List bounds
        /// </exceptions>
        public static bool SequenceEqualHpcPar<T>(this List<T> first, List<T> second, IEqualityComparer<T> equalityComparer)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Count != second.Count)
                return false;
            return SequenceEqualInner<T>(first, second, 0, first.Count - 1, equalityComparer);
        }

        public static bool EqualSse(this int[] first, int[] second)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException("Equality cannot be determined when one or both arrays are null");
            if (first.Length == 0 || second.Length == 0)
                throw new ArgumentException("Equality cannot be determined when one or both arrays are empty");
            return first.EqualSseInner(second, 0, first.Length - 1);
        }

        public static bool EqualSse(this int[] first, int[] second, int start, int length)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException("Equality cannot be determined when one or both arrays are null");
            if (first.Length == 0 || second.Length == 0 || length == 0)
                throw new ArgumentException("Equality cannot be determined when one or both arrays are empty, or length is zero");
            return first.EqualSseInner(second, start, start + length - 1);
        }

        // Assumes that at least one element in an array to be processed and l <= r to also ensure that at least one element is being processed, to ensure a Min result is always possible
        private static bool EqualSseInner(this int[] first, int[] second, int l, int r)
        {
            int sseIndexEnd = l + ((r - l + 1) / Vector<int>.Count) * Vector<int>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVectorFirst  = new Vector<int>(first,  i);
                var inVectorSecond = new Vector<int>(second, i);
                if (!Vector.EqualsAll(inVectorFirst, inVectorSecond))
                    return false;
            }
            for (; i <= r; i++)
            {
                if (first[i] != second[i])
                    return false;
            }
            return true;
        }
    }
}