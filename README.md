# High Performance Computing in C# (HPCsharp)

High performance C# generic algorithms. Community driven to raise C# performance.
Parallel algorithms for sorting, merging, copying and others. Parallel Merge Sort and parallel Merge of arrays and lists.
Linear and stable Radix Sort algorithm for arrays and lists of user defined classes sorted by key.
Free and open source HPCsharp package on https://www.nuget.org

**_Version 3.0.0_** Just released!

Higher performance parallel and serial 2-way Merge, with parallel faster by 1.7%. Stable Merge Sort.
In-place Merge Sort interfaces for arrays and lists.
Parallel and serial Multi-Merge. Changed interfaces on Merge Sort and Merge to be consistent with Microsoft C# algorithms.
Dynamic Priority Queue and Fixed Size Priority Queue.

More info coming soon... Give it a shot

**_Version 2.0_** algorithm performance is shown in the following tables:

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*MegaInts/sec*|*Data Type*
--- | --- | --- | --- | --- | --- | --- | ---
Radix Sort|Array, List|Random|4X-7X|14X-35X|4X-9X|93, 75|UInt32
Radix Sort|Array, List|Presorted|0.3X-0.5X|3X-5X|1X-3X||UInt32
Radix Sort|Array, List|Constant|1X-1.5X|5X-8X|2X-3X||UInt32

Radix Sort is linear time O(N) and stable. Radix Sort runs on a single core, whereas Linq.AsParallel ran on all the cores.
Only slower when sorting presorted Array or List, but faster in all other cases, even faster than parallel Linq.OrderBy.AsParallel.

Parallel Merge Sort uses multiple CPU cores to accelerate performance. On a quad-core laptop, performance is:

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*Description*
--- | --- | --- | --- | --- | --- | ---
Parallel Merge Sort|Array|Random|3X|12X|5X|
Parallel Merge Sort|Array|Presorted|2X|22X|13X|
Parallel Merge Sort|Array|Constant|2X-3X|13X-20X|7X-13X|

Faster than Array.Sort and List.Sort across all distributions. Substantially faster than Linq.OrderBy and Linq.OrderBy.AsParallel

**_28-core (56-threads) AWS c5.18xlarge_**

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*|*Description*
--- | --- | --- | --- | --- | --- | ---
Parallel Merge Sort|Array|Random|5X-14X|19X-90X|7X-47X|Stable
Parallel Merge Sort|Array|Presorted|1X-6X|5X-60X|16X-122X|Stable
Parallel Merge Sort|Array|Constant|TBD|TBD|9X-44X|Stable

*Algorithm*|*Collection*|*Distribution*|*vs .Sort*|*vs Linq*|*vs Linq.AsParallel*
--- | --- | --- | --- | --- | ---
Merge Sort|Array|Random|0.6X-0.7X|2X-4X|1X-3X
Merge Sort|Array|Presorted|0.3X-0.3X|4X-5X|2X-3X|
Merge Sort|Array|Constant|0.5X-0.6X|3X-4X|2X-3X|

Merge Sort is O(NlgN), never O(N<sup>2</sup>), generic, and runs on a single CPU core. Faster than Linq.OrderBy and Linq.OrderBy.AsParallel.

Other algorithms provided:
- Insertion Sort which is O(N<sup>2</sup>), and useful for fast in-place sorting of very small collections.
- Binary Search algorithm
- Parallel Merge algorithm, which merges two presorted collections using multiple cores. Used by Parallel Merge Sort.
- A few parallel Linq-style methods for Min, Max, Average, etc.

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

*Algorithm*|*Collection*|*vs Linq*|*Parallel vs Linq*
--- | --- | --- | ---
SequenceEqual|Array, List|4X faster|up to 11X faster
Min|Array, List|1.5-3X faster
Max|Array, List|1.5X faster

Parallel Copying:

*Method*|*Collection*|*Parallel*
--- | --- | ---
Parallel CopyTo|List to Array|1.7X-2.5X faster

Discussion on when it's appropriate to use parallel copy is coming soon...

# Examples of Usage
See HPCsharpExample folder in this GitHub repository for usage examples - a complete working VisualStudio 2017 solution is provided.

# Related Blogs
For details on the motivation see blog:
https://duvanenko.tech.blog/2018/03/03/high-performance-c/

For more performance discussion see blog:
https://duvanenko.tech.blog/2018/05/23/faster-sorting-in-c/

# Website for Feature Votes
Visit us at https://foostate.com/ and let us know what other high performance algorithms are important to you, and you'd like to see in this NuGet package.

# Encouragement
If you like it, then buy us a cup of coffee, to help us keep more good stuff like this coming

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LDD8L7UPAC7QL)
