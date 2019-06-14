// TODO: Implement a new nullable array that does not throw an exception when you access null data, but returns the default value. This may not be a desirable behavior for all use cases, but is faster, as
//       it allows for the boolean "HasValue" and "Value" to be separated into their own arrays to be able to process them using SIMD/SSE instructions for data parallel implementation.
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class IsArrayZero
    {
#if false
        // https://stackoverflow.com/questions/33294580/sse-instruction-to-check-if-byte-array-is-zeroes-c-sharp
        static unsafe bool BySimdUnrolled(byte[] data)
        {
            fixed (byte* bytes = data)
            {
                int len = data.Length;
                int rem = len % (16 * 16);
                Vector16b* b = (Vector16b*)bytes;
                Vector16b* e = (Vector16b*)(bytes + len - rem);
                Vector16b zero = Vector16b.Zero;

                while (b < e)
                {
                    if ((*(b) | *(b + 1) | *(b + 2) | *(b + 3) | *(b + 4) |
                        *(b + 5) | *(b + 6) | *(b + 7) | *(b + 8) |
                        *(b + 9) | *(b + 10) | *(b + 11) | *(b + 12) |
                        *(b + 13) | *(b + 14) | *(b + 15)) != zero)
                        return false;
                    b += 16;
                }

                for (int i = 0; i < rem; i++)
                    if (data[len - 1 - i] != 0)
                        return false;

                return true;
            }
        }
#endif
        // Stackoverflow post claims this version is faster than the SIMD implementation above
        static unsafe bool ByFixedLongUnrolled(byte[] data)
        {
            fixed (byte* bytes = data)
            {
                int len = data.Length;
                int rem = len % (sizeof(long) * 16);
                long* b = (long*)bytes;
                long* e = (long*)(bytes + len - rem);

                while (b < e)
                {
                    if ((*(b) | *(b + 1) | *(b + 2) | *(b + 3) | *(b + 4) |
                        *(b + 5) | *(b + 6) | *(b + 7) | *(b + 8) |
                        *(b + 9) | *(b + 10) | *(b + 11) | *(b + 12) |
                        *(b + 13) | *(b + 14) | *(b + 15)) != 0)
                        return false;
                    b += 16;
                }

                for (int i = 0; i < rem; i++)
                    if (data[len - 1 - i] != 0)
                        return false;

                return true;
            }
        }
    }
}
