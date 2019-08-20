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

            // HPCsharp parallel (multi-core) Array.Copy() variations. More modern and convenient interface
            arraySource.CopyPar(arrayDestination, arraySource.Length);
            arraySource.CopyPar(0, arrayDestination, 0, arraySource.Length);

            // HPCsharp parallel (multi-core) Array.Copy() variations. Additional convenient interfaces
            arraySource.CopyPar(arrayDestination);
            arrayDestination = arraySource.CopyPar();


            // C# built-in Array.CopyTo() variations
            arraySource.CopyTo(arrayDestination, 0);

            // HPCsharp parallel (multi-core) Array.CopyTo()
            arraySource.CopyToPar(arrayDestination, 0);


            // HPCsharp parallel List.ToArray() variations
            var listSource = new List<int> { 5, 7, 16, 3 };

            // C# built-in copy from List to Array
            arrayDestination = listSource.ToArray();

            // HPCsharp parallel (multi-core) copy from List to Array
            arrayDestination = listSource.ToArrayPar();
            arrayDestination = listSource.ToArrayPar(0, listSource.Count);  // additional interface

            // Even faster version, whenever Destination Array can be re-used (paged-in)
            listSource.ToArrayPar(arrayDestination);

            // Additional flexibility in interface
            listSource.ToArrayPar(arrayDestination, listSource.Count);
            listSource.ToArrayPar(0, arrayDestination, 0, listSource.Count);
        }
    }
}
