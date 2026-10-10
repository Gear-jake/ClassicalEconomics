$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

# ─────────────────────────────────────────────────────────────────────────────
# W2.6 契约门禁：图表宿主必须**常驻**（只创建一次、不进 _lines），且配色取自本体。
#
# 为什么需要它：这是 W2.6 的全部价值所在 —— 图表若每次刷新重建 GameObject，
# ChartMeshGraphic 的 _lastSig 脏标记就永远命中不了（签名从 int.MinValue 开始），
# 属于"退回去也不报错、只是悄悄变慢"的回归，只有源码断言拦得住。
#
# 返回**违规名**（字符串）而不是布尔：变异测试失败时能直接指出是哪个不变量破了。
# ⚠ 本文件必须带 UTF-8 BOM（含中文注释；PowerShell 5.1 按 ANSI 读无 BOM 文件会吞换行）。
# ─────────────────────────────────────────────────────────────────────────────

$chartsPath  = Join-Path $root 'UI\EconomyHUD.Charts.cs'
$meshPath    = Join-Path $root 'UI\ChartMeshGraphic.cs'
$palettePath = Join-Path $root 'UI\VanillaChartPalette.cs'

foreach ($p in @($chartsPath, $meshPath, $palettePath)) {
    if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
        Write-Host "CHART_HOST_RED: source file not found: $p"
        exit 1
    }
}

function Get-ChartContractViolation([string]$charts, [string]$mesh, [string]$palette) {
    $sl = [System.Text.RegularExpressions.RegexOptions]::Singleline

    if (-not [Regex]::IsMatch($charts, 'private void EnsureGdpHost\([^)]*\)[\s\S]*?if \(_gdpBox != null\) return;', $sl)) { return 'gdp-host-not-created-once' }
    if (-not [Regex]::IsMatch($charts, 'private void EnsureGiniHost\([^)]*\)[\s\S]*?if \(_giniBox != null\) return;', $sl)) { return 'gini-host-not-created-once' }
    if ([Regex]::IsMatch($charts, '_lines\.Add\(_gdpBox', $sl)) { return 'gdp-host-registered-in-lines' }
    if ([Regex]::IsMatch($charts, '_lines\.Add\(_giniBox', $sl)) { return 'gini-host-registered-in-lines' }
    if ([Regex]::Matches($charts, 'new GameObject\("GdpChartBox"').Count -ne 1) { return 'gdp-host-created-more-than-once' }
    if ([Regex]::Matches($charts, 'new GameObject\("GiniChartBox"').Count -ne 1) { return 'gini-host-created-more-than-once' }
    if (-not [Regex]::IsMatch($charts, '_gdpGraph\.SetChartData\(', $sl)) { return 'gdp-refresh-must-reuse-graph' }
    if (-not [Regex]::IsMatch($charts, '_giniGraph\.SetChartData\(', $sl)) { return 'gini-refresh-must-reuse-graph' }
    if (-not [Regex]::IsMatch($charts, '_gdpHover\.SetPointCount\(snaps\.Count\)', $sl)) { return 'hover-point-count-not-refreshed' }

    if (-not [Regex]::IsMatch($mesh, 'areaAlphaTop = VanillaChartPalette\.AreaAlphaTop', $sl)) { return 'area-alpha-top-not-vanilla' }
    if (-not [Regex]::IsMatch($mesh, 'areaAlphaBottom = VanillaChartPalette\.AreaAlphaBottom', $sl)) { return 'area-alpha-bottom-not-vanilla' }
    if (-not [Regex]::IsMatch($mesh, 'Color top = fill; top\.a = areaAlphaTop;', $sl)) { return 'area-gradient-top-missing' }
    if (-not [Regex]::IsMatch($mesh, 'Color bottom = fill; bottom\.a = areaAlphaBottom;', $sl)) { return 'area-gradient-bottom-missing' }
    if ([Regex]::IsMatch($mesh, 'gridColor = new Color\(', $sl)) { return 'grid-color-hardcoded' }
    if (-not [Regex]::IsMatch($mesh, 'gridColor = VanillaChartPalette\.GridLine', $sl)) { return 'grid-color-not-vanilla' }
    if (-not [Regex]::IsMatch($mesh, 'return VanillaChartPalette\.PhaseBand\(phase\);', $sl)) { return 'phase-band-not-vanilla' }

    if (-not [Regex]::IsMatch($palette, 'Toolbox\.makeColor\(', $sl)) { return 'palette-not-from-vanilla-toolbox' }
    if (-not [Regex]::IsMatch($palette, 'Toolbox\.colorBetween\(', $sl)) { return 'palette-axis-ramp-not-from-vanilla' }
    if (-not [Regex]::IsMatch($palette, 'base/Toolbox\.cs:\d+', $sl)) { return 'palette-missing-toolbox-citation' }
    if (-not [Regex]::IsMatch($palette, 'base/HistoryDataAsset\.cs:\d+', $sl)) { return 'palette-missing-graph-material-citation' }

    return ''
}

$charts  = [System.IO.File]::ReadAllText($chartsPath)
$mesh    = [System.IO.File]::ReadAllText($meshPath)
$palette = [System.IO.File]::ReadAllText($palettePath)

$violation = Get-ChartContractViolation $charts $mesh $palette
if ($violation -ne '') {
    Write-Host "CHART_HOST_RED: $violation"
    exit 1
}

function Test-Mutation([string]$name, [string]$c, [string]$m, [string]$p) {
    $v = Get-ChartContractViolation $c $m $p
    if ($v -eq '') {
        Write-Host "CHART_HOST_RED: mutation '$name' did not turn RED (gate not sensitive)"
        exit 1
    }
    Write-Host "MUTATION_RED_OK: $name -> $v"
}

Test-Mutation 'host-guard-removed' ($charts -replace 'if \(_gdpBox != null\) return;', '') $mesh $palette
Test-Mutation 'host-registered-in-lines' ($charts -replace '(_gdpBox\.SetActive\(true\);)', '$1 _lines.Add(_gdpBox);') $mesh $palette
Test-Mutation 'area-flat-again' $charts ($mesh -replace 'Color bottom = fill; bottom\.a = areaAlphaBottom;', 'Color bottom = fill; bottom.a = 0.14f;') $palette
Test-Mutation 'grid-color-hardcoded' $charts ($mesh -replace 'gridColor = VanillaChartPalette\.GridLine', 'gridColor = new Color(0.35f, 0.35f, 0.4f, 0.45f)') $palette
Test-Mutation 'palette-hardcoded' $charts $mesh ($palette -replace 'Toolbox\.makeColor\(', 'MakeColorLocal(')

Write-Host 'CHART_HOST_GREEN: chart hosts persistent + vanilla-sourced palette + 5 mutations RED'
exit 0
