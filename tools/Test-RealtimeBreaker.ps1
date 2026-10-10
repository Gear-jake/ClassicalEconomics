param(
    [string]$MainPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'EconomyModMain.cs'),
    [string]$ConfigPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'Models\UnrestConfig.cs')
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $MainPath -PathType Leaf)) {
    Write-Host "REALTIME_BREAKER_RED: source file not found: $MainPath"
    exit 1
}
if (-not (Test-Path -LiteralPath $ConfigPath -PathType Leaf)) {
    Write-Host "REALTIME_BREAKER_RED: source file not found: $ConfigPath"
    exit 1
}

$main = [System.IO.File]::ReadAllText($MainPath)
$cfg = [System.IO.File]::ReadAllText($ConfigPath)

# 1) Breaker must be config-driven (threshold 3000 -> 2000, exposed as RealTimeRefreshThreshold).
#    Any hardcoded numeric comparison against aliveList.Count is RED.
if ($main -notmatch 'aliveList\.Count\s*>=\s*cfg\.RealTimeRefreshThreshold') {
    Write-Host 'REALTIME_BREAKER_RED: breaker must compare aliveList.Count >= cfg.RealTimeRefreshThreshold (config-driven, not hardcoded)'
    exit 1
}
if ($main -match 'aliveList\.Count\s*>=\s*[0-9]+') {
    Write-Host 'REALTIME_BREAKER_RED: breaker still uses a hardcoded numeric threshold'
    exit 1
}

# 2) Default threshold must be 2000 (was 3000); budget default matches so that
#    below-threshold behavior stays a full synchronous refresh.
if ($cfg -notmatch 'public int RealTimeRefreshThreshold\s*=\s*2000') {
    Write-Host 'REALTIME_BREAKER_RED: UnrestConfig.RealTimeRefreshThreshold default must be 2000'
    exit 1
}
if ($cfg -notmatch 'public int RealTimeRefreshBudget\s*=\s*2000') {
    Write-Host 'REALTIME_BREAKER_RED: UnrestConfig.RealTimeRefreshBudget default must be 2000'
    exit 1
}

# 3) Below threshold must stay synchronous: the realtime path still runs the full
#    synchronous collect + compute, with the configured per-refresh budget.
if ($main -notmatch 'DataCollector\.Collect\(applySideEffects: false, postCycle: false, maxUnits: cfg\.RealTimeRefreshBudget\)') {
    Write-Host 'REALTIME_BREAKER_RED: below-threshold synchronous path must collect with the configured budget'
    exit 1
}
# 【W4.4 契约换实现】实时路径从「主线程同步计算」改为「异步提交 + 后续帧消费」：
# 契约不变：阈值下必须真的重算（用配置预算），且**不得推进周期号**。
if ($main -notmatch 'TradeSimulationWorker\.PostCycle\(advanceCycle: false\)') {
    Write-Host 'REALTIME_BREAKER_RED: below-threshold path must submit an async cycle without advancing the cycle index'
    exit 1
}
if ($main -notmatch '_rtCyclePending') {
    Write-Host 'REALTIME_BREAKER_RED: realtime cycle must be tracked separately from the annual _cyclePending'
    exit 1
}

# 4) Default-off RealTimeRefresh semantics preserved.
if ($cfg -notmatch 'public bool RealTimeRefresh\s*=\s*false') {
    Write-Host 'REALTIME_BREAKER_RED: RealTimeRefresh default must remain false (default-off)'
    exit 1
}

Write-Host 'REALTIME_BREAKER_GREEN: breaker threshold 2000, config-driven, below-threshold synchronous'
exit 0