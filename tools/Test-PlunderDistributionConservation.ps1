$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\SocialCrisisEngine.cs'))
$pattern = 'long per = amount / poorCount;.*long remain = amount - per \* poorCount;.*long give = per \+ \(first \? remain : 0L\);.*GameHelpers\.AddPositiveMoney\(a, give\);.*given \+= give;'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'PLUNDER_DISTRIBUTION_RED: small or large transfers can disappear during poor relief'
    exit 1
}

Write-Host 'PLUNDER_DISTRIBUTION_GREEN: full transfer is distributed in safe chunks'
exit 0
