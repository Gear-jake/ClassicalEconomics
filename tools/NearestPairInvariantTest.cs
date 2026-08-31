using System;
using System.Collections.Generic;

internal static class NearestPairInvariantTest
{
    private readonly struct Point
    {
        public Point(long id, int x, int y) { Id = id; X = x; Y = y; }
        public long Id { get; }
        public int X { get; }
        public int Y { get; }
    }

    private static void Main()
    {
        var first = new List<Point>();
        var second = new List<Point>();
        for (int i = 0; i < 20; i++)
        {
            first.Add(new Point(i + 1, 0, i * 100));
            second.Add(new Point(i + 101, 0, i * 100 + (i == 19 ? 0 : 30)));
        }
        AssertEqual(first, second);

        var random = new Random(139);
        for (int round = 0; round < 100; round++)
        {
            first.Clear(); second.Clear();
            for (int i = 0; i < 30; i++) first.Add(new Point(i + 1, random.Next(8), random.Next(2000)));
            for (int i = 0; i < 35; i++) second.Add(new Point(i + 101, random.Next(8), random.Next(2000)));
            AssertEqual(first, second);
        }
        Console.WriteLine("NEAREST_PAIR_INVARIANTS_GREEN: adversarial + 100 random scenarios passed");
    }

    private static void AssertEqual(List<Point> first, List<Point> second)
    {
        first.Sort(Compare); second.Sort(Compare);
        var expected = BruteForce(first, second);
        var actual = ExactSweep(first, second);
        for (int i = 0; i < 2; i++)
            if (expected[i] != actual[i]) Fail($"pair {i}: expected {expected[i]}, actual {actual[i]}");
    }

    private static (long, long)[] ExactSweep(List<Point> first, List<Point> second)
    {
        var best = new List<(double Distance, long A, long B)>(2);
        var source = first.Count <= second.Count ? first : second;
        var target = first.Count <= second.Count ? second : first;
        bool swapped = first.Count > second.Count;
        foreach (var point in source)
        {
            int low = LowerBound(target, point.X);
            int left = low - 1, right = low;
            while (left >= 0 || right < target.Count)
            {
                double leftDx = left >= 0 ? Square((double)target[left].X - point.X) : double.MaxValue;
                double rightDx = right < target.Count ? Square((double)target[right].X - point.X) : double.MaxValue;
                double bound = best.Count == 2 ? best[1].Distance : double.MaxValue;
                if (leftDx > bound && rightDx > bound) break;
                Point other;
                if (leftDx <= rightDx) { other = target[left--]; }
                else { other = target[right++]; }
                Add(best, swapped ? other : point, swapped ? point : other);
            }
        }
        return new[] { (best[0].A, best[0].B), (best[1].A, best[1].B) };
    }

    private static (long, long)[] BruteForce(List<Point> first, List<Point> second)
    {
        var best = new List<(double Distance, long A, long B)>(2);
        foreach (var a in first) foreach (var b in second) Add(best, a, b);
        return new[] { (best[0].A, best[0].B), (best[1].A, best[1].B) };
    }

    private static void Add(List<(double Distance, long A, long B)> best, Point a, Point b)
    {
        long first = Math.Min(a.Id, b.Id), second = Math.Max(a.Id, b.Id);
        double distance = Square((double)a.X - b.X) + Square((double)a.Y - b.Y);
        var candidate = (distance, first, second);
        if (best.Contains(candidate)) return;
        best.Add(candidate);
        best.Sort((x, y) => x.Distance != y.Distance ? x.Distance.CompareTo(y.Distance)
            : x.A != y.A ? x.A.CompareTo(y.A) : x.B.CompareTo(y.B));
        if (best.Count > 2) best.RemoveAt(2);
    }

    private static int LowerBound(List<Point> points, int x)
    {
        int low = 0, high = points.Count;
        while (low < high) { int mid = low + ((high - low) >> 1); if (points[mid].X < x) low = mid + 1; else high = mid; }
        return low;
    }

    private static int Compare(Point a, Point b) => a.X != b.X ? a.X.CompareTo(b.X) : a.Y != b.Y ? a.Y.CompareTo(b.Y) : a.Id.CompareTo(b.Id);
    private static double Square(double value) => value * value;
    private static void Fail(string message) { Console.Error.WriteLine("NEAREST_PAIR_INVARIANTS_RED: " + message); Environment.Exit(1); }
}
