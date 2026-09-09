param(
    [string]$ModsRoot = 'D:\Program Files (x86)\Steam\steamapps\common\worldbox\Mods'
)

# Deploy bin\EconomyMod.dll into the game's mod folder (companion to build_local.ps1).
# Only the DLL is replaced: this change is code-only, so default_config.json and Locales
# are left untouched to avoid disturbing player settings.
# The previous DLL is backed up as EconomyMod.dll.bak-yyyyMMdd-HHmmss before overwrite.

$ErrorActionPreference = 'Stop'

# Resolve the repo root from the script's own path. $MyInvocation.MyCommand.Path is used
# because $PSScriptRoot proved unreliable for this file under `powershell -File`.
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrEmpty($root)) { $root = $PSScriptRoot }

$dll = Join-Path $root 'bin\EconomyMod.dll'

if (-not (Test-Path -LiteralPath $dll -PathType Leaf)) {
    Write-Host "DEPLOY_RED: built DLL not found at $dll - run build_local.ps1 first"
    exit 1
}
if (-not (Test-Path -LiteralPath $ModsRoot -PathType Container)) {
    Write-Host "DEPLOY_RED: Mods folder not found at $ModsRoot"
    exit 1
}

$target = $null
foreach ($dir in (Get-ChildItem -LiteralPath $ModsRoot -Directory)) {
    if (Test-Path -LiteralPath (Join-Path $dir.FullName 'EconomyMod.dll') -PathType Leaf) {
        $target = $dir
        break
    }
}
if ($null -eq $target) {
    Write-Host "DEPLOY_RED: no installed mod folder containing EconomyMod.dll under $ModsRoot"
    exit 1
}

$destDll = Join-Path $target.FullName 'EconomyMod.dll'
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$backup = Join-Path $target.FullName ('EconomyMod.dll.bak-' + $stamp)
Copy-Item -LiteralPath $destDll -Destination $backup -Force

Copy-Item -LiteralPath $dll -Destination $destDll -Force

$after = Get-Item -LiteralPath $destDll
$source = Get-Item -LiteralPath $dll
if ($after.Length -ne $source.Length) {
    Write-Host "DEPLOY_RED: size mismatch after copy (source $($source.Length), target $($after.Length))"
    exit 1
}

Write-Host "DEPLOY_GREEN: deployed $($source.Length) bytes to '$($target.Name)\EconomyMod.dll'"
Write-Host "DEPLOY_GREEN: previous DLL backed up as $(Split-Path -Leaf $backup)"
exit 0
