$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$helper = [System.IO.File]::ReadAllText((Join-Path $root 'Core\GameHelpers.cs'))
$collector = [System.IO.File]::ReadAllText((Join-Path $root 'Core\DataCollector.cs'))
$pattern = 'public static bool IsCivilizedActor\(Actor actor\).*actor\.city != null.*actor\.hasKingdom\(\).*actor\.kingdom != null.*return false;'

$helperMatches = [Regex]::IsMatch($helper, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)
$collectorUsesSharedPredicate = $collector -notmatch 'actor\.asset == null \|\| !actor\.asset\.civ'
if (-not $helperMatches -or -not $collectorUsesSharedPredicate) {
    Write-Host 'CIVILIZATION_ELIGIBILITY_RED: economy collection still relies on species civ flags'
    exit 1
}

Write-Host 'CIVILIZATION_ELIGIBILITY_GREEN: collection uses city-or-kingdom eligibility'
exit 0
