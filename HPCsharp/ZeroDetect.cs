// TODO: Implement not just Zero detection, but detection of any constant value, specified by the user. The constant
//       value needs to be flexible and be any numeric data type. For example, we should be able to specify uint 0x00ff0000
//       as the RGB value for a constant color array.
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class ZeroDetect
    {
        public static bool ByFor(byte[] data)
        {
            // Local variable is accessed faster than a property, and loop multiplies the difference.
            int len = data.Length;
            for (int i = 0; i < len; i++)
                if (data[i] != 0)
                    return false;
            return true;
        }

        public static bool ByForUnrolled(byte[] data)
        {
            // Bitwise-or instead of addition does not gain performance.
            int len = (data.Length / 16) * 16;
            int rem = len % (sizeof(long) * 16);

            for (int i = 0; i < len; i += 16)
            {
                if ((data[i] | data[i + 1] | data[i + 2] | data[i + 3] |
                    data[i + 4] | data[i + 5] | data[i + 6] | data[i + 7] |
                    data[i + 8] | data[i + 9] | data[i + 10] | data[i + 11] |
                    data[i + 12] | data[i + 13] | data[i + 14] | data[i + 15]) != 0)
                    return false;
            }
            for (int i = 0; i < rem; i++)
                if (data[len - 1 - i] != 0)
                    return false;
            return true;
        }
        // https://stackoverflow.com/questions/33294580/sse-instruction-to-check-if-byte-array-is-zeroes-c-sharp
        // Stackoverflow post claims this version is faster than the SIMD implementation above
        // Opinion: One unfairness in comparison is that with pointers long (8-byte type) can be used instead of a single byte, which is 8X acceleration.
        //          Byte pointer versus Byte[] is a bit over 2X speedup, when both are unrolled.
        // Thoughts: SIMD/SSE should help, since SIMD/SSE is 256-bit (32 bytes) and possibly 512-bit (64 bytes). SIMD that was used in the stackoverflow write up was 16 byte (128-bit).
        //           Thus, we should be able to beat scalar results, even pointers by using 32 byte and 64 byte SIMD/SSE
        //           Unrolling helps by about 2X
        public static unsafe bool ByFixedLongUnrolled(byte[] data)
        {
            fixed (byte* bytes = data)
            {
                int len = data.Length;
                int rem = len % (sizeof(long) * 16);
                long* b = (long*)bytes;
                long* e = (long*)(bytes + len - rem);

                while (b < e)
                {
                    if ((*(b)     | *(b +  1) | *(b +  2) | *(b +  3) | *(b + 4) |
                        *(b +  5) | *(b +  6) | *(b +  7) | *(b +  8) |
                        *(b +  9) | *(b + 10) | *(b + 11) | *(b + 12) |
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

        public static unsafe bool ByFixedLongUnrolled(byte[] data, int l, int r)
        {
            fixed (byte* bytes = data)
            {
                int len = r - l + 1;
                int rem = len % (sizeof(long) * 16);
                long* b = (long*)(bytes + l);
                long* e = (long*)(bytes + l + len - rem);

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
