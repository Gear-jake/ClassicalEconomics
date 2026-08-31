$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\SocialCrisisEngine.cs'))
$pattern = 'if \(receiverKingdoms\.Count == 0\) return 0L;.*long actualExtract = GameHelpers\.DeductCoins\(units, extract\);.*if \(actualExtract <= 0\) return 0L;.*long perKingdom = actualExtract / receiverKingdoms\.Count;.*return actualExtract;'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'REVOLUTION_TRANSFER_RED: redistribution pays the requested amount instead of actual deductions'
    exit 1
}

Write-Host 'REVOLUTION_TRANSFER_GREEN: redistribution is bounded by actual deductions'
exit 0
