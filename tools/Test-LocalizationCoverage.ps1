param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

# 本地化覆盖门禁：
# 1) 代码引用的每个本地化键（L/Lf/LocalizationService.Get 的字面量）必须存在于全部 4 个语言文件；
# 2) Core/UI 代码中 Notify( 调用行禁止硬编码 CJK 文案（必须走 NotifyLocalized）；
# 3) OnLanguageChanged 必须刷新全部四个悬浮窗（标题刷新接线）。
# 任一断言失败即 RED。

$ErrorActionPreference = 'Stop'

$failures = New-Object System.Collections.Generic.List[string]
$utf8 = New-Object System.Text.UTF8Encoding($false)
$localeDir = Join-Path $Root 'Locales'

# ===== 加载四个语言文件的键集合 =====
$langKeys = @{}
foreach ($loc in @('ch.json', 'en.json', 'zh_tw.json', 'ru.json')) {
    $locPath = Join-Path $localeDir $loc
    if (-not (Test-Path -LiteralPath $locPath -PathType Leaf)) {
        Write-Host "LOCALIZATION_COVERAGE_RED: locale file not found: $locPath"
        exit 1
    }
    try {
        $json = Get-Content -LiteralPath $locPath -Raw -Encoding UTF8 | ConvertFrom-Json
    } catch {
        Write-Host "LOCALIZATION_COVERAGE_RED: $loc is not valid JSON: $($_.Exception.Message)"
        exit 1
    }
    $set = New-Object 'System.Collections.Generic.HashSet[string]'
    foreach ($p in $json.PSObject.Properties) { [void]$set.Add($p.Name) }
    $langKeys[$loc] = $set
}

# ===== 收集产品源文件（排除 tools/evidence）=====
$sources = @(Get-ChildItem -LiteralPath $Root -Filter '*.cs' -File -Recurse | Where-Object {
    $_.FullName -notmatch '[\\/](bin|obj|evidence|tools)[\\/]'
})
if ($sources.Count -eq 0) {
    Write-Host 'LOCALIZATION_COVERAGE_RED: no product C# sources found'
    exit 1
}

# 键引用正则（字面量取词）：L("k") / Lf("k" / LocalizationService.Get("k"
$rxL  = [regex]'(?<![A-Za-z0-9_])L\("([^"]+)"\)'
$rxLf = [regex]'(?<![A-Za-z0-9_])Lf\("([^"]+)"\)'
$rxGet = [regex]'LocalizationService\.Get\("([^"]+)"\)'
$rxNotify = [regex]'\bNotify\('
$rxCjk = [regex]'[\u4e00-\u9fff]'
$rxLineComment = [regex]'^\s*//'

$referencedKeys = New-Object 'System.Collections.Generic.HashSet[string]'

foreach ($f in $sources) {
    $lines = [System.IO.File]::ReadAllLines($f.FullName, $utf8)
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($rxLineComment.IsMatch($line)) { continue }

        # 1) 字面量键引用
        foreach ($rx in @($rxL, $rxLf, $rxGet)) {
            foreach ($m in $rx.Matches($line)) {
                $k = $m.Groups[1].Value
                if ($k.Length -eq 0) { continue }
                [void]$referencedKeys.Add($k)
                foreach ($loc in $langKeys.Keys) {
                    if (-not $langKeys[$loc].Contains($k)) {
                        $failures.Add("$loc missing key '$k' (referenced at $($f.Name) line $($i + 1))")
                    }
                }
            }
        }

        # 2) Notify( 携带硬编码 CJK（仅 Core/UI；Services/Models 无 Notify 调用）
        if (($f.FullName -match '[\\/]Core[\\/]|[\\/]UI[\\/]') -and $rxNotify.IsMatch($line) -and $rxCjk.IsMatch($line)) {
            $failures.Add("hardcoded CJK in Notify at $($f.Name) line $($i + 1): $($line.Trim())")
        }
    }
}

# ===== 2b) 四语言键集合完全一致（全面本地化硬约束）=====
$chSet = $langKeys['ch.json']; $enSet = $langKeys['en.json']
$zhTwSet = $langKeys['zh_tw.json']; $ruSet = $langKeys['ru.json']
foreach ($pair in @(
    @($chSet, $enSet, 'en.json'),
    @($chSet, $zhTwSet, 'zh_tw.json'),
    @($chSet, $ruSet, 'ru.json')
)) {
    foreach ($k in $pair[0]) {
        if (-not $pair[1].Contains($k)) { $failures.Add("$($pair[2]) missing key '$k' (key-set divergence)") }
    }
    foreach ($k in $pair[1]) {
        if (-not $pair[0].Contains($k)) { $failures.Add("ch.json missing key '$k' (key-set divergence)") }
    }
}

# ===== 3) 语言切换必须刷新全部四个窗口标题 =====
$cbText = [System.IO.File]::ReadAllText((Join-Path $Root 'Services\ConfigCallbacks.cs'), $utf8)
foreach ($w in @('EconomyHUD', 'TradeShareWindow', 'EventWindow', 'RichListWindow')) {
    if ($cbText -notmatch ([regex]::Escape("$w.Instance?.RefreshAllTexts()"))) {
        $failures.Add("OnLanguageChanged does not refresh $w title/content")
    }
}

# ===== 4) 组合键：工具栏按钮 tooltip（id 与 id_description）必须四语言齐全 =====
$buttonIds = @('economy_toggle', 'economy_intervene', 'economy_collect', 'economy_clear',
               'economy_rich', 'economy_events', 'economy_trade_share', 'economy_cycle_phase')
foreach ($id in $buttonIds) {
    foreach ($loc in $langKeys.Keys) {
        if (-not $langKeys[$loc].Contains($id)) { $failures.Add("$loc missing toolbar tooltip key '$id'") }
        if (-not $langKeys[$loc].Contains($id + '_description')) { $failures.Add("$loc missing toolbar tooltip key '$($id)_description'") }
    }
}

if ($failures.Count -gt 0) {
    foreach ($f2 in $failures) { Write-Host "LOCALIZATION_COVERAGE_RED: $f2" }
    Write-Host "LOCALIZATION_COVERAGE_RED: $($failures.Count) coverage check(s) failed"
    exit 1
}

Write-Host "LOCALIZATION_COVERAGE_GREEN: $($referencedKeys.Count) referenced keys present in 4 locales; no hardcoded-CJK Notify; all window titles wired"
exit 0
