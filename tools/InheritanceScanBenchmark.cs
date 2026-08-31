using System;
using System.Collections.Generic;
using System.Diagnostics;

internal static class InheritanceScanBenchmark
{
    private const int Population = 20000;
    private const int RemovedEvery = 7;
    private const int Iterations = 100;

    private static void Main()
    {
        var records = new Dictionary<long, int>(Population);
        var aliveMap = new Dictionary<long, int>(Population);
        var seen = new HashSet<long>();
        for (long id = 1; id <= Population; id++) records[id] = 1;

        int oldHashWrites = 0;
        int newHashWrites = 0;
        var oldDead = new List<long>();
        var newDead = new List<long>();

        long oldTicks = Measure(() =>
        {
            aliveMap.Clear();
            seen.Clear();
            for (long id = 1; id <= Population; id++)
            {
                if (id % RemovedEvery == 0) continue;
                seen.Add(id);
                aliveMap[id] = 1;
                oldHashWrites += 2;
            }
            oldDead.Clear();
            foreach (var entry in records)
                if (!seen.Contains(entry.Key)) oldDead.Add(entry.Key);
        });

        long newTicks = Measure(() =>
        {
            aliveMap.Clear();
            for (long id = 1; id <= Population; id++)
            {
                if (id % RemovedEvery == 0) continue;
                aliveMap[id] = 1;
                newHashWrites++;
            }
            newDead.Clear();
            foreach (var entry in records)
                if (!aliveMap.ContainsKey(entry.Key)) newDead.Add(entry.Key);
        });

        if (oldDead.Count != newDead.Count) Fail("dead count differs");
        for (int i = 0; i < oldDead.Count; i++)
            if (oldDead[i] != newDead[i]) Fail("dead ordering differs at " + i);
        if (oldHashWrites != newHashWrites * 2) Fail("hash write reduction is not exactly 2:1");

        Console.WriteLine("INHERITANCE_SCAN_EQUIVALENT: dead={0}", newDead.Count);
        Console.WriteLine("INHERITANCE_SCAN_HASH_WRITES: old={0} new={1}", oldHashWrites, newHashWrites);
        Console.WriteLine("INHERITANCE_SCAN_TIMING_TICKS: old={0} new={1}", oldTicks, newTicks);
    }

    private static long Measure(Action action)
    {
        action();
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < Iterations; i++) action();
        stopwatch.Stop();
        return stopwatch.ElapsedTicks;
    }

    private static void Fail(string message)
    {
        Console.Error.WriteLine("INHERITANCE_SCAN_FAILED: " + message);
        Environment.Exit(1);
    }
}
