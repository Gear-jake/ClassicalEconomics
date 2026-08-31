$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'EconomyModMain.cs'))
$pattern = 'if \(currentYear != _lastCollectedYear\)\s*\{\s*if \(RunOneCycle\(currentYear\)\) _lastCollectedYear = currentYear;\s*\}'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'ANNUAL_CYCLE_RETRY_RED: failed collection still consumes the game year'
    exit 1
}

Write-Host 'ANNUAL_CYCLE_RETRY_GREEN: year commits only after successful submission'
exit 0
