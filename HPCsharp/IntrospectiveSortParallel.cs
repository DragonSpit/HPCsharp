using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;

namespace HPCsharp
{
    static public partial class ParallelAlgorithm
    {
		// The main function that implements 
		// Introsort low --> Starting index, high --> Ending index, depthLimit --> recursion level
		private static void IntroSortInnerPar(uint[] src, int begin, int end, int depthLimit)
		{
			const Int32 cutoff = 4096;

			if (end - begin <= cutoff)
			{
				Algorithm.IntroSortInner(src, begin, end, depthLimit);
			}
			else
			{
				if (end - begin > 16)
				{
					if (depthLimit == 0)
					{
						// if the recursion limit is occurred call heap sort
						Algorithm.heapSort(src, begin, end);
						return;
					}

					depthLimit = depthLimit - 1;
					int pivot = Algorithm.findPivot(src, begin, begin + ((end - begin) / 2) + 1, end);
					Algorithm.swap(src, pivot, end);

					// p is partitioning index, arr[p] is now at right place
					int p = Algorithm.partition(src, begin, end);

					// Separately sort elements before partition and after partition
					Parallel.Invoke(
						() => { IntroSortInnerPar(src, begin, p - 1, depthLimit); },
						() => { IntroSortInnerPar(src, p + 1, end, depthLimit); }
					);
				}
				else
				{
					// if the data set is small, call insertion sort
					Algorithm.insertionSort(src, begin, end);
				}
			}
		}

		// A utility function to begin the Introsort module
		public static void IntroSortPar(uint[] src)
		{
			// Initialise the depthLimit as 2*log(length(data))
			int depthLimit = (int)(2 * Math.Floor(Math.Log(src.Length) / Math.Log(2)));

			IntroSortInnerPar(src, 0, src.Length - 1, depthLimit);
		}
	}
}
