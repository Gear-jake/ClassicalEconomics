param(
    [string]$Golden = '',
    [string]$Current = '',
    [double]$Tolerance = 0.01,
    [switch]$SelfTest
)

# 黄金行为一致性比较器（波次 0，W0-C）。
# 用途：优化前后各跑一次固定存档 + 固定配置的固定年数，由模组写出 perf-parity.json，
#       本脚本判定两者行为是否等价 —— 整数项逐位相等，浮点项按相对容差比较。
#
# 不带参数运行时执行自检：合成两份 dump，验证「相同 dump 判 GREEN」「篡改 dump 判 RED」，
# 因此在 run_all_tests.ps1 中被自动执行也是有意义的（证明比较器本身有效）。
# 带 -Golden / -Current 时对真实 dump 做比较。

$ErrorActionPreference = 'Stop'

$integerFields = @(
    'Year', 'CycleIndex', 'MoneyTotal', 'ActorCount', 'CivilizedCount',
    'TraitGolden', 'TraitRevival', 'TraitFlourish', 'TraitCollapse',
    'TraitEdu', 'TraitWelfare', 'TraitMil', 'TraitAusterity'
)
$floatFields = @('GlobalGDP', 'AvgWealth', 'GiniCoefficient')
$allFields = @($integerFields) + @($floatFields)

function Get-FieldValue {
    param($Object, [string]$Name)
    if (-not ($Object.PSObject.Properties.Name -contains $Name)) { return $null }
    return $Object.PSObject.Properties[$Name].Value
}

function Compare-ParityDumps {
    param(
        [string]$GoldenPath,
        [string]$CurrentPath,
        [double]$Tol
    )
    $mismatches = New-Object System.Collections.Generic.List[string]

    foreach ($p in @($GoldenPath, $CurrentPath)) {
        if (-not (Test-Path -LiteralPath $p -PathType Leaf)) {
            $mismatches.Add("dump file not found: $p")
            return $mismatches
        }
    }

    $golden = Get-Content -LiteralPath $GoldenPath -Raw | ConvertFrom-Json
    $current = Get-Content -LiteralPath $CurrentPath -Raw | ConvertFrom-Json

    foreach ($field in $allFields) {
        if (-not ($golden.PSObject.Properties.Name -contains $field)) {
            $mismatches.Add("golden dump missing field $field")
        }
        if (-not ($current.PSObject.Properties.Name -contains $field)) {
            $mismatches.Add("current dump missing field $field")
        }
    }
    if ($mismatches.Count -gt 0) { return $mismatches }

    foreach ($field in $integerFields) {
        $a = Get-FieldValue $golden $field
        $b = Get-FieldValue $current $field
        if ($a -ne $b) {
            $mismatches.Add("$field differs (golden $a, current $b) - integer fields must match exactly")
        }
    }

    foreach ($field in $floatFields) {
        $a = [double](Get-FieldValue $golden $field)
        $b = [double](Get-FieldValue $current $field)
        $scale = [Math]::Max(1.0, [Math]::Abs($a))
        if ([Math]::Abs($a - $b) -gt ($Tol * $scale)) {
            $mismatches.Add("$field differs beyond tolerance $Tol (golden $a, current $b)")
        }
    }

    return $mismatches
}

if ($Golden.Length -gt 0 -and $Current.Length -gt 0) {
    $problems = Compare-ParityDumps -GoldenPath $Golden -CurrentPath $Current -Tol $Tolerance
    if ($problems.Count -gt 0) {
        foreach ($p in $problems) { Write-Host "BEHAVIOR_PARITY_RED: $p" }
        Write-Host "BEHAVIOR_PARITY_RED: $($problems.Count) mismatch(es) - behaviour is NOT equivalent"
        exit 1
    }
    Write-Host "BEHAVIOR_PARITY_GREEN: current dump is behaviourally equivalent to golden (integers exact, floats within $Tolerance)"
    exit 0
}

# ===== 自检模式 =====
$tempDir = $env:TEMP
$stamp = Get-Date -Format 'yyyyMMddHHmmss'
$basePath = Join-Path $tempDir ("ParityBase_" + $stamp + '.json')
$samePath = Join-Path $tempDir ("ParitySame_" + $stamp + '.json')
$diffPath = Join-Path $tempDir ("ParityDiff_" + $stamp + '.json')

$base = @{
    Schema = 'perf-parity/1'
    Year = 12
    CycleIndex = 12
    MoneyTotal = 987654321
    ActorCount = 5000
    CivilizedCount = 4821
    TraitGolden = 0
    TraitRevival = 3
    TraitFlourish = 1
    TraitCollapse = 0
    TraitEdu = 120
    TraitWelfare = 40
    TraitMil = 12
    TraitAusterity = 0
    GlobalGDP = 1234567.25
    AvgWealth = 246.91
    GiniCoefficient = 0.4213
}

function Write-Dump {
    param([string]$Path, $Object)
    ($Object | ConvertTo-Json) | Set-Content -LiteralPath $Path -Encoding UTF8
}

$failures = New-Object System.Collections.Generic.List[string]
try {
    Write-Dump -Path $basePath -Object $base
    Write-Dump -Path $samePath -Object $base

    $diff = @{}
    foreach ($key in $base.Keys) { $diff[$key] = $base[$key] }
    $diff['MoneyTotal'] = $base['MoneyTotal'] + 1
    Write-Dump -Path $diffPath -Object $diff

    $sameResult = Compare-ParityDumps -GoldenPath $basePath -CurrentPath $samePath -Tol $Tolerance
    if ($sameResult.Count -ne 0) {
        $failures.Add("self-test: identical dumps must compare equal, got $($sameResult.Count) mismatch(es)")
    }

    $diffResult = Compare-ParityDumps -GoldenPath $basePath -CurrentPath $diffPath -Tol $Tolerance
    if ($diffResult.Count -eq 0) {
        $failures.Add('self-test: a tampered MoneyTotal must be rejected')
    } elseif (-not (($diffResult -join ' | ') -match 'MoneyTotal')) {
        $failures.Add('self-test: tampered dump rejected for the wrong reason: ' + ($diffResult -join ' | '))
    }
} finally {
    foreach ($p in @($basePath, $samePath, $diffPath)) {
        if (Test-Path -LiteralPath $p -PathType Leaf) { Remove-Item -LiteralPath $p -Force }
    }
}

if ($failures.Count -gt 0) {
    foreach ($f in $failures) { Write-Host "BEHAVIOR_PARITY_RED: $f" }
    Write-Host "BEHAVIOR_PARITY_RED: $($failures.Count) self-test failure(s)"
    exit 1
}

Write-Host 'BEHAVIOR_PARITY_GREEN: comparator self-test passed (identical dumps match, tampered MoneyTotal rejected)'
Write-Host 'BEHAVIOR_PARITY_NOTE: pass -Golden <golden.json> -Current <dump.json> to compare two real in-game dumps'
exit 0
