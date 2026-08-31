$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\EconomyCycleModulator.cs'))
$pattern = 'foreach \(var kingdom in GameHelpers\.KingdomSnapshot\(\)\).*if \(!localLow\) SetTrait\(kingdom, TaxLocalLow, false\);.*if \(!localLow\).*return;.*var top = EconomyEngine\.TopKingdoms\(ModulateKingdoms\);'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'TAX_TRAIT_CLEANUP_RED: former top kingdoms can retain boom tax traits forever'
    exit 1
}

Write-Host 'TAX_TRAIT_CLEANUP_GREEN: default policy clears the mod trait from every kingdom'
exit 0
