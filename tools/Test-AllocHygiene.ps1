param(
    [string]$SourcePath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'Core\TradeSimulationWorker.cs')
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $SourcePath -PathType Leaf)) {
    Write-Host "ALLOC_HYGIENE_RED: source file not found: $SourcePath"
    exit 1
}

$source = [System.IO.File]::ReadAllText($SourcePath)

# 1) Route-preparation cycle region: PrepareRoutes signature .. CompareCities signature.
#    Every List/Dictionary creation inside this span is rejected, including return-based
#    allocations, so a rent-like helper cannot be smuggled into the cycle path.
#    Singleline non-greedy match is line-ending agnostic (`.` matches `\r` and `\n`).
$region = [regex]::Match(
    $source,
    'private static CycleResult Compute\((?<body>.*?)private static float ComputeGini\(',
    [System.Text.RegularExpressions.RegexOptions]::Singleline)
if (-not $region.Success) {
    Write-Host 'ALLOC_HYGIENE_RED: cycle-path region (Compute .. ComputeGini) not found; invariant unverifiable'
    exit 1
}
$regionStart = $region.Index
$regionEnd = $region.Index + $region.Length

# 2) Line scan with exact offsets. Split on LF and keep any trailing CR inside each element
#    so offsets stay correct for CRLF and LF files alike. In-region collection creation is
#    always RED; out-of-region creations must be field initializers (single-line or
#    multi-line, declaration line ending with '=') or the bounded pool-growth return of
#    the rent helper.
$lines = $source.Split("`n")
$offset = 0
for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    $lineStart = $offset
    $offset = $lineStart + $line.Length + 1
    if ($line -notmatch 'new\s+(List|Dictionary)<') { continue }

    if ($lineStart -ge $regionStart -and $lineStart -lt $regionEnd) {
        Write-Host "ALLOC_HYGIENE_RED: collection creation inside cycle-path region (Compute .. ComputeGini) at line $($i + 1): $($line.Trim())"
        exit 1
    }

    # Outside the region: bounded pool growth (rent helper) or field initializer only.
    if ($line -match '^\s*return new\s+List<') { continue }
    if ($line -match '^\s*(public|private|internal|protected)\s+.*\b(readonly|static)\b.*=\s*new\s+(List|Dictionary)<') { continue }
    $previous = ''
    for ($j = $i - 1; $j -ge 0; $j--) {
        if ($lines[$j].Trim().Length -gt 0) { $previous = $lines[$j]; break }
    }
    if ($previous -match '^\s*(public|private|internal|protected)\s+.*\b(readonly|static)\b.*=\s*$') { continue }
    Write-Host "ALLOC_HYGIENE_RED: non-declaration collection creation at line $($i + 1): $($line.Trim())"
    exit 1
}

Write-Host 'ALLOC_HYGIENE_GREEN: cycle path has no local List/Dictionary allocation'
exit 0