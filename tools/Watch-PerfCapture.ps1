param(
    [Parameter(Mandatory = $true)]
    [int]$Population,
    [string]$DumpDir = '',
    [string]$OutDir = '',
    [int]$IntervalSeconds = 2,
    [int]$DurationSeconds = 0,
    [string]$ModsRoot = 'D:\Program Files (x86)\Steam\steamapps\common\worldbox\Mods'
)

# 采集自动归档守卫：你只管打游戏，本脚本轮询模组目录，每当 perf-capture.json /
# perf-parity.json 被模组重写（即一个游戏年结束），就自动归档一份带人口档位的副本。
# 这样四档人口 (500/2000/5000/10000) 不需要在每档结束后记得手动执行 PerfCapture.ps1。
#
# 停止方式：Ctrl+C，或在 OutDir 下出现 watch.stop 文件，或 -DurationSeconds 到期。
# 归档命名：capture-<人口>-<时间戳>.json / parity-<人口>-<时间戳>.json

$ErrorActionPreference = 'Stop'

# 本脚本位于 tools\，仓库根是其父目录（$MyInvocation 在本环境常为空，故以 $PSScriptRoot 为准）。
$root = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrEmpty($root)) { $root = Split-Path -Parent $MyInvocation.MyCommand.Path }
if ([string]::IsNullOrEmpty($OutDir)) { $OutDir = Join-Path $root 'perf' }

if ([string]::IsNullOrEmpty($DumpDir)) {
    if (-not (Test-Path -LiteralPath $ModsRoot -PathType Container)) {
        Write-Host "WATCH_PERF_CAPTURE_RED: Mods folder not found at $ModsRoot"
        exit 1
    }
    $found = $null
    foreach ($dir in (Get-ChildItem -LiteralPath $ModsRoot -Directory)) {
        if (Test-Path -LiteralPath (Join-Path $dir.FullName 'EconomyMod.dll') -PathType Leaf) {
            $found = $dir
            break
        }
    }
    if ($null -eq $found) {
        Write-Host "WATCH_PERF_CAPTURE_RED: no installed mod folder containing EconomyMod.dll under $ModsRoot"
        exit 1
    }
    $DumpDir = $found.FullName
}

if (-not (Test-Path -LiteralPath $DumpDir -PathType Container)) {
    Write-Host "WATCH_PERF_CAPTURE_RED: dump dir not found at $DumpDir"
    exit 1
}
if (-not (Test-Path -LiteralPath $OutDir -PathType Container)) {
    New-Item -ItemType Directory -Path $OutDir -Force | Out-Null
}

$stopFile = Join-Path $OutDir 'watch.stop'
if (Test-Path -LiteralPath $stopFile -PathType Leaf) { Remove-Item -LiteralPath $stopFile -Force }

$seen = @{}
$archivedCapture = 0
$archivedParity = 0
$start = Get-Date

Write-Host "WATCH_PERF_CAPTURE_START: watching '$DumpDir' for population $Population"
Write-Host "WATCH_PERF_CAPTURE_START: archives go to '$OutDir'; Ctrl+C or create 'watch.stop' to stop"

while ($true) {
    foreach ($name in @('perf-capture.json', 'perf-parity.json')) {
        $path = Join-Path $DumpDir $name
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { continue }
        $item = Get-Item -LiteralPath $path
        $key = $name
        if ($seen.ContainsKey($key) -and $seen[$key] -eq $item.LastWriteTimeUtc) { continue }
        $seen[$key] = $item.LastWriteTimeUtc

        $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
        $prefix = if ($name -eq 'perf-capture.json') { 'capture' } else { 'parity' }
        $dest = Join-Path $OutDir ($prefix + '-' + $Population + '-' + $stamp + '.json')
        Copy-Item -LiteralPath $path -Destination $dest -Force
        if ($prefix -eq 'capture') { $archivedCapture++ } else { $archivedParity++ }
        Write-Host ("WATCH_PERF_CAPTURE_ARCHIVED: " + (Split-Path -Leaf $dest))
    }

    if (Test-Path -LiteralPath $stopFile -PathType Leaf) {
        Remove-Item -LiteralPath $stopFile -Force
        break
    }
    if ($DurationSeconds -gt 0) {
        if (((Get-Date) - $start).TotalSeconds -ge $DurationSeconds) { break }
    }
    Start-Sleep -Seconds $IntervalSeconds
}

Write-Host "WATCH_PERF_CAPTURE_DONE: archived $archivedCapture capture file(s), $archivedParity parity file(s) for population $Population"
exit 0
