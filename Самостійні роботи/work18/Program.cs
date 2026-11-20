using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace IndependentWork12
{
    public static class MathWorkload
    {
        public static bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            int boundary = (int)Math.Sqrt(number);
            for (int i = 3; i <= boundary; i += 2)
            {
                if (number % i == 0)
                    return false;
            }
            return true;
        }

        public static double HeavyTransform(int number)
        {
            double acc = 0;
            int limit = 25 + (number % 25);
            for (int i = 1; i <= limit; i++)
            {
                acc += Math.Sin(number * i) * Math.Cos(i) + Math.Sqrt(number + i);
            }
            return Math.Round(acc, 4);
        }
    }

    public static class PlinqBenchmark
    {
        public static List<int> GenerateData(int count, int seed = 123)
        {
            var rng = new Random(seed);
            var list = new List<int>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(rng.Next(10_000, 500_000));
            }
            return list;
        }

        public static void RunComparison(int size)
        {
            var data = GenerateData(size);

            var swSeq = Stopwatch.StartNew();
            var seqResult = data
                .Where(n => MathWorkload.IsPrime(n))
                .Select(n => MathWorkload.HeavyTransform(n))
                .ToList();
            swSeq.Stop();

            var swPar = Stopwatch.StartNew();
            var parResult = data
                .AsParallel()
                .Where(n => MathWorkload.IsPrime(n))
                .Select(n => MathWorkload.HeavyTransform(n))
                .ToList();
            swPar.Stop();

            Console.WriteLine($"Розмір колекції: {size:N0} елементів");
            Console.WriteLine($"  LINQ (послідовний): {swSeq.ElapsedMilliseconds} мс (результатів: {seqResult.Count})");
            Console.WriteLine($"  PLINQ (паралельний): {swPar.ElapsedMilliseconds} мс (результатів: {parResult.Count})");
            Console.WriteLine();
        }
    }

    public static class SideEffectsDemo
    {
        public static void RunUnsafeDemo(List<int> data)
        {
            Console.WriteLine("=== Демонстрація побічних ефектів (НЕПОТОКОБЕЗПЕЧНО) ===");
            for (int run = 1; run <= 3; run++)
            {
                int unsafeCounter = 0;
                _ = data
                    .AsParallel()
                    .Where(n =>
                    {
                        bool prime = MathWorkload.IsPrime(n);
                        if (prime)
                        {
                            unsafeCounter++;
                        }
                        return prime;
                    })
                    .ToList();

                Console.WriteLine($"  Запуск #{run}: unsafeCounter = {unsafeCounter}");
            }
        }

        public static void RunSafeDemo(List<int> data)
        {
            Console.WriteLine("\n=== Виправлення потокобезпечності (Interlocked / lock) ===");
            int interlockedCounter = 0;
            int lockCounter = 0;
            object syncRoot = new object();

            _ = data
                .AsParallel()
                .Where(n =>
                {
                    bool prime = MathWorkload.IsPrime(n);
                    if (prime)
                    {
                        Interlocked.Increment(ref interlockedCounter);
                        lock (syncRoot)
                        {
                            lockCounter++;
                        }
                    }
                    return prime;
                })
                .ToList();

            Console.WriteLine($"  InterlockedCounter = {interlockedCounter}");
            Console.WriteLine($"  LockCounter        = {lockCounter}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("=== Порівняння продуктивності LINQ vs PLINQ ===");
            PlinqBenchmark.RunComparison(1_000_000);
            PlinqBenchmark.RunComparison(5_000_000);
            PlinqBenchmark.RunComparison(10_000_000);

            var sampleData = PlinqBenchmark.GenerateData(200_000, seed: 777);
            SideEffectsDemo.RunUnsafeDemo(sampleData);
            SideEffectsDemo.RunSafeDemo(sampleData);
        }
    }
}