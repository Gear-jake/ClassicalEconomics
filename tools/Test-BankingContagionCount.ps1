$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\BankingEngine.cs'))
$pattern = 'bool firstContagion = !_contagionLossByKingdom\.TryGetValue\(kvp\.Key, out accumulatedLoss\);.*if \(firstContagion\) LastContagions\+\+;'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'BANKING_CONTAGION_COUNT_RED: repeated partners inflate the affected-kingdom count'
    exit 1
}

Write-Host 'BANKING_CONTAGION_COUNT_GREEN: affected kingdoms are counted once'
exit 0
