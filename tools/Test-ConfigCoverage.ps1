param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

# W3 配置覆盖门禁：把「配置单一来源」从靠人记变成靠门禁。
#
# 配置在本项目分散在 4 处：
#   default_config.json (NML 配置项) / Models\UnrestConfig.cs (运行时字段)
#   Services\ConfigCallbacks.cs (AllConfigIds + 解析 + 回调) / Locales\*.json (文案)
# 任何一处漏改都会静默漂移（键能改但值不生效 / 无文案）。
# 本门禁对**全部**配置项校验上述四处的一致性。

$ErrorActionPreference = 'Stop'

$failures = New-Object System.Collections.Generic.List[string]

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { $failures.Add($Message) }
}

$utf8 = New-Object System.Text.UTF8Encoding($false)
$Q = [char]34

$cfgPath = Join-Path $Root 'default_config.json'
$ucPath  = Join-Path $Root 'Models\UnrestConfig.cs'
$cbPath  = Join-Path $Root 'Services\ConfigCallbacks.cs'
$locDir  = Join-Path $Root 'Locales'

foreach ($p in @($cfgPath, $ucPath, $cbPath)) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
        Write-Host "CONFIG_COVERAGE_RED: required file not found: $p"
        exit 1
    }
}

$cfg    = Get-Content -LiteralPath $cfgPath -Raw -Encoding UTF8 | ConvertFrom-Json
$ucText = [System.IO.File]::ReadAllText($ucPath, $utf8)
$cbText = [System.IO.File]::ReadAllText($cbPath, $utf8)

$items = New-Object System.Collections.Generic.List[object]
foreach ($prop in $cfg.PSObject.Properties) {
    foreach ($it in @($prop.Value)) {
        if ($null -ne $it.Id) { $items.Add($it) }
    }
}
Assert-True ($items.Count -gt 0) 'default_config.json 未解析出任何配置项'

# 1) Id 唯一
$dup = @($items | Group-Object Id | Where-Object { $_.Count -gt 1 })
foreach ($d in $dup) { $failures.Add("default_config.json 存在重复 Id: $($d.Name)") }

# 2) Type 与 Callback 合法
foreach ($it in $items) {
    Assert-True (@('SWITCH','TEXT') -contains $it.Type) "$($it.Id) 的 Type 非法: $($it.Type)（只允许 SWITCH / TEXT）"
    Assert-True (-not [string]::IsNullOrEmpty($it.Callback)) "$($it.Id) 缺少 Callback"
}

# 3) AllConfigIds 必须与配置项一一对应
$allIdsBlock = [regex]::Match($cbText, 'AllConfigIds\s*=\s*\{[^}]*\}').Value
Assert-True ($allIdsBlock.Length -gt 0) 'ConfigCallbacks.cs 中未找到 AllConfigIds 块'
foreach ($it in $items) {
    $needle = $Q + $it.Id + $Q
    Assert-True ($allIdsBlock.Contains($needle)) "AllConfigIds 缺少配置项 $($it.Id)"
}
$allIdsCount = ([regex]::Matches($allIdsBlock, $Q)).Count / 2
Assert-True ($allIdsCount -eq $items.Count) "AllConfigIds 条目数 $allIdsCount 与 default_config.json 配置项数 $($items.Count) 不一致"

# 4) 回调方法必须存在，且其赋值的字段必须在 UnrestConfig 中声明
foreach ($it in $items) {
    $parts = $it.Callback -split ':'
    Assert-True (($parts.Count -eq 2) -and ($parts[0] -eq 'EconomyConfigCallbacks')) "$($it.Id) 的 Callback 格式非法: $($it.Callback)"
    if ($parts.Count -ne 2) { continue }
    $method = $parts[1]
    if ($cbText -notmatch [regex]::Escape("public static void $method(")) {
        $failures.Add("ConfigCallbacks.cs 缺少回调方法 $method（配置项 $($it.Id)）")
        continue
    }
    $body = [regex]::Match($cbText, [regex]::Escape("public static void $method(") + '[\s\S]*?\n\s*\}').Value
    # 直接写法：UnrestConfig.Instance.<Field>
    $fields = New-Object System.Collections.Generic.List[string]
    foreach ($f in [regex]::Matches($body, 'UnrestConfig\.Instance\.([A-Za-z_][A-Za-z0-9_]*)')) { $fields.Add($f.Groups[1].Value) }
    # 别名写法：var u = UnrestConfig.Instance; 之后 u.<Field>
    $aliases = @()
    foreach ($a in [regex]::Matches($body, 'var\s+([A-Za-z_][A-Za-z0-9_]*)\s*=\s*UnrestConfig\.Instance\s*;')) { $aliases += $a.Groups[1].Value }
    foreach ($al in $aliases) {
        $pat = [regex]::Escape($al) + '\.([A-Za-z_][A-Za-z0-9_]*)'
        foreach ($f in [regex]::Matches($body, $pat)) { $fields.Add($f.Groups[1].Value) }
    }
    $uniq = @($fields | Sort-Object -Unique)
    Assert-True ($uniq.Count -gt 0) "回调 $method 未对 UnrestConfig.Instance 赋值（配置项 $($it.Id)）"
    foreach ($name in $uniq) {
        $declared = $ucText -match ('public\s+[\w\[\]<>\.]+\s+' + [regex]::Escape($name) + '\s*[;=]')
        if (-not $declared) { $failures.Add("UnrestConfig.cs 未声明字段 $name（被回调 $method 引用）") }
    }
}

# 5) 本地化：六个语言都要有 label 与 Description
$locales = @('ch.json','zh_tw.json','en.json','ja.json','de.json','ru.json')
foreach ($loc in $locales) {
    $lp = Join-Path $locDir $loc
    if (-not (Test-Path -LiteralPath $lp -PathType Leaf)) { $failures.Add("locale file not found: $loc"); continue }
    $locText = [System.IO.File]::ReadAllText($lp, $utf8)
    foreach ($it in $items) {
        if (-not $locText.Contains($Q + $it.Id + $Q)) { $failures.Add("$loc 缺少配置项文案 $($it.Id)") }
        if (-not $locText.Contains($Q + $it.Id + ' Description' + $Q)) { $failures.Add("$loc 缺少配置项描述 $($it.Id) Description") }
    }
}

if ($failures.Count -gt 0) {
    foreach ($f in $failures) { Write-Host "CONFIG_COVERAGE_RED: $f" }
    Write-Host "CONFIG_COVERAGE_RED: $($failures.Count) check(s) failed"
    exit 1
}

Write-Host "CONFIG_COVERAGE_GREEN: $($items.Count) 个配置项在 config/runtime/callbacks/6 locales 四处全部一致"
exit 0
