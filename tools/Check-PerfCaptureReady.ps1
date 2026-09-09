param(
    [string]$ModsRoot = 'D:\Program Files (x86)\Steam\steamapps\common\worldbox\Mods'
)

# 采集前置自检：开游戏前确认「这一局一定能采到 perf-capture.json」。
# 覆盖三种会让采集静默失败的情况：
#   1) 部署的 DLL 是旧版（不含采集台）-> 永远不会写出 dump
#   2) default_config.json 缺 perf_diagnostics_enabled -> 玩家打不开开关
#   3) 模组目录不可写 -> dump 写盘失败，只剩一条 LogWarning
# 因此本脚本刻意不以 Test- 开头：它检查的是本机部署状态，不是仓库不变量，
# 不应被 run_all_tests.ps1 当作门禁拾取。

$ErrorActionPreference = 'Stop'

# 本脚本位于 tools\，仓库根是其父目录（与仓库内其它 tools 脚本一致的 $PSScriptRoot 约定）。
$root = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrEmpty($root)) { $root = Split-Path -Parent $MyInvocation.MyCommand.Path }
$localDll = Join-Path $root 'bin\EconomyMod.dll'

$failures = New-Object System.Collections.Generic.List[string]

if (-not (Test-Path -LiteralPath $ModsRoot -PathType Container)) {
    Write-Host "PERF_CAPTURE_READY_RED: Mods folder not found at $ModsRoot"
    exit 1
}

$target = $null
foreach ($dir in (Get-ChildItem -LiteralPath $ModsRoot -Directory)) {
    if (Test-Path -LiteralPath (Join-Path $dir.FullName 'EconomyMod.dll') -PathType Leaf) {
        $target = $dir
        break
    }
}
if ($null -eq $target) {
    Write-Host "PERF_CAPTURE_READY_RED: no installed mod folder containing EconomyMod.dll under $ModsRoot"
    exit 1
}

# 1) DLL 是否与本地构建一致
if (-not (Test-Path -LiteralPath $localDll -PathType Leaf)) {
    $failures.Add("local build missing at $localDll - run build_local.ps1")
} else {
    $deployedDll = Join-Path $target.FullName 'EconomyMod.dll'
    $a = (Get-Item -LiteralPath $localDll).Length
    $b = (Get-Item -LiteralPath $deployedDll).Length
    if ($a -ne $b) {
        $failures.Add("deployed DLL is stale ($b bytes) vs local build ($a bytes) - run deploy_local.ps1")
    }
}

# 2) 部署的 DLL 是否真的含采集台（字符串字面量存于 UTF-16 堆）
$deployedDll = Join-Path $target.FullName 'EconomyMod.dll'
$dllText = [System.IO.File]::ReadAllText($deployedDll, [System.Text.Encoding]::Unicode)
foreach ($literal in @('perf-capture.json', 'perf-parity.json', 'FullActorScans', 'FindEquipTargetCalls', 'UiGoCreated', 'FinishCycleCalls')) {
    if (-not $dllText.Contains($literal)) {
        $failures.Add("deployed DLL lacks instrumentation literal '$literal' - it is not the harness build")
    }
}

# 3) 玩家能否打开开关
$cfgPath = Join-Path $target.FullName 'default_config.json'
if (-not (Test-Path -LiteralPath $cfgPath -PathType Leaf)) {
    $failures.Add("deployed default_config.json not found at $cfgPath")
} else {
    $cfg = Get-Content -LiteralPath $cfgPath -Raw | ConvertFrom-Json
    $item = $cfg.economy_general | Where-Object { $_.Id -eq 'perf_diagnostics_enabled' }
    if ($null -eq $item) {
        $failures.Add('deployed default_config.json lacks perf_diagnostics_enabled - the switch will not appear in game')
    } else {
        if ($item.Type -ne 'SWITCH') { $failures.Add('perf_diagnostics_enabled must be a SWITCH entry') }
        if ($item.Callback -ne 'EconomyConfigCallbacks:OnPerfDiagnosticsEnabledChanged') {
            $failures.Add('perf_diagnostics_enabled callback is not wired to OnPerfDiagnosticsEnabledChanged')
        }
    }
}

# 4) 落盘目录可写
$probe = Join-Path $target.FullName ('__ready_probe_' + (Get-Date -Format 'HHmmss') + '.tmp')
try {
    [System.IO.File]::WriteAllText($probe, 'probe')
    if (Test-Path -LiteralPath $probe) { Remove-Item -LiteralPath $probe -Force }
} catch {
    $failures.Add("mod folder is not writable, dumps would silently fail: $($_.Exception.Message)")
}

if ($failures.Count -gt 0) {
    foreach ($f in $failures) { Write-Host "PERF_CAPTURE_READY_RED: $f" }
    Write-Host "PERF_CAPTURE_READY_RED: $($failures.Count) check(s) failed - do not start a measurement run"
    exit 1
}

Write-Host "PERF_CAPTURE_READY_GREEN: deployed DLL matches local build, instrumentation present, switch wired, dump folder writable"
Write-Host "PERF_CAPTURE_READY_GREEN: dumps will land in '$($target.FullName)' as perf-capture.json and perf-parity.json"
exit 0
