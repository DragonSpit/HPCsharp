# High Performance Computing in C# (HPC#)

High performance C#/.NET generic algorithms with some complimenting Linq. Provides a trade-off between performance and level of abstraction: Array, List, IList, IEnumerable.
Lowering the level of abstraction results in higher performance. Some algorithms are parallel for additional performance.
Expanded interface to allow a sub-range of Array or List to be operated on. Open source.

Version 1.0 includes the following Linq style algorithms:

*Method*|*Collection*|*vs Linq*|*Parallel vs Linq*
--- | --- | --- | ---
SequenceEqual|Array, List|4X faster|up to 11X faster
Min|Array, List|1.5-3X faster
Max|Array, List|1.5X faster

- More standard C# algorithms to come shortly...

Additional algorithms:

*Method*|*Collection*|*Description*
--- | --- | ---
Insertion Sort|Array, List|for fast sorting of very small collections in-place
Merge|Array, List|merges two pre-sorted collections

- More to come...

See HPCsharpExample folder in this repo for usage examples - a complete VisualStudio 2017 solution provided.

For more details on the motivation see blog:
https://duvanenko.tech.blog/2018/03/03/high-performance-c/

More high performance algorithms will soon be available at:
https://foostate.com/

*Method*|*Collection*|*Parallel vs Array.Sort*|*Parallel vs List.Sort*|*Parallel vs Linq*
--- | --- | --- | --- | ---
Sort|Array|2X-3X faster||
Sort|List||1.5-3X faster|
CopyTo|List to Array||2-2.5X faster|



[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=LDD8L7UPAC7QL)
