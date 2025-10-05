// TODO: Implement nonrecursive version of QuickSelect (p. 343 Algorithms in C++, Sedgewick).
using System;
using System.Collections;
using System.Collections.Generic;

namespace HPCsharp
{
    public static partial class Algorithm
    {
        // AI generated code for finding the median of five elements directly
        //private static int FindMedianOfFiveDirect(int[] arr, int start)
        //{
        //    if (arr[start] > arr[start + 1])
        //        (arr[start], arr[start + 1]) = (arr[start + 1], arr[start]); // arr[start] <= arr[start + 1]
        //    if (arr[start + 2] > arr[start + 3])
        //        (arr[start + 2], arr[start + 3]) = (arr[start + 3], arr[start + 2]); // arr[start + 2] <= arr[start + 3]
        //    if (arr[start] > arr[start + 2])
        //        (arr[start], arr[start + 2]) = (arr[start + 2], arr[start]); // arr[start] <= arr[start + 2], arr[start + 1]
        //    if (arr[start + 1] > arr[start + 3])
        //        (arr[start + 1], arr[start + 3]) = (arr[start + 3], arr[start + 1]); // arr[start] <= arr[start + 2], arr[start + 1] <= arr[start + 3]
        //    if (arr[start + 1] > arr[start + 2])
        //        (arr[start + 1], arr[start + 2]) = (arr[start + 2], arr[start + 1]); // arr[start] <= arr[start + 1] <= arr[start + 2] <= arr[start + 3]
        //    if (arr[start + 4] < arr[start + 2]) // arr[start + 4] < arr[start + 2] <= arr[start + 3]
        //        return arr[start + 2];
        //}

        // C# implementation of the Worst Case Linear Time algorithm
        // to find the k-th smallest element using Median of Medians
        // Returns median of a small group (size <= 5)
        private static int GetMedian(int[] group)
        {
            Array.Sort(group);
            return group[group.Length / 2];
        }

        // Function to Partition array from index 
        // l to r around  the pivot value x
        private static int PartitionAroundPivot(int[] arr, int l, int r, int x)
        {
            // Move pivot x to end
            int i;
            for (i = l; i < r; i++)
            {
                if (arr[i] == x) break;
            }
            (arr[r], arr[i]) = (arr[i], arr[r]);

            // Standard partition logic
            i = l;
            for (int j = l; j < r; j++)
            {
                if (arr[j] <= x)
                {
                    (arr[j], arr[i]) = (arr[i], arr[j]);
                    i++;
                }
            }
            (arr[r], arr[i]) = (arr[i], arr[r]);

            // Final position of pivot
            return i;
        }

        // Recursively finds the k-th smallest element in arr[l..r]
        private static int SelectKthSmallest(int[] arr, int l, int r, int k)
        {
            if (k > 0 && k <= r - l + 1)
            {
                int n = r - l + 1;
                var medians = new List<int>();
                int i;

                // Divide array into groups of 5 and store their medians
                for (i = 0; i < n / 5; i++)
                {
                    int[] group = new int[5];
                    Array.Copy(arr, l + i * 5, group, 0, 5);
                    medians.Add(GetMedian(group));
                }

                // Handle the last group with less than 5 elements
                if (i * 5 < n)
                {
                    int len = n % 5;
                    int[] lastGroup = new int[len];
                    Array.Copy(arr, l + i * 5, lastGroup, 0, len);
                    medians.Add(GetMedian(lastGroup));
                }

                // Find median of medians
                int pivot;
                if (medians.Count == 1) pivot = medians[0];
                else
                {
                    int[] medArr = medians.ToArray();
                    pivot = SelectKthSmallest(medArr, 0, medArr.Length - 1, medArr.Length / 2);
                }

                // Partition array and get position of pivot
                int pos = PartitionAroundPivot(arr, l, r, pivot);

                // If position matches k, return result
                if (pos - l == k - 1) return arr[pos];

                // Recur on left or right part accordingly
                if (pos - l > k - 1)
                    return SelectKthSmallest(arr, l, pos - 1, k);

                return SelectKthSmallest(arr, pos + 1, r, k - pos + l - 1);
            }
            return int.MaxValue;
        }

        // Function to find kth Smallest in Array
        private static int KthSmallest(int[] arr, int k)
        {
            return SelectKthSmallest(arr, 0, arr.Length - 1, k);
        }

        // l and r are inclusive
        private static int MedianOfMedians(int[] arr, int l, int r)
        {
            if (r == l) return arr[l];

            int i, n = r - l + 1;
            var medians = new List<int>();

            // Divide array into groups of 5 and store their medians
            for (i = 0; i < n / 5; i++)
            {
                var group = new int[5];
                Array.Copy(arr, l + i * 5, group, 0, 5);
                medians.Add(GetMedian(group));
            }
            // Handle the last group with less than 5 elements
            if (i * 5 < n)
            {
                int len = n % 5;
                int[] lastGroup = new int[len];
                Array.Copy(arr, l + i * 5, lastGroup, 0, len);
                medians.Add(GetMedian(lastGroup));
            }
            int[] medArr = medians.ToArray();
            return MedianOfMedians(medArr, 0, medArr.Length - 1);
        }

        // l and r are inclusive
        private static int MedianOfMedians2(int[] arr, int l, int r)
        {
            if (r == l) return arr[l];

            int i, n = r - l + 1;
            var medians = new List<int>();

            // Divide array into groups of 5 and store their medians
            for (i = 0; i < n / 5; i++)
            {
                var group = new int[5];
                Array.Copy(arr, l + i * 5, group, 0, 5);
                medians.Add(GetMedian(group));
            }
            // Handle the last group with less than 5 elements
            if (i * 5 < n)
            {
                int len = n % 5;
                int[] lastGroup = new int[len];
                Array.Copy(arr, l + i * 5, lastGroup, 0, len);
                medians.Add(GetMedian(lastGroup));
            }
            int[] medArr = medians.ToArray();
            return MedianOfMedians(medArr, 0, medArr.Length - 1);
        }

        // QuickSelect partition
        private static int Partition(int[] arr, int l, int r, Random rand)
        {
            int i = l - 1, j = r; // index of smaller element
            int r_index = rand.Next(l, r + 1);
            (arr[r], arr[r_index]) = (arr[r_index], arr[r]); // Move pivot to end
            int v = arr[r];       // pivot arbitrarily chosen as last element

            //int v = MedianOfMedians(arr, l, r); // pivot chosen as median of medians
            //for (i = l; i < r; i++) if (arr[i] == v) break;
            //(arr[r], arr[i]) = (arr[i], arr[r]); // Move pivot to end

            while (true)
            {
                // find item on left to swap
                while (arr[++i] < v) ;
                // find item on right to swap
                while (v < arr[--j]) if (j == l) break;
                // check if pointers cross
                if (i >= j) break;
                // swap
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            // swap arr[i+1] and arr[r] (or pivot)
            (arr[i], arr[r]) = (arr[r], arr[i]);
            return i;
        }

        private static int Partition(int[] arr, int l, int r)
        {
            int i = l - 1, j = r; // index of smaller element
            int v = arr[r];       // pivot arbitrarily chosen as last element

            while (true)
            {
                // find item on left to swap
                while (arr[++i] < v) ;
                // find item on right to swap
                while (v < arr[--j]) if (j == l) break;
                // check if pointers cross
                if (i >= j) break;
                // swap
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            // swap arr[i+1] and arr[r] (or pivot)
            (arr[i], arr[r]) = (arr[r], arr[i]);
            return i;
        }

        // QuickSelect recursive
        public static void QuickSelect(int[] arr, int l, int r, int k)
        {
            if (r <= l) return;
            int i = Partition(arr, l, r);
            if (i > k) QuickSelect(arr, l,     i - 1, k);
            if (i < k) QuickSelect(arr, i + 1, r,     k);
        }

        private static void QuickSelectRandom_loc(int[] arr, int l, int r, int k, Random rand)
        {
            if (r <= l) return;
            int i = Partition(arr, l, r, rand);
            if (i > k) QuickSelectRandom_loc(arr, l, i - 1, k, rand);
            if (i < k) QuickSelectRandom_loc(arr, i + 1, r, k, rand);
        }

        public static void QuickSelectRandom(int[] arr, int l, int r, int k, int randSeed = -1)
        {
            Random rand = (randSeed < 0) ? new Random() : new Random(randSeed);
            QuickSelectRandom_loc(arr, l, r, k, rand);
        }

        public static void QuickSelectRandom(int[] arr, int k, int randSeed = -1)
        {
            Random rand = (randSeed < 0) ? new Random() : new Random(randSeed);
            QuickSelectRandom_loc(arr, 0, arr.Length - 1, k, rand);
        }

        // partition function similar to quick sort 
        // Considers last element as pivot and adds 
        // elements with less value to the left and 
        // high value to the right and also changes 
        // the pivot position to its respective position
        // in the readonly array.
        private static int Partitions(int[] arr, int low, int high)
        {
            int pivot = arr[high], pivotloc = low;
            for (int i = low; i <= high; i++)
            {
                // inserting elements of less value to the left of the pivot location
                if (arr[i] < pivot)
                {
                    (arr[i], arr[pivotloc]) = (arr[pivotloc], arr[i]);
                    pivotloc++;
                }
            }
            // swapping pivot to the readonly pivot location
            (arr[high], arr[pivotloc]) = (arr[pivotloc], arr[high]);

            return pivotloc;
        }

        // finds the kth position (of the sorted array) 
        // in a given unsorted array i.e this function 
        // can be used to find both kth largest and 
        // kth smallest element in the array. 
        // ASSUMPTION: all elements in []arr are distinct
        private static int KthSmallestQuickSelect_int(int[] arr, int low, int high, int k)
        {
            // find the partition 
            int partition = Partitions(arr, low, high);

            // if partition value is equal to the kth position, return value at k.
            if (partition == k)
                return arr[partition];

            // if partition value is less than kth position,
            // search right side of the array.
            else if (partition < k)
                return KthSmallestQuickSelect_int(arr, partition + 1, high, k);

            // if partition value is more than kth position, 
            // search left side of the array.
            else return KthSmallestQuickSelect_int(arr, low, partition - 1, k);
        }

        // Function to find kth Smallest in Array
        public static int KthSmallestQuickSelect(int[] arr, int k)
        {
            return KthSmallestQuickSelect_int(arr, 0, arr.Length - 1, k);
        }

        private static int Partitions<T>(T[] arr, int low, int high, IComparer<T> comparer = null)
        {
            var equalityComparer = comparer ?? Comparer<T>.Default;
            T pivot = arr[high];
            int pivotloc = low;
            for (int i = low; i <= high; i++)
            {
                // inserting elements of smaller value to the left of the pivot location
                if (equalityComparer.Compare(arr[i], pivot) < 0)
                {
                    (arr[i], arr[pivotloc]) = (arr[pivotloc], arr[i]);
                    pivotloc++;
                }
            }
            // swapping pivot to the readonly pivot location
            (arr[high], arr[pivotloc]) = (arr[pivotloc], arr[high]);

            return pivotloc;
        }
        // finds the kth position (of the sorted array) 
        // in a given unsorted array i.e this function 
        // can be used to find both kth largest and 
        // kth smallest element in the array. 
        // ASSUMPTION: all elements in []arr are distinct
        private static T KthSmallestQuickSelect_int<T>(T[] arr, int low, int high, int k, IComparer<T> comparer = null)
        {
            // find the partition 
            int partition = Partitions(arr, low, high, comparer);

            // if partition value is equal to the kth position, return value at k.
            if (partition == k)
                return arr[partition];

            // if partition value is less than kth position,
            // search right side of the array.
            else if (partition < k)
                return KthSmallestQuickSelect_int(arr, partition + 1, high, k, comparer);

            // if partition value is more than kth position, 
            // search left side of the array.
            else return KthSmallestQuickSelect_int(arr, low, partition - 1, k, comparer);
        }

        // Function to find kth Smallest in Array
        public static T KthSmallestQuickSelect<T>(T[] arr, int k, IComparer<T> comparer = null)
        {
            return KthSmallestQuickSelect_int(arr, 0, arr.Length - 1, k, comparer);
        }
    }
}


