param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

# 自动内存清理扩展门禁：清理提示三通道（横幅/HUD/日志）、配置链路、字典重建缩容
# 仅挂空闲路径、EraEngine 死国清扫兜底、读档清 biome 缓存。任一断言失败即 RED。

$ErrorActionPreference = 'Stop'

$failures = New-Object System.Collections.Generic.List[string]

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { $failures.Add($Message) }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)
$configPath = Join-Path $Root 'default_config.json'
$mcePath = Join-Path $Root 'Core\MemoryCleanupEngine.cs'
$tswPath = Join-Path $Root 'Core\TradeSimulationWorker.cs'
$inheritPath = Join-Path $Root 'Core\InheritanceEngine.cs'
$collectorPath = Join-Path $Root 'Core\DataCollector.cs'
$trackerPath = Join-Path $Root 'Core\DamageTracker.cs'
$eraPath = Join-Path $Root 'Core\EraEngine.cs'
$mainPath = Join-Path $Root 'EconomyModMain.cs'
$hudPath = Join-Path $Root 'UI\EconomyHUD.cs'
$configCsPath = Join-Path $Root 'Models\UnrestConfig.cs'
$callbacksPath = Join-Path $Root 'Services\ConfigCallbacks.cs'
$localeDir = Join-Path $Root 'Locales'

foreach ($p in @($configPath, $mcePath, $tswPath, $inheritPath, $collectorPath, $trackerPath, $eraPath, $mainPath, $hudPath, $configCsPath, $callbacksPath)) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
        Write-Host "MEMORY_CLEANUP_NOTIFY_RED: required file not found: $p"
        exit 1
    }
}

$config = Get-Content -LiteralPath $configPath -Raw -Encoding UTF8 | ConvertFrom-Json
$mceText = [System.IO.File]::ReadAllText($mcePath, $utf8)
$tswText = [System.IO.File]::ReadAllText($tswPath, $utf8)
$inheritText = [System.IO.File]::ReadAllText($inheritPath, $utf8)
$collectorText = [System.IO.File]::ReadAllText($collectorPath, $utf8)
$trackerText = [System.IO.File]::ReadAllText($trackerPath, $utf8)
$eraText = [System.IO.File]::ReadAllText($eraPath, $utf8)
$mainText = [System.IO.File]::ReadAllText($mainPath, $utf8)
$hudText = [System.IO.File]::ReadAllText($hudPath, $utf8)
$ucText = [System.IO.File]::ReadAllText($configCsPath, $utf8)
$cbText = [System.IO.File]::ReadAllText($callbacksPath, $utf8)

# ===== 1. 配置链路：memory_cleanup_notify_enabled =====
$item = $config.economy_general | Where-Object { $_.Id -eq 'memory_cleanup_notify_enabled' }
Assert-True ($null -ne $item) 'default_config.json missing memory_cleanup_notify_enabled'
if ($null -ne $item) {
    Assert-True ($item.Type -eq 'SWITCH') 'memory_cleanup_notify_enabled must be SWITCH'
    Assert-True ($item.BoolVal -eq $true) 'memory_cleanup_notify_enabled default must be true'
    Assert-True ($item.Callback -eq 'EconomyConfigCallbacks:OnMemoryCleanupNotifyEnabledChanged') 'memory_cleanup_notify_enabled callback mismatch'
}
Assert-True ($ucText -match [regex]::Escape('public bool MemoryCleanupNotifyEnabled = true;')) 'UnrestConfig missing MemoryCleanupNotifyEnabled field'
$allIdsBlock = [regex]::Match($cbText, 'AllConfigIds\s*=\s*\{[^}]*\}').Value
Assert-True ($allIdsBlock -match '"memory_cleanup_notify_enabled"') 'ConfigCallbacks AllConfigIds missing memory_cleanup_notify_enabled'
Assert-True ($cbText -match 'u\.MemoryCleanupNotifyEnabled = \w+\.BoolVal') 'SyncFromModConfig missing BoolVal parse for MemoryCleanupNotifyEnabled'
Assert-True ($cbText -match [regex]::Escape('public static void OnMemoryCleanupNotifyEnabledChanged(bool pValue)')) 'callback OnMemoryCleanupNotifyEnabledChanged missing or wrong signature'
Assert-True ($cbText -match [regex]::Escape('UnrestConfig.Instance.MemoryCleanupNotifyEnabled = pValue;')) 'callback must assign UnrestConfig.Instance.MemoryCleanupNotifyEnabled'

# ===== 2. 四语言键：配置标签 + 提示/HUD 字符串 =====
$localeKeys = @(
    'memory_cleanup_notify_enabled', 'memory_cleanup_notify_enabled Description',
    'memory_cleanup_toast', 'hud_mem_cleanup', 'hud_mem_cleanup_pending', 'hud_mem_usage'
)
foreach ($loc in @('ch.json', 'en.json', 'zh_tw.json', 'ru.json')) {
    $locPath = Join-Path $localeDir $loc
    if (-not (Test-Path -LiteralPath $locPath -PathType Leaf)) {
        $failures.Add("locale file not found: $loc")
        continue
    }
    $null = Get-Content -LiteralPath $locPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $locText = [System.IO.File]::ReadAllText($locPath, $utf8)
    foreach ($k in $localeKeys) {
        Assert-True ($locText -match ('"' + [regex]::Escape($k) + '"')) "$loc missing locale key $k"
    }
}

# ===== 3. 清理提示三通道（MemoryCleanupEngine）=====
# 3a. 横幅受通知开关门控，且仅在释放量达到阈值或执行强制 GC 时弹出
Assert-True ($mceText -match 'MemoryCleanupNotifyEnabled') 'MemoryCleanupEngine must gate the toast on MemoryCleanupNotifyEnabled'
Assert-True ($mceText -match 'MinToastBytes') 'MemoryCleanupEngine must define a meaningful-free threshold (MinToastBytes)'
Assert-True ($mceText -match 'GameHelpers\.Notify') 'MemoryCleanupEngine must surface the toast via GameHelpers.Notify'
Assert-True ($mceText -match 'memory_cleanup_toast') 'toast text must come from the memory_cleanup_toast locale key'
# 3b. 释放量测量：清理前后 GC.GetTotalMemory
Assert-True ($mceText -match 'GetTotalMemory\(false\)') 'MemoryCleanupEngine must measure the managed heap via GC.GetTotalMemory(false)'
# 3c. GC.Collect 仍唯一且行内引用 MemoryCleanupForceGc（与 performance_audit 10a 一致）
$gcLines = @()
foreach ($line in ($mceText.Split("`n"))) { if ($line -match '\bGC\s*\.\s*Collect\s*\(') { $gcLines += $line } }
Assert-True ($gcLines.Count -eq 1 -and $gcLines[0] -match 'MemoryCleanupForceGc') 'MemoryCleanupEngine must keep exactly one GC.Collect gated on MemoryCleanupForceGc'
# 3d. 每次清理一条 Debug.Log
Assert-True ($mceText -match '自动内存清理') 'MemoryCleanupEngine must log one Debug line per cleanup'
# 3e. 忙碌时短延迟重试
Assert-True ($mceText -match 'BusyRetryDelaySeconds') 'MemoryCleanupEngine must define a busy-retry delay'
Assert-True (($mceText -match 'IsSettling') -and ($mceText -match 'IsBusy\(\)')) 'cleanup must stay gated on !IsSettling && !IsBusy'
# 3f. HUD 可读统计属性
foreach ($member in @('LastFreedBytes', 'LastShrunkCount', 'LastCleanupRealtime', 'ManagedHeapBytes', 'UnityUsedBytes', 'UnityReservedBytes')) {
    Assert-True ($mceText -match ('public static [^\r\n]*\b' + $member + '\b')) "MemoryCleanupEngine missing public stat $member"
}

# ===== 4. 字典重建缩容访问器：声明存在，且唯一调用方是 MemoryCleanupEngine =====
$pairs = @(
    @{ File = 'tsw';   Text = $tswText;      Gets = @('FlowCityRefsForTrim', 'ResidentOwedForTrim', 'ResidentPaidForTrim', 'EdgeCacheForTrim', 'KnownCityTopologyForTrim', 'AccScratchForTrim', 'CityIndexScratchForTrim', 'KingdomIndexScratchForTrim', 'BoatsScratchForTrim', 'SeaCapacityScratchForTrim') },
    @{ File = 'inh';   Text = $inheritText;  Gets = @('RecordsForTrim', 'AliveMapForTrim') },
    @{ File = 'col';   Text = $collectorText; Gets = @('CityRefsForTrim') },
    @{ File = 'trk';   Text = $trackerText;  Gets = @('DamageForTrim', 'PrevHealthForTrim', 'InactiveScansForTrim') }
)
$replaceNames = @()
foreach ($p in $pairs) {
    foreach ($g in $p.Gets) {
        Assert-True ($p.Text -match ('internal static [^\r\n]*\b' + $g + '\b')) "$($p.File) missing ForTrim accessor $g"
    }
}
# Replace 方法名清单（owner 文件内声明，唯一调用方是 MemoryCleanupEngine）
$replaceNames = @(
    'ReplaceFlowCityRefsForTrim', 'ReplaceResidentOwedForTrim', 'ReplaceResidentPaidForTrim',
    'ReplaceEdgeCacheForTrim', 'ReplaceKnownCityTopologyForTrim', 'ReplaceAccScratchForTrim',
    'ReplaceCityIndexScratchForTrim', 'ReplaceKingdomIndexScratchForTrim', 'ReplaceBoatsScratchForTrim',
    'ReplaceSeaCapacityScratchForTrim', 'ReplaceRecordsForTrim', 'ReplaceAliveMapForTrim',
    'ReplaceCityRefsForTrim', 'ReplaceDamageForTrim', 'ReplacePrevHealthForTrim', 'ReplaceInactiveScansForTrim'
)
$ownerFiles = @{
    'ReplaceFlowCityRefsForTrim' = $tswText; 'ReplaceResidentOwedForTrim' = $tswText; 'ReplaceResidentPaidForTrim' = $tswText
    'ReplaceEdgeCacheForTrim' = $tswText; 'ReplaceKnownCityTopologyForTrim' = $tswText; 'ReplaceAccScratchForTrim' = $tswText
    'ReplaceCityIndexScratchForTrim' = $tswText; 'ReplaceKingdomIndexScratchForTrim' = $tswText
    'ReplaceBoatsScratchForTrim' = $tswText; 'ReplaceSeaCapacityScratchForTrim' = $tswText
    'ReplaceRecordsForTrim' = $inheritText; 'ReplaceAliveMapForTrim' = $inheritText
    'ReplaceCityRefsForTrim' = $collectorText
    'ReplaceDamageForTrim' = $trackerText; 'ReplacePrevHealthForTrim' = $trackerText; 'ReplaceInactiveScansForTrim' = $trackerText
}
$productSources = @(Get-ChildItem -LiteralPath $Root -Filter '*.cs' -File -Recurse | Where-Object {
    $_.FullName -notmatch '[\\/](bin|obj|evidence|tools)[\\/]'
})
foreach ($name in $replaceNames) {
    Assert-True ($ownerFiles[$name] -match ('\b' + $name + '\b')) "owner file missing replace method $name"
    foreach ($f in $productSources) {
        if ($f.Name -ieq 'MemoryCleanupEngine.cs') { continue }
        $text = [System.IO.File]::ReadAllText($f.FullName, $utf8)
        if ($text -match ('\b' + $name + '\b')) {
            if (-not ($ownerFiles[$name] -eq $text)) {
                $failures.Add("$name referenced outside its owner file and MemoryCleanupEngine ($($f.Name))")
            }
        }
    }
}
Assert-True ($mceText -match 'CompactDict<') 'MemoryCleanupEngine must use the generic CompactDict rebuild helper'
Assert-True ($mceText -match 'MinCompactEntries') 'MemoryCleanupEngine must gate rebuilds on a minimum entry count'

# ===== 5. EraEngine 死国清扫兜底（Tick 内、与 EraEnabled 无关）=====
$eraTick = [regex]::Match($eraText, 'public static void Tick\(int currentYear\)(?<body>.*?)public static void Reset\(\)', [System.Text.RegularExpressions.RegexOptions]::Singleline).Value
Assert-True ($eraTick.Length -gt 0) 'EraEngine.Tick region not found'
Assert-True ($eraTick -match 'FindKingdom') 'EraEngine.Tick must sweep dead kingdoms via FindKingdom'
Assert-True ($eraTick -match '_prevAvg\.Remove') 'EraEngine.Tick sweep must remove stale _prevAvg entries'

# ===== 6. 读档时清 biome 缓存 =====
Assert-True ($mainText -match 'BiomeEconomy\.ClearCache\(\)') 'EconomyModMain must call BiomeEconomy.ClearCache on new map and on save load'

# ===== 7. HUD 内存状态行 =====
foreach ($k in @('hud_mem_cleanup', 'hud_mem_cleanup_pending', 'hud_mem_usage')) {
    Assert-True ($hudText -match [regex]::Escape($k)) "EconomyHUD missing usage of $k"
}
Assert-True ($hudText -match 'MemoryCleanupEnabled') 'EconomyHUD memory line must be gated on MemoryCleanupEnabled'
Assert-True ($hudText -match 'MemoryCleanupEngine\.ManagedHeapBytes') 'EconomyHUD must show the managed heap'

if ($failures.Count -gt 0) {
    foreach ($f in $failures) { Write-Host "MEMORY_CLEANUP_NOTIFY_RED: $f" }
    Write-Host "MEMORY_CLEANUP_NOTIFY_RED: $($failures.Count) consistency check(s) failed"
    exit 1
}

Write-Host 'MEMORY_CLEANUP_NOTIFY_GREEN: notify channels, config chain, trim accessors, era sweep and load-cache checks all pass'
exit 0
