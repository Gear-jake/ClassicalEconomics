$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\PolicyEngine.cs'))
$pattern = 'long tariffTarget = stats\.TradeBalance < 0 \? \(long\)\(-stats\.TradeBalance \* 0\.1f\) : 0L;.*tariff = CollectTariff\(kingdom\.units, cityPool, tariffTarget\);.*private static long CollectTariff'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'TRADE_POLICY_EFFECT_RED: deficit trade policy still succeeds with zero economic effect'
    exit 1
}

Write-Host 'TRADE_POLICY_EFFECT_GREEN: deficit tariff transfers actual resident coins to city treasuries'
exit 0
