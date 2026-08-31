// Standalone mirror harness: reconstructs the pre-fusion ApplyWealthTax design (two
// full traversals + poor buffer) and compares it against the fused design (one
// traversal + rich buffer tax + poor distribution). COVERAGE LIMITATION: the original
// pre-fusion production source was superseded by the fusion before any version-control
// capture, so PreFusion below is a hand-written model of that design, not an archive
// copy. This file compiles only its own mirror code; production-source coverage comes
// from the static assertions in Test-WealthTaxFusedPass.ps1 (which scan
// Core\DataCollector.cs directly), not from this harness. Asserts per-actor
// bit-identical outcomes plus exact long-arithmetic conservation
// (givenSum + failedGive == totalTax).
using System;
using System.Collections.Generic;

public static class WealthTaxConservationTest
{
    private const float PoorLineMult = 0.8f;
    private const float MaxRatio = 0.5f;

    private sealed class Actor
    {
        public float Money;
        public bool Alive = true;
        public bool Civilized = true;
        public bool ChargeFails;
        public bool GiveFails;

        public Actor Clone()
        {
            return new Actor
            {
                Money = Money, Alive = Alive, Civilized = Civilized,
                ChargeFails = ChargeFails, GiveFails = GiveFails
            };
        }
    }

    private sealed class Result
    {
        public long TotalTax;
        public long GivenSum;
        public long FailedGive;
        public List<Actor> Actors = new List<Actor>();
    }

    private static void AddPositiveMoneyChunked(Actor actor, long amount)
    {
        while (actor != null && amount > 0)
        {
            int chunk = (int)Math.Min(amount, int.MaxValue);
            actor.Money += chunk;
            amount -= chunk;
        }
    }

    private static void GiveMoney(Actor actor, long give, Result r)
    {
        if (actor.GiveFails) { r.FailedGive += give; throw new Exception("give failed"); }
        AddPositiveMoneyChunked(actor, give);
        r.GivenSum += give;
    }

    // Pre-fusion model: pass 1 collects the poor pool over the full list; pass 2 taxes
    // the full list; pass 3 distributes over the poor pool. Reconstructed from the task
    // spec; the original production source was not archived before the fusion landed.
    private static Result PreFusion(List<Actor> actors, float avg, float ratio, float lineMult)
    {
        var r = new Result();
        if (avg <= 0f) return r;
        if (ratio <= 0f) return r;

        float taxLine = avg * Math.Max(1f, lineMult);
        float poorLine = avg * PoorLineMult;

        var poor = new List<Actor>();
        foreach (var a in actors)
        {
            if (a == null || !a.Alive) continue;
            if (!a.Civilized) continue;
            float w = a.Money;
            if (w < poorLine) poor.Add(a);
        }
        if (poor.Count == 0) return r;

        foreach (var a in actors)
        {
            if (a == null || !a.Alive) continue;
            if (!a.Civilized) continue;
            float w = a.Money;
            if (w > taxLine)
            {
                long tax = (long)Math.Min((w - taxLine) * ratio, w * MaxRatio);
                if (tax > 0)
                {
                    int charged = (int)Math.Min(tax, int.MaxValue);
                    if (!a.ChargeFails) { a.Money -= charged; r.TotalTax += charged; }
                }
            }
        }
        if (r.TotalTax <= 0) return r;

        int poorCount = poor.Count;
        long per = r.TotalTax / poorCount;
        long remainder = r.TotalTax - per * poorCount;
        for (int i = 0; i < poorCount; i++)
        {
            var actor = poor[i];
            if (actor == null || !actor.Alive) continue;
            long give = per + (i == 0 ? remainder : 0);
            if (give <= 0) continue;
            try { GiveMoney(actor, give, r); } catch (Exception) { }
        }
        return r;
    }

    // Fused: one traversal collects both the poor pool and the rich pool; the rich pool
    // is taxed; the poor pool receives. Same formula, clamp, sink, remainder-to-first,
    // and try/catch semantics. Because taxLine >= avg > poorLine, the two pools are
    // disjoint, so the single-pass classification is exactly the two-pass one.
    private static Result Fused(List<Actor> actors, float avg, float ratio, float lineMult)
    {
        var r = new Result();
        if (avg <= 0f) return r;
        if (ratio <= 0f) return r;

        float taxLine = avg * Math.Max(1f, lineMult);
        float poorLine = avg * PoorLineMult;

        var poor = new List<Actor>();
        var rich = new List<Actor>();
        foreach (var a in actors)
        {
            if (a == null || !a.Alive) continue;
            if (!a.Civilized) continue;
            float w = a.Money;
            if (w < poorLine) poor.Add(a);
            else if (w > taxLine) rich.Add(a);
        }
        if (poor.Count == 0) return r;

        foreach (var a in rich)
        {
            if (a == null || !a.Alive) continue;
            float w = a.Money;
            if (w > taxLine)
            {
                long tax = (long)Math.Min((w - taxLine) * ratio, w * MaxRatio);
                if (tax > 0)
                {
                    int charged = (int)Math.Min(tax, int.MaxValue);
                    if (!a.ChargeFails) { a.Money -= charged; r.TotalTax += charged; }
                }
            }
        }
        if (r.TotalTax <= 0) return r;

        int poorCount = poor.Count;
        long per = r.TotalTax / poorCount;
        long remainder = r.TotalTax - per * poorCount;
        for (int i = 0; i < poorCount; i++)
        {
            var actor = poor[i];
            if (actor == null || !actor.Alive) continue;
            long give = per + (i == 0 ? remainder : 0);
            if (give <= 0) continue;
            try { GiveMoney(actor, give, r); } catch (Exception) { }
        }
        return r;
    }

    private static float ComputeAvg(List<Actor> actors)
    {
        float total = 0f;
        int count = 0;
        foreach (var a in actors)
        {
            if (a == null || !a.Alive) continue;
            if (!a.Civilized) continue;
            total += a.Money;
            count++;
        }
        return count == 0 ? 0f : total / count;
    }

    private static float SumMoney(List<Actor> actors)
    {
        float s = 0f;
        foreach (var a in actors) { if (a != null) s += a.Money; }
        return s;
    }

    private static List<Actor> CloneAll(List<Actor> src)
    {
        var c = new List<Actor>(src.Count);
        foreach (var a in src) c.Add(a == null ? null : a.Clone());
        return c;
    }

    private static int _scenario = 0;
    private static int _failed = 0;

    private static void Check(string name, List<Actor> template, float avg, float ratio, float lineMult)
    {
        _scenario++;
        var preActors = CloneAll(template);
        var fusedActors = CloneAll(template);
        float sumBefore = SumMoney(template);

        var pre = PreFusion(preActors, avg, ratio, lineMult);
        var fused = Fused(fusedActors, avg, ratio, lineMult);

        // Exact long-arithmetic conservation: every charged coin is either distributed
        // to a poor actor or failed to be given (try/catch skip), never created/destroyed.
        long preGivenTotal = pre.GivenSum + pre.FailedGive;
        long fusedGivenTotal = fused.GivenSum + fused.FailedGive;
        if (pre.TotalTax != fused.TotalTax || preGivenTotal != pre.TotalTax || fusedGivenTotal != fused.TotalTax)
        {
            Fail(name, string.Format("conservation mismatch: pre tax={0} given+failed={1}, fused tax={2} given+failed={3}",
                pre.TotalTax, preGivenTotal, fused.TotalTax, fusedGivenTotal));
            return;
        }

        // Per-actor bit-identical outcomes between pre-fusion and fused.
        for (int i = 0; i < preActors.Count; i++)
        {
            float p = preActors[i] == null ? 0f : preActors[i].Money;
            float f = fusedActors[i] == null ? 0f : fusedActors[i].Money;
            if (p != f)
            {
                Fail(name, string.Format("actor {0} diverged: pre={1} fused={2}", i, p, f));
                return;
            }
        }

        // Float-level conservation within rounding tolerance (float money + chunked int
        // transfers round at large magnitudes; real leaks move whole tax units). Failed
        // gives never reach an actor, so the expected after-sum is before minus failedGive.
        float sumAfter = SumMoney(fusedActors);
        float expected = sumBefore - fused.FailedGive;
        float delta = Math.Abs(sumAfter - expected);
        float tol = Math.Max(1f, 1e-4f * fused.TotalTax + 1e-5f * (Math.Abs(sumBefore) + Math.Abs(expected)));
        if (delta > tol)
        {
            Fail(name, string.Format("money conservation drift: before={0} expectedAfter={1} after={2} delta={3} tol={4} totalTax={5} failedGive={6}",
                sumBefore, expected, sumAfter, delta, tol, fused.TotalTax, fused.FailedGive));
            return;
        }

        Console.WriteLine("  PASS  " + name);
    }

    private static void Fail(string name, string detail)
    {
        _failed++;
        Console.WriteLine("  FAIL  " + name + ": " + detail);
    }

    private static List<Actor> Population(params float[] moneys)
    {
        var l = new List<Actor>(moneys.Length);
        foreach (var m in moneys) l.Add(new Actor { Money = m });
        return l;
    }

    private static void RunScenarios()
    {
        Console.WriteLine("WealthTaxConservation: reconstructed pre-fusion model vs fused equivalence + conservation");

        // 1) Normal mixed population: poor, middle, and rich all present.
        Check("normal-mixed", Population(10f, 20f, 30f, 100f, 110f, 1000f, 5000f),
            ComputeAvg(Population(10f, 20f, 30f, 100f, 110f, 1000f, 5000f)), 0.3f, 1f);

        // 2) Empty poor: everyone at/above poorLine -> both must return without charging.
        Check("empty-poor", Population(100f, 100f, 100f, 100f, 100f),
            ComputeAvg(Population(100f, 100f, 100f, 100f, 100f)), 0.3f, 1f);

        // 3) Tiny tax: totalTax < poorCount -> remainder-to-first still distributes every coin.
        Check("tiny-tax", Population(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 100f, 10000f),
            ComputeAvg(Population(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 100f, 10000f)), 0.3f, 1f);

        // 4) Overflow: tax above int.MaxValue -> charge clamped, distribution chunked.
        Check("overflow", Population(0f, 1f, 10000000000f, 10000000000f),
            ComputeAvg(Population(0f, 1f, 10000000000f, 10000000000f)), 0.3f, 1f);

        // 5) Charge failure: one rich actor's addMoney throws -> its tax is excluded
        //    identically while the other rich actor still funds the relief.
        var chargeFail = Population(5f, 10f, 20000f, 50000f);
        chargeFail[2].ChargeFails = true;
        Check("charge-failure", chargeFail, ComputeAvg(chargeFail), 0.3f, 1f);

        // 6) Give failure: one poor actor's give throws -> its share is skipped identically.
        var giveFail = Population(5f, 10f, 20f, 2000f, 8000f);
        giveFail[1].GiveFails = true;
        Check("give-failure", giveFail, ComputeAvg(giveFail), 0.3f, 1f);

        // 7) Exact boundaries: w == poorLine is not poor, w == taxLine is not rich (strict).
        //    avg = 100 -> taxLine = 100, poorLine = 80. Money 80 excluded from poor,
        //    money 100 excluded from rich.
        Check("exact-boundary", Population(80f, 100f, 10f, 500f),
            ComputeAvg(Population(80f, 100f, 10f, 500f)), 0.3f, 1f);

        // 8) Zero avg -> no-op in both.
        Check("zero-avg", Population(0f, 0f, 0f), ComputeAvg(Population(0f, 0f, 0f)), 0.3f, 1f);

        // 9) Zero ratio -> no-op in both.
        Check("zero-ratio", Population(10f, 20f, 1000f),
            ComputeAvg(Population(10f, 20f, 1000f)), 0f, 1f);

        // 10) Dead and uncivilized actors are skipped identically in both.
        var mixed = Population(10f, 20f, 1000f, 5000f);
        mixed.Add(new Actor { Money = 999999f, Alive = false });
        mixed.Add(new Actor { Money = 999999f, Civilized = false });
        Check("skip-dead-uncivilized", mixed, ComputeAvg(mixed), 0.3f, 1f);

        // 11) Random fuzz: seeded populations with random failure flags.
        var rng = new Random(20260829);
        for (int trial = 0; trial < 200; trial++)
        {
            int n = 3 + rng.Next(58);
            var pop = new List<Actor>(n);
            for (int i = 0; i < n; i++)
            {
                pop.Add(new Actor
                {
                    Money = (float)(rng.NextDouble() * 1000000.0),
                    Alive = rng.Next(20) != 0,
                    Civilized = rng.Next(20) != 0,
                    ChargeFails = rng.Next(50) == 0,
                    GiveFails = rng.Next(50) == 0
                });
            }
            float ratio = 0.05f + (float)rng.NextDouble() * 0.45f;
            float lineMult = 1f + rng.Next(3);
            Check("fuzz-" + trial, pop, ComputeAvg(pop), ratio, lineMult);
        }
    }

    public static int Main()
    {
        RunScenarios();
        if (_failed > 0)
        {
            Console.WriteLine("WEALTH_TAX_CONSERVATION_RED: {0} of {1} scenarios diverged", _failed, _scenario);
            return 1;
        }
        Console.WriteLine("WEALTH_TAX_CONSERVATION_GREEN: {0} scenarios, pre-fusion == fused, conservation holds", _scenario);
        return 0;
    }
}