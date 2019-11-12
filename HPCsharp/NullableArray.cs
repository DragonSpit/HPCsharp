// TODO: Implement a new nullable array that does not throw an exception when you access null data, but returns the default value. This may not be a desirable behavior for all use cases, but is faster, as
//       it allows for the boolean "HasValue" and "Value" to be separated into their own arrays to be able to process them using SIMD/SSE instructions for data parallel implementation.
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class NullableArray
    {
    }
}
