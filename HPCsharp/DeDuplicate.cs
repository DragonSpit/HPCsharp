// TODO: Implement a generic version of the Unique algorithm that works with any type that implements IComparable<T> or IEquatable<T> since this algorithm uses comparisons.
using System;
using System.Threading.Tasks;

namespace HPCsharp
{
    public static partial class ParallelAlgorithm
    {
        sealed class CustomDataDeDuplicate
        {
            public int currIndex;
            public int wii;          // work item index
        }
        // left and right are inclusive indices of the array to deduplicate
        // Scan each quantum of the input array to deduplicate in parallel, counting the number of unique elements in each quantum. Then combine the counts to determine the starting index of each quantum.
        // Then copy the unique elements from the input array to the output array while skipping the duplicates to the left. Do this in parallel for each quantum and do it not-in-place.
        private static int UniqueParInternal(uint[] arrayToDeDup, int l, int r, int parallelWorkQuantum = 64 * 1024)
        {
            if (l > r)      // zero elements in the array to deduplicate
                return 0;
            if ((r - l + 1) <= parallelWorkQuantum)
                return Algorithm.Unique(arrayToDeDup);

            uint[] outputArray = new uint[arrayToDeDup.Length];
            int numWorkItems = (arrayToDeDup.Length % parallelWorkQuantum) == 0 ? (arrayToDeDup.Length / parallelWorkQuantum)
                                                                                : (arrayToDeDup.Length / parallelWorkQuantum + 1);
#if True
            // Sequential version.
            int[] numUniques = new int[numWorkItems];
            int[] startQuanta = new int[numWorkItems];
            // Count the number of unique elements in each quantum
            int currIndex, wi;
            for (wi = 0; wi < (numWorkItems - 1); wi++)
            {
                currIndex = l + wi * parallelWorkQuantum;
                for (uint i = 0; i < (parallelWorkQuantum - 1); i++)
                {
                    if (arrayToDeDup[currIndex] != arrayToDeDup[currIndex + 1])
                        numUniques[wi]++;
                    currIndex++;
                }
            }
            // The last work item may not have the full parallelWorkQuanta of items to process
            currIndex = l + (numWorkItems - 1) * parallelWorkQuantum;   // redundant computation - for clarity and safety
            int numItems = arrayToDeDup.Length % parallelWorkQuantum;
            for (int i = 0; i < (numItems - 1); i++)
            {
                if (arrayToDeDup[currIndex] != arrayToDeDup[currIndex + 1])
                    numUniques[wi]++;
                currIndex++;
            }

            // Combine the counts to determine the starting index of each quantum
            int numUniquesTotal = 0;
            for (wi = 0; wi < numWorkItems; wi++)
            {
                startQuanta[wi] = numUniquesTotal;
                numUniquesTotal += numUniques[wi];
            }

            // Copy unique elements to the output array
            for (int w = 0; w < numWorkItems; w++)
            {
                currIndex = l + w * parallelWorkQuantum;
                Algorithm.Unique(arrayToDeDup, currIndex, parallelWorkQuantum, outputArray, startQuanta[w]);
            }
#else
            // Parallel version.
            // Partially parallel version, where only the final copy of unique elements to the output array is done in parallel. The counting of unique elements in each quantum is done sequentially.
            int[] numUniques  = new int[numWorkItems];
            int[] startQuanta = new int[numWorkItems];
            // Count the number of unique elements in each quantum
            int currIndex, wi;
            for (wi = 0; wi < (numWorkItems - 1); wi++)
            {
                currIndex = l + wi * parallelWorkQuantum;
                for (uint i = 0; i < (parallelWorkQuantum - 1); i++)
                {
                    if (arrayToDeDup[currIndex] != arrayToDeDup[currIndex + 1])
                        numUniques[wi]++;
                    currIndex++;
                }
            }
            // The last work item may not have the full parallelWorkQuanta of items to process
            currIndex = l + wi * parallelWorkQuantum;   // redundant comptation - for clarity and safety
            int numItems = arrayToDeDup.Length % parallelWorkQuantum;
            for (int i = 0; i < (numItems - 1); i++)
            {
                if (arrayToDeDup[currIndex] != arrayToDeDup[currIndex + 1])
                    numUniques[wi]++;
                currIndex++;
            }

            // Combine the counts to determine the starting index of each quantum
            int numUniquesTotal = 0;
            for (wi = 0; wi < numWorkItems; wi++)
            {
                startQuanta[wi] = numUniquesTotal;
                numUniquesTotal += numUniques[wi];
            }
            // The last work item may not have the full parallelWorkQuanta of items to process
            int numberOfFullQuantas = arrayToDeDup.Length / parallelWorkQuantum;
            Task[] taskArray = new Task[numWorkItems];
            for (wi = 0; wi < numberOfFullQuantas; wi++)
            {
                currIndex = l + wi * parallelWorkQuantum;
                taskArray[wi] = Task.Factory.StartNew((Object obj) => {
                    CustomDataDeDuplicate data = obj as CustomDataDeDuplicate;
                    int endIndex = data.currIndex + parallelWorkQuantum;

                    Algorithm.Unique(arrayToDeDup, currIndex, parallelWorkQuantum, outputArray, startQuanta[data.wii]);
                },
                new CustomDataDeDuplicate() { currIndex = currIndex, wii = wi }
                );
            }
            if (numWorkItems > numberOfFullQuantas)      // last partially filled workQuantum
            {
                currIndex = l + wi * parallelWorkQuantum;
                taskArray[wi] = Task.Factory.StartNew((Object obj) => {
                    CustomDataDeDuplicate data = obj as CustomDataDeDuplicate;
                    int endIndex = data.currIndex + parallelWorkQuantum;

                    Algorithm.Unique(arrayToDeDup, currIndex, parallelWorkQuantum, outputArray, startQuanta[data.wii]);
                },
                new CustomDataDeDuplicate() { currIndex = currIndex, wii = wi }
                );
            }
            Task.WaitAll(taskArray);    // wait for all work quanta to finish before starting the next pass, as passes have to be done in sequence
#endif
            //HPCsharp.ParallelAlgorithms.Copy.CopyPar(outputArray, 0, arrayToDeDup, 0, numUniquesTotal);
            Array.Copy(outputArray, 0, arrayToDeDup, 0, numUniquesTotal);
            return numUniquesTotal;
        }
        /// <summary>
        /// Removes duplicate array elements from a presorted array. A not-in-place, stable and parallel algorithm.
        /// Unique elements are returned in the first part of the output array. The return value is the number of unique elements.
        /// </summary>
        /// <param name="arrayToDeDup">array that is to be sorted in place</param>
        /// <returns>The number of unique elements in the array</returns>
        public static int UniquePar(uint[] arrayToDeDup, int parallelThreshold = 64 * 1024)
        {
            if (arrayToDeDup == null)
                throw new ArgumentNullException(nameof(arrayToDeDup));
            // Split the array into quanta and process each quantum in parallel
            // For each quantum, count the number of duplicate elements. Then combine duplicates from left to right to determine the offset of each quantum
            // and then copy the unique elements while skipping the duplicates to the left. Do this in parallel for each quantum and do it not-in-place.
            // This will need a Unique algorithm version that is not-in-place and does not need to return the number of unique elements, since we counted them in the first pass.
            return UniqueParInternal(arrayToDeDup, 0, arrayToDeDup.Length - 1, parallelThreshold);
        }
        /// <summary>
        /// Not-in-place DeDuplicate algorithm, which removes duplicate array elements. A stable algorithm.
        /// The array is sorted not in place and the unique elements are returned in the first part of the array. The return value is the number of unique elements.
        /// </summary>
        /// <param name="arrayToDeDup">array that is to be deduplicated in place</param>
        /// <returns>The number of unique elements in the array</returns>
        public static int DeDuplicateNotInPlace(this uint[] arrayToDeDup)
        {
            HPCsharp.ParallelAlgorithm.SortRadixPar(arrayToDeDup);
            return HPCsharp.ParallelAlgorithm.UniqueParInternal(arrayToDeDup, 0, arrayToDeDup.Length - 1, 64 * 1024);
        }
    }

    public static partial class Algorithm
    {
        /// <summary>
        /// Removes duplicate array elements from a presorted array. An in-place and stable algorithm.
        /// Unique elements are returned in the first part of the array. The return value is the number of unique elements.
        /// </summary>
        /// <param name="arrayToDeDup">array that is to be DeDuplicated in place</param>
        /// <returns>The number of unique elements in the array</returns>
        public static int Unique(this uint[] arrayToDeDup)
        {
            if (arrayToDeDup == null)
                throw new ArgumentNullException(nameof(arrayToDeDup));
            int rIndex = 0, wIndex = 0;
            for (; rIndex < (arrayToDeDup.Length - 1); rIndex++)
            {
                arrayToDeDup[wIndex] = arrayToDeDup[rIndex];
                if (arrayToDeDup[rIndex] != arrayToDeDup[rIndex + 1])
                    wIndex++;
            }
            arrayToDeDup[wIndex] = arrayToDeDup[rIndex];  // copy the last array element to its proper index
            return wIndex + 1;
        }

        /// <summary>
        /// Removes duplicate array elements from a presorted array. An in-place and stable algorithm.
        /// Unique elements are returned in the first part of the array. The return value is the number of unique elements.
        /// </summary>
        /// <param name="arrayToDeDup">array that is to be DeDuplicated in place</param>
        /// <returns>The number of unique elements in the array</returns>
        public static int Unique(this uint[] arrayToDeDup, int start, int length)
        {
            if (arrayToDeDup == null)
                throw new ArgumentNullException(nameof(arrayToDeDup));
            int rIndex = start, wIndex = start;
            for (; rIndex < (start + length - 1); rIndex++)
            {
                arrayToDeDup[wIndex] = arrayToDeDup[rIndex];
                if (arrayToDeDup[rIndex] != arrayToDeDup[rIndex + 1])
                    wIndex++;
            }
            arrayToDeDup[wIndex] = arrayToDeDup[rIndex];  // copy the last array element to its proper index
            return wIndex + 1;
        }

        /// <summary>
        /// Removes duplicate array elements from a presorted array. A not-in-place and stable algorithm.
        /// Unique elements are returned in the first part of the outputarray. The return value is the number of unique elements.
        /// </summary>
        /// <param name="arrayToDeDupIn">array that is to be sorted in place</param>
        /// <param name="arrayToDeDupOut">array to store the unique elements</param>
        /// <returns>The number of unique elements in the array</returns>
        public static int Unique(this uint[] arrayToDeDupIn, uint[] arrayToDeDupOut)
        {
            if (arrayToDeDupIn == null)
                throw new ArgumentNullException(nameof(arrayToDeDupIn));
            if (arrayToDeDupOut == null)
                throw new ArgumentNullException(nameof(arrayToDeDupOut));
            int rIndex = 0, wIndex = 0;
            for (; (rIndex + 1) < arrayToDeDupIn.Length; rIndex++)
            {
                arrayToDeDupOut[wIndex] = arrayToDeDupIn[rIndex];
                if (arrayToDeDupIn[rIndex] != arrayToDeDupIn[rIndex + 1])
                    wIndex++;
            }
            arrayToDeDupOut[wIndex] = arrayToDeDupIn[rIndex];  // copy the last array element to its proper index
            return wIndex + 1;
        }

        /// <summary>
        /// Removes duplicate array elements from a presorted array. A not-in-place and stable algorithm.
        /// Unique elements are returned in the first part of the output array. The return value is the number of unique elements.
        /// </summary>
        /// <param name="arrayToDeDupIn">array that is to be DeDuplicated in place</param>
        /// <param name="arrayToDeDupOut">array to store the unique elements</param>
        /// <returns>The number of unique elements in the array</returns>
        public static int Unique(this uint[] arrayToDeDupIn, int startIn, int length, uint[] arrayToDeDupOut, int startOut)
        {
            if (arrayToDeDupIn == null)
                throw new ArgumentNullException(nameof(arrayToDeDupIn));
            if (arrayToDeDupOut == null)
                throw new ArgumentNullException(nameof(arrayToDeDupOut));
            int rIndex = startIn, wIndex = startOut;
            for (; rIndex < (startIn + length - 1); rIndex++)
            {
                arrayToDeDupOut[wIndex] = arrayToDeDupIn[rIndex];
                if (arrayToDeDupIn[rIndex] != arrayToDeDupIn[rIndex + 1])
                    wIndex++;
            }
            arrayToDeDupOut[wIndex] = arrayToDeDupIn[rIndex];  // copy the last array element to its proper index
            return wIndex + 1 - startOut;
        }

        private static int DeDuplicatePriv(this uint[] arrayToDeDup)
        {
            HPCsharp.Algorithm.SortRadixMsd( arrayToDeDup);
            return HPCsharp.Algorithm.Unique(arrayToDeDup);
        }

        private static int DeDuplicatePriv(this uint[] arrayToDeDup, int start, int length)
        {
            HPCsharp.Algorithm.SortRadixMsd( arrayToDeDup, start, length);
            return HPCsharp.Algorithm.Unique(arrayToDeDup, start, length);
        }

        /// <summary>
        /// In-place DeDuplicate algorithm, which removes duplicate array elements. Not a stable algorithm.
        /// The array is sorted in place and the unique elements are returned in the first part of the array. The return value is the number of unique elements.
        /// </summary>
        /// <param name="arrayToDeDup">array that is to be deduplicated in place</param>
        /// <returns>The number of unique elements in the array</returns>
        public static int DeDuplicate(this uint[] arrayToDeDup)
        {
            if (arrayToDeDup == null)
                throw new ArgumentNullException(nameof(arrayToDeDup));
            return DeDuplicatePriv(arrayToDeDup);
        }

        /// <summary>
        /// In-place DeDuplicate algorithm, which removes duplicate array elements. Not a stable algorithm.
        /// The array is sorted in place and the unique elements are returned in the first part of the array. The return value is the number of unique elements.
        /// </summary>
        /// <param name="arrayToDeDup">array that is to be deduplicated in place</param>
        /// <param name="start"="start">The starting index of the array to be deduplicated</param>
        /// <param name="length">The number of elements in the array to be deduplicated</param>
        /// <returns>The number of unique elements in the array</returns>
        public static int DeDuplicate(this uint[] arrayToDeDup, int start, int length)
        {
            if (arrayToDeDup == null)
                throw new ArgumentNullException(nameof(arrayToDeDup));
            return DeDuplicatePriv(arrayToDeDup, start, length);
        }

        /// <summary>
        /// Not-in-place DeDuplicate algorithm, which removes duplicate array elements. A stable algorithm.
        /// The array is sorted not in place and the unique elements are returned in the first part of the array. The return value is the number of unique elements.
        /// </summary>
        /// <param name="arrayToDeDup">array that is to be deduplicated in place</param>
        /// <returns>The number of unique elements in the array</returns>
        public static int DeDuplicateNotInPlace(this uint[] arrayToDeDup)
        {
            HPCsharp.Algorithm.SortRadixLsd( arrayToDeDup);
            return HPCsharp.Algorithm.Unique(arrayToDeDup);
        }

        /// <summary>
        /// Not-in-place DeDuplicate algorithm, which removes duplicate array elements. A stable algorithm.
        /// The array is sorted not in place and the unique elements are returned in the first part of the array. The return value is the number of unique elements.
        /// </summary>
        /// <param name="arrayToDeDup">array that is to be deduplicated in place</param>
        /// <param name="start"="start">The starting index of the array to be deduplicated</param>
        /// <param name="length">The number of elements in the array to be deduplicated</param>
        /// <returns>The number of unique elements in the array</returns>
        public static int DeDuplicateNotInPlace(this uint[] arrayToDeDup, int start, int length)
        {
            HPCsharp.Algorithm.SortRadixLsd( arrayToDeDup, start, length);
            return HPCsharp.Algorithm.Unique(arrayToDeDup, start, length);
        }
    }
}
