using System;
using System.Collections.Generic;

internal static class TopNBoundaryInvariantTest
{
    private static void Main()
    {
        var top = new List<float>();
        float edge = 0f;
        foreach (float value in new[] { 100f, 90f, 95f, 92f })
            UpdateTopN(top, ref edge, value, 2);
        top.Sort();
        if (top.Count != 2 || top[0] != 95f || top[1] != 100f || edge != 95f)
        {
            Console.Error.WriteLine($"TOP_N_BOUNDARY_RED: [{string.Join(",", top)}] edge={edge}");
            Environment.Exit(1);
        }
        Console.WriteLine("TOP_N_BOUNDARY_GREEN: replacement keeps the true boundary");
    }

    private static void UpdateTopN(List<float> pool, ref float edge, float value, int capacity)
    {
        if (pool.Count < capacity)
        {
            pool.Add(value);
            edge = pool.Count == 1 ? value : Math.Min(edge, value);
            return;
        }
        if (value <= edge) return;

        int edgeIndex = 0;
        float current = pool[0];
        for (int i = 1; i < pool.Count; i++)
        {
            if (pool[i] < current) { current = pool[i]; edgeIndex = i; }
        }
        pool[edgeIndex] = value;

        edge = pool[0];
        for (int i = 1; i < pool.Count; i++) edge = Math.Min(edge, pool[i]);
    }
}
