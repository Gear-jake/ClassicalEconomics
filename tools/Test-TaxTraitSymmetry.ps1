$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$cycle = [System.IO.File]::ReadAllText((Join-Path $root 'Core\EconomyCycleModulator.cs'))
$policy = [System.IO.File]::ReadAllText((Join-Path $root 'Core\PolicyEngine.cs'))
$cyclePattern = 'TaxLocalHigh = "tax_rate_local_high";.*foreach \(var kingdom in GameHelpers\.KingdomSnapshot\(\)\).*SetTrait\(kingdom, TaxLocalHigh, false\);.*if \(!localLow\).*SetTrait\(kingdom, TaxLocalLow, false\);'
$policyPattern = 'if \(isBoom\).*removeTrait\("tax_rate_local_high"\).*else.*removeTrait\("tax_rate_local_low"\)'

$cycleMatches = [Regex]::IsMatch($cycle, $cyclePattern, [Text.RegularExpressions.RegexOptions]::Singleline)
$policyMatches = [Regex]::IsMatch($policy, $policyPattern, [Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $cycleMatches -or -not $policyMatches) {
    Write-Host 'TAX_TRAIT_SYMMETRY_RED: low and high tax traits can remain active together or outlive their phase'
    exit 1
}

Write-Host 'TAX_TRAIT_SYMMETRY_GREEN: fiscal and cycle policies remove opposite tax traits'
exit 0
