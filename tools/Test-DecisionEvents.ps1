$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot

function Fail($msg) { Write-Host "DECISION_EVENTS_RED: $msg"; exit 1 }

# ===== 0. events.json 存在且可解析 =====
$jsonPath = Join-Path $Root 'events.json'
if (-not (Test-Path -LiteralPath $jsonPath -PathType Leaf)) { Fail 'events.json missing from mod root' }
try { $json = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json } catch { Fail "events.json is not valid JSON: $_" }
if (-not $json.events -or $json.events.Count -lt 300) { Fail "events.json must define at least 325 events (found $($json.events.Count))" }

# ===== 1. 每事件结构完整性 =====
$validFamilies = @('finance','disaster','court','military','civil','diplomacy')
foreach ($e in $json.events) {
    if (-not $e.id) { Fail 'an event is missing id' }
    if ($validFamilies -notcontains $e.family) { Fail "event $($e.id): family '$($e.family)' not in [finance|disaster|court|military|civil|diplomacy]" }
    if (-not $e.options -or $e.options.Count -lt 2) { Fail "event $($e.id): must define at least 2 options" }
    if ($null -eq $e.fallback -or $e.fallback -lt 0 -or $e.fallback -ge $e.options.Count) { Fail "event $($e.id): fallback index out of range" }
    if (-not $e.timeoutYears -or $e.timeoutYears -lt 1 -or $e.timeoutYears -gt 3) { Fail "event $($e.id): timeoutYears must be 1..3" }
    if ($null -eq $e.cooldownYears) { Fail "event $($e.id): cooldownYears missing" }
    foreach ($o in $e.options) {
        if (-not $o.key) { Fail "event $($e.id): an option is missing key" }
    }
}
$ids = @($json.events | ForEach-Object { $_.id })
$byId = @{}
foreach ($e2 in $json.events) { $byId[$e2.id] = $e2 }
if ($ids.Count -ne ($ids | Sort-Object -Unique).Count) { Fail 'duplicate event ids in events.json' }
foreach ($e in $json.events) {
    if ($e.onlyPlayer -ne $null -and $e.onlyPlayer -ne $true -and $e.onlyPlayer -ne $false) { Fail "event $($e.id): onlyPlayer must be boolean" }
    if ($null -ne $e.phase -and $e.phase -ge 0 -and $e.phase -gt 3) { Fail "event $($e.id): phase must be -1 or 0..3" }
    if ($e.chainNext -and ($ids -notcontains $e.chainNext)) { Fail "event $($e.id): chainNext '$($e.chainNext)' does not exist" }
    if ($null -ne $e.chainAfterOption -and $e.chainAfterOption -ge 0 -and (-not $e.options -or $e.chainAfterOption -ge $e.options.Count)) { Fail "event $($e.id): chainAfterOption out of range" }
    # v1.7.0：条件必须扁平（嵌套 conditions 会被 Newtonsoft 静默忽略——历史 14 事件已在 v1.7.0 转扁平）
    if ($null -ne $e.conditions) { Fail "event $($e.id): nested 'conditions' object forbidden (flatten to top-level keys)" }
    foreach ($ck in @('treasuryRatioMax','treasuryRatioMin','giniMin','giniMax','atWar','bankRiskMin','minPop','maxPop','phase')) {
        if ($e.PSObject.Properties.Name -contains $ck) {
            $v = $e.$ck
            if ($null -ne $v) {
                $numeric = $v -is [int] -or $v -is [double] -or $v -is [decimal]
                if (-not $numeric) { Fail "event $($e.id): '$ck' must be numeric (got $($v.GetType().Name))" }
            }
        }
    }
    if ($null -ne $e.variantGroup) {
        if ($e.variantGroup -isnot [string] -or $e.variantGroup.Length -eq 0) { Fail "event $($e.id): variantGroup must be a non-empty string" }
        if ($e.chainNext) { Fail "event $($e.id): variantGroup event must not also be a chain member (chain units stay whole)" }
    }
    # v1.8.0：稀有度权重 ∈ [0.05, 1]（缺省 1）
    if ($null -ne $e.rarityWeight) {
        $rw = [double]$e.rarityWeight
        if ($rw -lt 0.05 -or $rw -gt 1.0) { Fail "event $($e.id): rarityWeight must be in [0.05, 1] (got $rw)" }
    }
    # v1.8.0：选项效果字段白名单校验
    foreach ($o in $e.options) {
        if ($o.PSObject.Properties.Name -contains 'declareWarTarget') {
            $dw = [int]$o.declareWarTarget
            if ($dw -ne 0 -and $dw -ne 1 -and $dw -ne 2 -and $dw -ne -1) { Fail "event $($e.id): declareWarTarget must be -1/0/1/2 (got $dw)" }
        }
        if ($o.PSObject.Properties.Name -contains 'formAllianceTarget') {
            $fa = [int]$o.formAllianceTarget
            if ($fa -ne 0 -and $fa -ne 1 -and $fa -ne -1) { Fail "event $($e.id): formAllianceTarget must be -1/0/1 (got $fa)" }
        }
        if ($o.PSObject.Properties.Name -contains 'upgradeBuildings') {
            $ub = [int]$o.upgradeBuildings
            if ($ub -lt 0 -or $ub -gt 12) { Fail "event $($e.id): upgradeBuildings must be 0..12 (got $ub)" }
        }
        if ($o.PSObject.Properties.Name -contains 'citizenWealthRatio') {
            $cw = [double]$o.citizenWealthRatio
            if ($cw -lt -1.0 -or $cw -gt 1.0) { Fail "event $($e.id): citizenWealthRatio must be in [-1, 1] (got $cw)" }
        }
        foreach ($fb in @('moveCapital','worldWar')) {
            if ($o.PSObject.Properties.Name -contains $fb -and $o.$fb -ne $true -and $o.$fb -ne $false) {
                Fail "event $($e.id): '$fb' must be boolean"
            }
        }
    }
}
# 链无环（chainNext 图中每个 id 沿链不超过事件总数）
foreach ($e in $json.events) {
    $seen = @{}
    $nextId = $e.id
    while ($null -ne $nextId -and $nextId.Length -gt 0) {
        if ($seen.ContainsKey($nextId)) { Fail "event $($e.id): chainNext cycle at '$nextId'" }
        $seen[$nextId] = $true
        $def = $byId[$nextId]
        if ($null -ne $def -and $def.chainNext) { $nextId = $def.chainNext } else { $nextId = $null }
    }
}
# 变体组内事件 id 唯一（组定义本身）
$groupSets = @{}
foreach ($e in $json.events) {
    if ($e.variantGroup) {
        if (-not $groupSets.ContainsKey($e.variantGroup)) { $groupSets[$e.variantGroup] = @{} }
        $groupSets[$e.variantGroup][$e.id] = $true
    }
}

# ===== 2. 产品代码接线 =====
$src = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\DecisionEvents.cs'))
if ($src -notmatch 'JsonConvert\.DeserializeObject<EventsFile>') { Fail 'DecisionEvents must deserialize events.json via JsonConvert' }
if ($src -notmatch 'WarnOnce\(') { Fail 'DecisionEvents must fail-open with WarnOnce on load errors' }
if ($src -notmatch 'MaxPending\s*=\s*8') { Fail 'DecisionEvents pending pool must be bounded (MaxPending=8)' }
if ($src -notmatch 'BuildWorldPool\(') { Fail 'DecisionEvents must build the per-run event pool (BuildWorldPool)' }
if ($src -notmatch 'current_world_seed_id') { Fail 'DecisionEvents must derive the pool from the world seed' }
if ($src -notmatch 'PoolActive\(') { Fail 'EvaluateYear must filter candidates by PoolActive' }
if ($src -notmatch 'FamilyWeight\(') { Fail 'EvaluateYear must weight candidates by family bias (FamilyWeight)' }
if ($src -notmatch 'StartWarBetween\(' -or $src -notmatch 'TryWorldWar\(' -or $src -notmatch 'TryUpgradeBuildings\(') { Fail 'v1.8 effects must wire StartWarBetween/TryWorldWar/TryUpgradeBuildings' }

$pipeline = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\AnnualPipeline.cs'))
if ($pipeline -notmatch 'Nation,\s*\r?\n\s*Events,\s*\r?\n\s*Snapshot') { Fail 'AnnualStage enum must declare Events between Nation and Snapshot' }
if ($pipeline -notmatch 'case AnnualStage\.Events:[\s\S]*?DecisionEvents\.EvaluateYear') { Fail 'RunStage must call DecisionEvents.EvaluateYear in the Events stage' }

$main = [System.IO.File]::ReadAllText((Join-Path $Root 'EconomyModMain.cs'))
if ($main -notmatch 'DecisionEvents\.Load\(\)') { Fail 'OnModLoad must call DecisionEvents.Load' }
if ($main -notmatch 'DecisionEvents\.Reset\(\)') { Fail 'ResetAllEngines must call DecisionEvents.Reset' }
if ($main -notmatch 'DecisionEvents\.PopupQueued') { Fail 'snapshot tail must consume the popup queue (no mid-pipeline UI)' }

# ===== 3. 存档读写对称（NationSave 三键） =====
$save = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\NationSave.cs'))
foreach ($k in @('rb_ev_pending', 'rb_ev_cooldown', 'rb_ev_lastGlobal')) {
    if ($save -notmatch [regex]::Escape($k)) { Fail "NationSave must persist $k" }
}
if ($save -notmatch 'DecisionEvents\.Serialize' -or $save -notmatch 'DecisionEvents\.Restore') { Fail 'NationSave must round-trip DecisionEvents state' }

# ===== 4. 事件流：TypeDecision 为史书级事件 =====
$ess = [System.IO.File]::ReadAllText((Join-Path $Root 'Services\EventStreamService.cs'))
if ($ess -notmatch 'TypeDecision = "ev_decision"') { Fail 'EventStreamService must declare TypeDecision' }
$majorBlock = [regex]::Match($ess, 'IsMajorType[\s\S]*?return true;[\s\S]*?default:').Value
if ($majorBlock -notmatch 'TypeDecision') { Fail 'TypeDecision must be a major (history-grade) event type' }

# ===== 5. 四语键齐全（每事件 6 键 + UI/配置键） =====
$localeDir = Join-Path $Root 'Locales'
$required = @()
foreach ($e in $json.events) {
    $required += @("ev_$($e.id)", "ev_$($e.id)_desc")
    for ($i = 1; $i -le $e.options.Count; $i++) {
        $required += @("ev_$($e.id)_opt$i", "ev_$($e.id)_res$i")
    }
}
$required += @('event_choice_title','event_choice_header','event_choice_countdown','event_choice_none',
    'event_choice_next','event_choice_cost','event_choice_gain','event_choice_tax','event_choice_relief',
    'event_choice_goodwill','event_choice_unrest','toast_event_pending','cabinet_pending_row','cabinet_pending_open',
    'events_filter_all','events_filter_decision','events_filter_politics','events_filter_economy',
    'events_fold_year','events_year_hdr','ev_desc_decision',
    'event_chance_player','event_chance_player Description','event_chance_ai','event_chance_ai Description',
    'event_cooldown_years','event_cooldown_years Description')
foreach ($loc in @('ch.json','zh_tw.json','en.json','ru.json')) {
    $lp = Join-Path $localeDir $loc
    if (-not (Test-Path -LiteralPath $lp -PathType Leaf)) { Fail "locale file not found: $loc" }
    $loc2 = Get-Content -LiteralPath $lp -Raw -Encoding UTF8 | ConvertFrom-Json
    $locProps = $loc2.PSObject.Properties.Name
    foreach ($key in $required) {
        if ($locProps -notcontains $key) { Fail "$loc missing key '$key'" }
    }
}

Write-Host 'DECISION_EVENTS_GREEN: json integrity, pipeline wiring, save round-trip, major-type and 4-locale coverage all pass'
exit 0
