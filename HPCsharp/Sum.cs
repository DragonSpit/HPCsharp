using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        // TODO: more this to scalar namespace (Algorithm)
        public static long SumIntToLong(this int[] arrayToSum)
        {
            long overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }
    }
}
