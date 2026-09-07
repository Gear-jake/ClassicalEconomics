$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot

function Assert-True($cond, $msg) {
    if (-not $cond) { Write-Host "BANK_SYSTEM_RED: $msg"; exit 1 }
}

function Fail($msg) { Write-Host "BANK_SYSTEM_RED: $msg"; exit 1 }

# ===== 1. 引擎文件与账本模型（有界：无 Actor 对象字段、贷款簿按城） =====
$eng = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\BankEngine.cs'))
Assert-True ($eng -match 'class LoanRecord') 'BankEngine must define LoanRecord'
Assert-True ($eng -match 'internal class CityLedger') 'BankEngine must define CityLedger'
Assert-True ($eng -match 'internal class AiBank') 'BankEngine must define the AI simplified pool'
Assert-True ($eng -notmatch 'Actor\s+\w+;') 'BankEngine ledger records must not retain Actor references (bounded-memory invariant)'
Assert-True ($eng -match 'MaxPending|_borrowerPool') 'BankEngine must reuse scratch buffers'

# ===== 2. 年度生命周期四步齐全 =====
foreach ($step in @('EvaluatePlayer', 'CollectDue', 'EvaluateAi', 'CollectCommerceTax')) {
    Assert-True ($eng -match [regex]::Escape("static long EvaluatePlayer") -or $eng -match "static long $step" -or $eng -match $step) "BankEngine must implement $step"
}
Assert-True ($eng -match 'EconomyCycleModulator\.MoneySupply \+=') 'loan net growth must feed MoneySupply (real monetary policy)'
$gh = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\GameHelpers.cs'))
Assert-True ($gh -match 'public static Building PlaceNativeBuilding') 'GameHelpers must expose PlaceNativeBuilding (unified native placement)'

# ===== 3. 玩家三要素档位 + 预设通道 + 商业政策互斥 =====
foreach ($m in @('RatePermilleOf', 'ApplyChannel', 'SetFranchise', 'SetFairPrice', 'CommerceTaxMult')) {
    Assert-True ($eng -match $m) "BankEngine must implement $m"
}
Assert-True ($eng -match 'if \(on\) FairPriceOn = false') 'franchise must exclude fair-price (mutual exclusion)'
Assert-True ($eng -match 'if \(on\) FranchiseOn = false') 'fair-price must exclude franchise (mutual exclusion)'

# ===== 4. 管线：Bank 阶段位于 Banking 与 Nation 之间，且调 Evaluate =====
$pipeline = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\AnnualPipeline.cs'))
Assert-True ($pipeline -match 'Banking,\s*\r?\n\s*Bank,\s*\r?\n\s*Nation,') 'AnnualStage must declare Bank between Banking and Nation'
Assert-True ($pipeline -match 'case AnnualStage\.Bank:[\s\S]*?BankEngine\.Evaluate') 'RunStage must call BankEngine.Evaluate in the Bank stage'

# ===== 5. 存档对称（NationSave rb_bank_* 三键）=====
$save = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\NationSave.cs'))
foreach ($k in @('rb_bank_set', 'rb_bank_ledger', 'rb_bank_ai')) {
    Assert-True ($save -match [regex]::Escape($k)) "NationSave must persist $k"
}
Assert-True ($save -match 'BankEngine\.Serialize' -and $save -match 'BankEngine\.Restore') 'NationSave must round-trip BankEngine state'

# ===== 6. 货币联动配置键接线 =====
$cfg = [System.IO.File]::ReadAllText((Join-Path $Root 'Models\UnrestConfig.cs'))
foreach ($f in @('BankEnabled', 'BankMoneySupplyFactor', 'BankMaxLoansPerCity', 'BankDepositRatioDefault')) {
    Assert-True ($cfg -match [regex]::Escape("public bool $f =") -or $cfg -match [regex]::Escape("public float $f =") -or $cfg -match [regex]::Escape("public int $f =")) "UnrestConfig must declare $f"
}

# ===== 7. 抉择事件：bankRiskMin 条件 + commercePenaltyYears 效果 =====
$de = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\DecisionEvents.cs'))
Assert-True ($de -match 'bankRiskMin') 'DecisionEvents must support the bankRiskMin condition'
Assert-True ($de -match 'commercePenaltyYears') 'DecisionEvents must support the commercePenaltyYears effect'
Assert-True ($de -match 'BankEngine\.RiskTier') 'bankRiskMin must read BankEngine.RiskTier'
Assert-True ($de -match 'BankEngine\.SetCommercePenalty') 'commercePenaltyYears must route into BankEngine'

# ===== 8. events.json：银行事件存在且条件合法 =====
$jsonPath = Join-Path $Root 'events.json'
$json = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
$byId = @{}
foreach ($e in $json.events) { $byId[$e.id] = $e }
foreach ($id in @('caravan_ambush', 'trade_route_cut', 'guild_petition2', 'bank_run', 'bank_run_aftermath')) {
    if (-not $byId.ContainsKey($id)) { Fail "events.json missing banking event $id" }
}
if ($byId['bank_run'].bankRiskMin -ne 2) { Fail 'bank_run must require bankRiskMin = 2 (danger tier)' }
if ($byId['bank_run'].chainNext -ne 'bank_run_aftermath') { Fail 'bank_run must chain to bank_run_aftermath' }

# ===== 9. 四语键：页签/UI + 5 事件键组 =====
$localeDir = Join-Path $Root 'Locales'
$required = @('cabinet_tab_bank', 'bank_stats_reserves', 'bank_stats_loans', 'bank_stats_default',
    'bank_risk_label', 'bank_risk_0', 'bank_risk_1', 'bank_risk_2',
    'bank_rate_label', 'bank_quota_label', 'bank_reserve_label',
    'bank_preset_stimulus', 'bank_preset_neutral', 'bank_preset_suppress', 'bank_preset_applied',
    'bank_commerce_title', 'bank_commerce_tax', 'bank_policy_franchise', 'bank_policy_fairprice',
    'bank_policy_on', 'bank_policy_off', 'bank_disabled_note')
foreach ($e in @('caravan_ambush', 'trade_route_cut', 'guild_petition2', 'bank_run', 'bank_run_aftermath')) {
    $n = $byId[$e].options.Count
    $required += @("ev_$e", "ev_${e}_desc")
    for ($i = 1; $i -le $n; $i++) {
        $required += @("ev_${e}_opt$i", "ev_${e}_opt${i}_desc", "ev_${e}_res$i")
    }
}
foreach ($loc in @('ch.json', 'zh_tw.json', 'en.json', 'ru.json')) {
    $lp = Join-Path $localeDir $loc
    if (-not (Test-Path -LiteralPath $lp -PathType Leaf)) { Fail "locale file not found: $loc" }
    $locData = Get-Content -LiteralPath $lp -Raw -Encoding UTF8 | ConvertFrom-Json
    $props = $locData.PSObject.Properties.Name
    foreach ($key in $required) {
        if ($props -notcontains $key) { Fail "$loc missing key '$key'" }
    }
}

Write-Host 'BANK_SYSTEM_GREEN: ledger model, lifecycle, policy channel, save round-trip, events and 4-locale coverage all pass'
exit 0
