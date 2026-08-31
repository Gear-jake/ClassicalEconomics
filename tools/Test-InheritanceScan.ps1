$ErrorActionPreference = 'Stop'
$runDirectory = Join-Path ([System.IO.Path]::GetTempPath()) ("ClassicalEconomics.InheritanceScan." + [Guid]::NewGuid().ToString('N'))
[System.IO.Directory]::CreateDirectory($runDirectory) | Out-Null
$out = Join-Path $runDirectory 'InheritanceScanBenchmark.exe'
$source = Join-Path $PSScriptRoot 'InheritanceScanBenchmark.cs'
$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'

try {
    & $csc /nologo /optimize+ "/out:$out" $source
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    & $out
    exit $LASTEXITCODE
} finally {
    if (Test-Path -LiteralPath $runDirectory) { Remove-Item -LiteralPath $runDirectory -Recurse -Force }
}
