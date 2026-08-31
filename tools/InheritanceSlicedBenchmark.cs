using System;
using System.Collections.Generic;

internal static class InheritanceSlicedBenchmark
{
    private const float WindowSeconds = 3f;

    // ===== Mirror state (InheritanceEngine sliced scan) =====
    private static readonly List<int> aliveList = new List<int>();
    private static readonly Dictionary<int, bool> aliveFlag = new Dictionary<int, bool>();
    private static readonly Dictionary<int, int> aliveMap = new Dictionary<int, int>();
    private static readonly Dictionary<int, int> records = new Dictionary<int, int>();
    private static readonly List<int> deadIds = new List<int>();
    private static readonly List<int> staleIds = new List<int>();
    private static float timer;
    private static bool scanActive;
    private static int cursor;
    private static int visited;
    private static int scanPerFrame = 2000;

    private static void Main()
    {
        CheckOrdinaryFrameCap();
        CheckCursorProgressionAndComplete();
        CheckDeadlineFrameDrain();
        CheckInsertBehindCursorReconciled();
        CheckScannedThenDied();
        CheckResetClearsState();
        Console.WriteLine("INHERITANCE_SLICED_GREEN");
    }

    // ===== Tick mirror =====
    private static int Tick(float deltaTime)
    {
        timer += deltaTime;

        if (!scanActive)
        {
            if (timer < WindowSeconds) return 0;
            timer = 0f;
            aliveMap.Clear();
            cursor = 0;
            scanActive = true;
        }

        int cap = scanPerFrame;
        if (cap < 1) cap = 1;
        if (cap > 100000) cap = 100000;
        bool deadline = timer >= WindowSeconds;
        int scanned = 0;
        while (cursor < aliveList.Count && (scanned < cap || deadline))
        {
            ScanActor(aliveList[cursor]);
            cursor++;
            scanned++;
        }

        if (cursor >= aliveList.Count)
        {
            CompleteWindow();
        }
        return scanned;
    }

    // ===== ScanActor mirror =====
    private static void ScanActor(int id)
    {
        bool alive;
        if (!aliveFlag.TryGetValue(id, out alive) || !alive) return;
        visited++;
        aliveMap[id] = id;
        if (!records.ContainsKey(id))
        {
            records[id] = id;
        }
    }

    // ===== CompleteWindow mirror =====
    private static void CompleteWindow()
    {
        // Reconcile pass: scan any alive entry the cursor skipped or that appeared mid-window
        foreach (int id in aliveList)
        {
            if (aliveMap.ContainsKey(id)) continue;
            ScanActor(id);
        }

        // Stale pass: entries no longer passing the alive filter leave aliveMap
        staleIds.Clear();
        foreach (var kv in aliveMap)
        {
            bool alive;
            if (!aliveFlag.TryGetValue(kv.Key, out alive) || !alive) staleIds.Add(kv.Key);
        }
        foreach (int id in staleIds) aliveMap.Remove(id);

        // Death pass: last seen alive, gone now
        deadIds.Clear();
        foreach (var kv in records)
        {
            if (!aliveMap.ContainsKey(kv.Key)) deadIds.Add(kv.Key);
        }

        scanActive = false;
        cursor = 0;
    }

    // ===== Reset mirror =====
    private static void Reset()
    {
        aliveList.Clear();
        aliveFlag.Clear();
        aliveMap.Clear();
        records.Clear();
        deadIds.Clear();
        staleIds.Clear();
        timer = 0f;
        scanActive = false;
        cursor = 0;
        visited = 0;
        scanPerFrame = 2000;
    }

    private static void Populate(int count)
    {
        for (int id = 1; id <= count; id++)
        {
            aliveList.Add(id);
            aliveFlag[id] = true;
        }
    }

    // ===== Check 1: ordinary frame scans exactly cap=2000 =====
    private static void CheckOrdinaryFrameCap()
    {
        Reset();
        Populate(6000);
        int scanned = Tick(WindowSeconds);
        Check(scanned == 2000, "ordinary frame must scan exactly cap=2000, got " + scanned);
        Check(cursor == 2000, "cursor must be 2000 after opening frame, got " + cursor);
        Check(aliveMap.Count == 2000, "aliveMap must hold 2000 entries, got " + aliveMap.Count);
    }

    // ===== Check 2: cursor 2000 / 4000 then window completes =====
    private static void CheckCursorProgressionAndComplete()
    {
        Reset();
        Populate(6000);
        Tick(WindowSeconds);
        Check(cursor == 2000, "cursor must be 2000 after opening frame, got " + cursor);
        int scanned = Tick(0.016f);
        Check(scanned == 2000, "second frame must scan 2000, got " + scanned);
        Check(cursor == 4000, "cursor must be 4000 after second frame, got " + cursor);
        scanned = Tick(0.016f);
        Check(scanned == 2000, "completing frame must scan 2000, got " + scanned);
        Check(!scanActive, "window must complete when cursor reaches count");
        Check(cursor == 0, "cursor must reset to 0 after completion");
        Check(deadIds.Count == 0, "no deaths expected, got " + deadIds.Count);
        Check(aliveMap.Count == 6000, "all 6000 must remain alive, got " + aliveMap.Count);
        Check(visited == 6000, "visited must be 6000, got " + visited);
    }

    // ===== Check 3: deadline frame drains exactly the remaining 3000 =====
    private static void CheckDeadlineFrameDrain()
    {
        Reset();
        scanPerFrame = 1000;
        Populate(6000);
        Tick(WindowSeconds);   // opens window, scans 1000 -> cursor 1000
        Tick(0.016f);          // scans 1000 -> cursor 2000
        Tick(0.016f);          // scans 1000 -> cursor 3000
        int scanned = Tick(WindowSeconds); // timer >= 3f -> deadline, drains remaining 3000
        Check(scanned == 3000, "deadline frame must drain exactly 3000, got " + scanned);
        Check(!scanActive, "deadline frame must complete the window");
        Check(cursor == 0, "cursor must reset after deadline completion");
        Check(aliveMap.Count == 6000, "all units must be alive after deadline drain, got " + aliveMap.Count);
    }

    // ===== Check 4: unit inserted behind cursor is reconciled, not dead =====
    private static void CheckInsertBehindCursorReconciled()
    {
        Reset();
        Populate(6000);
        Tick(WindowSeconds);           // cursor 2000
        aliveList.Insert(0, 6001);     // inserted behind cursor
        aliveFlag[6001] = true;
        Tick(0.016f);                  // cursor 4000
        Tick(0.016f);                  // cursor 6000
        Tick(0.016f);                  // completes window
        Check(deadIds.Count == 0, "inserted unit must not be dead, got " + deadIds.Count);
        Check(aliveMap.ContainsKey(6001), "inserted unit must be reconciled into aliveMap");
        Check(records.ContainsKey(6001), "inserted unit must have a record");
        Check(aliveMap.Count == 6001, "aliveMap must hold 6001 entries, got " + aliveMap.Count);
    }

    // ===== Check 5: scanned-then-died unit lands in deadIds via stale pass =====
    private static void CheckScannedThenDied()
    {
        Reset();
        Populate(6000);
        Tick(WindowSeconds);           // scans ids 1..2000, cursor 2000
        Check(aliveMap.ContainsKey(100), "precondition: id 100 must have been scanned");
        aliveFlag[100] = false;        // dies mid-window
        Tick(0.016f);                  // scans 2001..4000
        Tick(0.016f);                  // scans 4001..6000, completes window
        Check(deadIds.Contains(100), "scanned-then-died unit must land in deadIds");
        Check(!aliveMap.ContainsKey(100), "dead unit must leave aliveMap via stale pass");
        Check(records.ContainsKey(100), "dead unit record must remain until death handling");
    }

    // ===== Check 6: Reset clears all state =====
    private static void CheckResetClearsState()
    {
        Reset();
        Populate(6000);
        Tick(WindowSeconds);
        Tick(0.016f);
        Reset();
        Check(timer == 0f, "timer must reset to 0");
        Check(!scanActive, "scanActive must reset to false");
        Check(cursor == 0, "cursor must reset to 0");
        Check(visited == 0, "visited must reset to 0");
        Check(aliveList.Count == 0, "aliveList must clear");
        Check(aliveFlag.Count == 0, "aliveFlag must clear");
        Check(aliveMap.Count == 0, "aliveMap must clear");
        Check(records.Count == 0, "records must clear");
        Check(deadIds.Count == 0, "deadIds must clear");
        Check(staleIds.Count == 0, "staleIds must clear");
        int scanned = Tick(0.016f);
        Check(scanned == 0, "tick after reset must scan nothing, got " + scanned);
    }

    private static void Check(bool condition, string message)
    {
        if (condition) return;
        Console.Error.WriteLine("INHERITANCE_SLICED_FAILED: " + message);
        Environment.Exit(1);
    }
}