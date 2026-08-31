$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$repositoryRoot = [System.IO.Path]::GetFullPath($root).ToUpperInvariant()
$bytes = [System.Text.Encoding]::UTF8.GetBytes($repositoryRoot)
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$hash = [System.BitConverter]::ToString($sha256.ComputeHash($bytes)).Replace('-', '').Substring(0, 16)
$sha256.Dispose()
$mutexName = "Local\ClassicalEconomicsBuild_$hash"
$shell = (Get-Process -Id $PID).Path
$command = "`$m = New-Object System.Threading.Mutex(`$false, '$mutexName'); `$null = `$m.WaitOne(); exit 0"

$owner = Start-Process -FilePath $shell -ArgumentList @('-NoProfile', '-Command', $command) -PassThru
$owner.WaitForExit()
& (Join-Path $root 'build_local.ps1')
if ($LASTEXITCODE -ne 0) {
    Write-Host "ABANDONED_BUILD_MUTEX_RED: build exit $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host 'ABANDONED_BUILD_MUTEX_GREEN: abandoned owner recovered'
exit 0
