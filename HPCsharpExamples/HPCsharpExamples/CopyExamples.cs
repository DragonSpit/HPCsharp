using HPCsharp.Algorithms;
using HPCsharp;
using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace HPCsharpExamples
{
    class CopyExamples
    {
        public static void Examples()
        {
            int[] arraySource = new int[] { 5, 7, 16, 3 };
            int[] arrayDestination = new int[4];

            Array.Copy(arraySource, arrayDestination, arraySource.Length);
            Array.Copy(arraySource, arrayDestination, arraySource.Length);
            Array.Copy(arraySource, 0, arrayDestination, 0, arraySource.Length);

            arraySource.CopyPar(arrayDestination);
            arraySource.CopyPar(arrayDestination, arraySource.Length);
            arraySource.CopyPar(0, arrayDestination, 0, arraySource.Length);
            arrayDestination = arraySource.CopyPar();

            var listSource = new List<int> { 5, 7, 16, 3 };


        }
    }
}
