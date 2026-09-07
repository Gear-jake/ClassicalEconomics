# -*- encoding: utf-8 -*-
# Test-EventPool.ps1 —— v1.7.0 每局事件池规则门禁（结构性断言，不重复实现 C# 哈希）。
# 断言：
# 1) 整数域基线：事件数 ≥300、选项 ≥2、chainNext 引用存在、id 唯一；
# 2) 链完整性：链成员（含头/中/尾）沿 chainNext 走到唯一链尾、无环；
#    链事件不得挂 variantGroup（引擎按链尾整链启用，组/链不交叉）；
# 3) 变体组：非空、组内成员 ≥2（互斥才有意义）、组内无链事件；
# 4) 保底可达：每族总数 ≥ max(5, floor(族数*0.45))、onlyPlayer 总数 ≥ 6；
# 5) 引擎常量锚点：保留率 0.68 / 族倾向 [0.65,1.4] 源码常量与 BuildWorldPool 接线。
# 只读 events.json + Core/DecisionEvents.cs，不触碰运行时代码。

param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)
$ErrorActionPreference = 'Stop'
function Fail($msg) { Write-Host "EVENT_POOL_RED: $msg"; exit 1 }

$jsonPath = Join-Path $Root 'events.json'
try { $json = Get-Content -LiteralPath $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json } catch { Fail "events.json not valid JSON: $_" }
$events = @($json.events)

# ---- 1) 整数域基线 ----
if ($events.Count -lt 300) { Fail "event count $($events.Count) below 300" }
$byId = @{}
foreach ($e in $events) { $byId[$e.id] = $e }
$ids = @($events | ForEach-Object { $_.id })
if ($ids.Count -ne ($ids | Sort-Object -Unique).Count) { Fail 'duplicate event ids' }
foreach ($e in $events) {
    if (-not $e.options -or $e.options.Count -lt 2) { Fail "event $($e.id): options < 2" }
    if ($e.chainNext -and -not $byId.ContainsKey($e.chainNext)) { Fail "event $($e.id): chainNext '$($e.chainNext)' missing" }
}

# ---- 2) 链完整性：唯一链尾 + 无环；链/组不交叉 ----
$chainIds = New-Object 'System.Collections.Generic.HashSet[string]'
foreach ($e in $events) {
    if ($e.chainNext) { [void]$chainIds.Add($e.id); [void]$chainIds.Add($e.chainNext) }
}
if ($chainIds.Count -eq 0) { Fail 'no chainNext anywhere (pool has no chains to validate)' }
foreach ($e in $events) {
    if ($e.chainNext -and $e.variantGroup) { Fail "event $($e.id): chain member must not carry variantGroup (chain units stay whole)" }
    if ($e.variantGroup -and $chainIds.Contains($e.id)) { Fail "event $($e.id): variantGroup member is a chain event" }
}
foreach ($id in $chainIds) {
    $cur = $id; $seen = @{}; $guard = 0
    while ($guard++ -lt 16 -and $cur -and $byId[$cur] -and $byId[$cur].chainNext) {
        if ($seen[$cur]) { Fail "chain cycle at '$cur' (member $id)" }
        $seen[$cur] = $true
        $cur = $byId[$cur].chainNext
    }
}

# ---- 3) 变体组合法性 ----
$groups = @{}
foreach ($e in $events) {
    if ($e.variantGroup) {
        if ($e.variantGroup -isnot [string] -or $e.variantGroup.Length -eq 0) { Fail "event $($e.id): variantGroup must be non-empty string" }
        if (-not $groups.ContainsKey($e.variantGroup)) { $groups[$e.variantGroup] = New-Object System.Collections.Generic.List[object] }
        $groups[$e.variantGroup].Add($e)
    }
}
if ($groups.Count -lt 5) { Fail "only $($groups.Count) variant groups (expected >= 5 for per-run variety)" }
foreach ($g in $groups.Values) {
    if ($g.Count -lt 2) { Fail "variantGroup '$($g[0].variantGroup)' has only $($g.Count) members (need >= 2 for mutex)" }
}

# ---- 4) 保底可达 ----
$familyTotal = @{}
$onlyPlayerTotal = 0
foreach ($e in $events) {
    $f = 'civil'
    if ($e.family) { $f = $e.family }
    if (-not $familyTotal.ContainsKey($f)) { $familyTotal[$f] = 0 }
    $familyTotal[$f] = $familyTotal[$f] + 1
    if ($e.onlyPlayer) { $onlyPlayerTotal = $onlyPlayerTotal + 1 }
}
foreach ($f in $familyTotal.Keys) {
    $total = $familyTotal[$f]
    $min = [System.Math]::Max(5, [int][System.Math]::Floor($total * 0.45))
    if ($total -lt $min) { Fail "family '$f': floor $min exceeds total $total (backfill unsatisfiable)" }
}
if ($onlyPlayerTotal -lt 6) { Fail "onlyPlayer total $onlyPlayerTotal below 6 (backfill floor unsatisfiable)" }

# ---- 5) 引擎常量锚点（源码文本；改动须同步本门禁）----
$src = [System.IO.File]::ReadAllText((Join-Path $Root 'Core\DecisionEvents.cs'))
if ($src -notmatch 'PoolRetainRatio\s*=\s*0\.68f') { Fail 'PoolRetainRatio must stay 0.68f' }
if ($src -notmatch 'FamilyBiasMin\s*=\s*0\.65f') { Fail 'FamilyBiasMin must stay 0.65f' }
if ($src -notmatch 'FamilyBiasMax\s*=\s*1\.4f') { Fail 'FamilyBiasMax must stay 1.4f' }
if ($src -notmatch 'BuildWorldPool\(' -or $src -notmatch 'PoolActive\(' -or $src -notmatch 'FamilyWeight\(') {
    Fail 'engine must wire BuildWorldPool/PoolActive/FamilyWeight'
}

Write-Host "EVENT_POOL_GREEN: $($events.Count) events, $($chainIds.Count) chain members, $($groups.Count) variant groups, family/onlyPlayer floors reachable, engine constants anchored"
exit 0
