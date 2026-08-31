$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\EconomyCycleModulator.cs'))
$pattern = 'if \(!_initialized\).*MoneySupply = gdp;.*CurrentCPI = 1f;.*float actualStimulus = 0f;.*actualStimulus = \(float\)count \* perActor;.*MoneySupply \+= actualStimulus;.*BubbleValue \+= actualStimulus \* cfg\.BoomBubbleFactor'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'MONEY_SUPPLY_ACCOUNTING_RED: CPI and bubble state do not track baseline and actual injected coins'
    exit 1
}

Write-Host 'MONEY_SUPPLY_ACCOUNTING_GREEN: monetary state tracks baseline and actual injection'
exit 0
