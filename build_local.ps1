param(
    [string]$LibsDir = (Join-Path $PSScriptRoot '..\libs'),
    [string]$OutputDir = (Join-Path $PSScriptRoot 'bin')
)

$mutexScope = [System.IO.Path]::GetFullPath($PSScriptRoot).ToUpperInvariant()
$mutexBytes = [System.Text.Encoding]::UTF8.GetBytes($mutexScope)
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$mutexHash = [System.BitConverter]::ToString(
    $sha256.ComputeHash($mutexBytes)
).Replace('-', '').Substring(0, 16)
$sha256.Dispose()
$buildMutex = New-Object System.Threading.Mutex($false, "Local\ClassicalEconomicsBuild_$mutexHash")
$buildLockHeld = $false
try {
    try {
        $buildLockHeld = $buildMutex.WaitOne([TimeSpan]::FromMinutes(5))
    } catch [System.Threading.AbandonedMutexException] {
        $buildLockHeld = $true
    }
    if (-not $buildLockHeld) {
        Write-Host 'Timed out waiting for the ClassicalEconomics build lock.'
        exit 6
    }

$root = $PSScriptRoot
$diag = Join-Path $root 'build_diag.txt'
$errors = Join-Path $root 'errors.txt'

if (-not [System.IO.Path]::IsPathRooted($LibsDir)) {
    $LibsDir = Join-Path $root $LibsDir
}
if (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $root $OutputDir
}
$LibsDir = [System.IO.Path]::GetFullPath($LibsDir)
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
$out = Join-Path $OutputDir 'EconomyMod.dll'

"BUILD_ROOT: $root" | Out-File -Encoding utf8 $diag
"LIBS_DIR: $LibsDir" | Out-File -Encoding utf8 -Append $diag
"OUTPUT: $out" | Out-File -Encoding utf8 -Append $diag
'' | Out-File -Encoding utf8 $errors

$cscCandidates = @(
    'C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe',
    'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
)
$csc = $cscCandidates | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
if (-not $csc) {
    $message = "csc.exe not found. Checked: $($cscCandidates -join ', ')"
    Write-Host $message
    $message | Out-File -Encoding utf8 $errors
    'CSC_NOT_FOUND' | Out-File -Encoding utf8 -Append $diag
    'EXIT_CODE: 1' | Out-File -Encoding utf8 -Append $diag
    exit 1
}

"CSC_OK: $csc" | Out-File -Encoding utf8 -Append $diag
Write-Host "CSC: $csc"

$referenceNames = @(
    'Assembly-CSharp.dll',
    'NeoModLoader.dll',
    'UnityEngine.dll',
    'UnityEngine.CoreModule.dll',
    'UnityEngine.UI.dll',
    'UnityEngine.UIModule.dll',
    'UnityEngine.TextRenderingModule.dll',
    'UnityEngine.InputLegacyModule.dll'
    'UnityEngine.ImageConversionModule.dll',
    'mscorlib.dll',
    'System.dll',
    'System.Core.dll',
    'netstandard.dll',
    'Newtonsoft.Json.dll',
    '0Harmony.dll'
)
$refs = $referenceNames | ForEach-Object { Join-Path $LibsDir $_ }
$missingRefs = @($refs | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
if ($missingRefs.Count -gt 0) {
    $message = "Missing dependencies:`r`n$($missingRefs -join "`r`n")"
    Write-Host $message
    $message | Out-File -Encoding utf8 $errors
    'DEPENDENCY_CHECK_FAILED' | Out-File -Encoding utf8 -Append $diag
    'EXIT_CODE: 2' | Out-File -Encoding utf8 -Append $diag
    exit 2
}
'DEPENDENCIES_OK' | Out-File -Encoding utf8 -Append $diag

$excludedDirectories = @('bin', 'obj', 'evidence', 'tools')
$src = @(Get-ChildItem -LiteralPath $root -Filter '*.cs' -File -Recurse | Where-Object {
    $relativePath = $_.FullName.Substring($root.Length).TrimStart('\', '/')
    $pathParts = $relativePath -split '[\\/]'
    -not ($pathParts | Where-Object { $excludedDirectories -contains $_ })
} | ForEach-Object { $_.FullName })

if ($src.Count -eq 0) {
    $message = "No C# source files found under $root"
    Write-Host $message
    $message | Out-File -Encoding utf8 $errors
    'SOURCE_CHECK_FAILED' | Out-File -Encoding utf8 -Append $diag
    'EXIT_CODE: 3' | Out-File -Encoding utf8 -Append $diag
    exit 3
}
"SOURCE_COUNT: $($src.Count)" | Out-File -Encoding utf8 -Append $diag

try {
    if (-not (Test-Path -LiteralPath $OutputDir -PathType Container)) {
        New-Item -ItemType Directory -Path $OutputDir -Force -ErrorAction Stop | Out-Null
    }
    if (Test-Path -LiteralPath $out -PathType Leaf) {
        Remove-Item -LiteralPath $out -Force -ErrorAction Stop
    }
} catch {
    $message = "Unable to prepare output: $($_.Exception.Message)"
    Write-Host $message
    $message | Out-File -Encoding utf8 $errors
    'OUTPUT_PREPARE_FAILED' | Out-File -Encoding utf8 -Append $diag
    'EXIT_CODE: 4' | Out-File -Encoding utf8 -Append $diag
    exit 4
}

$compilerArgs = @(
    '/target:library',
    "/out:$out",
    '/nostdlib',
    '/noconfig',
    '/langversion:latest',
    '/nowarn:CS0436',
    "/lib:$LibsDir"
)
$compilerArgs += $refs | ForEach-Object { "/reference:$_" }

& $csc @compilerArgs @src *> $errors
$cscExitCode = $LASTEXITCODE

Write-Host "EXIT_CODE: $cscExitCode"
"EXIT_CODE: $cscExitCode" | Out-File -Encoding utf8 -Append $diag
if ($cscExitCode -eq 0) {
    $file = Get-Item -LiteralPath $out -ErrorAction SilentlyContinue
    if ($file) {
        Write-Host "BUILD_OK: $($file.FullName) ($($file.Length) bytes)"
        "BUILD_OK: $($file.Length)" | Out-File -Encoding utf8 -Append $diag
    } else {
        Write-Host "BUILD_OK: csc exited successfully; output file was not found at $out"
        'BUILD_OK: OUTPUT_NOT_FOUND' | Out-File -Encoding utf8 -Append $diag
        exit 5
    }
} else {
    Write-Host "BUILD_FAILED (see $errors)"
    'BUILD_FAILED' | Out-File -Encoding utf8 -Append $diag
}

exit $cscExitCode
} finally {
    if ($buildLockHeld) { $buildMutex.ReleaseMutex() }
    $buildMutex.Dispose()
}
