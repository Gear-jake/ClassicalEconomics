param(
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$failures = New-Object System.Collections.Generic.List[string]

function Assert-SourcePattern {
    param(
        [string]$RelativePath,
        [string]$Pattern,
        [string]$Message
    )

    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures.Add("Missing file: $RelativePath")
        return
    }
    $text = [System.IO.File]::ReadAllText($path)
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($text, $Pattern,
            [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
        $failures.Add($Message)
    }
}

if (-not $SkipBuild) {
    & (Join-Path $root 'build_local.ps1')
    if ($LASTEXITCODE -ne 0) {
        $failures.Add("Build failed with exit code $LASTEXITCODE")
    }
}

$sourceCount = @(Get-ChildItem -LiteralPath $root -Filter '*.cs' -File -Recurse | Where-Object {
    $_.FullName -notmatch '[\\/](bin|obj|evidence|tools)[\\/]'
}).Count
if ($sourceCount -lt 39) {
    $failures.Add("Expected at least 39 C# source files, found $sourceCount")
}

Assert-SourcePattern 'build_local.ps1' '\$excludedDirectories\s*=\s*@\([^\)]*''tools''' `
    'Build source discovery does not exclude test and benchmark tools.'
Assert-SourcePattern 'build_local.ps1' 'ClassicalEconomicsBuild_\$mutexHash.*AbandonedMutexException.*finally.*ReleaseMutex' `
    'Build script does not serialize access to shared diagnostics and output files.'
Assert-SourcePattern 'build_local.ps1' '\$mutexScope\s*=\s*\[System\.IO\.Path\]::GetFullPath\(\$PSScriptRoot\)\.ToUpperInvariant\(\)' `
    'Build mutex is not scoped to the repository that owns the shared diagnostics files.'
Assert-SourcePattern 'Core\DamageTracker.cs' 'MaxAttackersPerVictim\s*=\s*16' `
    'Damage history is missing its per-victim attacker bound.'
Assert-SourcePattern 'Core\BankingEngine.cs' 'bool firstContagion = !_contagionLossByKingdom\.TryGetValue\(kvp\.Key, out accumulatedLoss\);.*if \(firstContagion\) LastContagions\+\+;' `
    'Banking contagion count still double-counts the same affected kingdom.'
Assert-SourcePattern 'Core\SocialCrisisEngine.cs' 'if \(receiverKingdoms\.Count == 0\) return 0L;.*long actualExtract = GameHelpers\.DeductCoins\(units, extract\);.*if \(actualExtract <= 0\) return 0L;.*long perKingdom = actualExtract / receiverKingdoms\.Count;.*return actualExtract;' `
    'Revolution redistribution still pays requested value instead of actual deductions.'
Assert-SourcePattern 'Core\SocialCrisisEngine.cs' 'long per = amount / poorCount;.*long remain = amount - per \* poorCount;.*long give = per \+ \(first \? remain : 0L\);.*GameHelpers\.AddPositiveMoney\(a, give\);.*given \+= give;' `
    'War plunder relief can still lose small remainders or truncate large transfers.'
Assert-SourcePattern 'Core\SocialCrisisEngine.cs' 'if \(transfer > 0 && HasPoorRecipients\(loserUnits, winnerUnits\)\).*actual = GameHelpers\.DeductCoins\(loserUnits, transfer\);' `
    'Transferable plunder can still be deducted when no poor recipient exists.'
Assert-SourcePattern 'Core\SocialCrisisEngine.cs' 'var receiverKingdoms = _kingdomPool;.*receiverKingdoms\.Add\(target\);.*long kingdomGive = perKingdom \+ \(i == 0 \? kingdomRemainder : 0L\);.*long actorGive = perActor \+ \(first \? actorRemainder : 0L\);.*GameHelpers\.AddPositiveMoney\(a, actorGive\);' `
    'Revolution receiver distribution can still lose invalid targets or integer remainders.'
Assert-SourcePattern 'Core\GameHelpers.cs' '_kingdomById\.TryGetValue' `
    'Kingdom lookup is not using the O(1) index.'
Assert-SourcePattern 'Core\GameHelpers.cs' 'wealth = a\.money \+ a\.loot;.*if \(float\.IsNaN\(wealth\) \|\| float\.IsInfinity\(wealth\)\).*wealth = 0f;.*return false;' `
    'Non-finite wealth is not rejected at the shared collection boundary.'
Assert-SourcePattern 'Core\GameHelpers.cs' 'public static bool IsCivilizedActor\(Actor actor\).*actor\.city != null.*actor\.hasKingdom\(\).*actor\.kingdom != null.*return false;' `
    'Civilized-actor eligibility is not defined by city-or-kingdom membership.'
Assert-SourcePattern 'Core\DataCollector.cs' 'private static void ReturnEntry\(RichEntryData e\).*e\.Name = null;.*e\.Kingdom = null;.*e\.Wealth = 0f;.*e\.Id = 0L;.*_entryPool\.Add\(e\);' `
    'Rich-entry pool still retains old-world names and values.'
Assert-SourcePattern 'Core\DataCollector.cs' 'poor\.Clear\(\).*rich\.Clear\(\).*foreach \(var actor in aliveList\).*if \(w < poorLine\) poor\.Add\(actor\);.*else if \(w > taxLine\) rich\.Add\(actor\);.*if \(poor\.Count == 0\) return;.*long totalTax = 0;.*foreach \(var actor in rich\).*actor\.addMoney\(-charged\)' `
    'Wealth tax can still deduct funds before any recipient exists.'
Assert-SourcePattern 'EconomyModMain.cs' 'InheritanceEngine\.ClearWorldReferences\(\).*SpendingEngine\.Reset\(\)' `
    'Main-menu cleanup is missing inheritance or spending world-reference release.'
Assert-SourcePattern 'UI\EconomyHUD.cs' 'if \(index != _lastIndex\).*ForceRebuildLayoutImmediate' `
    'Chart tooltip layout is not guarded by data-index changes.'
Assert-SourcePattern 'UI\EconomyHUD.cs' 'OnWorldUnavailable\(\).*ClearWorldCaches\(\).*private static void ClearWorldCaches\(\).*_seriesPool\.Clear\(\);.*_chartSeriesEntryPool\.Clear\(\);.*_rankBuf\.Clear\(\);.*_dynIndex\.Clear\(\);.*_dynLastSeen\.Clear\(\);.*_dynSeenBuf\.Clear\(\);.*_keepIds\.Clear\(\);' `
    'Chart pools still retain old-world names and values.'
Assert-SourcePattern 'Services\EventStreamService.cs' 'var entry = major \? _majorEvents\[slot\] : _events\[slot\].*if \(entry == null\) entry = RentEntry\(\)' `
    'Event rings are not reusing overwritten EventEntry instances.'
Assert-SourcePattern 'Services\EventStreamService.cs' 'entry\.KingdomName = null.*_entryPool\.Add\(entry\)' `
    'Event pool return does not release retained event strings.'
Assert-SourcePattern 'EconomyModMain.cs' 'EventStreamService\.Clear\(\).*EconomyUI\.OnWorldUnavailable\(\)' `
    'World exit does not clear events and persistent UI delegates.'
Assert-SourcePattern 'UI\EconomyHUD.cs' 'long inciteTargetId = kingdom\.data\.id.*GameHelpers\.FindKingdom\(inciteTargetId\)' `
    'Kingdom picker listeners still risk retaining world Kingdom objects.'
Assert-SourcePattern 'Core\DamageTracker.cs' 'MaxInactiveScans\s*=\s*10.*_damage\.Remove\(id\)' `
    'Damage histories are missing inactivity expiry.'
Assert-SourcePattern 'Core\InheritanceEngine.cs' 'public long ParentId1.*public long ParentId2.*public long\[\] ChildIds;.*\r?\n\s*public int ChildCount;' `
    'Inheritance records must use fixed-size child buffers (no per-actor relationship lists).'
Assert-SourcePattern 'Core\InheritanceEngine.cs' 'if \(!aliveMap\.ContainsKey\(kv\.Key\)\) deadIds\.Add\(kv\.Key\)' `
    'Inheritance death detection is not reusing the existing alive actor index.'
Assert-SourcePattern 'Core\SpendingEngine.cs' 'BuildCooldownCycles\s*=\s*50' `
    'Native building creation is missing its cross-cycle city cooldown.'
Assert-SourcePattern 'Core\SpendingEngine.cs' 'WeakTierPrice = 30.*MidTierPrice = 80.*StrongTierPrice = 200' `
    'Tiered equipment generation is missing its per-tier price ladder (strong=high cost).'
Assert-SourcePattern 'Core\SpendingEngine.cs' 'private static int CraftTiered\(Actor actor, int budget, int maxRolls\).*while \(budget >= WeakTierPrice && rolls < maxRolls\)' `
    'Tiered equipment purchases must be budget-bounded with a roll cap.'
Assert-SourcePattern 'Core\SpendingEngine.cs' 'RunOncePerYear\(\).*PruneExpiredCityActions\(EconomyEngine\.CycleIndex\).*private static void PruneExpiredCityActions\(int cycle\).*PruneCooldowns\(_lastBuildCycle, cycle, BuildCooldownCycles\)' `
    'Stale building cooldowns still depend on a successful action at cycle multiples.'
Assert-SourcePattern 'Core\TradeSimulationWorker.cs' 'ClearWorldReferences\(\).*_generation\+\+.*_readyResult = null' `
    'World exit does not invalidate pending worker generations and results.'
Assert-SourcePattern 'Core\TradeSimulationWorker.cs' 'int idx = _cycleIndex \+ 1;.*bool queued = ThreadPool\.QueueUserWorkItem.*if \(!queued\) throw new InvalidOperationException.*_cycleIndex = idx;.*return true;' `
    'Rejected worker submission still advances the cycle index.'
Assert-SourcePattern 'Core\EconomyCycleModulator.cs' 'if \(!_initialized\).*MoneySupply = gdp;.*CurrentCPI = 1f;.*float actualStimulus = 0f;.*actualStimulus = \(float\)count \* perActor;.*MoneySupply \+= actualStimulus;.*BubbleValue \+= actualStimulus \* cfg\.BoomBubbleFactor' `
    'CPI and bubble state do not track baseline and actual injected coins.'
Assert-SourcePattern 'Core\EconomyCycleModulator.cs' 'bool bubbleExceeded = bubbleThreshold > 0f && BubbleValue >= bubbleThreshold;.*_highGiniStreak >= cfg\.CycleGiniPeriods \|\|\s*bubbleExceeded' `
    'Zero GDP still triggers an immediate zero-threshold bubble burst.'
Assert-SourcePattern 'Core\EconomyCycleModulator.cs' 'foreach \(var kingdom in GameHelpers\.KingdomSnapshot\(\)\).*if \(!localLow\) SetTrait\(kingdom, TaxLocalLow, false\);.*if \(!localLow\).*return;.*var top = EconomyEngine\.TopKingdoms\(ModulateKingdoms\);' `
    'Former top kingdoms can retain boom tax traits after leaving the top ranking.'
Assert-SourcePattern 'Core\EconomyCycleModulator.cs' 'TaxLocalHigh = "tax_rate_local_high";.*foreach \(var kingdom in GameHelpers\.KingdomSnapshot\(\)\).*SetTrait\(kingdom, TaxLocalHigh, false\);.*if \(!localLow\).*SetTrait\(kingdom, TaxLocalLow, false\);' `
    'Cycle policies do not clear conflicting high and low tax traits.'
Assert-SourcePattern 'Core\PolicyEngine.cs' 'if \(isBoom\).*removeTrait\("tax_rate_local_high"\).*else.*removeTrait\("tax_rate_local_low"\)' `
    'Fiscal policy does not remove the opposite tax trait.'
Assert-SourcePattern 'Core\EconomyEngine.cs' 'ResetCycle\(\).*GlobalGDP = 0f.*KingdomStats\.Clear\(\)' `
    'Economy reset leaves published metrics or kingdom objects retained.'
Assert-SourcePattern 'EconomyModMain.cs' 'if \(currentYear != _lastCollectedYear\)\s*\{\s*if \(RunOneCycle\(currentYear\)\) _lastCollectedYear = currentYear;\s*\}' `
    'Failed annual collection still consumes the game year instead of retrying.'
Assert-SourcePattern 'EconomyModMain.cs' 'private int _pendingYear = -1;.*if \(RunOneCycle\(currentYear\)\) _lastCollectedYear = currentYear;.*private bool RunOneCycle\(int year\).*if \(_cyclePending\) _pendingYear = year;.*int year = _pendingYear >= 0 \? _pendingYear : GetCurrentGameYear\(\);.*_pendingYear = -1;' `
    'Completed background data is labeled with the consumption year instead of its submission year.'

$eventSource = [System.IO.File]::ReadAllText((Join-Path $root 'Services\EventStreamService.cs'))
if ($eventSource -match 'Narrative|BuildNarrative') {
    $failures.Add('Event entries still allocate or retain unused narrative strings.')
}

$inheritanceSource = [System.IO.File]::ReadAllText((Join-Path $root 'Core\InheritanceEngine.cs'))
if ($inheritanceSource -match 'HashSet<long>\s+_seen') {
    $failures.Add('Inheritance scanning still retains a redundant population-sized live-id set.')
}
if ($inheritanceSource -match 'MaxCachedChildren|ChildIds\.Count\s*>=') {
    $failures.Add('Inheritance relationship caching still truncates living children and changes heir behavior.')
}

$helperSource = [System.IO.File]::ReadAllText((Join-Path $root 'Core\GameHelpers.cs'))
if ($helperSource -match 'const int MaxScan|scanned\s*>\s*MaxScan') {
    $failures.Add('Kingdom redistribution still samples a prefix instead of honoring its exact full-kingdom contract.')
}
if (-not [Regex]::IsMatch($helperSource,
        'pool\[edgeIdx\] = a;.*edge = 0f;.*for \(int i = 0; i < pool\.Count; i\+\+\).*edge = richest \? Mathf\.Min\(edge, wi\) : Mathf\.Max\(edge, wi\)',
        [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
    $failures.Add('Top-N replacement does not recompute the true post-replacement boundary.')
}

$config = Get-Content -LiteralPath (Join-Path $root 'default_config.json') -Raw | ConvertFrom-Json
$callbackSource = [System.IO.File]::ReadAllText((Join-Path $root 'Services\ConfigCallbacks.cs'))
foreach ($group in $config.PSObject.Properties) {
    foreach ($setting in $group.Value) {
        if ([string]::IsNullOrWhiteSpace($setting.Callback)) { continue }
        $method = ($setting.Callback -split ':')[-1]
        $parameterType = if ($setting.Type -eq 'SWITCH') { 'bool' } else { 'string' }
        $signature = 'public\s+static\s+void\s+' + [Regex]::Escape($method) + '\s*\(\s*' + $parameterType + '\s+pValue\s*\)'
        if (-not [Regex]::IsMatch($callbackSource, $signature)) {
            $failures.Add("Config callback $method is missing the required $parameterType parameter for $($setting.Id).")
        }
    }
}

# ===== Task 10: annual-cycle performance contract gates =====

$productSources = @(Get-ChildItem -LiteralPath $root -Filter '*.cs' -File -Recurse | Where-Object {
    $_.FullName -notmatch '[\\/](bin|obj|evidence|tools)[\\/]'
})
if ($productSources.Count -eq 0) {
    $failures.Add('GC API gate: no product C# sources found; invariant unverifiable.')
}

# 10a. Forbidden GC APIs (GC.Collect / GCSettings) in any product source.
# GC.Collect is permitted ONLY inside Core\MemoryCleanupEngine.cs AND only on the
# single gated line that also references MemoryCleanupForceGc; GCSettings stays absolute.
# 10c/10d. Track whether the budget fields are consumed outside their config definitions.
$frameBudgetConsumed = $false
$cycleAllocConsumed = $false
foreach ($file in $productSources) {
    $text = [System.IO.File]::ReadAllText($file.FullName)
    if ($file.FullName -notmatch 'Models[\\/]UnrestConfig\.cs$|Services[\\/]ConfigCallbacks\.cs$') {
        if ($text -match 'FrameBudgetMs') { $frameBudgetConsumed = $true }
        if ($text -match 'CycleAllocBudget') { $cycleAllocConsumed = $true }
    }
    $lines = $text.Split("`n")
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -match '\bGCSettings\b') {
            $rel = $file.FullName.Substring($root.Length + 1)
            $failures.Add("Forbidden GC API: $rel line $($i + 1): $($line.Trim())")
        }
        if ($line -match '\bGC\s*\.\s*Collect\s*\(') {
            $isGatedEngine = $file.FullName -match 'Core[\\/]MemoryCleanupEngine\.cs$' -and $line -match 'MemoryCleanupForceGc'
            if (-not $isGatedEngine) {
                $rel = $file.FullName.Substring($root.Length + 1)
                $failures.Add("Forbidden GC API: $rel line $($i + 1): $($line.Trim())")
            }
        }
    }
}

# 10b. No per-cycle collection creation in the annual settlement path.
function Assert-NoCycleAllocation {
    param(
        [string]$RelativePath,
        [string]$RegionStart,
        [string]$RegionEnd,
        [string]$Label
    )

    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $failures.Add("Missing file: $RelativePath")
        return
    }
    $text = [System.IO.File]::ReadAllText($path)
    $startMatch = [regex]::Match($text, [regex]::Escape($RegionStart))
    if (-not $startMatch.Success) {
        $failures.Add("Cycle allocation gate: region start for '$Label' not found in $RelativePath; invariant unverifiable.")
        return
    }
    $regionEndIndex = $text.Length
    if (-not [string]::IsNullOrEmpty($RegionEnd)) {
        $endMatch = [regex]::Match($text, [regex]::Escape($RegionEnd))
        if (-not $endMatch.Success) {
            $failures.Add("Cycle allocation gate: region end for '$Label' not found in $RelativePath; invariant unverifiable.")
            return
        }
        if ($endMatch.Index -lt $startMatch.Index) {
            $failures.Add("Cycle allocation gate: region anchors for '$Label' are out of order in $RelativePath; invariant unverifiable.")
            return
        }
        $regionEndIndex = $endMatch.Index
    }
    $lines = $text.Split("`n")
    $offset = 0
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        $lineStart = $offset
        $offset = $lineStart + $line.Length + 1
        if ($line -notmatch 'new\s+(List|Dictionary)<') { continue }
        if ($lineStart -ge $startMatch.Index -and $lineStart -lt $regionEndIndex) {
            $failures.Add("Cycle allocation gate: per-cycle collection creation inside $Label at line $($i + 1): $($line.Trim())")
        }
    }
}

Assert-NoCycleAllocation 'Core\DataCollector.cs' 'public static void ApplyWealthTax(' 'private static void UpdateTopRich(' 'DataCollector.ApplyWealthTax'
Assert-NoCycleAllocation 'Core\SpendingEngine.cs' 'public static void RunOncePerYear(' 'public static void ClearWorldReferences(' 'SpendingEngine.RunOncePerYear'
Assert-NoCycleAllocation 'Core\BankingEngine.cs' 'public static void Evaluate(' '' 'BankingEngine.Evaluate'

# 10c. Frame budget (4 ms/frame) configuration contract.
$unrestConfigPath = Join-Path $root 'Models\UnrestConfig.cs'
$unrestConfigSource = ''
if (Test-Path -LiteralPath $unrestConfigPath -PathType Leaf) {
    $unrestConfigSource = [System.IO.File]::ReadAllText($unrestConfigPath)
} else {
    $failures.Add('Missing file: Models\UnrestConfig.cs')
}
$frameBudgetItem = $config.economy_general | Where-Object { $_.Id -eq 'frame_budget_ms' }
if (-not $frameBudgetItem) {
    $failures.Add('Frame budget gate: default_config.json lacks config key frame_budget_ms (default 4 ms/frame). Task 7 staged-pipeline budget config is not implemented.')
} else {
    if ($frameBudgetItem.Type -ne 'TEXT') {
        $failures.Add('Frame budget gate: frame_budget_ms must be a TEXT config entry.')
    }
    if ($frameBudgetItem.TextVal -ne '4') {
        $failures.Add('Frame budget gate: frame_budget_ms default must be 4 (ms per frame).')
    }
    if ($frameBudgetItem.Callback -ne 'EconomyConfigCallbacks:OnFrameBudgetChanged') {
        $failures.Add('Frame budget gate: frame_budget_ms must be wired to EconomyConfigCallbacks:OnFrameBudgetChanged.')
    }
}
if (-not [Regex]::IsMatch($callbackSource, 'public\s+static\s+void\s+OnFrameBudgetChanged\s*\(\s*string\s+pValue\s*\)')) {
    $failures.Add('Frame budget gate: ConfigCallbacks.cs is missing OnFrameBudgetChanged(string pValue).')
}
if (-not [Regex]::IsMatch($callbackSource, 'u\.FrameBudgetMs\s*=\s*ParseInt\(\s*[A-Za-z_][A-Za-z0-9_]*\.TextVal\s*,\s*u\.FrameBudgetMs')) {
    $failures.Add('Frame budget gate: SyncFromModConfig must parse frame_budget_ms into UnrestConfig.FrameBudgetMs via bounded ParseInt.')
}
if ($unrestConfigSource.Length -gt 0 -and -not [Regex]::IsMatch($unrestConfigSource, 'public\s+int\s+FrameBudgetMs\s*=\s*4\s*;')) {
    $failures.Add('Frame budget gate: UnrestConfig.cs must declare public int FrameBudgetMs = 4;')
}
if (-not $frameBudgetConsumed) {
    $failures.Add('Frame budget gate: no product source consumes FrameBudgetMs; the annual pipeline must enforce the frame budget.')
}

# 10d. Per-cycle managed-allocation budget configuration contract.
$cycleAllocItem = $config.economy_general | Where-Object { $_.Id -eq 'cycle_alloc_budget' }
if (-not $cycleAllocItem) {
    $failures.Add('Allocation budget gate: default_config.json lacks config key cycle_alloc_budget (per-cycle managed-allocation budget). Task 1 PerfDiagnostics config is not implemented.')
} else {
    if ($cycleAllocItem.Type -ne 'TEXT') {
        $failures.Add('Allocation budget gate: cycle_alloc_budget must be a TEXT config entry.')
    }
    if ($cycleAllocItem.Callback -ne 'EconomyConfigCallbacks:OnCycleAllocBudgetChanged') {
        $failures.Add('Allocation budget gate: cycle_alloc_budget must be wired to EconomyConfigCallbacks:OnCycleAllocBudgetChanged.')
    }
}
if (-not [Regex]::IsMatch($callbackSource, 'public\s+static\s+void\s+OnCycleAllocBudgetChanged\s*\(\s*string\s+pValue\s*\)')) {
    $failures.Add('Allocation budget gate: ConfigCallbacks.cs is missing OnCycleAllocBudgetChanged(string pValue).')
}
if (-not [Regex]::IsMatch($callbackSource, 'u\.CycleAllocBudget\s*=\s*ParseInt\(\s*[A-Za-z_][A-Za-z0-9_]*\.TextVal\s*,\s*u\.CycleAllocBudget')) {
    $failures.Add('Allocation budget gate: SyncFromModConfig must parse cycle_alloc_budget into UnrestConfig.CycleAllocBudget via bounded ParseInt.')
}
if ($unrestConfigSource.Length -gt 0 -and -not [Regex]::IsMatch($unrestConfigSource, 'public\s+(long|int)\s+CycleAllocBudget')) {
    $failures.Add('Allocation budget gate: UnrestConfig.cs must declare a CycleAllocBudget field.')
}
if (-not $cycleAllocConsumed) {
    $failures.Add('Allocation budget gate: no product source consumes CycleAllocBudget; the annual pipeline must measure per-cycle allocations against it.')
}

# 10e. Over-budget fallback reduction order: consumption -> banking -> other,
# with the tax-conservation path (wealth tax / redistribution) never reduced.
$mainPath = Join-Path $root 'EconomyModMain.cs'
$mainSource = ''
if (Test-Path -LiteralPath $mainPath -PathType Leaf) {
    $mainSource = [System.IO.File]::ReadAllText($mainPath)
} else {
    $failures.Add('Missing file: EconomyModMain.cs')
}
$fallbackOrderPattern = '(?is)(?:fallback|reduc|over.?budget).{0,120}(?:spending|consum).{0,250}(?:banking|bank).{0,250}(?:other|remaining)'
if ($mainSource.Length -gt 0 -and -not [Regex]::IsMatch($mainSource, $fallbackOrderPattern)) {
    $failures.Add('Fallback reduction gate: annual pipeline has no consumption->banking->other over-budget fallback order. Task 4 operation caps / task 7 over-budget fallback not implemented.')
}
$taxExclusionPattern = '(?is)(?:fallback|reduc|over.?budget).{0,300}(?:tax|wealthtax).{0,120}(?:never|exclude|skip|conserv)'
if ($mainSource.Length -gt 0 -and -not [Regex]::IsMatch($mainSource, $taxExclusionPattern)) {
    $failures.Add('Fallback reduction gate: no tax-conservation exclusion guard; the wealth-tax/redistribution path must never be reduced by the fallback.')
}

if ($failures.Count -gt 0) {
    Write-Host "PERFORMANCE_AUDIT_FAILED: $($failures.Count) issue(s)"
    foreach ($failure in $failures) { Write-Host " - $failure" }
    exit 1
}

Write-Host "PERFORMANCE_AUDIT_OK: $sourceCount source files, build and invariants passed"
exit 0
