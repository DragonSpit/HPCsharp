using System;
using System.Collections.Generic;

namespace HPCsharp
{
    /// <summary>
    /// Algorithms operating on variety of containers, providing trade-off between abstraction and performance
    /// </summary>
    static public partial class Algorithm
    {
        // There are several ways to implement a modified binary search for insertion sort.  One way is to compare with the middle array
        // element in the first step.  Another way is to compare with the largest element in the first step and then the smallest element.
        // Or to compare largest and then middle element.  It just depends if we want to optimize for search or for nearly presorted or 
        // nearly inverted - i.e. worst and best cases and in between.
        //
        // What it will boil down to is to find an index where an element is larger and an index where an element is smaller and
        // the difference between these indexes is 1 - this is the termination condition of the loop.
        // From this termination condition it seems that we must test the upper limit and then the lower limit of the array and then
        // reduce the distance between max and min indexes by 2X.
        //
        // For binary search we want to do the following:
        // Compare with the largest element, which is what we are doing in the first if statement, and if the current element is
        //   bigger then no work is done and the element stays in its current spot.
        // Compare with the smallest element and if it's smaller then we are done as well and need to move the entire array over 

        // Searches for the value within array "a", from a[ left ] to a[ right ] inclusively
        // Returns the the left-most index at which the element of the array is larger or equal to the value.
        // Thus, the return index can be between left and (right + 1)
        // Expects "a" array to be pre-sorted with the smallest element on the left and the largest on the right.
        // It would be cool if the routine worked automagically for the condition of right < left  (i.e. no  elements) - return left
        // It would be cool if the routine worked automagically for the condition of left == right (i.e. one element )
        // This version is borrowed from "Introduction to Algorithms" 3rd edition, p. 799.
        /// <summary>
        /// Binary Search algorithm which searches for a value within a sorted List
        /// </summary>
        /// <typeparam name="T">data type of each element within the List</typeparam>
        /// <param name="value">value being searched for</param>
        /// <param name="a">List being searched within</param>
        /// <param name="left">left/low index of the source List, inclusive</param>
        /// <param name="right">right/high index of the source List, inclusive</param>
        /// <param name="comparer">comparison to be used</param>
        /// <returns>Returns the the left-most index at which the element of the array is larger or equal to the value</returns>
        public static Int32 BinarySearch<T>(T value, List<T> a, Int32 left, Int32 right, IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 low = left;
            Int32 high = Math.Max(left, right + 1);
            while (low < high)
            {
                Int32 mid = (low + high) / 2;
                if (equalityComparer.Compare(value, a[mid]) <= 0)   // if (value <= a[mid])
                    high = mid;
                else low = mid + 1; // because we compared to a[mid] and the value was larger than a[mid].
                                    // Thus, the next array element to the right from mid is the next possible
                                    // candidate for low, and a[mid] can not possibly be that candidate.
            }
            return high;
        }
        /// <summary>
        /// Binary Search algorithm which searches for a value within a sorted array
        /// </summary>
        /// <typeparam name="T">data type of each element within the array</typeparam>
        /// <param name="value">value being searched for</param>
        /// <param name="a">List being searched within</param>
        /// <param name="left">left/low index of the source array, inclusive</param>
        /// <param name="right">right/high index of the source array, inclusive</param>
        /// <param name="comparer">comparison to be used</param>
        /// <returns>Returns the the left-most index at which the element of the array is larger or equal to the value</returns>
        public static Int32 BinarySearch<T>(T value, T[] a, Int32 left, Int32 right, IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 low = left;
            Int32 high = Math.Max(left, right + 1);
            while (low < high)
            {
                Int32 mid = (low + high) / 2;
                if (equalityComparer.Compare(value, a[mid]) <= 0)   // if (value <= a[mid])
                    high = mid;
                else low = mid + 1; // because we compared to a[mid] and the value was larger than a[mid].
                                    // Thus, the next array element to the right from mid is the next possible
                                    // candidate for low, and a[mid] can not possibly be that candidate.
            }
            return high;
        }
    }
}
