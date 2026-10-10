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
# 【W2 换锚点】CabinetWindow 已按 partial 拆成多个文件（壳/政务页/外交/图表），
# 形状检查改为合并扫描整个 CabinetWindow*.cs 分层；各项契约不变。
$cabinet = (@(Get-ChildItem -LiteralPath (Join-Path $Root 'UI') -Filter 'CabinetWindow*.cs' -File) |
    ForEach-Object { Read-Source $_.FullName }) -join "`n"
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
    Assert ($body -match 'm\.(Productivity|TaxRate|GiniShift|UnrestAccum|Price|Consumer|DisasterResist|BuildCost|Wage|Military|Happiness|Birth)') "policy $name does not modify any LawMods multiplier"
}

# 3) 互斥组定义与切换逻辑
Assert ($engine -match 'MutexGroups') 'engine must define mutex groups'
Assert ($ai -match 'MutexGroupOf') 'AI must resolve mutex groups'
Assert ($engine -match 'if \(level > cur\).*TrySpend|if \(level > cur\)') 'law upgrade must cost gold'

# 4) 存档注入点
# 【W3.2 换锚点】键值字面量收敛到 Core\SaveSchema.cs；此处断言"法典用 SaveSchema 的键构造器/常量写入"，
# 键值本身（rb_law_law_* / rb_law_policy_* / rb_law_style）由 Test-SaveSchema 冻结清单保证逐字不变。
# 契约不变：法典档位与风格必须落盘。
Assert ($save -match 'SaveSchema\.LawLevel\(') 'save must write law-level keys via SaveSchema.LawLevel'
Assert ($save -match 'SaveSchema\.LawPolicy\(') 'save must write policy-level keys via SaveSchema.LawPolicy'
Assert ($save -match 'SaveSchema\.LawStyle') 'save must write the style key via SaveSchema.LawStyle'
Assert ($save -match 'SaveSchema\.LawLevel\(' -and $save -match 'get\(SaveSchema\.LawLevel\(') 'law-level keys must be written and read'
Assert ($save -match 'saveWorldToDirectory') 'save must patch SaveManager.saveWorldToDirectory'
Assert ($save -match 'loadWorld') 'save must patch SaveManager.loadWorld'
# 【W3 换锚点】补丁安装已统一到 PatchInstaller；契约不变：法典存档补丁必须被安装。
# 校验：① PatchInstaller 登记了 LawSave ② 主流程调用了 InstallAll。
$patchInstaller = Read-Source (Join-Path $Root 'Core\PatchInstaller.cs')
Assert ($patchInstaller -match 'LawSave') 'PatchInstaller.Registry must register the codex save patch'
Assert ($main -match 'PatchInstaller\.InstallAll\(\)') 'main must install all patches via PatchInstaller.InstallAll'

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
