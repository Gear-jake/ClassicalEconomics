param(
    [string]$SessionDirectory
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($SessionDirectory)) {
    $root = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\evidence\memory'))
    $latest = Get-ChildItem -LiteralPath $root -Directory -ErrorAction Stop |
        Sort-Object Name -Descending |
        Select-Object -First 1
    if ($null -eq $latest) { throw "No capture sessions found under $root" }
    $SessionDirectory = $latest.FullName
}

$SessionDirectory = [System.IO.Path]::GetFullPath($SessionDirectory)
$samplesPath = Join-Path $SessionDirectory 'samples.csv'
$eventsPath = Join-Path $SessionDirectory 'events.csv'

$samples = @(Import-Csv -LiteralPath $samplesPath)
if ($samples.Count -lt 2) { throw 'At least two samples are required.' }

$numericSamples = @($samples | ForEach-Object {
    [PSCustomObject]@{
        TimestampUtc = [DateTime]$_.TimestampUtc
        ElapsedSeconds = [double]$_.ElapsedSeconds
        PrivateMiB = [double]$_.PrivateMiB
        WorkingSetMiB = [double]$_.WorkingSetMiB
        Handles = [int]$_.Handles
        Threads = [int]$_.Threads
        CpuPercent = [double]$_.CpuPercent
    }
})

$first = $numericSamples[0]
$last = $numericSamples[$numericSamples.Count - 1]
$durationHours = [Math]::Max(0.000001, ($last.ElapsedSeconds - $first.ElapsedSeconds) / 3600.0)
$privateSlope = ($last.PrivateMiB - $first.PrivateMiB) / $durationHours
$workingSlope = ($last.WorkingSetMiB - $first.WorkingSetMiB) / $durationHours

$summary = [PSCustomObject]@{
    SessionDirectory = $SessionDirectory
    SampleCount = $numericSamples.Count
    DurationMinutes = [Math]::Round($durationHours * 60, 2)
    PrivateStartMiB = $first.PrivateMiB
    PrivateEndMiB = $last.PrivateMiB
    PrivateDeltaMiB = [Math]::Round($last.PrivateMiB - $first.PrivateMiB, 3)
    PrivateMiBPerHour = [Math]::Round($privateSlope, 3)
    WorkingSetStartMiB = $first.WorkingSetMiB
    WorkingSetEndMiB = $last.WorkingSetMiB
    WorkingSetDeltaMiB = [Math]::Round($last.WorkingSetMiB - $first.WorkingSetMiB, 3)
    WorkingSetMiBPerHour = [Math]::Round($workingSlope, 3)
    HandlesStart = $first.Handles
    HandlesEnd = $last.Handles
    ThreadsStart = $first.Threads
    ThreadsEnd = $last.Threads
    PeakPrivateMiB = [Math]::Round(($numericSamples | Measure-Object PrivateMiB -Maximum).Maximum, 3)
    PeakWorkingSetMiB = [Math]::Round(($numericSamples | Measure-Object WorkingSetMiB -Maximum).Maximum, 3)
    AverageCpuPercent = [Math]::Round(($numericSamples | Measure-Object CpuPercent -Average).Average, 3)
}

$summary | Format-List

if (Test-Path -LiteralPath $eventsPath) {
    Write-Output 'STAGES:'
    Import-Csv -LiteralPath $eventsPath | Format-Table TimestampUtc,Event,Detail -AutoSize
}
