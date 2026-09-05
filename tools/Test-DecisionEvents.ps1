$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot

function Fail($msg) { Write-Host "DECISION_EVENTS_RED: $msg"; exit 1 }

# ===== 0. events.json 存在且可解析 =====
$jsonPath = Join-Path $Root 'events.json'
if (-not (Test-Path -LiteralPath $jsonPath -PathType Leaf)) { Fail 'events.json missing from mod root' }
try { $json = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json } catch { Fail "events.json is not valid JSON: $_" }
if (-not $json.events -or $json.events.Count -lt 16) { Fail "events.json must define at least 16 events (found $($json.events.Count))" }

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
if ($ids.Count -ne ($ids | Sort-Object -Unique).Count) { Fail 'duplicate event ids in events.json' }

# ===== 2. 产品代码接线 =====
$src = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\DecisionEvents.cs'))
if ($src -notmatch 'JsonConvert\.DeserializeObject<EventsFile>') { Fail 'DecisionEvents must deserialize events.json via JsonConvert' }
if ($src -notmatch 'WarnOnce\(') { Fail 'DecisionEvents must fail-open with WarnOnce on load errors' }
if ($src -notmatch 'MaxPending\s*=\s*8') { Fail 'DecisionEvents pending pool must be bounded (MaxPending=8)' }

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
    $required += @("ev_$($e.id)", "ev_$($e.id)_desc", "ev_$($e.id)_opt1", "ev_$($e.id)_opt2", "ev_$($e.id)_res1", "ev_$($e.id)_res2")
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
