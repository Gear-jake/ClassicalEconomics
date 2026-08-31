$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\TradeSimulationWorker.cs'))
$pattern = 'int idx = _cycleIndex \+ 1;.*bool queued = ThreadPool\.QueueUserWorkItem.*if \(!queued\) throw new InvalidOperationException.*_cycleIndex = idx;.*return true;'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'CYCLE_INDEX_COMMIT_RED: rejected worker submission still advances the cycle index'
    exit 1
}

Write-Host 'CYCLE_INDEX_COMMIT_GREEN: cycle index commits only after queue acceptance'
exit 0
