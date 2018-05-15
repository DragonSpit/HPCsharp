# High Performance Computing in C# (HPC#)

High performance C#/.NET generic algorithms. Provides a trade-off between performance and level of abstraction: Array, List, IList, IEnumerable.
Lowering the level of abstraction results in higher performance. Some algorithms are parallel for additional performance.
Expanded interface to allow a sub-range of Array or List to be operated on. Open source.

Version 1.0 includes the following Linq style algorithms:

*Method*|*Collection*|*vs Linq*|*Parallel vs Linq*
--- | --- | --- | ---
SequenceEqual|Array, List|4X faster|up to 11X faster
Min|Array, List|1.5-3X faster
Max|Array, List|1.5X faster

- More standard C# algorithms to come shortly, such as Merge Sort and Radix Sort, ...

Additional algorithms:

*Method*|*Collection*|*Description*
--- | --- | ---
Insertion Sort|Array, List|for fast in-place sorting of very small collections
Merge|Array, List|merges two pre-sorted collections


See HPCsharpExample folder in this repo for usage examples - a complete VisualStudio 2017 solution provided.

For details on the motivation see blog:
https://duvanenko.tech.blog/2018/03/03/high-performance-c/

# More High Performance Algorithms
Soon to be available at https://foostate.com/

*Method*|*Collection*|*Parallel vs Array.Sort*|*Parallel vs List.Sort*|*Parallel vs Linq*|*Number of Cores*
--- | --- | --- | --- | --- | ---
Stable Parallel Sort|Array|2X-3X faster|||4 cores
Stable Parallel Sort|List||2X-3X faster||4 cores
Stable Parallel Sort|Array|3.5X-5X faster|||6 cores
Stable Parallel Sort|List||2.5X-4.5X faster||6 cores

*Method*|*Collection*|*Parallel*
--- | --- | ---
CopyTo|List to Array|1.7X-2.5X faster



[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LDD8L7UPAC7QL)
