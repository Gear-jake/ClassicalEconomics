param(
    [string]$Filter = '',
    [ValidateSet('All', 'Contract', 'Consistency', 'Infra')]
    [string]$Layer = 'All',
    [switch]$AllowShapeDrift
)

$ErrorActionPreference = 'Stop'

$toolsDir = Join-Path $PSScriptRoot 'tools'
$auditPath = Join-Path $PSScriptRoot 'performance_audit.ps1'
# 成功标记：多数脚本输出 *_GREEN / *_OK；Test-InheritanceScan 输出 *_EQUIVALENT。
$verdictPattern = '_(GREEN|OK|EQUIVALENT)\b'

# ─────────────────────────────────────────────────────────────────────────────
# 门禁分层（W0，2026-10-07）
#
#   Contract    行为契约层。RED = 产品行为/不变量可能真的坏了。
#               **重构期间也不得放松**；只允许改写断言的表达方式，不允许删除契约。
#   Consistency 跨文件一致性层（配置接线 / 本地化覆盖 / 文档口径 / 采集台接线）。
#               结构重构会**合法地**改写它们 —— 重构期可用 -AllowShapeDrift 让其不阻塞。
#   Infra       构建与审计工具自身的门禁（并发构建安全、审计 fail-closed）。
#
# 未登记的脚本一律按 Contract 处理（fail-safe：新增门禁默认是硬门禁）。
# 完整契约清单见 docs\门禁分层说明.md。
# ─────────────────────────────────────────────────────────────────────────────
$layerOf = @{
    'Test-AbandonedBuildMutex.ps1'           = 'Infra'
    'Test-AuditFailClosed.ps1'               = 'Infra'
    'Test-BuildMutexScope.ps1'               = 'Infra'
    'Test-ConcurrentBuild.ps1'               = 'Infra'
    'Test-RelativeOutputConcurrentBuild.ps1' = 'Infra'
    'Test-Ps1Bom.ps1'                        = 'Infra'

    'Test-ConfigDocs.ps1'                    = 'Consistency'
'Test-ConfigCoverage.ps1'                = 'Consistency'
'Test-PatchesManifest.ps1'               = 'Consistency'
'Test-ChartHostPersistent.ps1'           = 'Contract'
'Test-RegimeChangeSwitch.ps1'            = 'Contract'
'Test-ScrollMaskAlpha.ps1'               = 'Contract'
'Test-EventsSchema.ps1'                  = 'Contract'
'Test-SaveSchema.ps1'                    = 'Contract'
'Test-DocConsistency.ps1'                = 'Consistency'
    'Test-LocalizationCoverage.ps1'          = 'Consistency'
    'Test-MemoryCleanupNotify.ps1'           = 'Consistency'
    'Test-PerfDiagnostics.ps1'               = 'Consistency'
    'Test-RealtimeConfig.ps1'                = 'Consistency'
}

function Get-GateLayer([string]$name) {
    if ($layerOf.ContainsKey($name)) { return $layerOf[$name] }
    return 'Contract'
}

function Invoke-PsFile {
    param(
        [string]$Path,
        [string[]]$ArgumentList = @()
    )
    if ($ArgumentList.Count -eq 0) {
        $captured = & powershell -NoProfile -ExecutionPolicy Bypass -File $Path 2>&1 | Out-String
    } else {
        $captured = & powershell -NoProfile -ExecutionPolicy Bypass -File $Path @ArgumentList 2>&1 | Out-String
    }
    if ($null -eq $captured) { $captured = '' }
    return [pscustomobject]@{
        ExitCode = $LASTEXITCODE
        Output = [string]$captured
    }
}

$allTests = @(Get-ChildItem -LiteralPath $toolsDir -Filter 'Test-*.ps1' -File | Sort-Object -Property Name)

$tests = $allTests
if ($Filter.Length -gt 0) {
    $tests = @($tests | Where-Object {
        $_.Name.IndexOf($Filter, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
    })
}
if ($Layer -ne 'All') {
    $tests = @($tests | Where-Object { (Get-GateLayer $_.Name) -eq $Layer })
}

Write-Host ('RUN_ALL_TESTS: {0} test(s) discovered, {1} selected (layer={2}, filter=''{3}''); each gate runs in a fresh PowerShell process.' -f $allTests.Count, $tests.Count, $Layer, $Filter)

$rows = New-Object System.Collections.Generic.List[object]

foreach ($test in $tests) {
    $run = Invoke-PsFile -Path $test.FullName
    $isGreen = ($run.ExitCode -eq 0) -and [System.Text.RegularExpressions.Regex]::IsMatch(
        $run.Output, $verdictPattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $verdict = if ($isGreen) { 'GREEN' } else { 'RED' }
    $rows.Add([pscustomobject]@{
        Name = $test.Name
        Layer = (Get-GateLayer $test.Name)
        ExitCode = $run.ExitCode
        Verdict = $verdict
        Output = $run.Output
    })
}

$auditLayer = 'Contract'
if ($Layer -eq 'All' -or $Layer -eq $auditLayer) {
    $audit = Invoke-PsFile -Path $auditPath -ArgumentList @('-SkipBuild')
    $auditIsGreen = ($audit.ExitCode -eq 0) -and [System.Text.RegularExpressions.Regex]::IsMatch(
        $audit.Output, $verdictPattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $auditVerdict = if ($auditIsGreen) { 'GREEN' } else { 'RED' }
    $rows.Add([pscustomobject]@{
        Name = 'performance_audit.ps1 (-SkipBuild)'
        Layer = $auditLayer
        ExitCode = $audit.ExitCode
        Verdict = $auditVerdict
        Output = $audit.Output
    })
}

$nameWidth = 10
$layerWidth = 11
foreach ($row in $rows) {
    if ($row.Name.Length -gt $nameWidth) { $nameWidth = $row.Name.Length }
    if ($row.Layer.Length -gt $layerWidth) { $layerWidth = $row.Layer.Length }
}
$fmt = "{0,-$nameWidth}  {1,-$layerWidth}  {2,6}  {3}"
Write-Host ''
Write-Host ($fmt -f 'GATE', 'LAYER', 'EXIT', 'VERDICT')
Write-Host ($fmt -f ('-' * $nameWidth), ('-' * $layerWidth), '----', '-------')
foreach ($row in $rows) {
    Write-Host ($fmt -f $row.Name, $row.Layer, $row.ExitCode, $row.Verdict)
}

$reds = @($rows | Where-Object { $_.Verdict -eq 'RED' })
$hardReds = @($reds | Where-Object { $_.Layer -ne 'Consistency' })
$softReds = @($reds | Where-Object { $_.Layer -eq 'Consistency' })
$greens = $rows.Count - $reds.Count

Write-Host ''
Write-Host ('GREEN: {0}   RED: {1}   (Contract/Infra RED: {2}, Consistency RED: {3})' -f $greens, $reds.Count, $hardReds.Count, $softReds.Count)

foreach ($red in $reds) {
    Write-Host ('---- output for {0} [{1}] ----' -f $red.Name, $red.Layer)
    Write-Host $red.Output.TrimEnd()
    Write-Host ''
}

if ($hardReds.Count -eq 0 -and $softReds.Count -eq 0) {
    Write-Host ('RUN_ALL_TESTS_GREEN: {0}/{1} gates passed' -f $rows.Count, $rows.Count)
    exit 0
}

if ($hardReds.Count -eq 0 -and $AllowShapeDrift) {
    Write-Host ('RUN_ALL_TESTS_GREEN_SHAPE_DRIFT: hard gates all passed; {0} consistency gate(s) RED (allowed by -AllowShapeDrift): {1}' -f $softReds.Count, (($softReds | ForEach-Object { $_.Name }) -join ', '))
    exit 0
}

if ($hardReds.Count -eq 0) {
    Write-Host ('RUN_ALL_TESTS_RED: {0} consistency gate(s) failed (behaviour contracts are green; re-run with -AllowShapeDrift to ignore during refactor): {1}' -f $softReds.Count, (($softReds | ForEach-Object { $_.Name }) -join ', '))
    exit 1
}

Write-Host ('RUN_ALL_TESTS_RED: {0} hard gate(s) failed: {1}' -f $hardReds.Count, (($hardReds | ForEach-Object { $_.Name }) -join ', '))
if ($softReds.Count -gt 0) {
    Write-Host ('  (+ {0} consistency gate(s) also RED)' -f $softReds.Count)
}
exit 1
