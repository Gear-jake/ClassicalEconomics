$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\SocialCrisisEngine.cs'))
$pattern = 'if \(transfer > 0 && HasPoorRecipients\(loserUnits, winnerUnits\)\).*actual = GameHelpers\.DeductCoins\(loserUnits, transfer\);'
if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'PLUNDER_RECIPIENTS_RED: transferable plunder can be deducted with no poor recipients'
    exit 1
}
Write-Host 'PLUNDER_RECIPIENTS_GREEN: recipients are confirmed before transferable deductions'
exit 0
