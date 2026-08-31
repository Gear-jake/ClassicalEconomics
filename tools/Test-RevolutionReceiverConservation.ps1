$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\SocialCrisisEngine.cs'))
$pattern = 'var receiverKingdoms = _kingdomPool;.*receiverKingdoms\.Add\(target\);.*long kingdomGive = perKingdom \+ \(i == 0 \? kingdomRemainder : 0L\);.*long actorGive = perActor \+ \(first \? actorRemainder : 0L\);.*GameHelpers\.AddPositiveMoney\(a, actorGive\);'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'REVOLUTION_RECEIVER_RED: invalid receivers and integer remainders can destroy redistributed wealth'
    exit 1
}

Write-Host 'REVOLUTION_RECEIVER_GREEN: valid receivers get exact kingdom and actor remainders'
exit 0
