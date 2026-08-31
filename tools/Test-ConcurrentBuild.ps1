$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$shell = (Get-Process -Id $PID).Path
$runDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("ClassicalEconomics.ConcurrentBuild." + [Guid]::NewGuid().ToString('N'))
[System.IO.Directory]::CreateDirectory($runDirectory) | Out-Null

try {
    $jobs = @()
    for ($i = 1; $i -le 2; $i++) {
        $stdout = Join-Path $runDirectory "build-$i.stdout.log"
        $stderr = Join-Path $runDirectory "build-$i.stderr.log"
        $jobs += Start-Process -FilePath $shell -ArgumentList @(
            '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', (Join-Path $root 'build_local.ps1')
        ) -WorkingDirectory $root -RedirectStandardOutput $stdout -RedirectStandardError $stderr -PassThru
    }

    $exitCodes = @($jobs | ForEach-Object {
        $_.WaitForExit()
        $_.Refresh()
        [int]$_.ExitCode
    })
    $failed = @($exitCodes | Where-Object { $_ -ne 0 })
    if ($failed.Count -gt 0) {
        Write-Host "CONCURRENT_BUILD_RED: $($failed.Count) of $($jobs.Count) builds failed"
        for ($i = 1; $i -le 2; $i++) {
            Write-Host "--- build $i stdout ---"
            Get-Content -LiteralPath (Join-Path $runDirectory "build-$i.stdout.log")
            Write-Host "--- build $i stderr ---"
            Get-Content -LiteralPath (Join-Path $runDirectory "build-$i.stderr.log")
        }
        exit 1
    }

    Write-Host "CONCURRENT_BUILD_GREEN: $($jobs.Count) serialized builds passed"
    exit 0
} finally {
    if (Test-Path -LiteralPath $runDirectory) {
        Remove-Item -LiteralPath $runDirectory -Recurse -Force
    }
}
