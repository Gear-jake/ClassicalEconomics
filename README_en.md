# Classical Economics

> A complete macroeconomic simulation mod for WorldBox — wealth tracking, economic cycles, kingdom trade, social unrest and sapient spending.

**Author**: Jake
**Version**: 0.13.1
**Type**: Macro-economy / Simulation enhancement
**Target**: WorldBox 0.51.2+

---

## Overview

"Classical Economics" gives your world a real economy. The mod tracks the wealth of **all sapient species** (aliens, dragons and any race that can build a civilization), and simulates a full macroeconomic system on top of it:

- Every year it collects coins and loot from all sapient races, computing **global GDP, average wealth, Gini coefficient and price index (CPI)**
- Wealth drives a complete **Boom → Recession → Depression → Recovery** economic cycle
- Kingdoms redistribute wealth through **trade flows** (no coins created from nothing), with **biome specialties** creating comparative-advantage trade
- Inequality triggers **unrest, rebellion and revolution**, and prompts kings to **raise taxes or reform**
- Rich sapients spend heavily every year: buying weapons, building investments, crafting arsenals, paying taxes, giving charity…
- On death, wealth is inherited **parents → spouse → children**, with the city mayor collecting inheritance tax
- **Labor division**: employed population creates wages through labor (production function)
- **Banking credit**: the rich lend to the poor; default rates soar in depression and crises spread along trade routes
- **Disaster shocks**: volcanoes and meteors evaporate affected city wealth; volcanoes temporarily boost mining
- **4-language UI**: 简体中文 / 繁體中文 / English / Русский

Everything is built on **vanilla game mechanics** — no external resources, all computation runs on a background thread.

## Core Features

### 1. Macro statistics
- Tracks all sapient species (detected by intelligence trait; alien kings count too)
- Global GDP, average wealth, civilized population, Gini coefficient
- **Price index (CPI)**: rises with money velocity and bubble buildup — visible inflation
- Per-kingdom breakdown: wealth, average, inequality, population

### 2. Economic cycle
- Four phases: **Boom → Recession → Depression → Recovery**, driven by the global wealth gap
- Boom: credit expansion, coins injected by GDP ratio, bubbles keep building
- Bubble burst: when accumulated bubbles pass the threshold, large amounts of coins evaporate
- Depression: average wealth below survival line → famine and deaths
- Recovery: slow rebound and rebuilding of confidence
- Manually cycle the phase from the toolbar

### 3. Social unrest and revolution
- Auto-detect high-inequality kingdoms and trigger vanilla rebellion
- **Incite / suppress** unrest in any kingdom manually
- Prolonged rebellion → **revolution**: old regime overthrown, population killed by ratio
- High-Gini regime collapse chain: **street uprising** (full-city riot, kill-the-rich-to-feed-the-poor, dethrone the king)
- **War plunder**: winners loot loser coins (actually works since v0.8.4)

### 4. Kingdom trade flow
- Kingdoms above global average wealth are trade **surplus** (gain coins), below are **deficit** (pay coins)
- Settled via city treasury, same channel as vanilla taxes
- Total surplus = total deficit, **zero-sum, no coins created from nothing**
- **Adaptive trade parameters** (v0.11): distance decay / transport cost / arbitrage weight are derived each cycle from game state (map scale / sea-route share / fleet size / avg haul distance / price dispersion), EMA-smoothed, no fixed config
- **Regional prices**: each kingdom forms a local price from supply/demand (high output → low price, overcrowding → high price); price gaps drive arbitrage trade
- **Net trade balance ranking** (v0.13): a standalone floating window ranks cities/kingdoms by net trade (total exports − imports), surplus green / deficit red; a kingdom's value = sum of its cities
- **Trade military power** (v0.13): surplus nations' people gain damage/armor, deficit nations suffer damage (surplus rate = net ÷ GDP, thresholds configurable) — trade powerhouses fight stronger

### 5. Biome economy specialties
- Each kingdom gets an **output specialty** based on its territory (farming/hunting/lumber/mining/building…) with output bonuses
- Specialty kingdoms are trade **exporters**, others **importers** — driving **comparative-advantage trade**
- Bonuses dynamically match the kingdom's real territory, forming a natural division of labor

### 6. Labor division (production function)
- Reads vanilla `citizen_job` and links wealth productivity to each profession
- Employed population earns "wages" through labor each year (labor creates wealth); the unemployed create none
- Profession → code mapping and productivity table are pure data, aggregated per kingdom on a background thread

### 7. Sapient spending (7 methods)
Rich sapients spend coins every year instead of hoarding:

| Method | Description |
|--------|-------------|
| Buy weapons | Buy real weapons for themselves |
| Build investment | Invest in defensive towers |
| Craft arsenal | Mass-produce equipment and weapons |
| Wholesale weapons | Bulk-purchase weapons |
| Era event | Spend coins to trigger an era event |
| Charity | Transfer coins to the poorest sapient in the city |
| Pay taxes | Pay coins into the city treasury |

Coins **never vanish** — they really flow to city leaders, the poor or the state.

### 8. Kingdom era events
- **Golden Age**: happiness +30, birth rate +10
- **Revival**: happiness +35, damage +10, birth rate +10
- **Flourishing**: happiness +20, damage +5, armor +5
- **Economic Collapse**: triggered by depression + sharp average-wealth drop; happiness -15, damage +30, armor +20, plus population loss

### 9. Banking credit and crisis contagion
- Rich (wealth > 2× average) automatically lend to poor citizens of the same city at a yearly interest rate
- Default rate soars in depression, directly costing the rich lenders
- Above a contagion threshold, a **banking crisis** spreads along trade routes to deficit partners
- Statistical simulation (no per-loan records), zero memory growth

### 10. Disaster economic shocks
- Detects three signals: kingdom population drop >30%, city count drop >25%, city tile is a disaster type (volcano/meteor)
- Affected city treasury wealth **evaporates** by ratio
- Volcanoes temporarily boost mining: extra output bonus for affected regions during boom
- Detection via effects + tile reflection — no vanilla event hooks, compatible with all disaster types

### 11. Inheritance
- On death, coins pass **parents → spouse → children** in order
- The city mayor collects inheritance tax
- With no heirs at all, the full estate goes to the mayor

### 12. Policies and governance
- High-Gini kingdoms attempt **wealth redistribution policies**
- Failed policies depose (or even kill) the ruler — failed economic governance has a price

### 13. Population and labor
- Population pressure: kingdom population vs carrying capacity
- Labor productivity: output tracked by profession

---

## UI

A new **Economy Tab** (coin icon) is added to the bottom toolbar, with 7 buttons (hover for tooltips):

| Button | Function |
|--------|----------|
| Economy Overview | Toggle the main economy panel (draggable, resizable, game never pauses) |
| Intervene | Pick a kingdom to incite or suppress unrest |
| Collect Now | Run data collection and recalculation manually |
| Clear History | Wipe all historical snapshot data |
| Rich List | View the wealthiest actors |
| Economy Events | Toggle the event stream window |
| Cycle Phase | Cycle Boom → Recession → Depression → Recovery |

**Overview panel** contains:
- Overview page: GDP, average, population, wealth gap, kingdom ranking
- Trend page: **wealth trend chart** (global wealth + dynamic Top-5 kingdom lines + phase color bands + vertical legend + macro summary) + **Gini trend chart**
- Social unrest state, historical snapshots

**Trend chart features** (remade in v0.7.0):
- **Dynamic kingdom lines**: a line appears when a kingdom enters the ranking, breaks when it falls out, and resumes when it re-enters
- **Phase color bands** behind the charts: green boom / orange recession / red depression / blue recovery
- **Macro summary line**: total output, price index, population, average wealth, wealth gap
- **Vertical legend**: color swatch + name + latest value per kingdom; fallen kingdoms shown gray as "fallen"

**Event window** (dual ring buffers since v0.8.3):
- **Major events** (revolution/uprising/bubble burst/disaster/banking crisis/era/collapse/policy failure/throne change/war plunder) kept in an epic-grade buffer — never pushed out by high-frequency events
- **Ordinary events** (build investment / craft arsenal / wholesale weapons, etc.) in their own ring buffer
- Type statistics row at the top: shows only the types that actually occurred this game

4-language UI: 简体中文 / 繁體中文 / English / Русский (auto-follows the game language, or can be fixed in settings).

---

## Configuration

All parameters are adjustable through the NML mod settings window, including:
- **Interface language**: ui_language (zh / zh_tw / en / ru, independent of game language)
- Unrest detection toggle, trigger threshold, starting grace period, cities per riot
- Economic cycle toggle, danger/health lines for the wealth gap, consecutive-period limits
- Boom stimulus ratio, bubble accumulation coefficient, bubble burst threshold, max boom duration
- Survival line, famine death rate
- War plunder ratio, war waste ratio, revolution delay, revolution kill ratio
- Street uprising threshold, uprising delay, executed-rich ratio, rob-the-rich redistribution ratio
- Wealth tax toggle, rate, exemption line
- Trade toggle, trade flow ratio
- Labor division toggle, base wage
- Era events toggle, duration, collapse ratio
- Live data refresh toggle and interval
- Banking credit toggle, yearly interest, depression default rate, crisis contagion threshold
- Disaster shock toggle, wealth evaporation ratio, volcano mining bonus
- Money velocity, inflation bonus during bubbles
- World log output toggle

---

## Installation

Manual install:
1. Put the `EconomyMod` folder into `WorldBox/Mods/`
2. Make sure the structure is:
   ```
   WorldBox/
   └── Mods/
       └── EconomyMod/
           ├── EconomyMod.dll
           ├── mod.json
           ├── icon.png
           └── Locales/
               ├── ch.json
               ├── zh_tw.json
               ├── en.json
               └── ru.json
   ```
3. Launch the game and enable "Classical Economics" in the mod list

### Dependencies
- Requires **NeoModLoader (NML)**
- Latest WorldBox recommended (0.51.2+)

---

## Compatibility

- Built on vanilla game mechanics, no core game files modified
- Era traits registered through the native trait system (icons and descriptions work)
- Supports save loading and new maps (reset/persist logic handled)
- All statistics computed on a background thread — zero impact on game performance
- **Compatible with the Optime optimization mod**: Optime's `ActorJobFlatten` flattening rewrite lacks null-reference defense and throws NRE crashes when iterating over recycled slots after mass deaths (famine, killing-the-rich, war plunder). Our `OptimeCompatibility` layer **modifies no Optime file at all** — a Harmony Transpiler injects null defense into Optime's already-compiled flat loop at runtime (null actor → skip iteration, 100% of the optimization preserved), with a Finalizer as a safety net for residual NREs. Zero overhead on the normal path; the two mods coexist safely

---

## Changelog

### v0.13.1 (2026-08-19)
**Memory lifecycle fixes**.
- Release stale `City`, `Actor`, and `Kingdom` references when switching worlds or returning to the main menu.
- Evict trade edges belonging to removed cities and kingdoms, preventing cache growth in long-running worlds.
- Prevent compute-buffer reuse until old background trade workers have actually exited.
- Remove stale disaster and era state, and prevent hot reloads from duplicating the tick runner, toolbar, and tooltips.

### v0.13.0 (2026-08-14)
**Net trade balance ranking + trade military power**.
- **Net trade ranking**: the floating window is reworked from a share-trend line chart into two net-balance tables (cities / kingdoms), each row showing export / import / net (exports − imports), net descending, surplus green / deficit red. A kingdom's net = sum of its cities' trade, naturally consistent.
- **Trade military power**: trade now maps to combat — surplus nations (net ÷ GDP ≥ threshold) grant their people real damage/armor (+20 damage / +10 armor), deficit nations −20 damage. Configurable toggle + thresholds (4 languages); only re-traverses citizens when the tier changes, O(1) otherwise.
- **Same-kingdom city trade allowed**: city trade is no longer cross-border-only; same-kingdom city pairs also build land edges and settle, counted in city net (offsetting at kingdom level).

### v0.12.0 (2026-08-14)
**Trade pair tables (later reworked to net ranking in v0.13)**.
- Replaced the share line chart with city↔city / kingdom↔kingdom pair tables, cities showing real names (collected on main thread via `SafeCityName`).
- Fixes: trade trend chart freezing after 50 years (`HistoryService.GetRecent` ring-buffer read start bug → `start=_head-take`, Capacity 100→50); share window drag drift (anchor/pivot to `(0,0.5)` convention).

### v0.9.0 (2026-08-13)
**Geographic trade enhancement**: trade evolves from an abstract zero-dimensional flow into real geographic economics.
- **Geographic distance decay**: the background thread resolves each kingdom capital city's tile coordinates (multi-candidate field probing + caching, zero performance cost); trade decays as `1/(1+avg distance × decay factor)` — far-apart kingdoms trade less, economies become more localized; kingdoms with unknown coordinates fall back to factor 1 (no penalty)
- **Transport cost**: fixed trade friction eating a share of all trade volume; heavier logistics means less total trade
- **Regional prices & arbitrage trade**: each kingdom forms a regional price index from local supply/demand (high output → ample supply → low local price; overcrowding → strong demand → high local price); kingdoms below the average price run export surpluses, those above run import deficits, and the price-gap weight drives arbitrage flows
- **Panel additions**: "Price dispersion" stat card (coefficient of variation of local prices, 0 = uniform prices), "Local price" column in the kingdom ranking (≥1.3× baseline amber inflation / ≤0.8× info-blue bargain)
- **UI fix**: stat card grid rebuilt as multi-row layout — no more horizontal overflow when cards exceed 3
- New configs: distance decay (0–0.05), transport cost (0–0.3), price diff weight (0–1), localized in all four languages

### v0.8.4 (2026-08-12)
**Comprehensive optimization & slimming**: 15 dead-code removals + 10 bug fixes + 4 refactorings, behavior fully unchanged.
- **Dead code removed** (zero behavioral impact, all cross-file reference-verified): write-only fields in stat structs (Accum.Id, KingdomSim.Food/Boats, KingTrack.Seen, 4 KingdomStats fields), dead BiomeEconomy methods (GetName/IsComplementary) + name arrays, write-only BankingEngine state (_kingdomCredit/LastDefaults), EraEngine.HasActive, entry-class _tickRunner, EventStreamService.GetRecent compat entry, UnrestConfig.Instance setter → private
- **Bug fixes**:
  - **War plunder never worked** (critical): reading `WarData.winner` via reflection threw InvalidCastException (boxed enum unboxed as int) that was silently swallowed → switched to `System.Convert.ToInt32`; war plunder works for the first time
  - **Revolution delay gate broken**: GetState returned unrest duration including pre-rebellion accumulated years, making the delay gate unreachable → new `RebelYear` field computes the true rebellion duration
  - Uprising condition, leftover trait after rebellion ends, banking int overflow (negative cast added money back), disaster tile reflection soft-failure, policy-failure short-circuit ambiguity
  - **Spending: pay first, receive second** — eliminates free weapons / free buildings / double charges (buy weapon / build investment / craft arsenal / wholesale weapons / era events)
  - **Wealth tax coin conservation**: zero per-capita remainder now fully granted to the first poor recipient instead of being silently destroyed
  - HistoryService ring-buffer clear, no more stale snapshot references
- **Refactorings**: ApplyTaxPolicy always-false params removed + MoneySupply → private, ResetAllEngines(bool) extracted (shared by Reload and new-map), GameHelpers.Shuffle<T> made public (reused by EraEngine/UnrestEngine), one fewer O(N) pass in stat aggregation

### v0.8.3 (2026-08-12)
- **Dual ring buffers for the event stream**: major events (revolution/uprising/bubble burst/disaster/banking crisis/era/collapse/policy failure/throne change/plunder, capacity 100) and ordinary events (capacity 60) — major events are kept at epic-grade depth
- Fixed high-frequency spending events (build investment / craft arsenal / wholesale) pushing low-frequency major events out of the window
- Type statistics row added at the top of the event window (cumulative; only types that occurred this game are shown)

### v0.8.2-hotfix
- Fixed the Transpiler never taking effect: Harmony's decompiled stloc operand is a `LocalBuilder`, so it must be extracted with `ExtractLocalIndex()` — a direct `(int)` cast throws InvalidCastException (the v0.8.2 first release silently failed to inject)

### v0.8.2
- **Optime compatibility upgraded to "zero modification + runtime Transpiler injection"**: Optime sources/compiled output are no longer modified (the Optime folder stays pristine); instead a Harmony Transpiler is attached to Optime's compiled `ActorJobFlatten.BatchActors_u4_deadCheck_Prefix` — injecting a `null → skip iteration` branch right after actor load, preserving 100% of Optime's optimization; safe fallback if the IL pattern doesn't match
- **Two-line defense**: Transpiler main line + Finalizer backup (swallows only NREs whose stack hits `ActorJobFlatten`; everything else rethrows), 30-second log throttling
- **No Optime recompilation needed**: injection happens at runtime, Optime's build artifacts never change — just restart the game

### v0.8.1
- **Fully compatible with the Optime optimization mod**: Optime's `ActorJobFlatten` lacks null defense and throws NREs when its flat batch loop hits recycled slots after mass death; this mod adds the `OptimeCompatibility` layer (Harmony Finalizer) that swallows only NREs hitting the `ActorJobFlatten` frame — zero overhead on the normal path
- Compat layer installed on the first frame: detects and attaches to the Optime assembly after all mods load; auto-skips when Optime is absent

### v0.8.0
- **Fixed the economic cycle getting permanently stuck** (critical): HUD incite/suppress buttons now use a synchronous collection path and no longer post to an unconsumed background cycle; the cycle checker gained a self-healing branch that converges lingering stuck states
- **Fixed background-thread Unity object access** (critical): kingdom specialties are now read on the main thread during collection and stored as pure data; the background thread has zero Unity access, eliminating random crashes and data corruption
- **Fixed 9 medium-severity bugs**: double phase jump, negative money supply, wealth-tax divide-by-zero, negative inheritance compensation, fake revival recovery, NaN config pollution, cross-save static pollution (Unrest/Policy missing Reset), missing riot-year write, bubble evaporation hitting the poor disproportionately
- **Fixed icon texture leak** (minor): failed loads now destroy the texture and cache the failure
- Version bumped to 0.8.0, release repackaged

### v0.7.0
- New **labor division (production function)**: employed population creates wages via labor
- New **price index (CPI)**: rises with money velocity and bubbles; shown in panel and trend chart
- New **banking credit and crisis contagion**: rich lend, depression defaults, crises spread via trade routes
- New **disaster economic shocks**: population/city drop and disaster-tile detection, evaporates wealth, volcano mining boost
- New **biome specialties**: kingdoms specialize by territory, driving comparative-advantage trade
- **Trend chart remake**: dynamic Top-5 kingdom lines (enter to show / fall to break / re-enter to resume), phase color bands, macro summary, vertical legend
- **4-language localization**: 简体中文 / 繁體中文 / English / Русский (including all settings entries)
- Performance: chart texture cache + content fingerprint, object pooling — no more frozen charts or GC stutter
- Toolbar trimmed to 7 buttons, phase switching merged into a single cycle button

### v0.6.0
- High-Gini regime collapse chain: **street uprising** (full-city riot, kill-the-rich-to-feed-the-poor, dethrone the king); revolutions now execute the wealthy
- War plunder rework: waste ratio evaporates rich wealth + rob-the-rich-to-feed-the-poor redistribution, significantly lowers Gini
- Chart cache key gained a content fingerprint — fixes charts freezing when kingdoms change
- Button icon resources added, obsolete scripts removed

### v0.5.0
- Full macroeconomic system: GDP / Gini / economic cycle / kingdom trade
- Sapient spending system (7 methods)
- Kingdom era events
- Social unrest and revolution
- Inheritance and inheritance tax
- Policy adjustment and ruler turnover
- Global rich list, event stream, trend charts
- Chinese and English UI

---

## FAQ

**Q: Will the economy get stuck in recovery forever?**
No. Use the "Cycle Phase" toolbar button to manually cycle the phase (Boom → Recession → Depression → Recovery).

**Q: Does it affect game performance?**
No. All statistics are computed on a background thread; the main thread only collects and displays.
