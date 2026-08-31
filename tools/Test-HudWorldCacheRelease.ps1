$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'UI\EconomyHUD.cs'))
$pattern = 'OnWorldUnavailable\(\).*ClearWorldCaches\(\).*private static void ClearWorldCaches\(\).*_seriesPool\.Clear\(\);.*_chartSeriesEntryPool\.Clear\(\);.*_rankBuf\.Clear\(\);.*_dynIndex\.Clear\(\);.*_dynLastSeen\.Clear\(\);.*_dynSeenBuf\.Clear\(\);.*_keepIds\.Clear\(\);'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'HUD_WORLD_CACHE_RED: chart pools retain old-world names and values'
    exit 1
}

Write-Host 'HUD_WORLD_CACHE_GREEN: chart pools release old-world data'
exit 0
