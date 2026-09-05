# Classical Economics

> WorldBox macroeconomics & nation-governance simulation mod ｜ Current version *1.4.0* ｜ Requires NeoModLoader (NML)

## Feature Overview

**Macroeconomics**
- Wealth tracking for every race (demons, aliens...): GDP, average wealth, Gini coefficient, CPI per kingdom
- Business cycle: Boom → Recession → Depression → Recovery; bubble bursts, famine and inflation
- Labor wages, banking credit & default contagion (spreading to weaker kingdoms), disaster shocks, inheritance with damage-based split
- Social unrest: rebellion → uprising → revolution; war plunder and dynasty monitoring

**Playable Government · Central Banker**
- Hover a kingdom on the map and press **G** (rebindable): claim / open the Cabinet
- Royal treasury: founding grant + periodic levies; policies, decrees, buildings and diplomacy spend real gold
- 6 ongoing policies x 3 tiers with slots (coinage / state monopoly / propaganda / poor relief...), one-shot decrees (relief / festival), **vanilla building placement** (10 kinds, click the map to build)
- Track record + nation GDP trend chart (last 40 cycles); **the Central Banker fully persists** (treasury, claim, policies, buildings, records, diplomacy and GDP chart history all save and restore)
- Diplomacy: war / peace (ransom when weaker) / alliances / gifts / bilateral economic pacts - two-level style (list → detail: three-layer crest + nation stats)

**National Law Law**
- **Every kingdom (AI included): 28 laws x 5 tiers + 16 policies x 3 tiers**, evolving yearly from national situation and 6 national natures
- Mutex law pairs, paid upgrades / free downgrades; effects genuinely wired into 9 engines (production/price/Gini/unrest/consumption/disaster/build cost/wages/military)
- Law bonuses visible on **citizen traits** (Scholar State, Militarized, etc.); codex persisted in saves
- Live aggregate-effect summary + semantic tier names (e.g. Commercial Policy: Laissez-faire → Command economy)

**Decision Events**
- 16 events across 6 families (finance/disaster/court/military/civil/diplomacy); condition-filtered yearly draw, max one per kingdom per year
- Player kingdom: non-modal choice window + Cabinet to-do; the cautious option runs on timeout; AI kingdoms decide by national character and land in the event feed
- Event window rebuilt: filter chips (all/decisions/states & wars/economy) + fold by year + single-column timeline; **an always-open event window rebuilds only once per year**

**UI & Performance**
- The Cabinet **auto-refreshes after each yearly settlement**; **adjustable UI scale for fonts and buttons (0.8-1.6x, applies instantly from settings)**
- Frame-budgeted yearly settlement (4 ms/frame; taxes never reduced); automatic memory cleanup; background-thread statistics with no main-thread pathfinding
- 42 automated gates + performance audit; every gold transfer conserved
- Four-language UI (简体 / 繁體 / English / Русский); slate-and-gold 9-slice UI

## Installation

1. Install [NeoModLoader](https://github.com/neoModLoader) (NML)
2. Put the `EconomyMod` folder into `WorldBox/Mods/`
3. Enable "Classical Economics" in the mod list



### Performance group (annual closeout)

The NML settings window adds a **Performance** group. Every key below is synchronized across `default_config.json`, `UnrestConfig`, `ConfigCallbacks` (AllConfigIds + bounded ParseInt + callback), and all four locales (zh / zh_tw / en / ru), verified by the fail-closed `tools/Test-ConfigDocs.ps1` consistency gate:

| Config key | Default | Range | Description |
|------------|--------:|-------|-------------|
| `real_time_refresh_threshold` | 2000 | 100-100000 | Refresh breaker: skip recompute when alive units reach this (UI-only refresh) |
| `real_time_refresh_budget` | 2000 | 100-100000 | Max alive units processed per lightweight refresh |
| `spending_cap_per_year` | 5000 | 1-100000 | Max wealthy actors processed per year |
| `banking_default_cap_per_year` | 500 | 1-100000 | Max kingdoms processed for credit/defaults per year |
| `banking_contagion_cap_per_year` | 500 | 1-100000 | Max contagion partners evaluated per year |
| `inheritance_scan_per_frame` | 2000 | 1-100000 | Max alive units scanned per frame in the 3-second inheritance window |
| `frame_budget_ms` | 4 | 1-100 | Max ms the annual closeout state machine advances per frame (snapshot/UI only after all stages) |
| `cycle_window_ms` | 2000 | 100-10000 | Total ms allowed for the whole annual closeout; on timeout extend to 5000ms first, then reduce spending → banking → other; tax is never reduced |
| `perf_diagnostics_enabled` | false | switch | Records closeout stage times (Stopwatch) and managed-memory deltas; logs only over-budget stages. OFF = zero overhead (default) |
| `cycle_alloc_budget` | 4096 | 1-1048576 | Per-cycle managed-allocation budget in KB (default 4096 = 4MB); the yearly summary flags cycles exceeding it |
| `memory_cleanup_enabled` | true | switch | Auto memory cleanup: when ON, trims static scratch/cache collections (TrimExcess) at intervals while the game is idle (default ON) |
| `memory_cleanup_force_gc` | false | switch | Whether to run one System.GC.Collect when the cleanup interval fires (the only GC entry point in the mod, allowed by the performance gate on this single line only; default OFF) |
| `memory_cleanup_interval_seconds` | 30 | 5-300 | Auto memory cleanup interval (s): minimum seconds between two automatic memory cleanups |
| `memory_cleanup_notify_enabled` | true | switch | Shows a top-banner toast when a cleanup frees a meaningful amount (estimated ≥0.5 MB or forced GC ran); the HUD memory status line and logs are unaffected |
| `nation_play_enabled` | true | switch | Central banker gameplay: nation claiming, royal treasury, ongoing policies and one-shot decrees (default ON) |
| `treasury_income_ratio` | 5 | 1-20 | Royal treasury income ratio: percent of city warehouse gold levied each cycle |
| `policy_slots` | 3 | 1-5 | Ongoing policy slot cap: maximum simultaneous ongoing policies |
| `nation_claim_hotkey` | G | text | Hover a kingdom on the map and press this key to claim/open the cabinet (Unity KeyCode name; blank disables) |
| `event_chance_player` | 0.35 | 0-1 | Yearly chance of a decision event for your claimed kingdom (0 disables) |
| `event_chance_ai` | 0.15 | 0-1 | Yearly chance for AI kingdoms (they decide by national character) |
| `event_cooldown_years` | 3 | 1-10 | Minimum years between any two kingdom events |
| `ui_scale` | 1.2 | 0.8-1.6 | Overall scale of the cabinet panel font and buttons; applies immediately |

The economy panel overview shows a memory status line: last cleanup time, freed amount, managed heap and Unity used/reserved memory (`hud_mem_cleanup` / `hud_mem_cleanup_pending` / `hud_mem_usage`; the toast text is `memory_cleanup_toast`; all are locale strings in four languages, not config keys). The managed heap is the Mono GC view shared by the mod and the game; Unity used/reserved is the native-asset view — if the managed heap stays flat while Unity used keeps rising, the growth comes from the game itself, not the mod.

While the annual settlement runs, the HUD shows a settling marker (`settling_marker` / `settling_hint`) and manual collection / phase switching are disabled. The markers are pure locale strings (all four locales), not config keys.

---

## Controls

- **G** (rebindable): hover a kingdom → claim / open Cabinet (or the ledger button in nation/city windows)
- Cabinet tabs: Finance / Policies / Decrees & Build / Diplomacy / Law

## Links

- Full changelog (0.13.0 → 1.3.0): docs/更新总览_0.13.0_至今_简体中文.md
- [GitHub Releases](https://github.com/Gear-jake/ClassicalEconomics/releases)

---

Author: Jake ｜ Repo: github.com/Gear-jake/ClassicalEconomics
