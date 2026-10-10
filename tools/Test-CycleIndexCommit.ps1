$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'Core\TradeSimulationWorker.cs'))
# 【W4.4 契约换形状】PostCycle 增加 advanceCycle 后周期号改为条件推进。
# 契约不变：周期号只在任务真正排入 ThreadPool 之后才提交；被拒的提交不得推进。
$pattern = 'int idx = advanceCycle \? _cycleIndex \+ 1 : _cycleIndex;.*bool queued = ThreadPool\.QueueUserWorkItem.*if \(!queued\) throw new InvalidOperationException.*if \(advanceCycle\) _cycleIndex = idx;.*return true;'

if (-not [Regex]::IsMatch($source, $pattern, [Text.RegularExpressions.RegexOptions]::Singleline)) {
    Write-Host 'CYCLE_INDEX_COMMIT_RED: rejected worker submission still advances the cycle index'
    exit 1
}

Write-Host 'CYCLE_INDEX_COMMIT_GREEN: cycle index commits only after queue acceptance'
exit 0
