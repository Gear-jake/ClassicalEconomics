param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

# W3 events.json schema 门禁
#
# 抉择事件库是模组的核心内容（346 事件 / 6 族）。它是**数据驱动**的：
# JSON 写错不会编译报错，只会在运行时静默失效（事件不出现 / 选项点了没反应 /
# 连锁断链）。本门禁把 schema 与本地化完整性前置到构建期。

$ErrorActionPreference = 'Stop'

$failures = New-Object System.Collections.Generic.List[string]
function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { $failures.Add($Message) }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)
$evPath = Join-Path $Root 'events.json'
if (-not (Test-Path -LiteralPath $evPath -PathType Leaf)) {
    Write-Host 'EVENTS_SCHEMA_RED: events.json not found'
    exit 1
}

$doc = Get-Content -LiteralPath $evPath -Raw -Encoding UTF8 | ConvertFrom-Json
$events = @($doc.events)
Assert-True ($events.Count -gt 0) 'events.json 未解析出任何事件'

$validFamilies = @('finance','disaster','court','military','civil','diplomacy')
$required = @('id','family','minYear','timeoutYears','fallback','cooldownYears','options')
$numericLevel = @('minYear','timeoutYears','cooldownYears','chainDelay','treasuryRatioMin','treasuryRatioMax','giniMin','giniMax','bankRiskMin','rarityWeight')

$ids = @{}
$chainTargets = @()

for ($i = 0; $i -lt $events.Count; $i++) {
    $e = $events[$i]
    $label = if ($null -ne $e.id) { $e.id } else { "#$i" }

    foreach ($f in $required) {
        Assert-True ($null -ne $e.$f) "事件 $label 缺少必填字段 $f"
    }
    if ($null -eq $e.id) { continue }

    # id 唯一且非空
    Assert-True ($e.id -is [string] -and $e.id.Length -gt 0) "事件 $label 的 id 非法"
    if ($ids.ContainsKey($e.id)) { $failures.Add("事件 id 重复: $($e.id)") } else { $ids[$e.id] = $true }

    # family 合法
    Assert-True ($validFamilies -contains $e.family) "事件 $label 的 family 非法: $($e.family)"

    # 数值字段非负 / 合法
    foreach ($f in $numericLevel) {
        if ($null -ne $e.$f) {
            $v = [double]$e.$f
            Assert-True ($v -ge 0) "事件 $label 的 $f 为负: $v"
        }
    }

    # options 非空 + key 唯一
    $opts = @($e.options)
    Assert-True ($opts.Count -gt 0) "事件 $label 没有任何选项"
    $keys = @{}
    foreach ($o in $opts) {
        Assert-True ($null -ne $o.key -and $o.key.Length -gt 0) "事件 $label 有选项缺少 key"
        if ($null -ne $o.key) {
            if ($keys.ContainsKey($o.key)) { $failures.Add("事件 $label 的选项 key 重复: $($o.key)") } else { $keys[$o.key] = $true }
        }
    }

    # fallback 必须是合法选项下标
    if ($null -ne $e.fallback) {
        $fb = [int]$e.fallback
        Assert-True ($fb -ge 0 -and $fb -lt $opts.Count) "事件 $label 的 fallback 越界: $fb（选项数 $($opts.Count)）"
    }

    # chainAfterOption 必须是合法下标
    if ($null -ne $e.chainAfterOption) {
        $ca = [int]$e.chainAfterOption
        Assert-True ($ca -ge 0 -and $ca -lt $opts.Count) "事件 $label 的 chainAfterOption 越界: $ca"
    }

    # 连锁目标登记（稍后统一校验存在性）
    if ($null -ne $e.chainNext -and "$($e.chainNext)".Length -gt 0) {
        $chainTargets += ,@($label, "$($e.chainNext)")
    }
}

# chainNext 必须指向存在的事件（断链会导致剧情弧永远走不完）
foreach ($t in $chainTargets) {
    if (-not $ids.ContainsKey($t[1])) {
        $failures.Add("事件 $($t[0]) 的 chainNext 指向不存在的事件: $($t[1])")
    }
}

# 本地化：每个事件在 6 语言下都要有 标题 / 描述 / 每个选项的文案
$locales = @('ch.json','zh_tw.json','en.json','ja.json','de.json','ru.json')
foreach ($loc in $locales) {
    $lp = Join-Path (Join-Path $Root 'Locales') $loc
    if (-not (Test-Path -LiteralPath $lp -PathType Leaf)) { $failures.Add("locale file not found: $loc"); continue }
    $locText = [System.IO.File]::ReadAllText($lp, $utf8)
    $Q = [char]34
    $missing = 0
    foreach ($e in $events) {
        if ($null -eq $e.id) { continue }
        if (-not $locText.Contains($Q + 'ev_' + $e.id + $Q)) { $missing++ }
        elseif (-not $locText.Contains($Q + 'ev_' + $e.id + '_desc' + $Q)) { $missing++ }
        else {
            foreach ($o in @($e.options)) {
                if ($null -ne $o.key -and -not $locText.Contains($Q + 'ev_' + $e.id + '_' + $o.key + $Q)) { $missing++; break }
            }
        }
    }
    Assert-True ($missing -eq 0) "$loc 缺少 $missing 个事件的文案（标题/描述/选项）"
}

if ($failures.Count -gt 0) {
    $show = $failures | Select-Object -First 30
    foreach ($f in $show) { Write-Host "EVENTS_SCHEMA_RED: $f" }
    if ($failures.Count -gt 30) { Write-Host "EVENTS_SCHEMA_RED: ...还有 $($failures.Count - 30) 条" }
    Write-Host "EVENTS_SCHEMA_RED: $($failures.Count) check(s) failed"
    exit 1
}

Write-Host "EVENTS_SCHEMA_GREEN: $($events.Count) 个事件 schema 合法 + 6 语言文案完整 + 连锁无断链"
exit 0
