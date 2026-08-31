$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$production = [System.IO.File]::ReadAllText((Join-Path $root 'Core\TradeSimulationWorker.cs'))
if ($production -match 'NeighborWindow') {
    Write-Host 'EXACT_NEAREST_PAIRS_RED: fixed window can miss the true nearest city pair'
    exit 1
}

$runDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("ClassicalEconomics.NearestPair." + [Guid]::NewGuid().ToString('N'))
[System.IO.Directory]::CreateDirectory($runDirectory) | Out-Null
$out = Join-Path $runDirectory 'NearestPairInvariantTest.exe'
$source = Join-Path $PSScriptRoot 'NearestPairInvariantTest.cs'
$csc = 'C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe'
try {
    & $csc /nologo /optimize+ /langversion:latest "/out:$out" $source
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    & $out
    exit $LASTEXITCODE
} finally {
    if (Test-Path -LiteralPath $runDirectory) { Remove-Item -LiteralPath $runDirectory -Recurse -Force }
}
