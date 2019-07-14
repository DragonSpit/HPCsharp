If you like HPCsharp, then help us keep more good stuff coming.\
Let us know other algorithms that could use acceleration.

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LDD8L7UPAC7QL)

# High Performance Computing in C# (HPCsharp)

High performance cross-platform C# generic algorithms. Community driven to raise C# performance. Familiar interfaces,
similar to standard C# algorithms and Linq. Free, open source, on nuget.org

*Algorithm*|*\**|*\*\**|*SSE*|*Multi-Core*|*Array*|*List*|*Details*
--- | --- | --- | --- | :---: | :---: | --- | :--
Add\*\*\* | 2 | 4 | | | :heavy_check_mark: | :heavy_check_mark: | Adds two arrays element-wise
[Binary Search](#Binary-Search) | 1 | 2 | | | :heavy_check_mark: | :heavy_check_mark: | Generic IComparer\<T\>
[Block Swap](#Block-Swap) | 4 | 5 | | | :heavy_check_mark: | | Generic
[Parallel Copy](#Parallel-Copy) |  | | | |  | |
Copy List to Array |  | | | |  | |
[Counting Sort](#Counting-Sort) | 3 | 14 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | | byte, ushort, sbyte, short arrays. Ludicrous speed!
Divide-And-Conquer\*\*\* | 1 | 2 | | :heavy_check_mark: | :heavy_check_mark: | | Generic scalar and parallel abstraction
Fill | 4 | 10 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | | Numeric arrays
Histogram | 14 | 35 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | | Byte, N-bit components of numeric arrays
[Insertion Sort](#Insertion-Sort) | 1 | 2 | | | :heavy_check_mark: | :heavy_check_mark: | Generic IComparer\<T\>
[Max, Min](#Min-and-Max) | 2 | 12 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | Generic IComparer\<T\>
[Merge](#Merge) | 2 | 18 | | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | Generic IComparer\<T\>
Multi-way Merge | 1 | | | | :heavy_check_mark: | |
[Merge Sort](#Merge-Sort) | 2 | 25 | | :heavy_check_mark: | :heavy_check_mark: | | Generic, Stable or not, whole or partial
Priority Queue | 2 | 15 | | | :heavy_check_mark: | | 
[Radix Sort (LSD)](#LSD-Radix-Sort) | 6 | 40 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | | Numeric arrays, user defined types, Stable
Radix Sort (MSD) | 4 | 24| :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | | Numeric arrays, user defined types, In-place
Sequence Equal | 2 | 19 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | | 
[Sum](#Better-Sum-in-Many-Ways) | 7 | 157 | :heavy_check_mark: | :heavy_check_mark: | :heavy_check_mark: | | Numeric arrays. [Better in many ways](https://duvanenko.tech.blog/2019/04/23/better-sum-in-c/)
Swap | 4 | 4 | | | :heavy_check_mark: | | Generic swap variations
[Zero Array Detect](#Zero-Array-Detect) | 3 | 13 | :heavy_check_mark: | | :heavy_check_mark: | | Detect if byte array is all zeroes

\* Number of different algorithms\
\*\* Number of functions for this algorithm\
\*\*\* Coming soon

## Examples
Usage examples are provided in the HPCsharpExamples folder, which has a VisualStudio 2017 solution. Build and run it to see performance gains on your machine.
To get the maximum performance make sure to target x64 processor architecture for the Release build in VisualStudio, increasing performance by as much as 50%.

## Better Sum in Many Ways
HPCsharp improves .Sum() of numeric arrays in the following ways:
- No overflow exception for int[] while providing 5X higher performance
- Summation of all signed integer data types
- Summation of all unsigned integer data types
- Full accuracy summation of long[] and ulong[], without throwing overflow exception
- Two higher precision floating-point summation options, reducing error from ***O***(eN) downto ***O***(elgN) without reduction
in performance, and ***O***(e), with slight performance reduction. Implements pairwise and Kahan/Neumaier summation algorithms
- Implements many algorithms using multi-core and data parallel SIMD/SSE processor instructions

The table below compares performance (in Giga/second) of Linq.AsParallel().Sum() and HPCsharp.SumSsePar().

*Library*|*sbyte*|*byte*|*short*|*ushort*|*int*|*uint*|*long*|*ulong*|*float*|*double*|*decimal*
--- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | ---
Linq | n/a | n/a | n/a | n/a |0.9*|n/a|0.9*|n/a|0.9|0.9|0.12
HPCsharp |7.6|8.0|8.0|8.2|5.0|5.3|2.9*|2.8*|5.1|2.9|0.14
* overflow exception is possible

HPCsharp includes long[] and ulong[] summation, which do not throw an overflow exception while producing a perfectly accurate result.

For more details, see blog https://duvanenko.tech.blog/2019/04/23/better-sum-in-c/

## Counting Sort

*Algorithm*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*MegaBytes/sec*|*Data Type*
--- | --- | --- | --- | --- | --- | ---
Counting Sort|Random|27-56X|156-343X|39-70X|846|byte
Counting Sort|Presorted|26-56X|168-344X|38-66X|864|byte
Counting Sort|Constant|30-56X|165-321X|34-70X|847|byte

Counting Sort above is a linear time ***O***(N) algorithm, sorting an array of byte, sbyte, short or ushort data types.
In-place and not-in-place version have been implementated.
The above benchmark is on a single core! Multi-core sorts even faster, at GigaElements/second ludicrous speed.

## LSD Radix Sort

LSD Radix Sort is a linear time ***O***(N), stable sorting algorithm.
 
*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*MegaInts/sec*|*Data Type*
--- | --- | --- | --- | --- | --- | --- | ---
Radix Sort|Array, List|Random|5X-8X|14X-35X|4X-9X|82|UInt32
Radix Sort|Array, List|Presorted|0.3X-0.6X|3X-5X|1X-3X|48|UInt32
Radix Sort|Array, List|Constant|1.3X-1.8X|5X-8X|2X-3X|50|UInt32

LSD Radix Sort runs on a single core, whereas Linq.AsParallel ran on all the cores.
Only slower when sorting presorted Array or List, but faster for random and constant distributions,
even faster than parallel Linq.OrderBy.AsParallel.

Radix Sort has been extended to sort user defined classes based on a UInt32 or UInt64 key within the class. Radix Sort is currently using only a single core.

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*Description*
--- | --- | --- | --- | --- | --- | ---
Radix Sort|Array|Random|1X-4X|3X-5X|1X-2X|User defined class
Radix Sort|List|Random|2X-4X|3X-5X|1X-2X|User defined class
Radix Sort|Array|Presorted|1.2X-1.7X|0.9X-2.5X|0.9X-1.4X|User defined class
Radix Sort|List|Presorted|1.0X-1.2X|1.7X-2.1X|0.7X-1.1X|User defined class
Radix Sort|Array|Constant|3X-4X|4X-5X|2X-3X|User defined class
Radix Sort|List|Constant|2X-4X|3X-4X|1.5X-2X|User defined class

Only slightly slower than Array.Sort and List.Sort for presorted distribution, but faster for all other distributions. Uses a single core and is stable.
Faster than Linq.OrderBy and Linq.OrderBy.AsParallel

## Merge Sort

Parallel Merge Sort uses multiple CPU cores to accelerate performance. On a quad-core laptop, performance is:

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*MegaInts/sec*
--- | --- | --- | --- | --- | --- | ---
Parallel Merge Sort|Array|Random|3X|12X|5X|25
Parallel Merge Sort|Array|Presorted|2X|22X|13X|110
Parallel Merge Sort|Array|Constant|2X|15X|9X|74

Parallel Merge Sort is not stable, just like Array.Sort. Faster than Array.Sort and List.Sort across all distributions.
Substantially faster than Linq.OrderBy and Linq.OrderBy.AsParallel

**_28-core (56-threads) AWS c5.18xlarge_**

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*Description*
--- | --- | --- | --- | --- | --- | ---
Parallel Merge Sort|Array|Random|5X-14X|19X-90X|7X-47X|
Parallel Merge Sort|Array|Presorted|1X-6X|5X-60X|16X-122X|
Parallel Merge Sort|Array|Constant|TBD|TBD|9X-44X|

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*MegaInts/sec*
--- | --- | --- | --- | --- | --- | ---
Merge Sort (stable)|Array|Random|0.6X|2.5X|1X|5
Merge Sort (stable)|Array|Presorted|0.3X|3X|2X|17
Merge Sort (stable)|Array|Constant|0.5X|3X|2X|15

Merge Sort is ***O***(NlgN), never ***O***(N<sup>2</sup>), generic, stable, and runs on a single CPU core. Faster than Linq.OrderBy and Linq.OrderBy.AsParallel.

## Merge
***O***(N) linear-time generic merge algorithms for arrays and list containers. Merges two pre-sorted arrays or lists,
of any data type that defines IComparer<T>.
Two not-in-place algorithms: comparison at the heads, and divide-and-conquer.
Parallel Merge algorithm, using divide-and-conquer, merges two presorted collections using multiple cores.
Used by Parallel Merge Sort. See example solution for working code samples.

## Insertion Sort
Insertion Sort, which is ***O***(N<sup>2</sup>), and useful for fast in-place sorting of very small collections, due to its cache-friendliness.
Generic implemenation for Array and List containers. Used by Parallel Merge Sort and MSD Radix Sort for the base case.

## Binary Search
Generic implementation of the binary search algorithm, for Array and List containers. Used by the scalar and parallel divide-and-conquer
Merge algorithms.

## Min and Max
*Algorithm*|*Collection*|*vs Linq*|*Parallel vs Linq*
--- | --- | --- | ---
SequenceEqual|Array, List|4X faster|up to 11X faster
Min|Array|14-26X faster|4-7X faster
Max|Array|1.5X faster

.Min() is implemented using SIMD/SSE instructions to run at 4 GigaInts/sec on a single core, and over 5 GigaInts/sec on quad-core.

## Block Swap
Three scalar algorithms for in-place swapping two neighboring sub-regions of an array, which do not have to be of equal size:
- Reversal
- Gries and Mills
- Juggle Bentley

See an article for more details (http://www.drdobbs.com/parallel/benchmarking-block-swapping-algorithms/232900395)

Also, several generic version of two element swap.

## Zero Array Detect
Detects whether a byte array is zero in every byte. Runs at 17 GBytes/sec on a quad-core laptop, with two memory channels, using a single core.
Provides short-circuit, early exit when a non-zero value is detected while scanning the array. Provides scalar, SSE, scalar-unrolled, SSE-unrolled,
scalar unrolled multi-core, and SSE unrolled multi-core implementations. Unrolled refers to the loop being unrolled a few times to gain
additional performance.

On dual memory channel CPUs, SSE-unrolled is the fastest, using a single core, saturating system memory bandwidth.
For systems with more memory channels, SSE unrolled multi-core will most likely have the highest performance.

## Parallel Copy
*Method*|*Collection*|*Parallel*
--- | --- | ---
Parallel CopyTo|List to Array|1.7X-2.5X faster

Discussion on when it's appropriate to use parallel copy is coming soon...

## Naming Conventions
HPCsharp follows a few simple naming conventions:
- SSE functions append "Sse" to the function name
- multi-core functions append "Par" to the function name
- if the function name clashes with C# Linq name, then "Hpc" is appended to the function name

## Blogs and Videos
For details on the motivation see blog:
https://duvanenko.tech.blog/2018/03/03/high-performance-c/

For more performance discussion see blog:
https://duvanenko.tech.blog/2018/05/23/faster-sorting-in-c/

Recent HPCsharp presentation at the Indianapolis .NET Consortium, March 2019 on https://youtu.be/IRNW4VGevvQ

## Website for Feature Votes
Visit us at https://foostate.com/ and let us know what other high performance algorithms are important to you, and you'd like to see in this NuGet package.

## Encouragement
If you like it, then help us keep more good stuff like this coming. Let us know other algorithms that could use acceleration.

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LDD8L7UPAC7QL)
