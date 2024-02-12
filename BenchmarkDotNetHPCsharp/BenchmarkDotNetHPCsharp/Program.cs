using System;
using System.Security.Cryptography;
using HPCsharp.Algorithms;
using HPCsharp.ParallelAlgorithms;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace MyBenchmarks
{
    [RPlotExporter]
    public class HPCsharpSum
    {
        [Params(100000, 1000000, 10000000, 100000000)]
        public int N;

        private int[] dataRandom;
        private int[] dataConstant;

        [GlobalSetup]
        public void Setup()
        {
            dataRandom   = new int[N];
            dataConstant = new int[N];
            Random randNum = new Random(42);
            for (int i = 0; i < dataRandom.Length; i++)
            {
                dataRandom[i] = randNum.Next(int.MinValue, int.MaxValue);
            }
            for (int i = 0; i < dataConstant.Length; i++)
            {
                dataConstant[i] = 1;
            }
        }

        [Benchmark]
        public long StdSumRandom() => dataRandom.Sum(v => (long)v);                        // extending from int to long is safer (avoids numeric overflow) but is slower

        [Benchmark]
        public long StdParallelSumRandom() => dataRandom.AsParallel().Sum(v => (long)v);   // extending from int to long is safer (avoids numeric overflow) but is slower

        [Benchmark]
        public int StdSumConstant() => dataConstant.Sum();                                 // using dataRandom causes a numeric overflow exception, using dataConstant instead to show speed (but not safe)

        [Benchmark]
        public int StdParallelSumConstant() => dataConstant.AsParallel().Sum();            // using dataRandom causes a numeric overflow exception, using dataConstant instead to show speed (but not safe)

        [Benchmark]
        public long HPCsharpSumRandom() => HPCsharp.ParallelAlgorithms.Sum.SumToLongSse(dataRandom);             // HPCsharp extends from int to long to avoid overflow and uses SIMD/SSE on a single core

        [Benchmark]
        public long HPCsharpParallelSumRandom() => HPCsharp.ParallelAlgorithms.Sum.SumToLongSsePar(dataRandom);  // HPCsharp extends from int to long to avoid overflow and uses SIMD/SSE and multi-core to stay fast
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<HPCsharpSum>();
        }
    }
}