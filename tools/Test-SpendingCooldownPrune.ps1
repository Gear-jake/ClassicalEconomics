$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\SpendingEngine.cs'))
$pattern = 'RunOncePerYear\(\).*PruneExpiredCityActions\(EconomyEngine\.CycleIndex\).*private static void PruneExpiredCityActions\(int cycle\).*PruneCooldowns\(_lastBuildCycle, cycle, BuildCooldownCycles\)'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'SPENDING_COOLDOWN_PRUNE_RED: stale city cooldowns depend on a successful action at cycle multiples'
    exit 1
}

Write-Host 'SPENDING_COOLDOWN_PRUNE_GREEN: stale city cooldowns prune once per annual run'
exit 0
