param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'

$mainPath = Join-Path $Root 'EconomyModMain.cs'
$uiPath = Join-Path $Root 'UI\EconomyUI.cs'
$hudPath = Join-Path $Root 'UI\EconomyHUD.cs'
$localeDir = Join-Path $Root 'Locales'

function Fail([string]$message) {
    Write-Host "PIPELINE_UI_RED: $message"
    exit 1
}

foreach ($p in @($mainPath, $uiPath, $hudPath)) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
        Fail "source file not found: $p"
    }
}

$main = [System.IO.File]::ReadAllText($mainPath)
$ui = [System.IO.File]::ReadAllText($uiPath)
$hud = [System.IO.File]::ReadAllText($hudPath)

$singleline = [System.Text.RegularExpressions.RegexOptions]::Singleline

function Test-Anchor([string]$haystack, [string]$pattern, [string]$label) {
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($haystack, $pattern, $singleline)) {
        Fail $label
    }
}

# ---- green anchors: settlement-period UI state ----

# 1) per-frame driver: Update drives the UI state from AnnualPipeline.IsSettling
Test-Anchor $main 'EconomyUI\.ApplySettlingState\(AnnualPipeline\.IsSettling\)' 'Update must drive EconomyUI.ApplySettlingState from AnnualPipeline.IsSettling'

# 2) EconomyUI exposes the state entry point with a change guard
Test-Anchor $ui 'public static void ApplySettlingState\(bool settling\)' 'EconomyUI must expose ApplySettlingState(bool)'
Test-Anchor $ui 'if \(_settling == settling\) return;' 'ApplySettlingState must skip redundant work when state is unchanged'

# 3) buttons are disabled during settlement and restored after
Test-Anchor $ui 'SetButtonInteractable\(_btnCollect, settling\);' 'ApplySettlingState must disable the collect button'
Test-Anchor $ui 'SetButtonInteractable\(_btnCyclePhase, settling\);' 'ApplySettlingState must disable the cycle-phase button'
Test-Anchor $ui 'if \(settling\) b\.interactable = false;' 'Settlement must disable the collect/phase buttons'
Test-Anchor $ui 'else b\.interactable = true;' 'Settlement end must restore the collect/phase buttons'

# 4) the panel renders the settling marker + hint while settlement is in flight
Test-Anchor $hud 'if \(AnnualPipeline\.IsSettling\)' 'RefreshCurrentSection must branch on AnnualPipeline.IsSettling'
Test-Anchor $hud 'AddLine\(UIHelpers\.L\("settling_marker"\)' 'Panel must render the settling marker line'
Test-Anchor $hud 'UIHelpers\.L\("settling_hint"\)' 'Panel must render the settling hint text'

# 5) locale keys in all 4 files
foreach ($localeFile in @('ch.json', 'en.json', 'zh_tw.json', 'ru.json')) {
    $lp = Join-Path $localeDir $localeFile
    if (-not (Test-Path -LiteralPath $lp -PathType Leaf)) {
        Fail "locale file not found: $lp"
    }
    $loc = [System.IO.File]::ReadAllText($lp)
    foreach ($key in @('settling_marker', 'settling_hint')) {
        Test-Anchor $loc ('"' + [System.Text.RegularExpressions.Regex]::Escape($key) + '"') "$localeFile must define $key"
    }
}

Write-Host 'PIPELINE_UI_GREEN: all anchors match'

# ---- mutation suite: every mutation must turn RED ----

function Test-PipelineContent([string]$cMain, [string]$cUi, [string]$cHud) {
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($cMain, 'EconomyUI\.ApplySettlingState\(AnnualPipeline\.IsSettling\)', $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($cUi, 'public static void ApplySettlingState\(bool settling\)', $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($cUi, 'if \(_settling == settling\) return;', $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($cUi, 'SetButtonInteractable\(_btnCollect, settling\);', $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($cUi, 'SetButtonInteractable\(_btnCyclePhase, settling\);', $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($cUi, 'if \(settling\) b\.interactable = false;', $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($cUi, 'else b\.interactable = true;', $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($cHud, 'if \(AnnualPipeline\.IsSettling\)', $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($cHud, 'AddLine\(UIHelpers\.L\("settling_marker"\)', $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($cHud, 'UIHelpers\.L\("settling_hint"\)', $singleline)) { return $false }
    return $true
}

function Test-Mutation([string]$name, [string]$cMain, [string]$cUi, [string]$cHud) {
    if (Test-PipelineContent $cMain $cUi $cHud) {
        Fail "mutation '$name' did not turn RED (gate not sensitive)"
    }
    Write-Host "MUTATION_RED_OK: $name"
}

if (-not (Test-PipelineContent $main $ui $hud)) {
    Fail 'green sources fail pipeline-UI anchors'
}

# M1: settling marker removed from the panel
Test-Mutation 'marker-removed' $main $ui ($hud -replace 'AddLine\(UIHelpers\.L\("settling_marker"\)[^;]+;', 'AddLine(UIHelpers.L("chart_no_data"));')

# M2: disable guard removed (buttons stay enabled during settlement)
Test-Mutation 'disable-guard' $main ($ui -replace 'if \(settling\) b\.interactable = false;', 'if (settling) { b.interactable = true; }') $hud

# M3: restore path removed (buttons stay disabled after settlement)
Test-Mutation 'restore-removed' $main ($ui -replace 'else b\.interactable = true;', 'else b.interactable = false;') $hud

# M4: cycle-phase call site removed (cycle-phase button stays enabled during settlement)
Test-Mutation 'cycle-phase-call-removed' $main ($ui -replace 'SetButtonInteractable\(_btnCyclePhase, settling\);', '') $hud

Write-Host 'PIPELINE_UI_OK: green anchors + 4 mutations RED'
exit 0