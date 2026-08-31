$ErrorActionPreference = 'Stop'
$runDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("ClassicalEconomics.TopN." + [Guid]::NewGuid().ToString('N'))
[System.IO.Directory]::CreateDirectory($runDirectory) | Out-Null
$out = Join-Path $runDirectory 'TopNBoundaryInvariantTest.exe'
$source = Join-Path $PSScriptRoot 'TopNBoundaryInvariantTest.cs'
$csc = 'C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe'

try {
    & $csc /nologo /optimize+ /langversion:latest "/out:$out" $source
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    & $out
    exit $LASTEXITCODE
} finally {
    if (Test-Path -LiteralPath $runDirectory) { Remove-Item -LiteralPath $runDirectory -Recurse -Force }
}
