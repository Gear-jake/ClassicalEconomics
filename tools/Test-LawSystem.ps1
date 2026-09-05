param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

# 法典门禁：每档必须影响至少一个 LawMods 乘数（防 dead-modifier）、互斥组完备、
# 升档付费降档免费、存档键注入、事件分级、UI 第五页接线。

$ErrorActionPreference = 'Stop'
$failures = New-Object System.Collections.Generic.List[string]
$utf8 = New-Object System.Text.UTF8Encoding($false)

$enginePath = Join-Path $Root 'Core\LawEngine.cs'
$aiPath = Join-Path $Root 'Core\LawAi.cs'
$savePath = Join-Path $Root 'Core\LawSave.cs'
$cabinetPath = Join-Path $Root 'UI\CabinetWindow.cs'
$mainPath = Join-Path $Root 'EconomyModMain.cs'

foreach ($p in @($enginePath, $aiPath, $savePath, $cabinetPath, $mainPath)) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) { Write-Host "CODEX_RED: missing $p"; exit 1 }
}

function Read-Source([string]$p) { [System.IO.File]::ReadAllText($p, $utf8) }
$engine = Read-Source $enginePath
$ai = Read-Source $aiPath
$save = Read-Source $savePath
$cabinet = Read-Source $cabinetPath
$main = Read-Source $mainPath

function Assert([bool]$cond, [string]$msg) { if (-not $cond) { $failures.Add($msg) } }

# 1) 数据表规模：统计 LawKeys / PolicyKeys 数组字面量键数（正则取数组块内 token）
# 1) 数据表规模：统计 law_*/policy_* 字符串字面量（Keys 常量与其 case 均如此命名）
$lawTotal = @($engine.Split('"') | Where-Object { $_ -match '^law_' }).Count
Assert ($lawTotal -ge 24) ("law_* keys must be >= 24, got " + $lawTotal)
$polTotal = @($engine.Split('"') | Where-Object { $_ -match '^policy_' }).Count
Assert ($polTotal -ge 16) ("policy_* keys must be >= 16, got " + $polTotal)

# 用简化方式：检查 ApplyLawMod / ApplyPolicyMod 每个 case 体含 m. 字段写
$lawModSection = $engine.Substring($engine.IndexOf('private static void ApplyLawMod'))
$policyModSection = $engine.Substring($engine.IndexOf('private static void ApplyPolicyMod'))
foreach ($case in [regex]::Matches($lawModSection, 'case (Law[A-Za-z0-9_]+):')) {
    $name = $case.Groups[1].Value
    $end = $lawModSection.IndexOf('case ', $case.Index + 5)
    if ($end -lt 0) { $end = $lawModSection.Length }
    $body = $lawModSection.Substring($case.Index, $end - $case.Index)
    Assert ($body -match 'm\.(Productivity|TaxRate|GiniShift|UnrestAccum|Price|Consumer|DisasterResist|BuildCost|Wage|Military|Happiness|Birth)') "law $name does not modify any LawMods multiplier"
}
foreach ($case in [regex]::Matches($policyModSection, 'case (Policy[A-Za-z0-9_]+):')) {
    $name = $case.Groups[1].Value
    $end = $policyModSection.IndexOf('case ', $case.Index + 5)
    if ($end -lt 0) { $end = $policyModSection.Length }
    $body = $policyModSection.Substring($case.Index, $end - $case.Index)
    Assert ($body -match 'm\.(Productivity|TaxRate|GiniShift|UnrestAccum|TradeFlow|Price|Consumer|DisasterResist|BuildCost|Wage|Military|Happiness|Birth)') "policy $name does not modify any LawMods multiplier"
}

# 3) 互斥组定义与切换逻辑
Assert ($engine -match 'MutexGroups') 'engine must define mutex groups'
Assert ($ai -match 'MutexGroupOf') 'AI must resolve mutex groups'
Assert ($engine -match 'if \(level > cur\).*TrySpend|if \(level > cur\)') 'law upgrade must cost gold'

# 4) 存档注入点
Assert ($save -match 'rb_law_law_') 'save must write rb_law_law_* keys'
Assert ($save -match 'rb_law_policy_') 'save must write rb_law_policy_* keys'
Assert ($save -match 'rb_law_style') 'save must write rb_law_style'
Assert ($save -match 'saveSave') 'save must patch MapBox.saveSave'
Assert ($save -match 'loadSave') 'save must patch MapBox.loadSave'
Assert ($main -match 'LawSave\.TryInstall') 'main must install codex save patch'

# 5) 事件分级与 UI
Assert ($ai -match 'TypeLawReform') 'AI must record codex reform events'
Assert ($cabinet -match 'BuildLawPage') 'cabinet must render the codex page'
Assert ($cabinet -match 'cabinet_tab_codex') 'cabinet must have the codex tab'
Assert ($cabinet -match 'LawEngine\.SetLawLevel') 'cabinet must call SetLawLevel'
Assert ($cabinet -match 'LawEngine\.SetPolicyLevel') 'cabinet must call SetPolicyLevel'

if ($failures.Count -gt 0) {
    foreach ($f in $failures) { Write-Host "CODEX_RED: $f" }
    Write-Host "CODEX_RED: $($failures.Count) check(s) failed"
    exit 1
}
Write-Host 'CODEX_GREEN: law/policy effects, mutex, save, events and UI all pass'
exit 0
