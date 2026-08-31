$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\EconomyCycleModulator.cs'))
$pattern = 'bool bubbleExceeded = bubbleThreshold > 0f && BubbleValue >= bubbleThreshold;.*_highGiniStreak >= cfg\.CycleGiniPeriods \|\|\s*bubbleExceeded'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'ZERO_GDP_CYCLE_RED: zero GDP is treated as an immediately burst bubble'
    exit 1
}

Write-Host 'ZERO_GDP_CYCLE_GREEN: empty economies do not trigger a zero-threshold bubble burst'
exit 0
