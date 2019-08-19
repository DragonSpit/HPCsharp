using HPCsharp.Algorithms;
using HPCsharp;
using System;
using System.Linq;
using System.Numerics;


namespace HPCsharpExamples
{
    class CopyExamples
    {
        public static void Examples()
        {
            int[] arraySource = new int[] { 5, 7, 16, 3 };
            int[] arrayDestination = new int[4];

            Array.Copy(arraySource, arrayDestination, arraySource.Length);
            arraySource.CopyTo(arrayDestination, 0);

            arraySource.CopyPar(arrayDestination);
            arrayDestination = arraySource.CopyPar();
        }
    }
}
