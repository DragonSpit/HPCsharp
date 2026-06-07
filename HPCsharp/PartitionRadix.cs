// TODO: Implement in a stable way by using SelectRadix as the starting point and then keep the elements which are < k-th bin on the left of k-th bin and move elements which are > k-th bin on the right of k-th bin.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPCsharp
{
    public static partial class Algorithm
    {
        /// <summary>
        /// In-place Partition using Radix by the k-th elements in an array. Not a stable algorithm.
        /// </summary>
        /// <param name="arrayToBePartitioned">array that is to be selected in place</param>
        /// <param name="start">starting index of the subarray</param>
        /// <param name="length">length of the subarray</param>
        /// <param name="k">Index to be used as the partitioning element. The value of the arrayToBePartitioned[k] will not be used for partitioning.</param>
        /// <param name="threshold">for array size smaller than threshold Array.Sort will be used instead of MSD Radix Sort like algorithm</param>
        public static void PartitionRadix(this uint[] arrayToBePartitioned, Int32 start, Int32 length, Int32 k, Int32 threshold = 1024)
        {
            if (arrayToBePartitioned == null)
                throw new ArgumentNullException(nameof(arrayToBePartitioned));
            if (arrayToBePartitioned.Length <= 0 || length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBePartitioned.Length), "array length or length is invalid");
            int shiftRightAmount = sizeof(uint) * 8 - Log2ofPowerOfTwoRadix;
            Int32[] kArray = new Int32[1] { k };
            // Insertion Sort or Heap Sort could be passed in as another base case since they are both in-place
            SelectRadixMsdUIntInner(arrayToBePartitioned, start, length, shiftRightAmount, kArray, 0, kArray.Length, Array.Sort, threshold);
            // The following does not work: Need to figure out how to pass InsertionSort method as an Action
            //RadixSortMsdUIntInner(arrayToBePartitioned, start, length, shiftRightAmount, (arr, startIndex, lengthOfArray) => InsertionSort(arrayToBePartitioned, start, length), threshold);
        }
        /// <summary>
        /// In-place Partition using Radix by the k-th elements in an array. Not a stable algorithm.
        /// </summary>
        /// <param name="arrayToBeParitioned">array that is to be selected from in place</param>
        /// <param name="k">Index to be used as the partitioning element. The value of the arrayToBePartitioned[k] will not be used for partitioning.</param>
        /// <param name="threshold">for array size smaller than threshold Array.Sort will be used instead of MSD Radix Sort like algorithm</param>
        public static void PartitionRadix(this uint[] arrayToBeParitioned, Int32 k, Int32 threshold = 1024)
        {
            if (arrayToBeParitioned == null)
                throw new ArgumentNullException(nameof(arrayToBeParitioned));
            if (arrayToBeParitioned.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBeParitioned.Length), "array length is invalid");
            Int32[] kArray = new Int32[1] { k };
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            SelectRadixMsdUIntInner(arrayToBeParitioned, 0, arrayToBeParitioned.Length, shiftRightAmount, kArray, 0, kArray.Length, Array.Sort, threshold);
        }
    }
}
