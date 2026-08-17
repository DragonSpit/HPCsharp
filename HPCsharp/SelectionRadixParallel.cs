using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPCsharp
{

    public static partial class ParallelAlgorithm
    {
        // Move elements outside the k-th bin, the bin that k is in, into the k-th bin
        // Generic implementation that work for regions to the left or to the right of the k-th bin, and for any digit size.
        private static int MoveOutsideOfKthBinIn(uint[] a_in, uint[] b_out, int startOfOb, int lengthOfOb, int startOfKthBin, int lengthOfKthBin,
                                                 int shiftRightAmount, uint bitMask, int kthBin)
        {
            int endOfKthBin = startOfKthBin + lengthOfKthBin - 1;
            int endOfOb = startOfOb + lengthOfOb - 1;
            int _current_ob = startOfOb, _current_ib = startOfKthBin, found_ob; // _ob = outside of bin, _ib = inside of bin
            while (true)
            {
                // Look for the element that belongs in the bin that k is in, to move into that bin
                for (found_ob = 0; _current_ob <= endOfOb; _current_ob++)
                    if (((a_in[_current_ob] >> shiftRightAmount) & bitMask) == kthBin) { found_ob = 1; break; }
                // Look for the first location in the bin that k is in, which has an element that does not belong in that bin
                if (found_ob == 1)
                    for (; _current_ib <= endOfKthBin; _current_ib++)
                        if (((a_in[_current_ib] >> shiftRightAmount) & bitMask) != kthBin) break;

                if (_current_ob > endOfOb || _current_ib > endOfKthBin) break; // All the element outside the bin have been exhausted or the bin that k is in is full or 
                b_out[_current_ib++] = a_in[_current_ob++];    // Move the element that belongs in the bin into the bin
            }
            return _current_ib;
        }
        // Move elements outside the k-th bin, the bin that k is in, into the k-th bin
        // For not-in-place version.
        private static void MoveOutsideOfKthBinIn_NotInPlace(uint[] a_in, uint[] b_out, int startOfOb, int lengthOfOb, int startWithinKthBin,
                                                            int shiftRightAmount, uint bitMask, int kthBin)
        {
            int endOfOb = startOfOb + lengthOfOb - 1;  // end of outside of bin is inclusive
            for (; startOfOb <= endOfOb; startOfOb++)
            {
                if (((a_in[startOfOb] >> shiftRightAmount) & bitMask) == kthBin)
                    b_out[startWithinKthBin++] = a_in[startOfOb];    // Move the element that belongs in the k-th bin into the k-th bin
            }
        }
        // Move elements outside the k-th bin, the bin that k is in, into the k-th bin
        // For not-in-place version.
        private static async void MoveOutsideOfKthBinIn_NotInPlaceAsync(uint[] a_in, uint[] b_out, int startOfOb, int lengthOfOb, int startWithinKthBin,
                                                            int shiftRightAmount, uint bitMask, int kthBin)
        {
            int endOfOb = startOfOb + lengthOfOb - 1;  // end of outside of bin is inclusive
            for (; startOfOb <= endOfOb; startOfOb++)
            {
                if (((a_in[startOfOb] >> shiftRightAmount) & bitMask) == kthBin)
                    b_out[startWithinKthBin++] = a_in[startOfOb];    // Move the element that belongs in the k-th bin into the k-th bin
            }
        }
        // Move elements outside the k-th bin, the bin that k is in, into the k-th bin
        // For not-in-place version.
        private static async Task MoveOutsideOfKthBinIn_NotInPlace_Async(uint[] a_in, uint[] b_out, int startOfOb, int lengthOfOb, int startWithinKthBin, int shiftRightAmount, uint bitMask, int kthBin)
        {
            int endOfOb = startOfOb + lengthOfOb - 1;  // end of outside of bin is inclusive
            for (; startOfOb <= endOfOb; startOfOb++)
            {
                if (((a_in[startOfOb] >> shiftRightAmount) & bitMask) == kthBin)
                    b_out[startWithinKthBin++] = a_in[startOfOb];    // Move the element that belongs in the k-th bin into the k-th bin
            }
        }
        public static (int[][] count, int[] sizeOfBin, int[][] startOfBin) ComputeStartOfBinsForSelectionPar(this uint[] inputArray, int start, int length, int workQuanta, int numberOfQuantas, uint digit)
        {
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
            const int numberOfBins = 256;

            int[][] count = ParallelAlgorithm.HistogramByteComponentsQCPar_2(inputArray, start, start + length - 1, workQuanta, numberOfQuantas, digit, workQuanta);

            int[][] startOfBin = new int[numberOfQuantas][];     // start of bin for each parallel work item
            for (int q = 0; q < numberOfQuantas; q++)
                startOfBin[q] = new int[numberOfBins + 1];

            int[] sizeOfBin = new int[numberOfBins];

            // Determine the overall size of each bin, across all work quanta
            for (uint b = 0; b < numberOfBins; b++)
            {
                sizeOfBin[b] = 0;
                for (int q = 0; q < numberOfQuantas; q++)
                {
                    sizeOfBin[b] += count[q][b];
                    //if (digit == 3)
                    //    Console.WriteLine("ComputeStartOfBinsForSelectionPar: digit = {0}  count[{1}][{2}] = {3}", digit, q, b, count[q][b]);
                }
                //Console.WriteLine("ComputeStartOfBinsForSelectionPar: digit = {0}  sizeOfBin[{1}] = {2}", digit, b, sizeOfBin[b]);
            }

            // Determine starting of bins for work quanta 0
            startOfBin[0][0] = start; startOfBin[numberOfQuantas - 1][numberOfBins] = start + length;
            //Console.WriteLine("ComputeStartOfBins: d = {0}  start = {1}", digit, start);
            for (uint b = 1; b < numberOfBins; b++)
            {
                startOfBin[0][b] = startOfBin[0][b - 1] + sizeOfBin[b - 1];  // TODO: Should this be + sizeOfBin[b-1] or count[0][b-1]?
                //if (digit == 3)
                //    Console.WriteLine("ComputeStartOfBins: d = {0}  startOfBin[0][{1}] = {2} sizeOfBin[{1}] = {3}", digit, b, startOfBin[0][b], sizeOfBin[b]);
            }

            // Determine starting of bins for work quanta 1 thru Q
            for (int q = 1; q < numberOfQuantas; q++)
            {
                for (uint b = 0; b < numberOfBins; b++)
                {
                    startOfBin[q][b] = startOfBin[q - 1][b] + count[q - 1][b];
                    //if (digit == 3)
                    //    Console.WriteLine("ComputeStartOfBins: d = {0}  startOfBin[{1}][{2}] = {3}  count[{1}][{2}] = {4}", digit, q, b, startOfBin[q][b], count[q][b]);
                }
            }

            return (count, sizeOfBin, startOfBin);
        }

        // TODO: Implement a sequential not-in-place version first, then implement a parallel equivalent version.
        //       The not-in-place version is much simpler to implement and understand. It may be worthwhile on its own, to help understanding and teaching.
        private static async void RadixSelectionParInner2(uint[] a, uint[] b, int first, int length, int k, int ParallelWorkQuantum = 64 * 1024)
        {
            const uint bitMask = PowerOfTwoRadix - 1;
            int shiftRightAmount = (sizeof(uint) * 8) - Log2ofPowerOfTwoRadix;
            int digit = sizeof(uint) - 1;
            int[] startOfBin = new int[PowerOfTwoRadix + 1];
            int quanta = a.Length / ParallelWorkQuantum + (a.Length % ParallelWorkQuantum == 0 ? 0 : 1);
            while (digit >= 0)
            {
                int last = first + length - 1;
                int kthBin;
                // TODO: ComputeStartOfBinsPar() does too much work since only the bin that k is in needs start of bins computed. But this is a good initial step.
                // TODO: count1[] is not used and can be removed from the return value of ComputeStartOfBinsForSelectionPar() and the call to it.
                var (count1, sizeOfBin, startOfBinPar) = ComputeStartOfBinsForSelectionPar(a, first, length, ParallelWorkQuantum, quanta, (uint)digit);
                //int[] count = Algorithm.HistogramByteOneComponent(a, first, last, digit);
                //for(int i = 0; i < PowerOfTwoRadix; i++)
                //{
                //    if (count[i] != sizeOfBin[i])
                //        throw new Exception($"RadixSelectionParInner2: count[{i}] = {count[i]} != sizeOfBin[{i}] = {sizeOfBin[i]}  d = {digit}");
                //}

                startOfBin[0] = first; startOfBin[PowerOfTwoRadix] = last + 1;
                for (int i = 1; i < PowerOfTwoRadix; i++)
                    startOfBin[i] = startOfBin[i - 1] + sizeOfBin[i - 1];
                // Determine which bin contains the k-th smallest element. kthBin will hold the bin number.
                kthBin = 0;
                for (; kthBin < PowerOfTwoRadix; kthBin++)
                    if (k >= startOfBin[kthBin] && k <= (startOfBin[kthBin + 1] - 1)) break;
#if True
#if False
                MoveOutsideOfKthBinIn_NotInPlace(a, b, first, length, startOfBin[kthBin], shiftRightAmount, bitMask, kthBin);  // Working version!
#else
                // This version also works now!
                int startQuanta = first / ParallelWorkQuantum;
                int endQuanta   = last  / ParallelWorkQuantum;

                if (length <= 0) break;

                //Console.WriteLine("RadixSelectionParInner2: startQuanta = {0}, endQuanta = {1}, ParallelWorkQuantum = {2}", startQuanta, endQuanta, ParallelWorkQuantum);
                // TODO: The following if/else should be simplified to just the else case, and possibly handle the start and end condition automatically.
                if (startQuanta == endQuanta)       // moving array elements within a single workQuantum, either partial or full
                {
                    int q = startQuanta;
                    MoveOutsideOfKthBinIn_NotInPlace(a, b, first, length, startOfBinPar[q][kthBin], shiftRightAmount, bitMask, kthBin);
                }
                else  // startQuanta < endQuanta, moving array elements of multiple workQuantums, either partial or full
                {
                    // process startQuanta, which is either partial or full, from first to the end of the startQuanta
                    int q = startQuanta;
                    MoveOutsideOfKthBinIn_NotInPlace(a, b, first, ((q + 1) * ParallelWorkQuantum) - first, startOfBinPar[q][kthBin], shiftRightAmount, bitMask, kthBin);

                    // process (startQuanta + 1) to (endQuanta - 1), which are all full workQuantas
                    for (q = startQuanta + 1; q <= (endQuanta - 1); q++)
                        MoveOutsideOfKthBinIn_NotInPlace(a, b, q * ParallelWorkQuantum, ParallelWorkQuantum, startOfBinPar[q][kthBin], shiftRightAmount, bitMask, kthBin);

                    // process endQuanta, which is either partial or full
                    MoveOutsideOfKthBinIn_NotInPlace(a, b, q * ParallelWorkQuantum, last - (q * ParallelWorkQuantum) + 1, startOfBinPar[q][kthBin], shiftRightAmount, bitMask, kthBin);
                }
#endif
#else
                List<Task> tasks = new List<Task>();
                // Implement parallel move of elements outside the k-th bin into the k-th bin
                for (int q = 0; q < quanta; q++)
                {
                    tasks.Add(Task.Run(() => MoveOutsideOfKthBinIn_NotInPlaceAsync(
                        a, b, first + (q * ParallelWorkQuantum), Math.Min(ParallelWorkQuantum, last - (first + (q * ParallelWorkQuantum)) + 1),
                        startOfBinPar[q][kthBin], shiftRightAmount, bitMask, kthBin)));
                }
                await Task.WhenAll(tasks);
#endif
                if (shiftRightAmount <= 0) break;
                digit--;
                length = startOfBin[kthBin + 1] - startOfBin[kthBin];
                if (length > 1)
                {
                    first = startOfBin[kthBin];
                    (a, b) = (b, a); // swap a and b for next iteration
                    if (shiftRightAmount >= Log2ofPowerOfTwoRadix) shiftRightAmount -= Log2ofPowerOfTwoRadix;
                    else shiftRightAmount = 0;
                }
                else if (length == 1)
                {
                    if (Int32.IsOddInteger(digit)) break; // Only one element in the bin that k is in, so it must be the k-th smallest element
                    else { a[startOfBin[kthBin]] = b[startOfBin[kthBin]]; break; }
                }
                else throw new Exception("RadixSelectiontInner2: No elements in the bin that k is in, which should never happen");
            }
        }
        /// <summary>
        /// Not-In-place Radix Selection, non-recursive implementation.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be selected from in place</param>
        /// <param name="start">starting index of the subarray</param>
        /// <param name="length">length of the subarray</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static uint SelectRadixPar(this uint[] arrayToBeSelected, Int32 start, Int32 length, Int32 k, int parallelThreshold = 64 * 1024)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (start < 0 || length <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "l or r are invalid");
            if (k < start || k > (start + arrayToBeSelected.Length))
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            uint[] tmpArray = new uint[arrayToBeSelected.Length];
            RadixSelectionParInner2(arrayToBeSelected, tmpArray, start, length, k, parallelThreshold);
            return arrayToBeSelected[k];
        }
        /// <summary>
        /// Not-In-place Radix Selection, non-recursive implementation.
        /// </summary>
        /// <param name="arrayToBeSelected">array that is to be sorted in place</param>
        /// <param name="k">index of the desired element to be selected</param>
        public static uint SelectRadixPar(this uint[] arrayToBeSelected, Int32 k, int parallelThreshold = 64 * 1024)
        {
            if (arrayToBeSelected == null)
                throw new ArgumentNullException(nameof(arrayToBeSelected));
            if (arrayToBeSelected.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(arrayToBeSelected.Length), "array length is invalid");
            if (k < 0 || k > arrayToBeSelected.Length)
                throw new ArgumentOutOfRangeException(nameof(k), "k must be between start and (start + length)");
            uint[] tmpArray = new uint[arrayToBeSelected.Length];
            RadixSelectionParInner2(arrayToBeSelected, tmpArray, 0, arrayToBeSelected.Length, k, parallelThreshold);
            return arrayToBeSelected[k];
        }
    }
}
