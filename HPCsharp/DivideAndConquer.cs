using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class AlgorithmPatterns
    {
        private static T DivideAndConquer<T>(this T[] arrayToProcess, int l, int r, Func<T[], int, int, T> baseCase, Func<T, T, T> reduce, int threshold = 16 * 1024)
        {
            T resultLeft = default(T);

            if (l > r)
                return resultLeft;
            if ((r - l + 1) <= threshold)
                return baseCase(arrayToProcess, l, r);

            int m = (r + l) / 2;

            T resultRight = default(T);

            Parallel.Invoke(
                () => { resultLeft  = DivideAndConquer(arrayToProcess, l,     m, baseCase, reduce, threshold); },
                () => { resultRight = DivideAndConquer(arrayToProcess, m + 1, r, baseCase, reduce, threshold); }
            );

            return reduce(resultLeft, resultRight);
        }

        private static T DivideAndConquerPar<T>(this T[] arrayToProcess, int l, int r, Func<T[], int, int, T> baseCase, Func<T, T, T> reduce, int thresholdPar = 16 * 1024)
        {
            T resultLeft = default(T);

            if (l > r)
                return resultLeft;
            if ((r - l + 1) <= thresholdPar)
                return baseCase(arrayToProcess, l, r);

            int m = (r + l) / 2;

            T resultRight = default(T);

            Parallel.Invoke(
                () => { resultLeft  = DivideAndConquerPar(arrayToProcess, l,     m, baseCase, reduce, thresholdPar); },
                () => { resultRight = DivideAndConquerPar(arrayToProcess, m + 1, r, baseCase, reduce, thresholdPar); }
            );

            return reduce(resultLeft, resultRight);
        }
    }
}
