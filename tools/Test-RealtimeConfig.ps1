param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'

$jsonPath = Join-Path $Root 'default_config.json'
$cbPath = Join-Path $Root 'Services\ConfigCallbacks.cs'
$localeDir = Join-Path $Root 'Locales'

if (-not (Test-Path -LiteralPath $jsonPath -PathType Leaf)) {
    Write-Host "REALTIME_CONFIG_RED: config file not found: $jsonPath"
    exit 1
}
if (-not (Test-Path -LiteralPath $cbPath -PathType Leaf)) {
    Write-Host "REALTIME_CONFIG_RED: config callbacks file not found: $cbPath"
    exit 1
}

# 1) Both keys present in default_config.json, TEXT type, default 2000, wired callbacks.
$config = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
$group = $config.economy_general
foreach ($entry in @(
    @{ Id = 'real_time_refresh_threshold'; Callback = 'OnRealTimeThresholdChanged' },
    @{ Id = 'real_time_refresh_budget';    Callback = 'OnRealTimeBudgetChanged' }
)) {
    $item = $group | Where-Object { $_.Id -eq $entry.Id }
    if (-not $item) {
        Write-Host "REALTIME_CONFIG_RED: default_config.json missing config key $($entry.Id)"
        exit 1
    }
    if ($item.Type -ne 'TEXT') {
        Write-Host "REALTIME_CONFIG_RED: $($entry.Id) must be a TEXT config entry"
        exit 1
    }
    if ($item.TextVal -ne '2000') {
        Write-Host "REALTIME_CONFIG_RED: $($entry.Id) default must be 2000"
        exit 1
    }
    if ($item.Callback -ne ("EconomyConfigCallbacks:" + $entry.Callback)) {
        Write-Host "REALTIME_CONFIG_RED: $($entry.Id) callback must be EconomyConfigCallbacks:$($entry.Callback)"
        exit 1
    }
}

# 2) SyncFromModConfig parses both keys with bounded ParseInt (100..100000).
$cb = [System.IO.File]::ReadAllText($cbPath)
if ($cb -notmatch 'u\.RealTimeRefreshThreshold = ParseInt\(rtt\.TextVal, u\.RealTimeRefreshThreshold, 100, 100000\)') {
    Write-Host 'REALTIME_CONFIG_RED: SyncFromModConfig must parse real_time_refresh_threshold via bounded ParseInt'
    exit 1
}
if ($cb -notmatch 'u\.RealTimeRefreshBudget = ParseInt\(rtb\.TextVal, u\.RealTimeRefreshBudget, 100, 100000\)') {
    Write-Host 'REALTIME_CONFIG_RED: SyncFromModConfig must parse real_time_refresh_budget via bounded ParseInt'
    exit 1
}

# 3) NML setting-window callback methods exist and are bounded the same way.
if ($cb -notmatch 'public static void OnRealTimeThresholdChanged\(string pValue\)') {
    Write-Host 'REALTIME_CONFIG_RED: OnRealTimeThresholdChanged callback missing'
    exit 1
}
if ($cb -notmatch 'public static void OnRealTimeBudgetChanged\(string pValue\)') {
    Write-Host 'REALTIME_CONFIG_RED: OnRealTimeBudgetChanged callback missing'
    exit 1
}

# 4) Both ids registered for settings-window localization.
if ($cb -notmatch '"real_time_refresh_threshold", "real_time_refresh_budget"') {
    Write-Host 'REALTIME_CONFIG_RED: AllConfigIds must register real_time_refresh_threshold and real_time_refresh_budget'
    exit 1
}

# 5) All four locale files carry labels and descriptions for both ids (no missing text).
foreach ($loc in @('ch.json', 'en.json', 'zh_tw.json', 'ru.json')) {
    $locPath = Join-Path $localeDir $loc
    if (-not (Test-Path -LiteralPath $locPath -PathType Leaf)) {
        Write-Host "REALTIME_CONFIG_RED: locale file not found: $locPath"
        exit 1
    }
    $text = [System.IO.File]::ReadAllText($locPath)
    foreach ($key in @(
        'real_time_refresh_threshold', 'real_time_refresh_threshold Description',
        'real_time_refresh_budget', 'real_time_refresh_budget Description'
    )) {
        if ($text -notmatch ('"' + [regex]::Escape($key) + '"')) {
            Write-Host "REALTIME_CONFIG_RED: $loc missing locale key $key"
            exit 1
        }
    }
}

Write-Host 'REALTIME_CONFIG_GREEN: threshold and budget parsed from config with bounded callbacks and locales'
exit 0