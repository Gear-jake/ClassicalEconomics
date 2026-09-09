param(
    [Parameter(Mandatory = $true)]
    [string]$DumpPath,
    [Parameter(Mandatory = $true)]
    [int]$Population,
    [string]$OutDir = ''
)

$ErrorActionPreference = 'Stop'

if ($OutDir.Length -eq 0) {
    $OutDir = Join-Path (Split-Path -Parent $PSScriptRoot) 'perf'
}

if (-not (Test-Path -LiteralPath $DumpPath -PathType Leaf)) {
    Write-Host "PERF_CAPTURE_RED: dump file not found at $DumpPath"
    exit 1
}

$requiredFields = @(
    'FrameCount', 'FrameP50Ms', 'FrameP95Ms', 'FrameMaxMs',
    'FindEquipTargetCalls', 'FindEquipTargetUnits',
    'FullActorScans', 'FullActorScanUnits',
    'UiGoCreated', 'UiGoDestroyed',
    'FinishCycleCalls', 'YearTotalMs', 'YearTotalBytes',
    'OverBudgetStages', 'Year'
)

$json = Get-Content -LiteralPath $DumpPath -Raw | ConvertFrom-Json

foreach ($field in $requiredFields) {
    if (-not ($json.PSObject.Properties.Name -contains $field)) {
        Write-Host "PERF_CAPTURE_RED: missing field $field"
        exit 1
    }
}

if ($json.FinishCycleCalls -lt 1) {
    Write-Host ("PERF_CAPTURE_RED: FinishCycleCalls ({0}) must be >= 1; a completed annual cycle is required" -f $json.FinishCycleCalls)
    exit 1
}
if ($json.FullActorScans -lt 1) {
    Write-Host ("PERF_CAPTURE_RED: FullActorScans ({0}) must be >= 1; the full-actor scan must have run" -f $json.FullActorScans)
    exit 1
}

if (-not (Test-Path -LiteralPath $OutDir -PathType Container)) {
    New-Item -ItemType Directory -Path $OutDir -Force | Out-Null
}

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$archivePath = Join-Path $OutDir ('capture-{0}-{1}.json' -f $Population, $stamp)
Copy-Item -LiteralPath $DumpPath -Destination $archivePath

Write-Host 'PERF_CAPTURE summary:'
Write-Host ("  frames {0}: p50 {1} ms, p95 {2} ms, max {3} ms" -f $json.FrameCount, $json.FrameP50Ms, $json.FrameP95Ms, $json.FrameMaxMs)
Write-Host ("  find-equip-target: {0} calls over {1} units" -f $json.FindEquipTargetCalls, $json.FindEquipTargetUnits)
Write-Host ("  full-actor-scan: {0} scans over {1} units" -f $json.FullActorScans, $json.FullActorScanUnits)
Write-Host ("  ui objects: {0} created, {1} destroyed" -f $json.UiGoCreated, $json.UiGoDestroyed)
Write-Host ("  year {0}: {1} finished cycle(s), {2} ms, {3} managed bytes, {4} over-budget stage(s)" -f $json.Year, $json.FinishCycleCalls, $json.YearTotalMs, $json.YearTotalBytes, $json.OverBudgetStages)
Write-Host 'GO/NO-GO checklist for the next optimization wave:'
Write-Host '  (1) baseline JSON present at all four population levels 500/2000/5000/10000'
Write-Host '  (2) golden behaviour-parity run reproducible 2/2 within the documented tolerance'
Write-Host '  (3) all gates green on a clean tree'
Write-Host '  (4) harness overhead below 5% of stage time'
Write-Host "PERF_CAPTURE_GREEN: archived $archivePath (population $Population)"
exit 0
