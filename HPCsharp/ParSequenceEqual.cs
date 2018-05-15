using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

namespace HPCsharp
{
    /// <summary>
    /// Container class for parallel extension methods
    /// </summary>
    static public partial class ParallelAlgorithms
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
        /// <summary>
        /// Summary:
        ///     Determines whether two sequences are equal by comparing the elements by using
        ///     the default equality comparer for their type.
        ///
        /// Parameters:
        ///   first:
        ///     An array to compare to the second sequence.
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
        public static bool SequenceEqualHpcPar<T>(this T[] first, T[] second, Int32 l, Int32 r)
        {
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (!(l >= 0 && r < first.Length && r >= 0 && r < second.Length))
                throw new System.ArgumentOutOfRangeException();
            return SequenceEqualInner<T>(first, second, l, r);
        }
        /// <summary>
        /// Summary:
        ///     Determines whether two sequences are equal by comparing the elements by using
        ///     the default equality comparer for their type.
        ///
        /// Parameters:
        ///   first:
        ///     An array to compare to the second sequence.
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
        public static bool SequenceEqualHpcPar<T>(this T[] first, T[] second)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Length != second.Length)
                return false;
            return SequenceEqualInner<T>(first, second, 0, first.Length - 1);
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
        ///    T:System.ArgumentOutOfRangeException: if l or r is not inside the array bounds
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
        public static bool SequenceEqualHpcPar<T>(this List<T> first, List<T> second)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Count != second.Count)
                return false;
            return SequenceEqualInner<T>(first, second, 0, first.Count - 1);
        }
        private static bool SequenceEqualInner(ArrayList array1, ArrayList array2, Int32 l, Int32 r)
        {
            if (l > r)      // zero elements to compare
                return true;
            var equalityComparer = Comparer.Default;
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
                leftHalfEqual = SequenceEqualInner(array1, array2, l, m);
            },
                () =>
                {
                    rightHalfEqual = SequenceEqualInner(array1, array2, m + 1, r);
                }
            );
            return (leftHalfEqual && rightHalfEqual);   // Combine two boolean results
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
        public static bool SequenceEqualHpcPar(this ArrayList first, ArrayList second, Int32 l, Int32 r)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (!(l >= 0 && r < first.Count && r >= 0 && r < second.Count))
                throw new System.ArgumentOutOfRangeException();
            return SequenceEqualInner(first, second, l, r);
        }
        /// <summary>
        /// Determines whether two ArrayList's are equal by comparing the elements by using the default equality comparer for their type.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>
        ///    true if the two source sequences are of equal length and their corresponding
        ///    elements are equal according to the default equality comparer for their type;
        ///    otherwise, false.
        /// </returns>
        /// <exceptions>T:System.ArgumentNullException: first or second is null.</exceptions>
        public static bool SequenceEqualHpcPar(this ArrayList first, ArrayList second)
        {
            // Performance lesson: Changing the interface to IEnumerable hugely reduces performance, to the point of parallelism not being worthwhile
            if (first == null || second == null)
                throw new System.ArgumentNullException();
            if (first.Count != second.Count)
                return false;
            return SequenceEqualInner(first, second, 0, first.Count - 1);
        }
    }
}