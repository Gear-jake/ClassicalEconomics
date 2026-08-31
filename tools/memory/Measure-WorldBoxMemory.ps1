param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\..\evidence\memory'),
    [double]$IntervalSeconds = 1,
    [int]$ProcessWaitSeconds = 900,
    [int]$MaximumDurationMinutes = 180
)

$ErrorActionPreference = 'Stop'

if ($IntervalSeconds -lt 0.2) {
    throw 'IntervalSeconds must be at least 0.2.'
}

$outputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
[System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null

$sessionId = Get-Date -Format 'yyyyMMdd-HHmmss'
$sessionDirectory = Join-Path $outputDirectory $sessionId
[System.IO.Directory]::CreateDirectory($sessionDirectory) | Out-Null

$samplesPath = Join-Path $sessionDirectory 'samples.csv'
$eventsPath = Join-Path $sessionDirectory 'events.csv'
$controlPath = Join-Path $outputDirectory 'active-session.json'
$stopPath = Join-Path $sessionDirectory 'stop.request'
$completedPath = Join-Path $sessionDirectory 'completed.json'
$eventsMutex = New-Object System.Threading.Mutex($false, 'Local\ClassicalEconomicsMemoryEvents')

$control = [ordered]@{
    SessionId = $sessionId
    SessionDirectory = $sessionDirectory
    SamplesPath = $samplesPath
    EventsPath = $eventsPath
    StopPath = $stopPath
    SamplerProcessId = $PID
    StartedUtc = [DateTime]::UtcNow.ToString('o')
}
[System.IO.File]::WriteAllText($controlPath, ($control | ConvertTo-Json), [System.Text.Encoding]::UTF8)

function Write-Event {
    param([string]$Name, [string]$Detail = '')

    $row = [PSCustomObject]@{
        TimestampUtc = [DateTime]::UtcNow.ToString('o')
        ElapsedSeconds = [Math]::Round($script:stopwatch.Elapsed.TotalSeconds, 3)
        Event = $Name
        Detail = $Detail
    }
    $locked = $false
    try {
        $locked = $script:eventsMutex.WaitOne(5000)
        if (-not $locked) { throw 'Timed out waiting for the events file lock.' }
        if (Test-Path -LiteralPath $eventsPath) {
            $row | Export-Csv -LiteralPath $eventsPath -NoTypeInformation -Append -Encoding UTF8
        } else {
            $row | Export-Csv -LiteralPath $eventsPath -NoTypeInformation -Encoding UTF8
        }
    } finally {
        if ($locked) { $script:eventsMutex.ReleaseMutex() }
    }
}

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
Write-Event -Name 'sampler_started' -Detail "Waiting for worldbox.exe (timeout ${ProcessWaitSeconds}s)"

$process = $null
while ($stopwatch.Elapsed.TotalSeconds -lt $ProcessWaitSeconds) {
    $process = Get-Process -Name 'worldbox' -ErrorAction SilentlyContinue |
        Sort-Object StartTime -Descending |
        Select-Object -First 1
    if ($null -ne $process) { break }
    if (Test-Path -LiteralPath $stopPath) { break }
    Start-Sleep -Milliseconds 500
}

if ($null -eq $process) {
    Write-Event -Name 'sampler_stopped' -Detail 'WorldBox process was not found.'
    exit 2
}

$processId = $process.Id
$processStartUtc = $process.StartTime.ToUniversalTime().ToString('o')
$sampleStart = [DateTime]::UtcNow
$previousCpu = $process.TotalProcessorTime.TotalSeconds
$previousTimestamp = $sampleStart
$processorCount = [Math]::Max(1, [Environment]::ProcessorCount)
$sampleCount = 0

Write-Event -Name 'process_attached' -Detail "PID=$processId; StartUtc=$processStartUtc"

try {
    while ($true) {
        if (Test-Path -LiteralPath $stopPath) {
            Write-Event -Name 'stop_requested'
            break
        }
        if (([DateTime]::UtcNow - $sampleStart).TotalMinutes -ge $MaximumDurationMinutes) {
            Write-Event -Name 'duration_limit_reached' -Detail "MaximumDurationMinutes=$MaximumDurationMinutes"
            break
        }

        try {
            $process.Refresh()
            if ($process.HasExited) {
                Write-Event -Name 'process_exited' -Detail "ExitCode=$($process.ExitCode)"
                break
            }

            $now = [DateTime]::UtcNow
            $cpu = $process.TotalProcessorTime.TotalSeconds
            $wallSeconds = [Math]::Max(0.001, ($now - $previousTimestamp).TotalSeconds)
            $cpuPercent = (($cpu - $previousCpu) / $wallSeconds / $processorCount) * 100.0

            $row = [PSCustomObject]@{
                TimestampUtc = $now.ToString('o')
                ElapsedSeconds = [Math]::Round(($now - $sampleStart).TotalSeconds, 3)
                ProcessId = $processId
                PrivateMiB = [Math]::Round($process.PrivateMemorySize64 / 1MB, 3)
                WorkingSetMiB = [Math]::Round($process.WorkingSet64 / 1MB, 3)
                VirtualMiB = [Math]::Round($process.VirtualMemorySize64 / 1MB, 3)
                PagedMiB = [Math]::Round($process.PagedMemorySize64 / 1MB, 3)
                NonpagedSystemMiB = [Math]::Round($process.NonpagedSystemMemorySize64 / 1MB, 3)
                PagedSystemMiB = [Math]::Round($process.PagedSystemMemorySize64 / 1MB, 3)
                Handles = $process.HandleCount
                Threads = $process.Threads.Count
                CpuPercent = [Math]::Round([Math]::Max(0, $cpuPercent), 3)
                TotalCpuSeconds = [Math]::Round($cpu, 3)
            }

            if ($sampleCount -eq 0) {
                $row | Export-Csv -LiteralPath $samplesPath -NoTypeInformation -Encoding UTF8
            } else {
                $row | Export-Csv -LiteralPath $samplesPath -NoTypeInformation -Append -Encoding UTF8
            }
            $sampleCount++
            $previousCpu = $cpu
            $previousTimestamp = $now
        } catch [System.InvalidOperationException] {
            Write-Event -Name 'process_unavailable' -Detail $_.Exception.Message
            break
        }

        Start-Sleep -Milliseconds ([int]($IntervalSeconds * 1000))
    }
} finally {
    $stopwatch.Stop()
    $completed = [ordered]@{
        SessionId = $sessionId
        ProcessId = $processId
        ProcessStartUtc = $processStartUtc
        CompletedUtc = [DateTime]::UtcNow.ToString('o')
        SampleCount = $sampleCount
        SamplesPath = $samplesPath
        EventsPath = $eventsPath
    }
    [System.IO.File]::WriteAllText($completedPath, ($completed | ConvertTo-Json), [System.Text.Encoding]::UTF8)

    if (Test-Path -LiteralPath $controlPath) {
        try {
            $active = Get-Content -LiteralPath $controlPath -Raw | ConvertFrom-Json
            if ($active.SessionId -eq $sessionId) {
                Remove-Item -LiteralPath $controlPath -Force
            }
        } catch { }
    }
    $eventsMutex.Dispose()
}

Write-Output "WORLD_BOX_MEMORY_CAPTURE_COMPLETE: $sessionDirectory ($sampleCount samples)"
