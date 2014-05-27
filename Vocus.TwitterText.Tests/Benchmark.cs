using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Vocus.TwitterText.Tests
{
    /**
     * Micro benchmark for discovering hotspots in our autolinker.
     */
    public class Benchmark : ConformanceTest
    {
        private static readonly int AUTO_LINK_TESTS = 10000;
        private static readonly int ITERATIONS = 10;

        public double testBenchmarkAutolinking()
        {
            var testCases = StringOutTestCases("autolink.yml", "all");
            Autolink(testCases);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < AUTO_LINK_TESTS; i++)
            {
                Autolink(testCases);
            }
            stopwatch.Stop();
            var diff = stopwatch.ElapsedMilliseconds;
            var totalLinks = AUTO_LINK_TESTS * testCases.Count;
            double autolinksPerMS = ((double)totalLinks) / diff;
            Console.WriteLine("Processed " + totalLinks + " links in " + diff + "ms.");
            return autolinksPerMS;
        }

        private void Autolink(List<StringOutTestCase> testCases)
        {
            var total = testCases.Count;
            for (var i = 0; i < total; i++ )
            {
                var test = testCases[i];
                TestAllAutolinking(test);
            }
        }

        public static void Main(String[] args)
        {
            Benchmark benchmark = new Benchmark();
            double total = 0;
            double best = Double.MaxValue;
            double worst = 0;
            Console.WriteLine("Running " + ITERATIONS + " iterations.");
            for (int i = 0; i < ITERATIONS; i++)
            {
                Console.WriteLine("Running iteration " + (i + 1));
                double result = benchmark.testBenchmarkAutolinking();
                Console.WriteLine("Iteration " + (i + 1) + " performed " + result + " autolink/ms.");
                if (best > result) best = result;
                if (worst < result) worst = result;
                total += result;
            }
            // Drop worst and best
            total -= best + worst;
            Console.WriteLine("Average: " + (total / (ITERATIONS - 2)));
        }
    }
}