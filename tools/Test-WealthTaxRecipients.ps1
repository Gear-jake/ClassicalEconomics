$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\DataCollector.cs'))
# Fused-pass invariant: the single collect pass fills both pools, the poor-empty guard
# fires BEFORE any rich deduction, and the tax loop charges from the rich pool only.
$pattern = 'poor\.Clear\(\).*rich\.Clear\(\).*foreach \(var actor in aliveList\).*if \(w < poorLine\) poor\.Add\(actor\);.*else if \(w > taxLine\) rich\.Add\(actor\);.*if \(poor\.Count == 0\) return;.*long totalTax = 0;.*foreach \(var actor in rich\).*actor\.addMoney\(-charged\)'
if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'WEALTH_TAX_RECIPIENTS_RED: tax can be deducted before any recipient exists'
    exit 1
}
Write-Host 'WEALTH_TAX_RECIPIENTS_GREEN: recipients are confirmed before deductions'
exit 0