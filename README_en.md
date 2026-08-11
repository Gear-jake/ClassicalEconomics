# Classical Economics

A complete macroeconomic simulation mod for WorldBox.

It tracks the wealth of all sapient species (including aliens and dragons), calculates global GDP, average wealth, the Gini coefficient and a price index (CPI), and simulates economic cycles, kingdom trade, social unrest, inheritance, spending, labor division, banking credit and disaster shocks.

## Features

### Macro statistics
- Tracks all sapient species (detected by intelligence trait; alien kings count too)
- Global GDP, average wealth, civilized population, Gini coefficient
- Price index (CPI): rises with money velocity and bubble buildup — visible inflation
- Per-kingdom breakdown: wealth, average, inequality, population

### Economic cycle
- Four phases: Boom → Recession → Depression → Recovery, driven by the global wealth gap
- Boom: credit expansion, coins injected by GDP ratio, bubbles keep building
- Bubble burst: when accumulated bubbles pass the threshold, large amounts of coins evaporate
- Depression: average wealth below survival line → famine and deaths
- Recovery: slow rebound and rebuilding of confidence
- Manually cycle the phase from the toolbar

### Social unrest and revolution
- Auto-detect high-inequality kingdoms and trigger vanilla rebellion
- Incite or suppress unrest in any kingdom manually
- Prolonged rebellion → revolution: old regime overthrown, population killed by ratio
- Street uprising: full-city riot, kill-the-rich-to-feed-the-poor, dethrone the king
- War plunder: winners loot loser coins; waste ratio evaporates rich wealth and redistributes

### Kingdom trade and comparative advantage
- Kingdoms above global average wealth are trade **surplus** (gain coins), below are **deficit** (pay coins)
- Settled via city treasury, same channel as vanilla taxes
- Total surplus = total deficit, **zero-sum, no coins created from nothing**
- Biome specialties: each kingdom gets an output specialty (farming/hunting/lumber/mining/building…) with bonuses, driving comparative-advantage trade

### Labor division (production function)
- Reads vanilla `citizen_job` and links wealth productivity to each profession
- Employed population earns "wages" through labor each year (labor creates wealth); the unemployed create none
- Profession → code mapping and productivity table are pure data, aggregated per kingdom on a background thread

### Sapient spending (7 methods)
Rich sapients spend coins every year instead of hoarding:
buy weapons, build investments, craft arsenals, wholesale weapons, era events, charity, taxes.
Coins never vanish — they really flow to cities, the poor or the state.

### Banking credit and crisis contagion
- Rich (wealth > 2× average) automatically lend to poor citizens of the same city at a yearly interest rate
- Default rate soars in depression, directly costing the rich lenders
- Above a contagion threshold, a **banking crisis** spreads along trade routes to deficit partners
- Statistical simulation (no per-loan records), zero memory growth

### Disaster economic shocks
- Detects three signals: kingdom population drop >30%, city count drop >25%, city tile is a disaster type (volcano/meteor)
- Affected city treasury wealth **evaporates** by ratio
- Volcanoes temporarily boost mining: extra output bonus for affected regions during boom
- Detection via effects + tile reflection — no vanilla event hooks, compatible with all disaster types

### Era events
Golden Age, Revival, Flourishing, Economic Collapse — each with national buffs (happiness, damage, armor, birth rate).

### Inheritance
When a sapient dies, wealth passes to family (parents, spouse, children); the city mayor collects inheritance tax.

### Policies
High-inequality kingdoms try wealth redistribution policies; failed policies may depose or kill the ruler.

### UI
- Draggable and resizable overview panel (game never pauses), Overview + Trend pages
- **Dynamic Top-5 kingdom wealth lines** (v0.7.0): a line appears when a kingdom enters the ranking, breaks when it falls out, and resumes when it re-enters
- **Phase color bands** behind the charts: green boom / orange recession / red depression / blue recovery
- **Macro summary line**: total output, price index, population, average wealth, wealth gap
- **Vertical legend**: color swatch + name + latest value per kingdom; fallen kingdoms shown gray as "fallen"
- Gini trend chart, global rich list, event stream window
- 4-language UI: 简体中文 / 繁體中文 / English / Русский (auto-follows game language, or fixed in settings)

## Toolbar (Economy Tab, 7 buttons)
| Button | Function |
|--------|----------|
| Economy Overview | Toggle the main economy panel |
| Intervene | Pick a kingdom to incite or suppress unrest |
| Collect Now | Run data collection and recalculation manually |
| Clear History | Wipe all historical snapshot data |
| Rich List | View the wealthiest actors |
| Economy Events | Toggle the event stream window |
| Cycle Phase | Cycle Boom → Recession → Depression → Recovery |

## Installation

1. Install NeoModLoader (NML)
2. Put the EconomyMod folder into WorldBox/Mods/
3. Launch the game and enable "Classical Economics" in the mod list

## Compatibility

- Built on vanilla game mechanics, no core game files modified
- Era traits registered through the native trait system (icons and descriptions work)
- Supports save loading and new maps (reset/persist logic handled)
- All statistics are computed on a background thread, zero impact on game performance

## Changelog

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

Version: 0.7.0
Author: Jake
