$csc = 'C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe'
$diag = 'e:\code\new\2026-08-11-16-45-00\ClassicalEconomics\build_diag.txt'
if (-not (Test-Path $csc)) {
    Write-Host "csc.exe not found at $csc, trying .NET Framework"
    $csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    if (-not (Test-Path $csc)) {
        Write-Host "csc.exe not found at $csc either"
        "CSC_NOT_FOUND" | Out-File -Encoding utf8 $diag
        exit 1
    }
}
"CSC_OK: $csc" | Out-File -Encoding utf8 $diag
Write-Host "CSC: $csc"

Set-Location 'e:\code\new\2026-08-11-16-45-00\ClassicalEconomics'
$out = 'bin\EconomyMod.dll'
if (Test-Path $out) { Remove-Item $out -Force }
if (-not (Test-Path 'bin')) { New-Item -ItemType Directory -Path 'bin' | Out-Null }

$src = @(
    'EconomyModMain.cs',
    'Core\DataCollector.cs',
    'Core\DamageTracker.cs',
    'Core\EconomyEngine.cs',
    'Core\EconomyCycleModulator.cs',
    'Core\UnrestEngine.cs',
    'Core\SocialCrisisEngine.cs',
    'Core\InheritanceEngine.cs',
    'Core\SpendingEngine.cs',
    'Core\EraEngine.cs',
    'Models\EconomySnapshot.cs',
    'Models\KingdomStats.cs',
    'Models\UnrestConfig.cs',
    'Services\HistoryService.cs',
    'Services\LocalizationService.cs',
    'Services\EventStreamService.cs',
    'Services\ConfigCallbacks.cs',
    'UI\EconomyUI.cs',
    'UI\EconomyHUD.cs',
    'UI\EventWindow.cs',
    'UI\RichListWindow.cs',
    'UI\FloatingWindow.cs',
    'UI\IconLoader.cs',
    'UI\UIHelpers.cs',
    'UI\UIStyles.cs',
    'UI\UIComponents.cs',
    'Core\GameHelpers.cs',
    'Core\TradeSimulationWorker.cs',
    'Core\LaborEngine.cs',
    'Core\PopulationEngine.cs',
    'Core\PolicyEngine.cs',
    'Core\KingdomMonitorEngine.cs',
    'Core\DisasterEngine.cs',
    'Core\BankingEngine.cs',
    'Core\BiomeEconomy.cs'
)

$libsDir = 'e:\code\new\libs'
$refs = @(
    "$libsDir\Assembly-CSharp.dll",
    "$libsDir\NeoModLoader.dll",
    "$libsDir\UnityEngine.dll",
    "$libsDir\UnityEngine.CoreModule.dll",
    "$libsDir\UnityEngine.UI.dll",
    "$libsDir\UnityEngine.UIModule.dll",
    "$libsDir\UnityEngine.TextRenderingModule.dll",
    "$libsDir\UnityEngine.ImageConversionModule.dll",
    "$libsDir\mscorlib.dll",
    "$libsDir\System.dll",
    "$libsDir\System.Core.dll",
    "$libsDir\netstandard.dll",
    "$libsDir\Newtonsoft.Json.dll"
)

$refArgs = $refs | ForEach-Object { "/reference:$_" }

& $csc `
    /target:library `
    /out:$out `
    /nostdlib `
    /noconfig `
    /langversion:latest `
    /nowarn:CS0436 `
    /nowarn:CS8019 `
    /lib:$libsDir `
    $refArgs `
    $src *> 'e:\code\new\2026-08-11-16-45-00\ClassicalEconomics\errors.txt'

Write-Host ("EXIT_CODE: " + $LASTEXITCODE)
"EXIT_CODE: $LASTEXITCODE" | Out-File -Encoding utf8 -Append $diag
if (Test-Path $out) {
    $f = Get-Item $out
    Write-Host ("BUILD_OK: " + $f.FullName + " (" + $f.Length + " bytes)")
    "BUILD_OK: $($f.Length)" | Out-File -Encoding utf8 -Append $diag
} else {
    Write-Host "BUILD_FAILED"
    "BUILD_FAILED" | Out-File -Encoding utf8 -Append $diag
}
