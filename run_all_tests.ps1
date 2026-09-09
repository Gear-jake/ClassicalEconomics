param(
    [string]$Filter = ''
)

$ErrorActionPreference = 'Stop'

$toolsDir = Join-Path $PSScriptRoot 'tools'
$auditPath = Join-Path $PSScriptRoot 'performance_audit.ps1'
# 成功标记：多数脚本输出 *_GREEN / *_OK；Test-InheritanceScan 输出 *_EQUIVALENT。
$verdictPattern = '_(GREEN|OK|EQUIVALENT)\b'

function Invoke-PsFile {
    param(
        [string]$Path,
        [string[]]$ArgumentList = @()
    )
    if ($ArgumentList.Count -eq 0) {
        $captured = & powershell -NoProfile -ExecutionPolicy Bypass -File $Path 2>&1 | Out-String
    } else {
        $captured = & powershell -NoProfile -ExecutionPolicy Bypass -File $Path @ArgumentList 2>&1 | Out-String
    }
    if ($null -eq $captured) { $captured = '' }
    return [pscustomobject]@{
        ExitCode = $LASTEXITCODE
        Output = [string]$captured
    }
}

$allTests = @(Get-ChildItem -LiteralPath $toolsDir -Filter 'Test-*.ps1' -File | Sort-Object -Property Name)

$tests = $allTests
if ($Filter.Length -gt 0) {
    $tests = @($tests | Where-Object {
        $_.Name.IndexOf($Filter, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
    })
}

Write-Host ("RUN_ALL_TESTS: {0} test(s) discovered, {1} selected by filter '{2}'; each gate runs in a fresh PowerShell process." -f $allTests.Count, $tests.Count, $Filter)

$rows = New-Object System.Collections.Generic.List[object]

foreach ($test in $tests) {
    $run = Invoke-PsFile -Path $test.FullName
    $isGreen = ($run.ExitCode -eq 0) -and [System.Text.RegularExpressions.Regex]::IsMatch(
        $run.Output, $verdictPattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $verdict = if ($isGreen) { 'GREEN' } else { 'RED' }
    $rows.Add([pscustomobject]@{
        Name = $test.Name
        ExitCode = $run.ExitCode
        Verdict = $verdict
        Output = $run.Output
    })
}

$audit = Invoke-PsFile -Path $auditPath -ArgumentList @('-SkipBuild')
$auditIsGreen = ($audit.ExitCode -eq 0) -and [System.Text.RegularExpressions.Regex]::IsMatch(
    $audit.Output, $verdictPattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
$auditVerdict = if ($auditIsGreen) { 'GREEN' } else { 'RED' }
$rows.Add([pscustomobject]@{
    Name = 'performance_audit.ps1 (-SkipBuild)'
    ExitCode = $audit.ExitCode
    Verdict = $auditVerdict
    Output = $audit.Output
})

$nameWidth = 10
foreach ($row in $rows) {
    if ($row.Name.Length -gt $nameWidth) { $nameWidth = $row.Name.Length }
}
$fmt = "{0,-$nameWidth}  {1,6}  {2}"
Write-Host ''
Write-Host ($fmt -f 'GATE', 'EXIT', 'VERDICT')
Write-Host ($fmt -f ('-' * $nameWidth), '----', '-------')
foreach ($row in $rows) {
    Write-Host ($fmt -f $row.Name, $row.ExitCode, $row.Verdict)
}

$reds = @($rows | Where-Object { $_.Verdict -eq 'RED' })
$greens = $rows.Count - $reds.Count

Write-Host ''
Write-Host ("GREEN: {0}   RED: {1}" -f $greens, $reds.Count)
if ($reds.Count -gt 0) {
    Write-Host ("RED gates: {0}" -f (($reds | ForEach-Object { $_.Name }) -join ', '))
    Write-Host ''
    foreach ($red in $reds) {
        Write-Host ("---- output for {0} ----" -f $red.Name)
        Write-Host $red.Output.TrimEnd()
        Write-Host ''
    }
}

if ($reds.Count -eq 0) {
    Write-Host ("RUN_ALL_TESTS_GREEN: {0}/{1} gates passed" -f $rows.Count, $rows.Count)
    exit 0
}

Write-Host ("RUN_ALL_TESTS_RED: {0} gate(s) failed: {1}" -f $reds.Count, (($reds | ForEach-Object { $_.Name }) -join ', '))
exit 1
