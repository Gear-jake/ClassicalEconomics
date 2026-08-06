# Classical Economics

A complete macroeconomic simulation mod for WorldBox.

It tracks the wealth of all sapient species (including aliens and dragons), calculates global GDP, average wealth and the Gini coefficient, and simulates economic cycles, kingdom trade, social unrest, inheritance and spending.

## Features

- Macro statistics: global GDP, average wealth, civilized population, Gini coefficient (wealth gap), per-kingdom breakdown
- Economic cycle: Boom, Recession, Depression, Recovery. Driven by the wealth gap, with credit expansion, bubble buildup and burst, and famine in depression
- Kingdom trade: zero-sum trade flows between kingdoms (surplus kingdoms gain coins, deficit kingdoms pay; no coins are created from nothing)
- Unrest and revolution: auto-detect high-inequality kingdoms and trigger rebellion; manually incite or suppress unrest in any kingdom; prolonged rebellion turns into revolution
- Sapient spending: rich sapients spend coins every year through 7 methods (buy weapons, build investments, craft arsenals, wholesale weapons, era events, charity, taxes). Coins never vanish, they really flow to cities, the poor or the state
- Era events: Golden Age, Revival, Flourishing, Economic Collapse, each with national buffs (happiness, damage, armor, birth rate)
- Inheritance: when a sapient dies, wealth is passed to family (parents, spouse, children); the city mayor collects inheritance tax
- Policies: high-inequality kingdoms try wealth redistribution policies; failed policies may depose or kill the ruler
- UI: draggable and resizable overview panel (game never pauses), wealth trend chart, Gini trend chart, global rich list, event stream window, Chinese and English UI

## Installation

1. Install NeoModLoader (NML)
2. Put the EconomyMod folder into WorldBox/Mods/
3. Launch the game and enable "Classical Economics" in the mod list

## Compatibility

- Built on vanilla game mechanics, no core game files modified
- Supports save loading and new maps
- All statistics are computed on a background thread, zero impact on game performance

Version: 0.5.0
Author: Jake
