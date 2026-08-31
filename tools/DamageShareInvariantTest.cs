using System;
using System.Collections.Generic;

internal static class DamageShareInvariantTest
{
    private struct Share
    {
        public long Id;
        public int Amount;
        public decimal Remainder;
    }

    private static void Main()
    {
        Check(10, new[] { (1L, 1f), (2L, 1f) }, new[] { (1L, 5), (2L, 5) });
        Check(10, new[] { (1L, 2f), (2L, 1f) }, new[] { (1L, 7), (2L, 3) });
        Check(2, new[] { (1L, 1f), (2L, 1f), (3L, 1f) }, new[] { (1L, 1), (2L, 1), (3L, 0) });
        Check(int.MaxValue, new[] { (1L, float.MaxValue), (2L, float.MaxValue) },
            new[] { (1L, 1073741824), (2L, 1073741823) });
        Console.WriteLine("DAMAGE_SHARE_INVARIANTS_GREEN: 4 scenarios passed");
    }

    private static void Check(int total, (long Id, float Damage)[] damage, (long Id, int Amount)[] expected)
    {
        var actual = Allocate(total, damage);
        long sum = 0;
        foreach (var item in actual) sum += item.Amount;
        if (sum != total) Fail("allocation sum differs: " + sum + " != " + total);
        if (actual.Count != expected.Length) Fail("recipient count differs");
        for (int i = 0; i < expected.Length; i++)
            if (actual[i].Id != expected[i].Id || actual[i].Amount != expected[i].Amount)
                Fail("scenario differs at " + i);
    }

    private static List<Share> Allocate(int total, (long Id, float Damage)[] damage)
    {
        Array.Sort(damage, (left, right) =>
        {
            int order = right.Damage.CompareTo(left.Damage);
            return order != 0 ? order : left.Id.CompareTo(right.Id);
        });
        double maxDamage = damage[0].Damage;
        decimal damageSum = 0m;
        foreach (var item in damage) damageSum += (decimal)(item.Damage / maxDamage);
        var shares = new List<Share>(damage.Length);
        long assigned = 0;
        foreach (var item in damage)
        {
            decimal quota = total * (decimal)(item.Damage / maxDamage) / damageSum;
            int amount = (int)decimal.Floor(quota);
            shares.Add(new Share { Id = item.Id, Amount = amount, Remainder = quota - amount });
            assigned += amount;
        }
        shares.Sort((left, right) =>
        {
            int order = right.Remainder.CompareTo(left.Remainder);
            return order != 0 ? order : left.Id.CompareTo(right.Id);
        });
        int remaining = (int)((long)total - assigned);
        for (int i = 0; i < remaining; i++)
        {
            Share share = shares[i];
            share.Amount++;
            shares[i] = share;
        }
        shares.Sort((left, right) => left.Id.CompareTo(right.Id));
        return shares;
    }

    private static void Fail(string message)
    {
        Console.Error.WriteLine("DAMAGE_SHARE_INVARIANTS_RED: " + message);
        Environment.Exit(1);
    }
}
