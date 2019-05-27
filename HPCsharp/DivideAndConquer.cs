using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class AlgorithmPatterns
    {
        private static T DivideAndConquer<T>(this T[] arrayToProcess, int l, int r, int thresholdPar, Func<T[], int, int, T> baseCase, Func<T, T, T> reduce)
        {
            T resultLeft = default(T);

            if (l > r)
                return resultLeft;
            if ((r - l + 1) <= thresholdPar)
                return baseCase(arrayToProcess, l, r);

            int m = (r + l) / 2;

            T resultRight = default(T);

            Parallel.Invoke(
                () => { resultLeft  = DivideAndConquer(arrayToProcess, l,     m, thresholdPar, baseCase, reduce); },
                () => { resultRight = DivideAndConquer(arrayToProcess, m + 1, r, thresholdPar, baseCase, reduce); }
            );

            return reduce(resultLeft, resultRight);
        }

        private static T DivideAndConquerPar<T>(this T[] arrayToProcess, int l, int r, int thresholdPar, Func<T[], int, int, T> baseCase, Func<T, T, T> reduce)
        {
            T resultLeft = default(T);

            if (l > r)
                return resultLeft;
            if ((r - l + 1) <= thresholdPar)
                return baseCase(arrayToProcess, l, r);

            int m = (r + l) / 2;

            T resultRight = default(T);

            Parallel.Invoke(
                () => { resultLeft  = DivideAndConquerPar(arrayToProcess, l,     m, thresholdPar, baseCase, reduce); },
                () => { resultRight = DivideAndConquerPar(arrayToProcess, m + 1, r, thresholdPar, baseCase, reduce); }
            );

            return reduce(resultLeft, resultRight);
        }
    }
}
