param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'

$spendingPath = Join-Path $Root 'Core\SpendingEngine.cs'
$bankingPath = Join-Path $Root 'Core\BankingEngine.cs'
$configPath = Join-Path $Root 'Models\UnrestConfig.cs'
$cbPath = Join-Path $Root 'Services\ConfigCallbacks.cs'
$jsonPath = Join-Path $Root 'default_config.json'
$localeDir = Join-Path $Root 'Locales'

foreach ($p in @($spendingPath, $bankingPath, $configPath, $cbPath, $jsonPath)) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
        Write-Host "SPENDING_BANK_CAP_RED: source file not found: $p"
        exit 1
    }
}

$spending = [System.IO.File]::ReadAllText($spendingPath)
$banking = [System.IO.File]::ReadAllText($bankingPath)
$config = [System.IO.File]::ReadAllText($configPath)
$cbText = [System.IO.File]::ReadAllText($cbPath)

# 1) SpendingEngine.RunOncePerYear bounds the wealthy-actor loop by a configurable per-year cap.
if ($spending -notmatch 'int cap = UnrestConfig\.Instance\.SpendingCapPerYear;') {
    Write-Host 'SPENDING_BANK_CAP_RED: SpendingEngine must read the per-year spending cap from config (SpendingCapPerYear)'
    exit 1
}
if (-not [Regex]::IsMatch($spending, 'foreach \(var actor in actors\)\s*\{\s*if \(\+\+processed > cap\) break;', [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'SPENDING_BANK_CAP_RED: SpendingEngine.RunOncePerYear must break out of the actor loop when the annual cap is exceeded'
    exit 1
}

# 2) BankingEngine bounds the kingdom default/credit loop by a configurable cap.
if (-not [Regex]::IsMatch($banking, 'int kingdomsProcessed = 0;.*int defaultCap = cfg\.BankingDefaultCapPerYear;.*foreach \(var kingdom in kingdoms\)\s*\{\s*if \(\+\+kingdomsProcessed > defaultCap\) break;', [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'SPENDING_BANK_CAP_RED: BankingEngine.Evaluate must break out of the kingdom default/credit loop when the annual cap is exceeded'
    exit 1
}

# 3) BankingEngine bounds the contagion partner loop by a configurable cap.
if (-not [Regex]::IsMatch($banking, 'int contagionChecked = 0;.*int contagionCap = cfg\.BankingContagionCapPerYear;.*foreach \(var kvp in kingdomStats\)\s*\{\s*if \(\+\+contagionChecked > contagionCap\) break;', [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'SPENDING_BANK_CAP_RED: BankingEngine contagion loop must break when the annual contagion cap is exceeded'
    exit 1
}

# 4) Runtime defaults declared in UnrestConfig (defaults preserve current-behavior scale).
if ($config -notmatch 'public int SpendingCapPerYear = 5000;') {
    Write-Host 'SPENDING_BANK_CAP_RED: UnrestConfig must declare public int SpendingCapPerYear = 5000;'
    exit 1
}
if ($config -notmatch 'public int BankingDefaultCapPerYear = 500;') {
    Write-Host 'SPENDING_BANK_CAP_RED: UnrestConfig must declare public int BankingDefaultCapPerYear = 500;'
    exit 1
}
if ($config -notmatch 'public int BankingContagionCapPerYear = 500;') {
    Write-Host 'SPENDING_BANK_CAP_RED: UnrestConfig must declare public int BankingContagionCapPerYear = 500;'
    exit 1
}

# 5) default_config.json keys: TEXT, exact defaults, wired callbacks.
$json = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
$group = $json.economy_general
if (-not $group) {
    Write-Host 'SPENDING_BANK_CAP_RED: default_config.json must expose an economy_general group'
    exit 1
}
foreach ($entry in @(
    @{ Id = 'spending_cap_per_year'; TextVal = '5000'; Callback = 'OnSpendingCapPerYearChanged' },
    @{ Id = 'banking_default_cap_per_year'; TextVal = '500'; Callback = 'OnBankingDefaultCapPerYearChanged' },
    @{ Id = 'banking_contagion_cap_per_year'; TextVal = '500'; Callback = 'OnBankingContagionCapPerYearChanged' }
)) {
    $item = $group | Where-Object { $_.Id -eq $entry.Id }
    if (-not $item) {
        Write-Host "SPENDING_BANK_CAP_RED: default_config.json missing config key $($entry.Id)"
        exit 1
    }
    if ($item.Type -ne 'TEXT') {
        Write-Host "SPENDING_BANK_CAP_RED: $($entry.Id) must be a TEXT config entry"
        exit 1
    }
    if ($item.TextVal -ne $entry.TextVal) {
        Write-Host "SPENDING_BANK_CAP_RED: $($entry.Id) default must be $($entry.TextVal)"
        exit 1
    }
    if ($item.Callback -ne ("EconomyConfigCallbacks:" + $entry.Callback)) {
        Write-Host "SPENDING_BANK_CAP_RED: $($entry.Id) callback must be EconomyConfigCallbacks:$($entry.Callback)"
        exit 1
    }
}

# 6) SyncFromModConfig bounded ParseInt wiring.
if ($cbText -notmatch 'u\.SpendingCapPerYear = ParseInt\(scp\.TextVal, u\.SpendingCapPerYear, 1, 100000\)') {
    Write-Host 'SPENDING_BANK_CAP_RED: SyncFromModConfig must parse spending_cap_per_year via bounded ParseInt'
    exit 1
}
if ($cbText -notmatch 'u\.BankingDefaultCapPerYear = ParseInt\(bdc\.TextVal, u\.BankingDefaultCapPerYear, 1, 100000\)') {
    Write-Host 'SPENDING_BANK_CAP_RED: SyncFromModConfig must parse banking_default_cap_per_year via bounded ParseInt'
    exit 1
}
if ($cbText -notmatch 'u\.BankingContagionCapPerYear = ParseInt\(bcc\.TextVal, u\.BankingContagionCapPerYear, 1, 100000\)') {
    Write-Host 'SPENDING_BANK_CAP_RED: SyncFromModConfig must parse banking_contagion_cap_per_year via bounded ParseInt'
    exit 1
}

# 7) Setting-window callbacks exist.
foreach ($m in @('OnSpendingCapPerYearChanged', 'OnBankingDefaultCapPerYearChanged', 'OnBankingContagionCapPerYearChanged')) {
    if ($cbText -notmatch "public static void $m\(string pValue\)") {
        Write-Host "SPENDING_BANK_CAP_RED: $m callback missing"
        exit 1
    }
}

# 8) AllConfigIds registration.
if ($cbText -notmatch '"spending_cap_per_year", "banking_default_cap_per_year", "banking_contagion_cap_per_year"') {
    Write-Host 'SPENDING_BANK_CAP_RED: AllConfigIds must register spending_cap_per_year, banking_default_cap_per_year, banking_contagion_cap_per_year'
    exit 1
}

# 9) Locale labels in all four locale files.
foreach ($loc in @('ch.json', 'en.json', 'zh_tw.json', 'ru.json')) {
    $locPath = Join-Path $localeDir $loc
    if (-not (Test-Path -LiteralPath $locPath -PathType Leaf)) {
        Write-Host "SPENDING_BANK_CAP_RED: locale file not found: $locPath"
        exit 1
    }
    $text = [System.IO.File]::ReadAllText($locPath)
    foreach ($key in @(
        'spending_cap_per_year', 'spending_cap_per_year Description',
        'banking_default_cap_per_year', 'banking_default_cap_per_year Description',
        'banking_contagion_cap_per_year', 'banking_contagion_cap_per_year Description'
    )) {
        if ($text -notmatch ('"' + [regex]::Escape($key) + '"')) {
            Write-Host "SPENDING_BANK_CAP_RED: $loc missing locale key $key"
            exit 1
        }
    }
}

Write-Host 'SPENDING_BANK_CAP_GREEN: annual spending and banking default/contagion loops are config-capped with defaults at current-behavior scale'
exit 0