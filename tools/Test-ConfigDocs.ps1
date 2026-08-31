param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'

$failures = New-Object System.Collections.Generic.List[string]

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { $failures.Add($Message) }
}

$configPath = Join-Path $Root 'default_config.json'
$configCsPath = Join-Path $Root 'Models\UnrestConfig.cs'
$callbacksPath = Join-Path $Root 'Services\ConfigCallbacks.cs'
$hudPath = Join-Path $Root 'UI\EconomyHUD.cs'
$localeDir = Join-Path $Root 'Locales'
$readmePath = Join-Path $Root 'README.md'
$readmeEnPath = Join-Path $Root 'README_en.md'

foreach ($p in @($configPath, $configCsPath, $callbacksPath, $hudPath, $readmePath, $readmeEnPath)) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
        Write-Host "CONFIG_DOCS_RED: required file not found: $p"
        exit 1
    }
}

# JSON parse (fail-closed on malformed files)
$config = Get-Content -LiteralPath $configPath -Raw -Encoding UTF8 | ConvertFrom-Json
foreach ($loc in @('ch.json', 'en.json', 'zh_tw.json', 'ru.json')) {
    $locPath = Join-Path $localeDir $loc
    if (-not (Test-Path -LiteralPath $locPath -PathType Leaf)) {
        Write-Host "CONFIG_DOCS_RED: locale file not found: $locPath"
        exit 1
    }
    $null = Get-Content -LiteralPath $locPath -Raw -Encoding UTF8 | ConvertFrom-Json
}

# Performance config keys (five-way consistency: config entry, runtime field,
# callbacks AllConfigIds + bounded ParseInt + callback method, 4 locales, READMEs).
$keys = @(
    @{ Id = 'real_time_refresh_threshold';      Field = 'RealTimeRefreshThreshold';      Type = 'TEXT';   Default = '2000';  Callback = 'OnRealTimeThresholdChanged';        Min = 100;    Max = 100000 },
    @{ Id = 'real_time_refresh_budget';         Field = 'RealTimeRefreshBudget';         Type = 'TEXT';   Default = '2000';  Callback = 'OnRealTimeBudgetChanged';           Min = 100;    Max = 100000 },
    @{ Id = 'spending_cap_per_year';            Field = 'SpendingCapPerYear';            Type = 'TEXT';   Default = '5000';  Callback = 'OnSpendingCapPerYearChanged';       Min = 1;      Max = 100000 },
    @{ Id = 'banking_default_cap_per_year';     Field = 'BankingDefaultCapPerYear';      Type = 'TEXT';   Default = '500';   Callback = 'OnBankingDefaultCapPerYearChanged'; Min = 1;      Max = 100000 },
    @{ Id = 'banking_contagion_cap_per_year';   Field = 'BankingContagionCapPerYear';    Type = 'TEXT';   Default = '500';   Callback = 'OnBankingContagionCapPerYearChanged'; Min = 1; Max = 100000 },
    @{ Id = 'inheritance_scan_per_frame';       Field = 'InheritanceScanPerFrame';       Type = 'TEXT';   Default = '2000';  Callback = 'OnInheritanceScanPerFrameChanged';  Min = 1;      Max = 100000 },
    @{ Id = 'frame_budget_ms';                  Field = 'FrameBudgetMs';                 Type = 'TEXT';   Default = '4';     Callback = 'OnFrameBudgetChanged';              Min = 1;      Max = 100 },
    @{ Id = 'cycle_window_ms';                  Field = 'CycleWindowMs';                 Type = 'TEXT';   Default = '2000';  Callback = 'OnCycleWindowChanged';              Min = 100;    Max = 10000 },
    @{ Id = 'perf_diagnostics_enabled';         Field = 'PerfDiagnosticsEnabled';        Type = 'SWITCH'; Default = 'false'; Callback = 'OnPerfDiagnosticsEnabledChanged';  Min = $null;  Max = $null },
    @{ Id = 'cycle_alloc_budget';               Field = 'CycleAllocBudget';              Type = 'TEXT';   Default = '4096';  Callback = 'OnCycleAllocBudgetChanged';         Min = 1;      Max = 1048576 },
    @{ Id = 'memory_cleanup_enabled';           Field = 'MemoryCleanupEnabled';          Type = 'SWITCH'; Default = 'true';  Callback = 'OnMemoryCleanupEnabledChanged';      Min = $null;  Max = $null },
    @{ Id = 'memory_cleanup_force_gc';          Field = 'MemoryCleanupForceGc';         Type = 'SWITCH'; Default = 'false'; Callback = 'OnMemoryCleanupForceGcChanged';     Min = $null;  Max = $null },
    @{ Id = 'memory_cleanup_interval_seconds';  Field = 'MemoryCleanupIntervalSeconds'; Type = 'TEXT';   Default = '30';    Callback = 'OnMemoryCleanupIntervalChanged';     Min = 5;      Max = 300 },
    @{ Id = 'memory_cleanup_notify_enabled';    Field = 'MemoryCleanupNotifyEnabled';   Type = 'SWITCH'; Default = 'true';  Callback = 'OnMemoryCleanupNotifyEnabledChanged'; Min = $null; Max = $null },
    @{ Id = 'nation_play_enabled';              Field = 'NationPlayEnabled';            Type = 'SWITCH'; Default = 'true';  Callback = 'OnNationPlayEnabledChanged';          Min = $null; Max = $null },
    @{ Id = 'treasury_income_ratio';            Field = 'TreasuryIncomeRatio';          Type = 'TEXT';   Default = '5';     Callback = 'OnTreasuryIncomeRatioChanged';        Min = 1;     Max = 20 },
    @{ Id = 'policy_slots';                     Field = 'PolicySlots';                  Type = 'TEXT';   Default = '3';     Callback = 'OnPolicySlotsChanged';                Min = 1;     Max = 5 },
    @{ Id = 'trade_astar_enabled';              Field = 'TradeAstarEnabled';            Type = 'SWITCH'; Default = 'true';  Callback = 'OnTradeAstarEnabledChanged';          Min = $null; Max = $null },
    @{ Id = 'nation_claim_hotkey';              Field = 'NationClaimHotkey';            Type = 'STRING';   Default = 'G';     Callback = 'OnNationClaimHotkeyChanged';          Min = $null; Max = $null }
)

$group = $config.economy_general
$utf8 = New-Object System.Text.UTF8Encoding($false)
$cbText = [System.IO.File]::ReadAllText($callbacksPath, $utf8)
$ucText = [System.IO.File]::ReadAllText($configCsPath, $utf8)
$allIdsBlock = [regex]::Match($cbText, 'AllConfigIds\s*=\s*\{[^}]*\}').Value

foreach ($k in $keys) {
    $item = $group | Where-Object { $_.Id -eq $k.Id }
    Assert-True ($null -ne $item) "default_config.json missing config key $($k.Id)"
    if ($null -eq $item) { continue }

    # STRING（文本）配置在 NML 中仍以 TEXT 类型声明；仅要求 SWITCH 精确匹配
    if ($k.Type -eq 'STRING') {
        Assert-True ($item.Type -eq 'TEXT') "$($k.Id) must be declared as TEXT in default_config.json (STRING is a docs-gate type"
    } else {
        Assert-True ($item.Type -eq $k.Type) "$($k.Id) Type must be $($k.Type), got $($item.Type)"
    }
    Assert-True ($item.Callback -eq "EconomyConfigCallbacks:$($k.Callback)") "$($k.Id) callback must be EconomyConfigCallbacks:$($k.Callback)"
    if ($k.Type -eq 'SWITCH') {
        $expected = [bool]::Parse($k.Default)
        Assert-True ($item.BoolVal -eq $expected) "$($k.Id) default must be $($k.Default)"
        Assert-True ($null -eq $item.TextVal) "$($k.Id) must not carry TextVal for a SWITCH entry"
    } else {
        Assert-True ($item.TextVal -eq $k.Default) "$($k.Id) default must be $($k.Default), got $($item.TextVal)"
    }

    if ($k.Type -eq 'STRING') {
        Assert-True ($ucText -match [regex]::Escape('public string ' + $k.Field + ' = "' + $k.Default + '";')) "UnrestConfig.cs missing string field declaration for $($k.Field)"
        Assert-True ($cbText -match ("u\.$($k.Field) = ")) "SyncFromModConfig missing assignment for $($k.Field)"
        Assert-True ($cbText -match [regex]::Escape("public static void $($k.Callback)(string pValue)")) "callback $($k.Callback) missing or wrong signature"
        Assert-True ($cbText -match ("UnrestConfig\.Instance\.$($k.Field) = ")) "callback $($k.Callback) must assign UnrestConfig.Instance.$($k.Field)"
    }

    Assert-True ($allIdsBlock -match [regex]::Escape('"' + $k.Id + '"')) "ConfigCallbacks.cs AllConfigIds missing $($k.Id)"
    if ($k.Type -eq 'SWITCH') {
        Assert-True ($ucText -match [regex]::Escape("public bool $($k.Field) = $($k.Default);")) "UnrestConfig.cs missing field declaration for $($k.Field)"
        Assert-True ($cbText -match "u\.$($k.Field) = \w+\.BoolVal") "SyncFromModConfig missing BoolVal parse for $($k.Field)"
        Assert-True ($cbText -match [regex]::Escape("public static void $($k.Callback)(bool pValue)")) "callback $($k.Callback) missing or wrong signature"
        Assert-True ($cbText -match [regex]::Escape("UnrestConfig.Instance.$($k.Field) = pValue;")) "callback $($k.Callback) must assign UnrestConfig.Instance.$($k.Field)"
    } elseif ($k.Type -eq 'TEXT') {
        Assert-True ($ucText -match [regex]::Escape("public int $($k.Field) = $($k.Default);")) "UnrestConfig.cs missing field declaration for $($k.Field)"
        Assert-True ($cbText -match "u\.$($k.Field) = ParseInt\(\w+\.TextVal, u\.$($k.Field), $($k.Min), $($k.Max)\)") "SyncFromModConfig must parse $($k.Id) via bounded ParseInt($($k.Min)..$($k.Max))"
        Assert-True ($cbText -match [regex]::Escape("public static void $($k.Callback)(string pValue)")) "callback $($k.Callback) missing or wrong signature"
        Assert-True ($cbText -match [regex]::Escape("UnrestConfig.Instance.$($k.Field) = ParseInt(pValue, UnrestConfig.Instance.$($k.Field), $($k.Min), $($k.Max));")) "callback $($k.Callback) must use bounded ParseInt($($k.Min)..$($k.Max))"
    } elseif ($k.Type -eq 'STRING') {
        Assert-True ($ucText -match [regex]::Escape('public string ' + $k.Field + ' = "' + $k.Default + '";')) "UnrestConfig.cs missing string field declaration for $($k.Field)"
        Assert-True ($cbText -match ("u\.$($k.Field) = ")) "SyncFromModConfig missing assignment for $($k.Field)"
        Assert-True ($cbText -match [regex]::Escape("public static void $($k.Callback)(string pValue)")) "callback $($k.Callback) missing or wrong signature"
        Assert-True ($cbText -match ("UnrestConfig\.Instance\.$($k.Field) = ")) "callback $($k.Callback) must assign UnrestConfig.Instance.$($k.Field)"
    }
}

# Locale coverage: config keys need both the label and the Description entry.
foreach ($loc in @('ch.json', 'en.json', 'zh_tw.json', 'ru.json')) {
    $locText = [System.IO.File]::ReadAllText((Join-Path $localeDir $loc), $utf8)
    foreach ($k in $keys) {
        Assert-True ($locText -match ('"' + [regex]::Escape($k.Id) + '"')) "$loc missing locale key $($k.Id)"
        Assert-True ($locText -match ('"' + [regex]::Escape($k.Id + ' Description') + '"')) "$loc missing locale key $($k.Id) Description"
    }
}

# Settling markers: locale-only UI strings (not config keys), used by EconomyHUD.
$settlingKeys = @('settling_marker', 'settling_hint')
$hudText = [System.IO.File]::ReadAllText($hudPath, $utf8)
foreach ($s in $settlingKeys) {
    foreach ($loc in @('ch.json', 'en.json', 'zh_tw.json', 'ru.json')) {
        $locText = [System.IO.File]::ReadAllText((Join-Path $localeDir $loc), $utf8)
        Assert-True ($locText -match ('"' + [regex]::Escape($s) + '"')) "$loc missing locale key $s"
    }
    Assert-True ($hudText -match ('L\("' + [regex]::Escape($s) + '"\)')) "EconomyHUD.cs missing usage of $s"
    Assert-True ($null -eq ($group | Where-Object { $_.Id -eq $s })) "$s must be a locale-only UI string, not a config key"
}

# README documentation (both languages): performance section headers + every key.
$readmeText = [System.IO.File]::ReadAllText($readmePath, $utf8)
$readmeEnText = [System.IO.File]::ReadAllText($readmeEnPath, $utf8)
Assert-True ($readmeText -match '### 性能分组（年度收尾治理）') 'README.md missing performance section header'
Assert-True ($readmeEnText -match '### Performance group \(annual closeout\)') 'README_en.md missing performance section header'
foreach ($k in $keys) {
    Assert-True ($readmeText -match [regex]::Escape('`' + $k.Id + '`')) "README.md missing documentation for $($k.Id)"
    Assert-True ($readmeEnText -match [regex]::Escape('`' + $k.Id + '`')) "README_en.md missing documentation for $($k.Id)"
}
foreach ($s in $settlingKeys) {
    Assert-True ($readmeText -match [regex]::Escape('`' + $s + '`')) "README.md missing settling marker $s"
    Assert-True ($readmeEnText -match [regex]::Escape('`' + $s + '`')) "README_en.md missing settling marker $s"
}

if ($failures.Count -gt 0) {
    foreach ($f in $failures) { Write-Host "CONFIG_DOCS_RED: $f" }
    Write-Host "CONFIG_DOCS_RED: $($failures.Count) consistency check(s) failed"
    exit 1
}

Write-Host 'CONFIG_DOCS_GREEN: all performance config keys consistent across config, runtime, callbacks, locales and READMEs'
exit 0