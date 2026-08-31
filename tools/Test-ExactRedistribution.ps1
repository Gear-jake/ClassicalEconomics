$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\GameHelpers.cs'))

if ($source -match 'const int MaxScan|scanned\s*>\s*MaxScan') {
    Write-Host 'EXACT_REDISTRIBUTION_RED: kingdom redistribution still samples a prefix'
    exit 1
}

Write-Host 'EXACT_REDISTRIBUTION_GREEN: all eligible kingdom actors are considered'
exit 0
