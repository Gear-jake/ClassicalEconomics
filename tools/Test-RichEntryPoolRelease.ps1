$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\DataCollector.cs'))
$pattern = 'private static void ReturnEntry\(RichEntryData e\).*e\.Name = null;.*e\.Kingdom = null;.*e\.Wealth = 0f;.*e\.Id = 0L;.*_entryPool\.Add\(e\);'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'RICH_ENTRY_POOL_RED: pooled entries retain old-world names'
    exit 1
}

Write-Host 'RICH_ENTRY_POOL_GREEN: pooled entries release old-world strings'
exit 0
