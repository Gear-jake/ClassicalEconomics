# v0.93 发布说明（可安装候选包已生成）

> 状态说明：本地化补全完成——窗口标题切语言即刷新 + 全部玩家可见文本四语言覆盖；44/44 门禁（43 项既有 + 新增 Test-LocalizationCoverage）+ performance_audit + 构建全绿。F3 实机验收由用户安装到游戏后亲自执行。

## 构建与哈希验证

- `build_local.ps1` 全量构建通过（exit 0，`EconomyMod.dll` 228864 字节，VS18 Roslyn csc，42 个源文件）。
- 暂存包 `release\ClassicalEconomics-0.93\EconomyMod\` 与 `bin\EconomyMod.dll`、zip 内 DLL 三方 SHA-256 一致（哈希见打包记录）。

## 本版本修复：窗口标题不随语言切换（用户上报）

- **病因**：`FloatingWindow.BuildPanel()` 仅在模组启动时执行一次且丢弃标题 `Text` 引用；切换界面语言时只刷新了 HUD 与贸易净额窗两个窗口的标题——事件窗（经济事件）与富豪榜窗（全球富豪榜）标题永远停留在启动时的语言。
- **修复**：基类持有 `_titleText` 并新增 `RefreshAllTexts()`（重写 `L(TitleKey)`）；四个窗口统一接线到 `OnLanguageChanged`；EventWindow/RichListWindow 可见时同步重建内容。现在切语言全部窗口标题即时生效。

## 本版本补全：其余未本地化文本（全量排查结论）

1. **18 处游戏顶部横幅（WorldTip）硬编码中文全部本地化**：政策引擎 9 处（推行调节/减税/增税/关税/财政失败/关税失败/退位/驾崩/内战）、动荡引擎 4 处（动荡爆发/起义/和谈/平息）、泡沫破裂、时代开启、银行危机、灾害冲击、革命。新增 `GameHelpers.NotifyLocalized(key, args)` 统一入口 + 18 个 `toast_*` 四语言键。
2. **时代事件名**：`EraEngine.EventName()` 改读现有 `ev_era_*` 四语言键（原硬编码盛世/复兴/强盛期/经济崩溃）。
3. **概览页指标卡标签**：人均/基尼复用 `col_avg`/`col_gini`，新增 `stat_pop`/`stat_trade`/`stat_bubble`（原硬编码中文）。
4. **事件流改革失败文案修正**：财政失败（值4）/贸易失败（值5）原来错误显示"国王退位"，新增 `ev_desc_policy_fail_fiscal`/`_trade` 并补分支。
5. **内存状态行 "N/A"** 占位符键化为 `hud_mem_na`。
6. **工具栏文案单一真相源**：Tab 名称/描述与 8 个按钮 tooltip 全部改从 `Locales/*.json` 取词（新增 `tab_economy_name`/`tab_economy_desc` 与 `economy_intervene`/`economy_trade_share`/`economy_cycle_phase` 及描述键），消灭"json + 代码内联四语言元组"双份维护。

## 新增门禁防回归

- `tools/Test-LocalizationCoverage.ps1`：① 代码引用的每个 `L/Lf/LocalizationService.Get` 字面量键必须存在于全部 4 个语言文件；② `Core/UI` 代码中 `Notify(` 调用行禁止携带硬编码 CJK（强制走 `NotifyLocalized`）；③ `OnLanguageChanged` 必须刷新全部四个悬浮窗；④ 8 个工具栏按钮 tooltip 键 + 描述键四语言齐全。
- 四语言文件各新增 31 键（现各 347 键），JSON 全量解析校验通过。

## 已知边界（如实告知，本轮不动）

- 6 个国民特质（盛世/复兴/强盛期/经济崩溃/贸易顺差/贸易逆差）的名称/描述仍只注册中/英双语（走原版 LocalizedTextManager，与模组自建本地化体系不同源）——俄/繁中玩家看到英文。需要研究原版 LTM 多语言表注册 API 后单独处理。
- 24 个孤儿键（存在于 json 但无代码引用，如 economy_unrest/suppress 等）保持原样，不影响运行。

## 暂缓任务

- 无。F3 实机验收由用户安装后亲自执行：切换模组设置里的"界面语言"，确认四个悬浮窗标题/按钮 tooltip/顶部横幅即时切换。
