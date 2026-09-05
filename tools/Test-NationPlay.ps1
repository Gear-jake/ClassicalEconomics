param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

# 中央银行家（v0.95）门禁：国家绑定/金库守恒/政策槽位/法令冷却/建筑摧毁/管线阶段接线。
# 配置链五方一致由 Test-ConfigDocs 覆盖，键覆盖由 Test-LocalizationCoverage 覆盖；本门禁聚焦引擎不变量。

$ErrorActionPreference = 'Stop'

$failures = New-Object System.Collections.Generic.List[string]
$utf8 = New-Object System.Text.UTF8Encoding($false)

$nationPath = Join-Path $Root 'Core\NationEngine.cs'
$pipelinePath = Join-Path $Root 'Core\AnnualPipeline.cs'
$unrestPath = Join-Path $Root 'Core\UnrestEngine.cs'
$tswPath = Join-Path $Root 'Core\TradeSimulationWorker.cs'
$disasterPath = Join-Path $Root 'Core\DisasterEngine.cs'
$crisisPath = Join-Path $Root 'Core\SocialCrisisEngine.cs'
$mainPath = Join-Path $Root 'EconomyModMain.cs'
$cabinetPath = Join-Path $Root 'UI\CabinetWindow.cs'
$uiPath = Join-Path $Root 'UI\EconomyUI.cs'
$eventsPath = Join-Path $Root 'Services\EventStreamService.cs'

foreach ($p in @($nationPath, $pipelinePath, $unrestPath, $tswPath, $disasterPath, $crisisPath, $mainPath, $cabinetPath, $uiPath, $eventsPath)) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
        Write-Host "NATION_PLAY_RED: required file not found: $p"
        exit 1
    }
}

function Read-Source([string]$p) { [System.IO.File]::ReadAllText($p, $utf8) }
$nation = Read-Source $nationPath
$pipeline = Read-Source $pipelinePath
$unrest = Read-Source $unrestPath
$tsw = Read-Source $tswPath
$disaster = Read-Source $disasterPath
$crisis = Read-Source $crisisPath
$main = Read-Source $mainPath
$cabinet = Read-Source $cabinetPath
$ui = Read-Source $uiPath
$events = Read-Source $eventsPath

function Assert([bool]$cond, [string]$msg) {
    if (-not $cond) { $failures.Add($msg) }
}

# ===== 1. 金库守恒与支出纪律 =====
Assert ($nation -match [regex]::Escape('if (_treasury < amount) return false;')) 'TryPay must reject when treasury is insufficient (no partial payment)'
Assert ($nation -match 'SwitchCooldownYears\s*=\s*10') 'nation switch cooldown must be 10 years'
Assert ($nation -match [regex]::Escape('ReliefCooldownYears = 5')) 'relief cooldown must be 5 years'
Assert ($nation -match [regex]::Escape('FestivalCooldownYears = 3')) 'festival cooldown must be 3 years'
Assert ($nation -match [regex]::Escape('BuildCooldownYears = 10')) 'building cooldown must be 10 years'
# 认领启动资金：先从城市仓库扣（takeResource），再入金库（守恒顺序）
$claimIdx = $nation.IndexOf('public static bool Claim(')
$runIdx = $nation.IndexOf('public static void RunAnnual(')
if ($claimIdx -lt 0 -or $runIdx -lt 0) { $failures.Add('NationEngine missing Claim/RunAnnual') }
else {
    $claimBody = $nation.Substring($claimIdx, [Math]::Max(1, $runIdx - $claimIdx))
    Assert ($claimBody -match 'takeResource') 'Claim must transfer startup funds from city warehouses'
    Assert ($claimBody -match [regex]::Escape('gold * 20 / 100')) 'Claim startup must take 20% of warehouse gold'
}
# 结算期禁用动作
Assert (($nation -split 'AnnualPipeline\.IsSettling').Count -ge 5) 'all nation actions must be gated on !AnnualPipeline.IsSettling'
# 槽位上限由配置约束
Assert ($nation -match '_slots\.Count >= cfg\.PolicySlots') 'EnablePolicy must enforce the configured policy slot cap'
# 金库不足时政策暂停而非取消
Assert ($nation -match 'toast_nation_policy_suspended') 'insufficient treasury must suspend (not cancel) policies'
# 无 GC 调用
Assert ($nation -notmatch 'GC\s*\.\s*Collect') 'NationEngine must not call GC.Collect'

# ===== 2. 年度管线：Nation 阶段在 Banking 之后、Snapshot 之前，顺序不变量 =====
$enumMatch = [regex]::Match($pipeline, 'public enum AnnualStage\s*\{(?<body>.*?)\}', [System.Text.RegularExpressions.RegexOptions]::Singleline)
Assert ($enumMatch.Success -and $enumMatch.Groups['body'].Value -match '\bBanking,\s*\r?\n\s*Nation,') 'AnnualStage enum must declare Nation after Banking (index assertions pin it before Snapshot)'
$runIdx2 = $pipeline.IndexOf('case AnnualStage.Banking:')
$nationIdx = $pipeline.IndexOf('case AnnualStage.Nation:')
$snapIdx = $pipeline.IndexOf('case AnnualStage.Snapshot:')
Assert ($runIdx2 -ge 0 -and $nationIdx -gt $runIdx2 -and $snapIdx -gt $nationIdx) 'RunStage must execute Nation after Banking and before Snapshot'

# ===== 3. 引擎挂钩 =====
Assert ($unrest -match 'NationEngine\.PropagandaActive\(kid\)') 'UnrestEngine must skip accrual for propaganda-protected nation'
Assert ($unrest -match 'public static bool TryFestivalClear') 'UnrestEngine must expose TryFestivalClear for the festival decree'
Assert ($nation -match 'MarketTaxBaseBonus') 'resident tax must apply the market tax-base bonus (v1.3.0)'
Assert ($nation -match 'residentMult.*MarketTaxBaseBonus|MarketTaxBaseBonus.*residentMult') 'resident tax must stack coinage policy with market bonus (v1.3.0)'
Assert ($nation -match 'CollectFromResidents') 'treasury income must collect from residents (conservation)'
Assert ($nation -match 'PolicyKind\.Tariff') 'state monopoly policy must run in the annual pipeline'
Assert ($disaster -match 'NationEngine\.IsGranaryCity') 'Disaster must apply the granary loss reduction'
Assert ($disaster -match [regex]::Escape('DestroyCityBuildings(cityId, "toast_nation_destroyed_disaster")')) 'Disaster must destroy nation buildings'
Assert ($crisis -match 'NationEngine\.OnKingdomPlundered') 'Plunder must destroy nation buildings via OnKingdomPlundered'
Assert ($main -match 'NationEngine\.Reset\(\)') 'ResetAllEngines must reset NationEngine'

# ===== 4. 事件流类型接线 =====
foreach ($t in @('TypeNationClaim', 'TypeNationPolicy', 'TypeNationRelief', 'TypeNationFestival', 'TypeNationBuild')) {
    Assert ($events -match ('public\s+const\s+string\s+' + $t + '\s*=\s*"ev_nation_')) "EventStreamService missing $t constant"
    Assert ($events -match ('case ' + $t + ':')) "EventStreamService IsKnownType missing $t"
}

# ===== 5. 原版国家界面入口（KingdomWindowIntegration）=====
$integrationPath = Join-Path $Root 'Core\KingdomWindowIntegration.cs'
if (-not (Test-Path -LiteralPath $integrationPath -PathType Leaf)) {
    $failures.Add('missing file: Core\KingdomWindowIntegration.cs')
} else {
    $integration = Read-Source $integrationPath
    Assert ($integration -match 'AccessTools\.Method\(typeof\(StatsWindow\), "create"\)') 'integration must manually patch StatsWindow.create (attribute patching is unreliable for precompiled DLL mods)'
    Assert ($integration -match 'HarmonyId') 'integration must patch via a named Harmony id'
    Assert ($integration -match '国家界面入口补丁已安装') 'integration must log patch installation for diagnosis'
    Assert ($integration -match 'PowerButtonCreator\.CreateSimpleButton') 'integration must create the entry via NML PowerButtonCreator (PowerBox pattern)'
    Assert ($integration -match 'NationEngine\.Claim') 'integration entry must claim the shown kingdom'
    Assert ($integration -match 'CabinetWindow\.Instance\.Show') 'integration entry must open the cabinet'
    Assert ($integration -match 'catch \(System\.Exception') 'integration must fail closed on reflection/unexpected errors'
    Assert ($main -match 'KingdomWindowIntegration\.TryInstall\(\)') 'EconomyTickRunner must install the integration on first frame'
    Assert ($integration -match 'GetMetaObject') 'integration must read meta_object via runtime reflection (non-public in raw assembly)'
    Assert ($integration -match 'TryHotkeyOpen') 'integration must support the C hotkey entry (RulerBox-style)'
    Assert ($main -match 'NationClaimHotkey') 'hotkey must read the configurable NationClaimHotkey key'
    Assert ($ui -notmatch 'economy_cabinet') 'toolbar cabinet button must be removed (entry is the vanilla kingdom UI)'
}

# ===== 6. 外交（NationDiplomacy）=====
$diplomacyPath = Join-Path $Root 'Core\NationDiplomacy.cs'
if (-not (Test-Path -LiteralPath $diplomacyPath -PathType Leaf)) {
    $failures.Add('missing file: Core\NationDiplomacy.cs')
} else {
    $dip = Read-Source $diplomacyPath
    Assert ($dip -match 'DiplomacyManager') 'diplomacy must invoke the vanilla DiplomacyManager'
    Assert ($dip -match 'startWar') 'diplomacy must support declaring war'
    Assert ($dip -match 'endWar') 'diplomacy must support peace (WarManager.endWar)'
    Assert ($dip -match 'newAlliance') 'diplomacy must support forming alliances'
    Assert ($dip -match 'PactIncomeRatio') 'diplomacy must expose pact tribute income ratio (v1.3.0)'
    Assert ($dip -match 'MaxPacts\s*=\s*2') 'bilateral pacts must be capped at 2'
    Assert ($dip -match 'GiveGift') 'diplomacy must support gifts'
    Assert ($dip -match 'GiftGoodwill') 'gifts must accumulate mod-side goodwill'
    Assert ($dip -match 'catch \(System\.Exception') 'diplomacy must fail closed on reflection errors'
    Assert ($dip -match 'NationEngine\.AddTreasury') 'pact tribute must credit the treasury (v1.3.0)'
    Assert ($nation -match 'NationDiplomacy\.RunAnnual') 'annual pipeline must charge bilateral pact fees'
    Assert ($nation -match 'NationDiplomacy\.Reset') 'nation reset must clear diplomacy state'
    Assert ($cabinet -match 'BuildDiplomacyPage|BuildDiplomacyList') 'cabinet must render the diplomacy page'
    Assert ($cabinet -match 'BuildDiplomacyDetail') 'cabinet must render the diplomacy detail page'
    Assert ($cabinet -match 'BuildLawEffectSummary') 'cabinet codex page must show the aggregate effect summary'
    Assert ($cabinet -match 'BuildGdpChart') 'finance page must render the nation GDP chart'
    Assert ($cabinet -match '_dipTargetId') 'diplomacy list rows must navigate into detail (RulerBox-style two-level)'
    Assert ($cabinet -match 'BuildNativeBuildings') 'cabinet must render the native-building section'
    # 防回归：CreateText 结果无 LayoutElement（GetComponent 直接取值会 NRE 中断整页构建）
    Assert ($cabinet -notmatch '\.GetComponent<LayoutElement>\(\)\.flexibleWidth') 'cabinet rows must null-check LayoutElement (NRE kills page build)' 
    Assert ($cabinet -match 'build_native_house_t1') 'cabinet must list vanilla building buttons'
    Assert ($nation -match 'BeginNativePlacement') 'nation must expose BeginNativePlacement'
    Assert ($nation -match 'TickNativePlacement') 'nation must expose the per-frame placement tick'
    Assert ($nation -match 'NativeAddBuildingTried') 'placement must call BuildingManager.addBuilding (reflection)'
    Assert ($nation -match 'GetKingdomOfCity') 'placement must validate own territory via GetKingdomOfCity'
    Assert ($main -match 'TickNativePlacement') 'EconomyTickRunner must drive the placement tick' 
    Assert ($cabinet -match 'DeclareWar|SueForPeace|FormAlliance|GiveGift|SignPact') 'cabinet must expose the five diplomacy actions'
    Assert ($main -match 'NationEngine\.NationKingdomId != 0') 'real-time refresh must auto-enable while a nation is claimed'
    Assert ($events -match 'TypeNationDiplomacy') 'event stream must track diplomacy actions'
}

# ===== 7. UI 接线 =====
Assert ($cabinet -match 'TitleKey => "cabinet_title"') 'CabinetWindow must use the cabinet_title locale key'
Assert ($ui -match 'CabinetWindow\.Create\(\)') 'EconomyUI must create the cabinet window'

if ($failures.Count -gt 0) {
    foreach ($f in $failures) { Write-Host "NATION_PLAY_RED: $f" }
    Write-Host "NATION_PLAY_RED: $($failures.Count) invariant check(s) failed"
    exit 1
}

Write-Host 'NATION_PLAY_GREEN: binding/treasury/policy/decrees/buildings/pipeline wiring all pass'
exit 0
