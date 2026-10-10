param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

# 【W3】政权更替开关门禁
#
# 背景（真实 bug）：用户报告"明明禁止政权动荡，但还是会动荡、政权更替"。
# 根因：UnrestEngine.TriggerUnrest（普通暴动）无条件调用原版
# DiplomacyHelpersRebellion.startRebellion —— 原版叛乱会产生叛乱王国/分裂领土，
# 结局可能推翻统治者；而当时只有"街头起义"分支受开关控制。
#
# 本门禁锁定修复后的不变量：
#   ① 原版叛乱路径必须受政权更替总开关（RegimeChangeEnabled）门控
#   ② 街头起义必须同时要求 基尼崩溃开关 与 政权更替总开关
#   ③ 政策失败的政权变动必须前置 RegimeChangeEnabled 早退

$ErrorActionPreference = 'Stop'

$failures = New-Object System.Collections.Generic.List[string]
function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { $failures.Add($Message) }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)
$uePath = Join-Path $Root 'Core\UnrestEngine.cs'
$pePath = Join-Path $Root 'Core\PolicyEngine.cs'
$cfgPath = Join-Path $Root 'Models\UnrestConfig.cs'

foreach ($p in @($uePath, $pePath, $cfgPath)) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
        Write-Host "REGIME_SWITCH_RED: required file not found: $p"
        exit 1
    }
}

$ue = [System.IO.File]::ReadAllText($uePath, $utf8)
$pe = [System.IO.File]::ReadAllText($pePath, $utf8)
$cfg = [System.IO.File]::ReadAllText($cfgPath, $utf8)

# 开关默认值：两者默认必须为关（默认关是安全侧）
Assert-True ($cfg -match 'public bool RegimeChangeEnabled\s*=\s*false;') 'UnrestConfig.RegimeChangeEnabled 默认必须为 false'
Assert-True ($cfg -match 'public bool GiniCollapseEnabled\s*=\s*false;') 'UnrestConfig.GiniCollapseEnabled 默认必须为 false'

# ① 原版叛乱路径门控：startRebellion 必须出现在 allowRebellion 守卫块内
$unrestBody = [regex]::Match($ue, 'private static int TriggerUnrest\([\s\S]*?\n        \}').Value
Assert-True ($unrestBody.Length -gt 0) 'UnrestEngine.TriggerUnrest 未找到'
Assert-True ($unrestBody -match 'bool allowRebellion = cfg\.RegimeChangeEnabled;') 'TriggerUnrest 必须先取政权更替开关（allowRebellion）'
Assert-True ($unrestBody -match 'if \(allowRebellion && cities != null\)') '原版叛乱循环必须以 allowRebellion 为条件'
Assert-True ($unrestBody -match 'DiplomacyHelpersRebellion\.startRebellion') 'TriggerUnrest 应仍保留原版叛乱路径（受开关控制）'

# ② 街头起义：必须同时要求两个开关
Assert-True ($ue -match 'cfg\.GiniCollapseEnabled && cfg\.RegimeChangeEnabled') '街头起义分支必须同时要求 GiniCollapseEnabled 与 RegimeChangeEnabled'

# ③ 政策失败：政权变动前必须有 RegimeChangeEnabled 早退
Assert-True ($pe -match 'if \(!UnrestConfig\.Instance\.RegimeChangeEnabled\)') 'PolicyEngine 政策失败必须前置 RegimeChangeEnabled 早退'
$earlyReturn = $pe.IndexOf('if (!UnrestConfig.Instance.RegimeChangeEnabled)')
$abdicate = $pe.IndexOf('TryRemoveKing')
Assert-True ($earlyReturn -ge 0 -and $abdicate -gt $earlyReturn) 'RegimeChangeEnabled 早退必须出现在退位/驾崩/内战逻辑之前'

if ($failures.Count -gt 0) {
    foreach ($f in $failures) { Write-Host "REGIME_SWITCH_RED: $f" }
    Write-Host "REGIME_SWITCH_RED: $($failures.Count) check(s) failed"
    exit 1
}

Write-Host 'REGIME_SWITCH_GREEN: 政权更替总开关门控全部路径（原版叛乱 / 街头起义 / 政策失败）'
exit 0
