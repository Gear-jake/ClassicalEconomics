$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\PolicyEngine.cs'))
$pattern = 'private static long AddGoldToCity\(City city, long amount\).*long added = 0L;.*try \{ city\.addResourcesToRandomStockpile\("gold", give\); \}.*catch \(System\.Exception\) \{ break; \}.*return added;'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'TRADE_POLICY_FAILURE_RED: one city stockpile failure aborts the annual policy chain'
    exit 1
}

Write-Host 'TRADE_POLICY_FAILURE_GREEN: city deposit failures are isolated'
exit 0
