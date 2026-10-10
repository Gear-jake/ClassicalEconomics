param(
    [string]$Version = '',
    [switch]$SkipGates,
    [switch]$NoDeploy,
    [switch]$AllowShapeDrift
)

# ─────────────────────────────────────────────────────────────────────────────
# 一次到位：编译 → 门禁 → 打包（源码 zip）→ 部署进游戏。
#
# 用法：
#   powershell -File tools\release_dev.ps1                     # 完整流水线
#   powershell -File tools\release_dev.ps1 -SkipGates          # 跳过门禁（仅本地试跑用）
#   powershell -File tools\release_dev.ps1 -NoDeploy           # 只打包不装游戏
#   powershell -File tools\release_dev.ps1 -Version 2.2.0-dev.w1  # 指定包名后缀
#
# 产物：
#   release\ClassicalEconomics-<version>.zip   （可安装的源码包）
#   游戏 Mods\古典经济学\                       （已部署，NML 缓存已清）
# ─────────────────────────────────────────────────────────────────────────────

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$steps = New-Object System.Collections.Generic.List[object]

function Step([string]$name, [scriptblock]$body) {
    Write-Host ""
    Write-Host ("=== " + $name + " ===")
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    & $body
    $code = $LASTEXITCODE
    if ($null -eq $code) { $code = 0 }
    $sw.Stop()
    $steps.Add([pscustomobject]@{ Name = $name; Exit = $code; Ms = $sw.ElapsedMilliseconds })
    if ($code -ne 0) {
        Write-Host ("RELEASE_RED: step failed: " + $name + " (exit " + $code + ")")
        exit $code
    }
}

Step "BUILD" { & (Join-Path $root "build_local.ps1") }

if (-not $SkipGates) {
    $gateArgs = @()
    if ($AllowShapeDrift) { $gateArgs += "-AllowShapeDrift" }
    Step "GATES" { & (Join-Path $root "run_all_tests.ps1") @gateArgs }
}

Step "PACK" {
    $packArgs = @((Join-Path $root "tools\pack_release.py"))
    if ($Version.Length -gt 0) { $packArgs += $Version }
    & python @packArgs
}

if (-not $NoDeploy) {
    Step "DEPLOY" { & (Join-Path $root "tools\deploy_source.ps1") }
}

Write-Host ""
Write-Host "RELEASE_STEPS"
foreach ($s in $steps) { Write-Host ("  {0,-8} exit={1}  {2} ms" -f $s.Name, $s.Exit, $s.Ms) }
$packed = Get-ChildItem -LiteralPath (Join-Path $root "release") -Filter "ClassicalEconomics-*.zip" -File -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($packed) { Write-Host ("RELEASE_PACKAGE: " + $packed.FullName + "  (" + $packed.Length + " bytes)") }
Write-Host "RELEASE_GREEN"
exit 0
