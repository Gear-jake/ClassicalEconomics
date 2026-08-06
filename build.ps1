$csc = 'C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe'
if (-not (Test-Path $csc)) {
    Write-Host "csc.exe not found at $csc"
    exit 1
}
Write-Host "CSC: $csc"

Set-Location 'e:\code\new\EconomyMod'
$out = 'bin\EconomyMod.dll'
if (Test-Path $out) { Remove-Item $out -Force }

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
    'Core\GameHelpers.cs',
    'Core\TradeSimulationWorker.cs',
    'Core\LaborEngine.cs',
    'Core\PopulationEngine.cs',
    'Core\PolicyEngine.cs',
    'Core\KingdomMonitorEngine.cs'
)

$refs = @(
    '..\libs\Assembly-CSharp.dll',
    '..\libs\NeoModLoader.dll',
    '..\libs\UnityEngine.dll',
    '..\libs\UnityEngine.CoreModule.dll',
    '..\libs\UnityEngine.UI.dll',
    '..\libs\UnityEngine.UIModule.dll',
    '..\libs\UnityEngine.TextRenderingModule.dll',
    '..\libs\UnityEngine.ImageConversionModule.dll',
    '..\libs\mscorlib.dll',
    '..\libs\System.dll',
    '..\libs\System.Core.dll',
    '..\libs\netstandard.dll',
    '..\libs\Newtonsoft.Json.dll'
)

$refArgs = $refs | ForEach-Object { "/reference:$_" }

# Use the Unity libs as framework path override (mscorlib etc.)
$framework = (Resolve-Path '..\libs').Path

& $csc `
    /target:library `
    /out:$out `
    /nostdlib `
    /noconfig `
    /langversion:latest `
    /nowarn:CS0436 `
    /nowarn:CS8019 `
    /lib:$framework `
    $refArgs `
    $src 2>&1

Write-Host ("EXIT_CODE: " + $LASTEXITCODE)
if (Test-Path $out) {
    $f = Get-Item $out
    Write-Host ("BUILD_OK: " + $f.FullName + " (" + $f.Length + " bytes)")
}
