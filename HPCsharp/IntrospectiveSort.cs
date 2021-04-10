// Adopted from https://www.geeksforgeeks.org/introsort-or-introspective-sort/
using System;
using System.Collections.Generic;
using System.Text;

namespace HPCsharp
{
	static public partial class Algorithm
	{
		// The utility function to swap two elements
		internal static void swap(int[] src, int i, int j)
		{
			int temp = src[i];
			src[i] = src[j];
			src[j] = temp;
		}

		// To maxHeap a subtree rooted with node i which is an index in []a. heapN is size of heap
		private static void maxHeap(int[] src, int i, int heapN, int begin)
		{
			int temp = src[begin + i - 1];
			int child;

			while (i <= heapN / 2)
			{
				child = 2 * i;

				if (child < heapN && src[begin + child - 1] < src[begin + child])
					child++;

				if (temp >= src[begin + child - 1])
					break;

				src[begin + i - 1] = src[begin + child - 1];
				i = child;
			}
			src[begin + i - 1] = temp;
		}

		// Function to build the heap (rearranging the array)
		private static void heapify(int[] src, int begin, int end, int heapN)
		{
			for (int i = (heapN) / 2; i >= 1; i--)
				maxHeap(src, i, heapN, begin);
		}

		// main function to do heapsort
		public static void heapSort(int[] src, int begin, int end)
		{
			int heapN = end - begin;

			// Build heap (rearrange array)
			heapify(src, begin, end, heapN);

			// One by one extract an element from heap
			for (int i = heapN; i >= 1; i--)
			{
				// Move current root to end
				swap(src, begin, begin + i);

				// call maxHeap() on the reduced heap
				maxHeap(src, 1, i, begin);
			}
		}

		// function that implements insertion sort
		internal static void insertionSort(int[] src, int left, int right)
		{
			for (int i = left; i <= right; i++)
			{
				int key = src[i];
				int j = i;

				// Move elements of arr[0..i-1], that are greater than the key, 
				// to one position ahead of their current position
				while (j > left && src[j - 1] > key)
				{
					src[j] = src[j - 1];
					j--;
				}
				src[j] = key;
			}
		}

		// Function for finding the median of the three elements
		internal static int findPivot(int[] src, int a1, int b1, int c1)
		{
			int max = Math.Max( Math.Max(src[a1], src[b1]), src[c1]);
			int min = Math.Min( Math.Min(src[a1], src[b1]), src[c1]);
			int median = max ^ min ^ src[a1] ^ src[b1] ^ src[c1];
			if (median == src[a1])
				return a1;
			if (median == src[b1])
				return b1;
			return c1;
		}

		// This function takes the last element as pivot, places the pivot element at 
		// its correct position in sorted array, and places all smaller 
		// (smaller than pivot) to the left of the pivot and greater elements to the right of the pivot
		internal static int partition(int[] src, int low, int high)
		{
			// pivot
			int pivot = src[high];

			// Index of smaller element
			int i = (low - 1);

			for (int j = low; j <= high - 1; j++)
			{
				// If the current element is smaller than or equal to the pivot
				if (src[j] <= pivot)
				{
					// increment index of smaller element
					i++;
					swap(src, i, j);
				}
			}
			swap(src, i + 1, high);
			return (i + 1);
		}

		// The main function that implements 
		// Introsort low --> Starting index, high --> Ending index, depthLimit --> recursion level
		private static void IntroSortInner(int[] src, int begin, int end, int depthLimit)
		{
			if (end - begin > 16)
			{
				if (depthLimit == 0)
				{
					// if the recursion limit is occurred call heap sort
					heapSort(src, begin, end);
					return;
				}

				depthLimit = depthLimit - 1;
				int pivot = findPivot(src, begin, begin + ((end - begin) / 2) + 1, end);
				swap(src, pivot, end);

				// p is partitioning index, arr[p] is now at right place
				int p = partition(src, begin, end);

				// Separately sort elements before partition and after partition
				IntroSortInner(src, begin, p - 1, depthLimit);
				IntroSortInner(src, p + 1, end,   depthLimit);
			}
			else
			{
				// if the data set is small, call insertion sort
				insertionSort(src, begin, end);
			}
		}

		// A utility function to begin the Introsort module
		public static void IntroSort(int[] src)
		{
			// Initialise the depthLimit as 2*log(length(data))
			int depthLimit = (int)(2 * Math.Floor(Math.Log(src.Length) / Math.Log(2)));

			IntroSortInner(src, 0, src.Length - 1, depthLimit);
		}

		// The utility function to swap two elements
		internal static void swap(uint[] src, int i, int j)
		{
			uint temp = src[i];
			src[i] = src[j];
			src[j] = temp;
		}

		// To maxHeap a subtree rooted with node i which is an index in []a. heapN is size of heap
		private static void maxHeap(uint[] src, int i, int heapN, int begin)
		{
			uint temp = src[begin + i - 1];
			int child;

			while (i <= heapN / 2)
			{
				child = 2 * i;

				if (child < heapN && src[begin + child - 1] < src[begin + child])
					child++;

				if (temp >= src[begin + child - 1])
					break;

				src[begin + i - 1] = src[begin + child - 1];
				i = child;
			}
			src[begin + i - 1] = temp;
		}

		// Function to build the heap (rearranging the array)
		private static void heapify(uint[] src, int begin, int end, int heapN)
		{
			for (int i = (heapN) / 2; i >= 1; i--)
				maxHeap(src, i, heapN, begin);
		}

		// main function to do heapsort
		public static void heapSort(uint[] src, int begin, int end)
		{
			int heapN = end - begin;

			// Build heap (rearrange array)
			heapify(src, begin, end, heapN);

			// One by one extract an element from heap
			for (int i = heapN; i >= 1; i--)
			{
				// Move current root to end
				swap(src, begin, begin + i);

				// call maxHeap() on the reduced heap
				maxHeap(src, 1, i, begin);
			}
		}

		// function that implements insertion sort
		internal static void insertionSort(uint[] src, int left, int right)
		{
			for (int i = left; i <= right; i++)
			{
				uint key = src[i];
				int j = i;

				// Move elements of arr[0..i-1], that are greater than the key, 
				// to one position ahead of their current position
				while (j > left && src[j - 1] > key)
				{
					src[j] = src[j - 1];
					j--;
				}
				src[j] = key;
			}
		}

		// Function for finding the median of the three elements
		internal static int findPivot(uint[] src, int a1, int b1, int c1)
		{
			uint max = Math.Max(Math.Max(src[a1], src[b1]), src[c1]);
			uint min = Math.Min(Math.Min(src[a1], src[b1]), src[c1]);
			uint median = max ^ min ^ src[a1] ^ src[b1] ^ src[c1];
			if (median == src[a1])
				return a1;
			if (median == src[b1])
				return b1;
			return c1;
		}

		// This function takes the last element as pivot, places the pivot element at 
		// its correct position in sorted array, and places all smaller 
		// (smaller than pivot) to the left of the pivot and greater elements to the right of the pivot
		internal static int partition(uint[] src, int low, int high)
		{
			// pivot
			uint pivot = src[high];

			// Index of smaller element
			int i = (low - 1);

			for (int j = low; j <= high - 1; j++)
			{
				// If the current element is smaller than or equal to the pivot
				if (src[j] <= pivot)
				{
					// increment index of smaller element
					i++;
					swap(src, i, j);
				}
			}
			swap(src, i + 1, high);
			return (i + 1);
		}

		// The main function that implements 
		// Introsort low --> Starting index, high --> Ending index, depthLimit --> recursion level
		internal static void IntroSortInner(uint[] src, int begin, int end, int depthLimit)
		{
			if (end - begin > 16)
			{
				if (depthLimit == 0)
				{
					// if the recursion limit is occurred call heap sort
					heapSort(src, begin, end);
					return;
				}

				depthLimit = depthLimit - 1;
				int pivot = findPivot(src, begin, begin + ((end - begin) / 2) + 1, end);
				swap(src, pivot, end);

				// p is partitioning index, arr[p] is now at right place
				int p = partition(src, begin, end);

				// Separately sort elements before partition and after partition
				IntroSortInner(src, begin, p - 1, depthLimit);
				IntroSortInner(src, p + 1, end, depthLimit);
			}
			else
			{
				// if the data set is small, call insertion sort
				insertionSort(src, begin, end);
			}
		}

		// A utility function to begin the Introsort module
		public static void IntroSort(uint[] src)
		{
			// Initialise the depthLimit as 2*log(length(data))
			int depthLimit = (int)(2 * Math.Floor(Math.Log(src.Length) / Math.Log(2)));

			IntroSortInner(src, 0, src.Length - 1, depthLimit);
		}
	}
}

