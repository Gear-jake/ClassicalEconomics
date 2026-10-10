param(
    [string]$DumpPath = '',
    [string]$GameRoot = 'D:\Program Files (x86)\Steam\steamapps\common\worldbox'
)

# 性能采集分析：把 perf-capture.json 读成人类可读的结论。
#
# 用法：
#   powershell -File tools\analyze_perf_capture.ps1                # 自动在游戏目录里找
#   powershell -File tools\analyze_perf_capture.ps1 -DumpPath <路径>
#
# 前提：模组设置里开启「性能诊断（年度收尾）」，并跑过至少 1 个经济年。

$ErrorActionPreference = 'Stop'

if ($DumpPath.Length -eq 0) {
    $candidates = @(Get-ChildItem -Path $GameRoot -Filter 'perf-capture.json' -Recurse -ErrorAction SilentlyContinue)
    if ($candidates.Count -eq 0) {
        Write-Host 'PERF_ANALYZE: 未找到 perf-capture.json。'
        Write-Host '  请先在模组设置里开启「性能诊断（年度收尾）」，载入存档并跑过至少 1 个经济年。'
        exit 1
    }
    $DumpPath = ($candidates | Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName
}

if (-not (Test-Path -LiteralPath $DumpPath -PathType Leaf)) { Write-Host "PERF_ANALYZE: 文件不存在: $DumpPath"; exit 1 }
$json = Get-Content -LiteralPath $DumpPath -Raw | ConvertFrom-Json
$age = (Get-Date) - (Get-Item -LiteralPath $DumpPath).LastWriteTime

Write-Host ''
Write-Host '===== 性能采集摘要 ====='
Write-Host "文件: $DumpPath"
Write-Host ("采集时间: {0:yyyy-MM-dd HH:mm}  （{1:N1} 分钟前）" -f (Get-Item -LiteralPath $DumpPath).LastWriteTime, $age.TotalMinutes)
Write-Host "Schema: $($json.Schema)   年份: $($json.Year)"
Write-Host ''

Write-Host '--- 帧时间 ---'
Write-Host ("帧数 {0}   p50 {1:N2}ms   p95 {2:N2}ms   max {3:N2}ms" -f $json.FrameCount, $json.FrameP50Ms, $json.FrameP95Ms, $json.FrameMaxMs)
if ($json.FrameP95Ms -gt 33) { Write-Host '  ⚠ p95 超过 33ms（<30 FPS）' -ForegroundColor Yellow }

if ($json.PSObject.Properties.Name -contains 'ModFrameSamples' -and $json.ModFrameSamples -gt 0) {
    Write-Host ''
    Write-Host '--- 模组自身每帧主线程耗时（最近'$json.ModFrameSamples'帧）---'
    Write-Host ("p50 {0:N3}ms   p95 {1:N3}ms   max {2:N3}ms   平均 {3:N3}ms" -f $json.ModFrameP50Ms, $json.ModFrameP95Ms, $json.ModFrameMaxMs, $json.ModFrameAvgMs)
    $share = if ($json.FrameP50Ms -gt 0) { 100.0 * $json.ModFrameP50Ms / $json.FrameP50Ms } else { 0 }
    if ($json.FrameP50Ms -gt 0) { Write-Host ("占帧时间中位数约 {0:N1}%" -f $share) }
    if ($json.PSObject.Properties.Name -contains 'ModSlow1') {
        $n = [Math]::Max(1, $json.ModFrameSamples)
        Write-Host ''
        Write-Host '--- 模组卡顿帧分布 ---'
        Write-Host ("  >1ms  {0,5} 帧 ({1,5:N2}%)   >5ms  {2,5} 帧 ({3,5:N2}%)" -f $json.ModSlow1, (100.0*$json.ModSlow1/$n), $json.ModSlow5, (100.0*$json.ModSlow5/$n))
        Write-Host ("  >16ms {0,5} 帧 ({1,5:N2}%)   >33ms {2,5} 帧 ({3,5:N2}%)" -f $json.ModSlow16, (100.0*$json.ModSlow16/$n), $json.ModSlow33, (100.0*$json.ModSlow33/$n))
    }
} else {
    Write-Host ''
    Write-Host '--- 模组自身每帧耗时 ---'
    Write-Host '  (无数据：需使用带该指标的模组版本)'
}

Write-Host '--- 年度收尾 ---'
Write-Host ("年度总耗时 {0}ms   托管分配 {1:N1} KB   超预算阶段 {2}" -f $json.YearTotalMs, ($json.YearTotalBytes / 1024.0), $json.OverBudgetStages)

Write-Host '--- 模组侧计数 ---'
Write-Host ("全量 Actor 扫描 {0} 次 / {1} 单位" -f $json.FullActorScans, $json.FullActorScanUnits)
Write-Host ("FindEquipTarget {0} 次 / {1} 单位" -f $json.FindEquipTargetCalls, $json.FindEquipTargetUnits)
Write-Host ("UI GameObject 创建 {0} / 销毁 {1}" -f $json.UiGoCreated, $json.UiGoDestroyed)
Write-Host ("FinishCycle 调用 {0}" -f $json.FinishCycleCalls)

$stages = @($json.Stages)
if ($stages.Count -eq 0) {
    Write-Host ''
    Write-Host '--- 分阶段明细 ---'
    Write-Host '  (无数据：需使用带分阶段采集的模组版本，且诊断开启后跑过至少 1 个经济年)'
} else {
    Write-Host ''
    Write-Host '--- 分阶段明细（按累计耗时降序）---'
    Write-Host ("{0,-18} {1,8} {2,10} {3,10} {4,10} {5,10}" -f 'Stage', 'Count', 'TotalMs', 'AvgMs', 'MaxMs', 'TotalKB')
    $sorted = $stages | Sort-Object -Property TotalMs -Descending
    $total = ($stages | Measure-Object -Property TotalMs -Sum).Sum
    foreach ($s in $sorted) {
        $pct = if ($total -gt 0) { 100.0 * $s.TotalMs / $total } else { 0 }
        Write-Host ("{0,-18} {1,8} {2,10} {3,10:N3} {4,10} {5,10}   {6,5:N1}%" -f $s.Name, $s.Count, $s.TotalMs, $s.AvgMs, $s.MaxMs, $s.TotalKB, $pct)
    }
    Write-Host ''
    $top = $sorted | Select-Object -First 1
    if ($null -ne $top) {
        Write-Host ("最慢阶段: {0}（累计 {1}ms，占总 {2:N1}%，单次峰值 {3}ms）" -f $top.Name, $top.TotalMs, (100.0 * $top.TotalMs / [Math]::Max(1, $total)), $top.MaxMs)
    }
}
Write-Host ''
Write-Host '===== 提示 ====='
Write-Host '· 这是"模组侧"的年度收尾数据，不含原版世界模拟（单位 AI/寻路/渲染）。'
Write-Host '· 判断"卡顿是否模组造成"需要对照实验：同存档开/关模组各跑一次，比帧时间 p95。'
Write-Host '· 分阶段 TotalMs 明显偏高的那一段，就是下一步优化的目标。'
Write-Host ''