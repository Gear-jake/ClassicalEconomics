# Classical Economics

> A complete macroeconomic simulation for WorldBox — wealth tracking, economic cycles, kingdom trade, social unrest, and sapient consumption.

**Author**: Jake
**Version**: 0.5.0
**Type**: Economy / Simulation
**Target**: WorldBox 719+

---

## About

"Classical Economics" gives your world a real economy. The mod identifies all **sapient species** (including aliens, dragons, and any race capable of building civilization), tracks their wealth, and simulates a full macroeconomic system:

- Collects the coins and loot of every civilized creature each year, calculating **global GDP, average wealth, and Gini coefficient**
- Wealth drives the full economic cycle: **Boom → Recession → Depression → Recovery**
- Kingdoms redistribute wealth through **trade flows** (no coins are created from nothing)
- Wealth inequality triggers **unrest, rebellion, and revolution** — and pushes kings to **tax and reform**
- Wealthy sapients spend heavily every year: buying weapons, funding construction, crafting arsenals, paying taxes, giving charity…
- When a sapient dies, their wealth is inherited **parents → spouse → children**, while city mayors collect inheritance tax

Everything builds on **vanilla game mechanics** — no external resources, and all statistics are computed on a background thread with zero impact on game performance.

---

## Core Features

### 1. Macroeconomic Statistics
- Tracks **all sapient species** (determined by sapience trait — alien rulers are counted too)
- Global GDP, average wealth, civilized population
- **Gini coefficient**: 0 = perfect equality, 1 = perfect inequality
- Per-kingdom breakdown: wealth, average, inequality, population

### 2. Economic Cycle
- Four-phase cycle: **Boom → Recession → Depression → Recovery**, driven by the global wealth gap
- Boom: credit expansion — coins injected as a share of GDP, bubbles accumulate
- Bubble burst: when accumulated value exceeds the threshold, coins evaporate into recession
- Depression: when average wealth falls below the survival line → famine, the poor starve
- Recovery: slow rebound, rebuilding confidence
- You can manually switch phases from the toolbar

### 3. Social Unrest & Revolution
- Auto-detects kingdoms with high wealth inequality and triggers vanilla rebellion
- **Incite Unrest / Suppress Unrest**: manually intervene on any kingdom
- Prolonged rebellion → **revolution**: the old regime falls, population killed proportionally
- War plunder: winners seize a share of the loser kingdom's coins

### 4. Kingdom Trade Flows
- Kingdoms above global average wealth run a trade **surplus** (gain coins); those below run a **deficit** (pay coins)
- Settled through city treasuries via the vanilla tax channel
- Total surplus = total deficit across all kingdoms — **zero-sum, no coins created from nothing**

### 5. Sapient Spending System
The rich no longer hoard coins — they spend a share of their wealth yearly through **seven rotating spending methods**:

| Method | Description |
|--------|-------------|
| Buy Weapon | Purchase a real weapon |
| Build Investment | Fund construction of towers |
| Craft Arsenal | Mass-produce equipment and weapons |
| Wholesale Weapons | Bulk weapon purchase |
| Era Event | Spend coins to trigger era events |
| Charity | Transfer coins to the poorest sapient in the same city |
| Pay Tax | Pay coins to the city treasury |

Coins **never vanish** — they genuinely flow to city leaders, the poor, or the state.

### 6. Kingdom Era Events
- **Golden Age**: happiness +30, birth rate +10
- **Revival**: happiness +35, damage +10, birth rate +10
- **Flourishing**: happiness +20, damage +5, armor +5
- **Economic Collapse**: triggered in depression with a sharp wealth drop — happiness −15, damage +30, armor +20, plus emigration pressure

### 7. Inheritance
- On death, wealth transfers **parents → spouse → children**
- City (mayor) collects inheritance tax
- With no heirs, the mayor inherits everything

### 8. Policy & Governance
- Kingdoms with high Gini attempt **wealth redistribution policies**
- Failed policies may depose (or kill) the ruler — bad economic governance has consequences

### 9. Population & Labor
- Population pressure: kingdom population vs capacity
- Labor productivity: output tracked by profession

---

## UI

A new **Economy tab** (coin icon) in the bottom toolbar with 11 buttons (hover for tooltips):

| Button | Function |
|--------|----------|
| Economy Overview | Toggle the main draggable/resizable panel (game never pauses) |
| Incite Unrest | Select a kingdom to trigger rebellion |
| Suppress Unrest | Select a kingdom to quell rebellion |
| Collect Now | Run data collection and recalculation manually |
| Clear History | Wipe all historical snapshot data |
| Rich List | View the wealthiest actors |
| Economy Events | Toggle the event stream window |
| Boom / Recession / Depression / Recovery | Manually switch the economic phase |

**Overview panel** includes:
- Overview page: GDP, average wealth, population, wealth gap, kingdom rankings
- Trend page: **wealth trend chart** + **Gini trend chart** (background bands show the phase: green boom / orange recession / red depression / blue recovery; red and green lines mark danger/health thresholds)
- Unrest status and historical snapshots

Supports **Chinese/English UI** toggle.

---

## Configuration

All parameters are adjustable via the NML mod settings, including:
- Unrest detection toggle, trigger threshold, grace period, max cities per outbreak
- Cycle toggle, danger/health wealth-gap lines, threshold streak
- Boom stimulus ratio, bubble factor, bubble threshold, max boom duration
- Survival line, famine death rate
- War plunder ratio, revolution delay, revolution kill ratio
- Real-time refresh toggle & interval
- World log output

---

## Installation

1. Install [NeoModLoader (NML)](https://github.com/WorldBoxOpenMods/ModLoader)
2. Put the `EconomyMod` folder into `WorldBox/Mods/`
3. Launch the game and enable "Classical Economics" in the mod list

---

## Compatibility

- Deep integration with vanilla mechanics; no core game files modified
- Era traits use the native trait system with proper icons and descriptions
- Supports save loading and new maps (auto reset / state preservation logic)
- All statistics computed on a background thread — zero main-thread overhead

---

## Changelog

### v0.5.0
- New macroeconomy: GDP / Gini / economic cycle / kingdom trade
- Sapient spending system (7 methods)
- Kingdom era events (Golden Age / Revival / Flourishing / Collapse)
- Social unrest & revolution mechanics
- Inheritance and inheritance tax
- Policy adjustment and ruler turnover
- Global rich list, event stream, trend charts
- Chinese & English UI

---

## FAQ

**Q: Why is my alien ruler missing from the economy?**
The mod determines civilized species by their sapience trait — any race that can build city-states is tracked.

**Q: Is the economy stuck in recovery?**
No. You can manually switch phases via the toolbar buttons.

**Q: Does it affect performance?**
No. All calculations run on a background thread; the main thread only collects and displays data.
