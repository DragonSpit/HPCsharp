// TODO: Implement nullable versions of Sum, only faster than the standard C# ones
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        public static long SumHpc(this int[] arrayToSum)
        {
            long overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static long SumHpc(this short[] arrayToSum)
        {
            long overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static long SumHpc(this sbyte[] arrayToSum)
        {
            long overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static ulong SumHpc(this uint[] arrayToSum)
        {
            ulong overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static ulong SumHpc(this ushort[] arrayToSum)
        {
            ulong overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static ulong SumHpc(this byte[] arrayToSum)
        {
            ulong overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }

        public static double SumHpc(this float[] arrayToSum)
        {
            double overallSum = 0;
            for (int i = 0; i < arrayToSum.Length; i++)
                overallSum += arrayToSum[i];
            return overallSum;
        }
    }
}
