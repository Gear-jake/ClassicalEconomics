$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = [System.IO.File]::ReadAllText((Join-Path $root 'build_local.ps1'))
$pattern = '\$mutexScope\s*=\s*\[System\.IO\.Path\]::GetFullPath\(\$PSScriptRoot\)\.ToUpperInvariant\(\)'

if (-not [Regex]::IsMatch($source, $pattern)) {
    Write-Host 'BUILD_MUTEX_SCOPE_RED: shared diagnostics are not locked by repository root'
    exit 1
}

Write-Host 'BUILD_MUTEX_SCOPE_GREEN: repository root owns the shared build lock'
exit 0
