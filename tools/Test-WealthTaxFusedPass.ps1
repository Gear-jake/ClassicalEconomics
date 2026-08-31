param([string]$Root = '')
$ErrorActionPreference = 'Stop'
if ($Root -eq '') { $Root = Split-Path -Parent $PSScriptRoot }
$sourcePath = Join-Path $Root 'Core\DataCollector.cs'

if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
    Write-Host 'WEALTH_TAX_FUSED_PASS_RED: DataCollector.cs not found; invariant unverifiable'
    exit 1
}
$source = [System.IO.File]::ReadAllText($sourcePath)

# 1) ApplyWealthTax region: ApplyWealthTax signature .. UpdateTopRich signature (Singleline = line-ending agnostic).
$region = [regex]::Match(
    $source,
    'public static void ApplyWealthTax\((?<body>.*?)private static void UpdateTopRich\(',
    [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $region.Success) {
    Write-Host 'WEALTH_TAX_FUSED_PASS_RED: ApplyWealthTax .. UpdateTopRich region not found; invariant unverifiable'
    exit 1
}
$body = $region.Groups['body'].Value

# 2) Exactly ONE aliveList traversal in the whole tax method (fused collect pass).
$aliveCount = [regex]::Matches($body, 'foreach \(var actor in aliveList\)').Count
if ($aliveCount -ne 1) {
    Write-Host "WEALTH_TAX_FUSED_PASS_RED: ApplyWealthTax must traverse aliveList exactly once (found $aliveCount); fusion incomplete"
    exit 1
}

# 3) Rich-buffer tax pass: a loop over the rich pool that charges and accumulates.
$richLoop = [regex]::Match($body, 'foreach \(var actor in rich\).*?actor\.addMoney\(-charged\); totalTax \+= charged;',
    [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $richLoop.Success) {
    Write-Host 'WEALTH_TAX_FUSED_PASS_RED: rich-buffer tax loop (foreach actor in rich -> addMoney(-charged), totalTax += charged) missing'
    exit 1
}

# 4) Poor-buffer distribution arithmetic pinned exactly (remainder-to-first over the
#    poor pool only). Dropping the remainder from `give` would still pass checks 1-3
#    and the harness (which compiles its own mirror), so these three lines are asserted
#    verbatim against the production source region, in source order.
$perLine = [regex]::Match($body, 'long per = totalTax / poorCount;')
if (-not $perLine.Success) {
    Write-Host 'WEALTH_TAX_FUSED_PASS_RED: distribution arithmetic "long per = totalTax / poorCount;" missing'
    exit 1
}
$remainderLine = [regex]::Match($body, 'long remainder = totalTax - per \* poorCount;')
if (-not $remainderLine.Success) {
    Write-Host 'WEALTH_TAX_FUSED_PASS_RED: distribution arithmetic "long remainder = totalTax - per * poorCount;" missing'
    exit 1
}
$giveLine = [regex]::Match($body, 'long give = per \+ \(i == 0 \? remainder : 0\);')
if (-not $giveLine.Success) {
    Write-Host 'WEALTH_TAX_FUSED_PASS_RED: distribution arithmetic "long give = per + (i == 0 ? remainder : 0);" missing (remainder must reach the first poor actor)'
    exit 1
}
if (-not ($perLine.Index -lt $remainderLine.Index -and $remainderLine.Index -lt $giveLine.Index)) {
    Write-Host 'WEALTH_TAX_FUSED_PASS_RED: distribution arithmetic out of order (per, remainder, give must appear in sequence)'
    exit 1
}
$addGive = $body.IndexOf('AddPositiveMoney(actor, give)')
if ($addGive -lt 0 -or $addGive -lt $giveLine.Index) {
    Write-Host 'WEALTH_TAX_FUSED_PASS_RED: poor-buffer distribution (AddPositiveMoney(actor, give) after give computation) missing'
    exit 1
}

# 5) Exact tax formula and charge clamp preserved verbatim.
if ($body -notmatch '\(long\)Mathf\.Min\(\(w - taxLine\) \* ratio, w \* MaxRatio\)') {
    Write-Host 'WEALTH_TAX_FUSED_PASS_RED: exact tax formula (long)Mathf.Min((w - taxLine) * ratio, w * MaxRatio) missing'
    exit 1
}
if ($body -notmatch 'int charged = \(int\)System\.Math\.Min\(tax, int\.MaxValue\);') {
    Write-Host 'WEALTH_TAX_FUSED_PASS_RED: int charge clamp (int)System.Math.Min(tax, int.MaxValue) missing'
    exit 1
}

# 6) Conservation order: recipients confirmed (poor empty -> return) BEFORE any rich deduction.
$poorGuard = $body.IndexOf('if (poor.Count == 0) return;')
if ($poorGuard -lt 0 -or $poorGuard -gt $richLoop.Index) {
    Write-Host 'WEALTH_TAX_FUSED_PASS_RED: poor-empty guard must precede the rich tax loop (no tax without recipients)'
    exit 1
}

Write-Host 'WEALTH_TAX_FUSED_PASS_GREEN: one aliveList pass, rich-buffer tax, poor-buffer distribution, conservation order intact'

# 7) Side-by-side conservation harness: pre-fusion vs fused must agree on every scenario.
$runDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("ClassicalEconomics.WealthTax." + [Guid]::NewGuid().ToString('N'))
[System.IO.Directory]::CreateDirectory($runDirectory) | Out-Null
$out = Join-Path $runDirectory 'WealthTaxConservationTest.exe'
$harness = Join-Path $PSScriptRoot 'WealthTaxConservationTest.cs'
$csc = 'C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe'
try {
    & $csc /nologo /optimize+ /langversion:latest "/out:$out" $harness
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    & $out
    exit $LASTEXITCODE
} finally {
    if (Test-Path -LiteralPath $runDirectory) { Remove-Item -LiteralPath $runDirectory -Recurse -Force }
}