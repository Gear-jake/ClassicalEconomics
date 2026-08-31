$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\GameHelpers.cs'))
$pattern = 'wealth = a\.money \+ a\.loot;.*if \(float\.IsNaN\(wealth\) \|\| float\.IsInfinity\(wealth\)\).*wealth = 0f;.*return false;'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'WEALTH_FINITE_BOUNDARY_RED: non-finite wealth can poison every economic aggregate'
    exit 1
}

Write-Host 'WEALTH_FINITE_BOUNDARY_GREEN: non-finite wealth is rejected at the shared boundary'
exit 0
