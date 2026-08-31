$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'EconomyModMain.cs'))
$pattern = 'private int _pendingYear = -1;.*if \(RunOneCycle\(currentYear\)\) _lastCollectedYear = currentYear;.*private bool RunOneCycle\(int year\).*if \(_cyclePending\) _pendingYear = year;.*int year = _pendingYear >= 0 \? _pendingYear : GetCurrentGameYear\(\);.*_pendingYear = -1;'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'PENDING_CYCLE_YEAR_RED: completed data is labeled with consumption year instead of submission year'
    exit 1
}

Write-Host 'PENDING_CYCLE_YEAR_GREEN: snapshots retain the submitted game year'
exit 0
