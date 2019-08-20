using HPCsharp.ParallelAlgorithms;
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

            // C# built-in Array.Copy() variations
            Array.Copy(arraySource, arrayDestination, arraySource.Length);
            Array.Copy(arraySource, 0, arrayDestination, 0, arraySource.Length);

            // HPCsharp parallel (multi-core) Array.Copy() variations. Similar interface
            ArrayHpc.CopyPar(arraySource, arrayDestination, arraySource.Length);
            ArrayHpc.CopyPar(arraySource, 0, arrayDestination, 0, arraySource.Length);

            // HPCsharp parallel (multi-core) Array.Copy() variations. More modern interface, and more convenient
            arraySource.CopyPar(arrayDestination, arraySource.Length);
            arraySource.CopyPar(0, arrayDestination, 0, arraySource.Length);

            // HPCsharp parallel (multi-core) Array.Copy() variations. More modern interface, and additional more convenient interfaces
            arraySource.CopyPar(arrayDestination);
            arrayDestination = arraySource.CopyPar();

            // HPCsharp parallel List.ToArray() variations
            var listSource = new List<int> { 5, 7, 16, 3 };


        }
    }
}
