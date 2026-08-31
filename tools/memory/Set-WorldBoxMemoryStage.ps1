param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Mark')]
    [ValidateSet('game_started', 'save_loaded', 'warmup_complete', 'long_run_started', 'long_run_complete',
        'main_menu', 'switch_started', 'switch_complete', 'custom')]
    [string]$Stage,

    [Parameter(ParameterSetName = 'Mark')]
    [string]$Detail = '',

    [Parameter(Mandatory = $true, ParameterSetName = 'Stop')]
    [switch]$Stop,

    [string]$OutputDirectory = ''
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $PSScriptRoot '..\..\evidence\memory'
}
$outputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
$controlPath = Join-Path $outputDirectory 'active-session.json'

if (-not (Test-Path -LiteralPath $controlPath -PathType Leaf)) {
    throw "No active capture was found at $controlPath"
}

$control = Get-Content -LiteralPath $controlPath -Raw | ConvertFrom-Json
$sampler = Get-Process -Id $control.SamplerProcessId -ErrorAction SilentlyContinue
if ($null -eq $sampler) {
    Remove-Item -LiteralPath $controlPath -Force
    throw "The capture session $($control.SessionId) is no longer running."
}

if ($Stop) {
    [System.IO.File]::WriteAllText($control.StopPath, [DateTime]::UtcNow.ToString('o'), [System.Text.Encoding]::UTF8)
    Write-Output "WORLD_BOX_MEMORY_STOP_REQUESTED: $($control.SessionId)"
    exit 0
}

$row = [PSCustomObject]@{
    TimestampUtc = [DateTime]::UtcNow.ToString('o')
    ElapsedSeconds = ''
    Event = $Stage
    Detail = $Detail
}

$mutex = New-Object System.Threading.Mutex($false, 'Local\ClassicalEconomicsMemoryEvents')
$locked = $false
try {
    $locked = $mutex.WaitOne(5000)
    if (-not $locked) { throw 'Timed out waiting for the events file lock.' }
    if (Test-Path -LiteralPath $control.EventsPath) {
        $row | Export-Csv -LiteralPath $control.EventsPath -NoTypeInformation -Append -Encoding UTF8
    } else {
        $row | Export-Csv -LiteralPath $control.EventsPath -NoTypeInformation -Encoding UTF8
    }
} finally {
    if ($locked) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}

Write-Output "WORLD_BOX_MEMORY_STAGE_RECORDED: $Stage"
