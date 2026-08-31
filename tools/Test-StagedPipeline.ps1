param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'

$pipelinePath = Join-Path $Root 'Core\AnnualPipeline.cs'
$mainPath = Join-Path $Root 'EconomyModMain.cs'
$configPath = Join-Path $Root 'Models\UnrestConfig.cs'
$cbPath = Join-Path $Root 'Services\ConfigCallbacks.cs'
$jsonPath = Join-Path $Root 'default_config.json'
$localeDir = Join-Path $Root 'Locales'

function Fail([string]$message) {
    Write-Host "STAGED_PIPELINE_RED: $message"
    exit 1
}

foreach ($p in @($pipelinePath, $mainPath, $configPath, $cbPath, $jsonPath)) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
        Fail "source file not found: $p"
    }
}

$pipeline = [System.IO.File]::ReadAllText($pipelinePath)
$main = [System.IO.File]::ReadAllText($mainPath)
$config = [System.IO.File]::ReadAllText($configPath)
$cb = [System.IO.File]::ReadAllText($cbPath)
$json = [System.IO.File]::ReadAllText($jsonPath)

$singleline = [System.Text.RegularExpressions.RegexOptions]::Singleline

function Test-Anchor([string]$haystack, [string]$pattern, [string]$label) {
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($haystack, $pattern, $singleline)) {
        Fail $label
    }
}

# ---- green anchors: pipeline state machine ----

# 1) AnnualStage enum declares the exact original closeout order (EraEvaluate before EraTick,
#    Snapshot strictly last before Done; mirrors the pre-split FinishCycle).
$stageOrder = 'enum AnnualStage\s*\{[^}]*TradeFlows,\s*WealthTax,\s*CycleModulator,\s*Unrest,\s*Policy,\s*KingdomMonitor,\s*SocialCrisis,\s*Population,\s*Spending,\s*EraEvaluate,\s*EraTick,\s*TradePower,\s*Disaster,\s*Banking,\s*Nation,\s*Snapshot,\s*Done'
Test-Anchor $pipeline $stageOrder 'AnnualStage enum must declare the closeout order (TradeFlows..Banking, Nation, Snapshot, Done)'

# 2) frame budget: derived from UnrestConfig.FrameBudgetMs and enforced per frame; Snapshot exempt.
Test-Anchor $pipeline 'int budgetMs = cfg\.FrameBudgetMs' 'Tick must derive the frame budget from UnrestConfig.FrameBudgetMs'
Test-Anchor $pipeline '_cursor != AnnualStage\.Snapshot && ElapsedMs\(frameStart\) >= budgetMs' 'Tick must enforce the frame budget (Snapshot exempt from slicing)'

# 3) closeout window: CycleWindowMs gates the window; hard cap 5000ms triggers the reduction fallback.
Test-Anchor $pipeline 'totalMs > cfg\.CycleWindowMs' 'Tick must enforce the closeout window via CycleWindowMs'
Test-Anchor $pipeline 'totalMs > HardWindowMs && !_reduced' 'Exceeding the hard window must trigger the reduction fallback'
Test-Anchor $pipeline 'private const long HardWindowMs = 5000L' 'HardWindowMs must be 5000ms'

# 4) fallback reduction order: spending first, banking second; tax path never reduced.
Test-Anchor $pipeline 'case AnnualStage\.Spending:.*if \(!_reduced\) SpendingEngine\.RunOncePerYear\(\)' 'Fallback reduction must skip SpendingEngine first'
Test-Anchor $pipeline 'case AnnualStage\.Banking:.*if \(!_reduced\) BankingEngine\.Evaluate\(\)' 'Fallback reduction must skip BankingEngine second'

# 5) snapshot/UI only after all stages: Snapshot is the final stage and calls the completion hook.
Test-Anchor $pipeline 'case AnnualStage\.Snapshot:.*EconomyMod\.EconomyModMain\.WriteCycleSnapshot' 'Snapshot stage must call EconomyModMain.WriteCycleSnapshot'
Test-Anchor $main 'public static void WriteCycleSnapshot\(int year\)' 'EconomyModMain must expose WriteCycleSnapshot(int year)'
Test-Anchor $main 'WriteCycleSnapshot\(int year\).*HistoryService\.AppendSnapshot\(snapshot\).*EconomyUI\.RefreshOverview\(\)' 'Snapshot/UI tail must live inside WriteCycleSnapshot'
Test-Anchor $main 'CopyTopBalances\(last != null \? last\.CityBalances : null, 40\)' 'WriteCycleSnapshot must preserve the CopyTopBalances(...,40) pinned pattern'

# ---- green anchors: pipeline wiring in EconomyModMain ----

Test-Anchor $pipeline 'public static bool IsSettling' 'AnnualPipeline must expose IsSettling'
Test-Anchor $main 'if \(AnnualPipeline\.IsSettling\)\s*\{?\s*AnnualPipeline\.Tick\(\)' 'Update must tick the pipeline while IsSettling'
Test-Anchor $main '!TradeSimulationWorker\.IsBusy\(\) && !AnnualPipeline\.IsSettling' 'ManualCollect must guard against in-flight settlement'
Test-Anchor $main 'TradeSimulationWorker\.IsBusy\(\) \|\| AnnualPipeline\.IsSettling' 'RealTimeRefresh must guard against in-flight settlement'
Test-Anchor $main '!AnnualPipeline\.IsSettling && World\.world != null' 'Update realtime path must skip during in-flight settlement'
Test-Anchor $main 'if \(AnnualPipeline\.IsSettling\) return false;' 'RunOneCycle must reject submissions while settlement is in flight'
Test-Anchor $main '_pendingYear = -1;\s*AnnualPipeline\.Abort\(\)' 'World-null branch must abort the pipeline'
Test-Anchor $main '_cyclePending = false;\s*AnnualPipeline\.Abort\(\)' 'Year-rollback branch must abort the pipeline'

# ---- green anchors: config chain ----

Test-Anchor $config 'public int FrameBudgetMs = 4;' 'UnrestConfig must default FrameBudgetMs to 4'
Test-Anchor $config 'public int CycleWindowMs = 2000;' 'UnrestConfig must default CycleWindowMs to 2000'
Test-Anchor $cb 'public static void OnFrameBudgetChanged\(string pValue\)' 'ConfigCallbacks must expose OnFrameBudgetChanged(string)'
Test-Anchor $cb 'public static void OnCycleWindowChanged\(string pValue\)' 'ConfigCallbacks must expose OnCycleWindowChanged(string)'
Test-Anchor $cb 'u\.FrameBudgetMs = ParseInt\(fbm\.TextVal, u\.FrameBudgetMs, 1, 100\)' 'SyncFromModConfig must bound FrameBudgetMs to 1..100'
Test-Anchor $cb 'u\.CycleWindowMs = ParseInt\(cwm\.TextVal, u\.CycleWindowMs, 100, 10000\)' 'SyncFromModConfig must bound CycleWindowMs to 100..10000'
Test-Anchor $cb '"frame_budget_ms", "cycle_window_ms"' 'AllConfigIds must contain frame_budget_ms and cycle_window_ms'
Test-Anchor $json '"Id": "frame_budget_ms",\s*"Type": "TEXT",\s*"TextVal": "4",\s*"Callback": "EconomyConfigCallbacks:OnFrameBudgetChanged"' 'frame_budget_ms must be TEXT with default 4 and callback'
Test-Anchor $json '"Id": "cycle_window_ms",\s*"Type": "TEXT",\s*"TextVal": "2000",\s*"Callback": "EconomyConfigCallbacks:OnCycleWindowChanged"' 'cycle_window_ms must be TEXT with default 2000 and callback'

# ---- green anchors: locales (4 keys in all 4 files) ----

foreach ($localeFile in @('ch.json', 'en.json', 'zh_tw.json', 'ru.json')) {
    $lp = Join-Path $localeDir $localeFile
    if (-not (Test-Path -LiteralPath $lp -PathType Leaf)) {
        Fail "locale file not found: $lp"
    }
    $loc = [System.IO.File]::ReadAllText($lp)
    foreach ($key in @('frame_budget_ms', 'frame_budget_ms Description', 'cycle_window_ms', 'cycle_window_ms Description')) {
        Test-Anchor $loc ('"' + [System.Text.RegularExpressions.Regex]::Escape($key) + '"') "$localeFile must define $key"
    }
}

# ---- green anchors: fallback-order comments preserved in EconomyModMain (audit 10e) ----

Test-Anchor $main 'consumption\(spending\) -> banking -> other' 'Fallback-order comment (consumption -> banking -> other) must stay in EconomyModMain'
Test-Anchor $main 'over-budget fallback reduction order: spending caps -> banking caps -> other stages' 'English fallback-order comment must stay in EconomyModMain'
Test-Anchor $main 'the wealth-tax/redistribution path is never reduced \(tax conservation\)' 'Tax-conservation comment must stay in EconomyModMain'

Write-Host 'STAGED_PIPELINE_GREEN: all anchors match'

# ---- mutation suite: every mutation must turn RED ----

function Test-PipelineContent([string]$content) {
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($content, $stageOrder, $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($content, 'int budgetMs = cfg\.FrameBudgetMs', $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($content, '_cursor != AnnualStage\.Snapshot && ElapsedMs\(frameStart\) >= budgetMs', $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($content, 'totalMs > cfg\.CycleWindowMs', $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($content, 'totalMs > HardWindowMs && !_reduced', $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($content, 'case AnnualStage\.Spending:.*if \(!_reduced\) SpendingEngine\.RunOncePerYear\(\)', $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($content, 'case AnnualStage\.Banking:.*if \(!_reduced\) BankingEngine\.Evaluate\(\)', $singleline)) { return $false }
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($content, 'case AnnualStage\.Snapshot:.*EconomyMod\.EconomyModMain\.WriteCycleSnapshot', $singleline)) { return $false }
    return $true
}

function Test-Mutation([string]$name, [string]$mutated) {
    if (Test-PipelineContent $mutated) {
        Fail "mutation '$name' did not turn RED (gate not sensitive)"
    }
    Write-Host "MUTATION_RED_OK: $name"
}

if (-not (Test-PipelineContent $pipeline)) {
    Fail 'green pipeline content fails pipeline-side anchors'
}

# M1: stage order broken (Banking and Disaster swapped in the enum)
Test-Mutation 'stage-order' ($pipeline -replace 'TradePower,\s*Disaster,\s*Banking,', 'TradePower, Banking, Disaster,')

# M2: frame budget removed (per-frame slice condition dropped)
Test-Mutation 'frame-budget' ($pipeline -replace '_cursor != AnnualStage\.Snapshot && ElapsedMs\(frameStart\) >= budgetMs', '_cursor != AnnualStage.Snapshot')

# M3: closeout window deadline removed
Test-Mutation 'window-deadline' ($pipeline -replace 'totalMs > cfg\.CycleWindowMs', 'totalMs > 999999999')

# M4: snapshot moved before completion (Snapshot case renamed to Banking)
Test-Mutation 'snapshot-not-last' ($pipeline -replace 'case AnnualStage\.Snapshot:', 'case AnnualStage.Banking:')

# M5: fallback reduction removed (spending and banking never skipped)
$m5 = $pipeline -replace 'if \(!_reduced\) SpendingEngine\.RunOncePerYear\(\);', 'SpendingEngine.RunOncePerYear();'
$m5 = $m5 -replace 'if \(!_reduced\) BankingEngine\.Evaluate\(\);', 'BankingEngine.Evaluate();'
Test-Mutation 'fallback-reduction' $m5

Write-Host 'STAGED_PIPELINE_OK: green anchors + 5 mutations RED'
exit 0