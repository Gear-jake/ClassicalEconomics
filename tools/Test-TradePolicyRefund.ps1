$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\PolicyEngine.cs'))
$pattern = 'actor\.addMoney\(-charge\);.*deposited \+= AddGoldToCity.*if \(deposited < charge\) GameHelpers\.AddPositiveMoney\(actor, charge - deposited\);.*collected \+= deposited;'
if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'TRADE_POLICY_REFUND_RED: uncredited tariff is not refunded to the payer'
    exit 1
}
Write-Host 'TRADE_POLICY_REFUND_GREEN: only credited tariff remains deducted'
exit 0
