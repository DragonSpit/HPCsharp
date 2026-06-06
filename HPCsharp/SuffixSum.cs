using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        public static void SuffixSum(int[] inOutArray, int startIndex, int length, int first = 0)
        {
            ArgumentNullException.ThrowIfNull(inOutArray);
            int prev = inOutArray[startIndex];                          // prev = count[0]
            inOutArray[startIndex] = first;                             // count[0] = 0
            int endIndex = startIndex + length - 1;
            for (int i = startIndex + 1; i <= endIndex; i++)
            {
                int current = inOutArray[i];
                inOutArray[i] = inOutArray[i - 1] + prev;
                prev = current;
            }
        }
    }
}
