$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$shell = (Get-Process -Id $PID).Path
$runDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("ClassicalEconomics.RelativeBuild." + [Guid]::NewGuid().ToString('N'))
$outputDirectory = Join-Path $runDirectory 'output'
$workingOne = Join-Path $runDirectory 'one'
$workingTwo = Join-Path $runDirectory 'two'
[System.IO.Directory]::CreateDirectory($workingOne) | Out-Null
[System.IO.Directory]::CreateDirectory($workingTwo) | Out-Null

try {
    $processes = @()
    $index = 0
    foreach ($workingDirectory in @($workingOne, $workingTwo)) {
        $index++
        $processes += Start-Process -FilePath $shell -ArgumentList @(
            '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', (Join-Path $root 'build_local.ps1'),
            '-OutputDir', $outputDirectory
        ) -WorkingDirectory $workingDirectory -RedirectStandardOutput (Join-Path $runDirectory "build-$index.stdout.log") `
            -RedirectStandardError (Join-Path $runDirectory "build-$index.stderr.log") -PassThru
    }

    $exitCodes = @($processes | ForEach-Object {
        $_.WaitForExit()
        $_.Refresh()
        [int]$_.ExitCode
    })
    $failed = @($exitCodes | Where-Object { $_ -ne 0 })
    if ($failed.Count -gt 0) {
        Write-Host "RELATIVE_OUTPUT_CONCURRENT_BUILD_RED: $($failed.Count) of $($processes.Count) builds failed"
        exit 1
    }

    Write-Host "RELATIVE_OUTPUT_CONCURRENT_BUILD_GREEN: $($processes.Count) builds serialized"
    exit 0
} finally {
    if (Test-Path -LiteralPath $runDirectory) { Remove-Item -LiteralPath $runDirectory -Recurse -Force }
}
