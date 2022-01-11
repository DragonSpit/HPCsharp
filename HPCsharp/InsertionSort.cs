// TODO: Implement Selection Sort, and especially the double Selection Sort (or Cocktail Sort), which looks for min and max elements in every pass, since it requires half as many passes, but more comparisons,
//       as this may end up faster than Insertion Sort (see Wikipedia page) and my DDJ article from Sept. 2009
// TODO: Implement Insertion Sort for arrays of built-in data types without the comparer function, as this should be faster
using System;
using System.Collections.Generic;

namespace HPCsharp
{
    /// <summary>
    /// Container class for HPCsharp extension methods
    /// </summary>
    static public partial class Algorithm
    {
        /// <summary>
        /// O(n^2) sorting algorithm that's in-place. Fast for small arrays
        /// l: left/starting index within the List where to start sorting
        /// size: number of elements to sort
        /// comparer: optional compare method
        /// </summary>
        public static void InsertionSort<T>(List<T> a, Int32 l, Int32 size, IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 r = l + size;
            for (Int32 i = l + 1; i < r; i++)
            {
                //if (a[i] < a[i - 1])        // no need to do (j > 0) compare for the first iteration
                if (equalityComparer.Compare(a[i], a[i - 1]) < 0)
                {
                    T currentElement = a[i];
                    a[i] = a[i - 1];
                    Int32 j = i - 1;
                    for (; j > l && equalityComparer.Compare(currentElement, a[j - 1]) < 0; j--)
                    {
                        a[j] = a[j - 1];
                    }
                    a[j] = currentElement;  // always necessary work/write
                }
                // Perform no work at all if the first comparison fails - i.e. never assign an element to itself!
            }
        }

        /// <summary>
        /// O(n^2) sorting algorithm that's in-place. Fast for small arrays
        /// l: left/starting index within the array where to start sorting
        /// size: number of elements to sort
        /// comparer: optional compare method
        /// </summary>
        public static void InsertionSort<T>(T[] a, Int32 l, Int32 size, IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 r = l + size;
            for (Int32 i = l + 1; i < r; i++)
            {
                //if (a[i] < a[i - 1])        // no need to do (j > 0) compare for the first iteration
                if (equalityComparer.Compare(a[i], a[i - 1]) < 0)
                {
                    T currentElement = a[i];
                    a[i] = a[i - 1];
                    Int32 j = i - 1;
                    for (; j > l && equalityComparer.Compare(currentElement, a[j - 1]) < 0; j--)
                    {
                        a[j] = a[j - 1];
                    }
                    a[j] = currentElement;  // always necessary work/write
                }
                // Perform no work at all if the first comparison fails - i.e. never assign an element to itself!
            }
        }

        /// <summary>
        /// O(n^2) sorting algorithm that's in-place. Fast for small arrays
        /// l: left/starting index within the array where to start sorting
        /// size: number of elements to sort
        /// comparer: optional compare method
        /// </summary>
        public static void InsertionSort<T1, T2>(T1[] a, T2[] b, Int32 l, Int32 size, IComparer<T1> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T1>.Default;
            Int32 r = l + size;
            for (Int32 i = l + 1; i < r; i++)
            {
                //if (a[i] < a[i - 1])        // no need to do (j > 0) compare for the first iteration
                if (equalityComparer.Compare(a[i], a[i - 1]) < 0)
                {
                    T1 a_currentElement = a[i];
                    T2 b_currentElement = b[i];
                    a[i] = a[i - 1];
                    b[i] = b[i - 1];
                    Int32 j = i - 1;
                    for (; j > l && equalityComparer.Compare(a_currentElement, a[j - 1]) < 0; j--)
                    {
                        a[j] = a[j - 1];
                        b[j] = b[j - 1];
                    }
                    a[j] = a_currentElement;  // always necessary work/write
                    b[j] = b_currentElement;
                }
                // Perform no work at all if the first comparison fails - i.e. never assign an element to itself!
            }
        }
    }
}